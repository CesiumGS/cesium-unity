using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reinterop.Tests
{
    /// <summary>
    /// Shared compilation-building infrastructure for tests that need to compile a Reinterop source
    /// snippet, plus a way to run the generation pipeline directly (bypassing the
    /// <see cref="ISourceGenerator"/>/driver entry point) so tests can inspect the resulting
    /// <see cref="GeneratedResult"/> objects - in particular their <see cref="GeneratedResult.InteropFunctions"/>
    /// recipes - instead of only the final printed C++/C# text.
    /// </summary>
    internal static class GenerationTestHelper
    {
        public static readonly CSharpParseOptions ParseOptions = new CSharpParseOptions(LanguageVersion.Preview);

        // Reinterop's generated code assumes it's compiled into a Unity project, which supplies this
        // attribute (normally in UnityEngine.CoreModule.dll) - stub it out so generated code compiles
        // standalone here.
        public const string MonoPInvokeCallbackAttributeStub =
            """
            namespace AOT
            {
                public class MonoPInvokeCallbackAttribute : System.Attribute
                {
                    public MonoPInvokeCallbackAttribute(System.Type type) { }
                }
            }
            """;

        public static readonly IReadOnlyList<MetadataReference> References = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
            .ToArray();

        public static CSharpCompilation CreateCompilation(string source)
        {
            return CSharpCompilation.Create(
                "TestAssembly",
                new[]
                {
                    CSharpSyntaxTree.ParseText(source, ParseOptions),
                    CSharpSyntaxTree.ParseText(MonoPInvokeCallbackAttributeStub, ParseOptions)
                },
                References,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
        }

        /// <summary>
        /// Runs generation directly against a source snippet with a single
        /// "[Reinterop] ConfigureReinterop.ExposeToCPP()" method (the convention used throughout this
        /// test project), and returns each generated type's <see cref="GeneratedResult"/>, keyed by its
        /// simple C# type name. Unlike going through <see cref="RoslynSourceGenerator"/>'s
        /// <see cref="ISourceGenerator"/> entry point, this never writes any files to disk - only
        /// <see cref="CodeGenerator.DistributeToSourceFiles"/>/<see cref="CppSourceFile.Write"/> (not
        /// called here) do that.
        /// </summary>
        public static Dictionary<string, GeneratedResult> GenerateResults(string source)
        {
            CSharpCompilation compilation = CreateCompilation(source);
            CppGenerationContext context = new CppGenerationContext(compilation);

            MethodDeclarationSyntax exposeMethod = compilation.SyntaxTrees
                .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
                .Single(method => method.Identifier.Text == "ExposeToCPP");

            SemanticModel semanticModel = compilation.GetSemanticModel(exposeMethod.SyntaxTree);
            ExposeToCppSyntaxWalker walker = new ExposeToCppSyntaxWalker(context, semanticModel);
            walker.Visit(exposeMethod);

            Dictionary<ITypeSymbol, TypeToGenerate> typeDictionary = TypeToGenerate.Combine(new[] { walker.GenerationItems.Values });
            foreach (TypeToGenerate item in typeDictionary.Values)
                InheritanceChainer.Chain(item, typeDictionary);

            CodeGenerator codeGenerator = new CodeGenerator(context);

            Dictionary<string, GeneratedResult> results = new Dictionary<string, GeneratedResult>();
            foreach (TypeToGenerate item in typeDictionary.Values)
            {
                GeneratedResult? result = codeGenerator.GenerateType(item);
                if (result != null)
                    results[item.Type.Name] = result;
            }

            return results;
        }

        /// <summary>
        /// Finds the single recipe named <paramref name="name"/> taking exactly
        /// <paramref name="parameterCount"/> parameters - used to disambiguate overloads that share a
        /// name (e.g. a property's or field's getter vs. setter).
        /// </summary>
        public static CppInteropFunction Find(this GeneratedResult result, string name, int parameterCount)
        {
            return result.InteropFunctions.Single(function => function.Name == name && function.Parameters().Count == parameterCount);
        }
    }
}
