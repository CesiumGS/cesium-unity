using System.Collections.Immutable;
using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Oxidize;

[Generator]
public class OxidizeGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor RandomWarning = new DiagnosticDescriptor(id: "RANDOM001",
                                                                                              title: "Some random warning",
                                                                                              messageFormat: "Yep something went wrong '{0}'",
                                                                                              category: "SomeCategory",
                                                                                              DiagnosticSeverity.Warning,
                                                                                              isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(CSharpOxidizeAttribute.Generate);
        context.RegisterPostInitializationOutput(CSharpOxidizeNativeImplementationAttribute.Generate);
        context.RegisterPostInitializationOutput(CSharpObjectHandleUtility.Generate);

        // For each method in the Oxidize class, look at the types, methods, and properties it uses and create from
        // that a list of items to be generated (GenerationItems).
        IncrementalValuesProvider<IEnumerable<TypeToGenerate>> perMethodGenerationItems =
            context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsOxidizeType,
                transform: GetOxidizeClass);

        // Consolidate the GenerationItems from the different methods into a single dictionary.
        IncrementalValueProvider<Dictionary<ITypeSymbol, TypeToGenerate>> generationItems =
            perMethodGenerationItems
                .Collect()
                .Select(CombineGenerationItems);

        // Process the generation items, for example, linking them together.
        IncrementalValuesProvider<TypeToGenerate> processedGenerationItems = generationItems.SelectMany(Process);

        var cppGenerator = context.CompilationProvider.Combine(context.AnalyzerConfigOptionsProvider).Select((pair, _) => CreateCppGenerator(pair.Left, pair.Right));
        //var cppGenerator = context.CompilationProvider.Select(CreateCppGenerator);
        var withCppGenerator = processedGenerationItems.Combine(cppGenerator);

        // Generate C++ code.
        var typeDefinitions = withCppGenerator.Select((pair, _) => pair.Right.GenerateType(pair.Left));
        var sourceFiles = typeDefinitions.Collect().Combine(cppGenerator).SelectMany((pair, _) => pair.Right.DistributeToSourceFiles(pair.Left));
        context.RegisterImplementationSourceOutput(sourceFiles, (context, file) => file.Write());

        // Generate C++ initialization function
        var typesAndGenerator = typeDefinitions.Collect().Combine(cppGenerator);
        context.RegisterImplementationSourceOutput(typesAndGenerator, (context, pair) => pair.Right.WriteInitializeFunction(pair.Left));
        context.RegisterImplementationSourceOutput(typesAndGenerator, (context, pair) => CppObjectHandle.Generate(pair.Right.Options));

        // Generate the required items
        context.RegisterSourceOutput(typesAndGenerator, (context, pair) => CodeGenerator.WriteCSharpCode(context, pair.Right.Options.Compilation, pair.Left));
    }

    private static Dictionary<ITypeSymbol, TypeToGenerate> CombineGenerationItems(ImmutableArray<IEnumerable<TypeToGenerate>> listOfItems, CancellationToken token)
    {
        Dictionary<ITypeSymbol, TypeToGenerate> result = new Dictionary<ITypeSymbol, TypeToGenerate>(SymbolEqualityComparer.Default);

        foreach (IEnumerable<TypeToGenerate> items in listOfItems)
        {
            foreach (TypeToGenerate item in items)
            {
                TypeToGenerate current;
                if (!result.TryGetValue(item.Type, out current))
                {
                    current = new TypeToGenerate(item.Type);
                    result.Add(item.Type, current);
                }

                if (current.ImplementationClassName == null)
                {
                    current.ImplementationClassName = item.ImplementationClassName;
                }
                else if (item.ImplementationClassName != null && item.ImplementationClassName != current.ImplementationClassName)
                {
                    // TODO: report conflicting implementation class name
                }

                if (current.ImplementationHeaderName == null)
                {
                    current.ImplementationHeaderName = item.ImplementationHeaderName;
                }
                else if (item.ImplementationHeaderName != null && item.ImplementationHeaderName != current.ImplementationHeaderName)
                {
                    // TODO: report conflicting implementation header name
                }

                foreach (IMethodSymbol method in item.Constructors)
                {
                    current.Constructors.Add(method);
                }

                foreach (IMethodSymbol method in item.Methods)
                {
                    current.Methods.Add(method);
                }

                foreach (IPropertySymbol property in item.Properties)
                {
                    current.Properties.Add(property);
                }

                foreach (IFieldSymbol enumValue in item.EnumValues)
                {
                    current.EnumValues.Add(enumValue);
                }

                foreach (IMethodSymbol method in item.MethodsImplementedInCpp)
                {
                    current.MethodsImplementedInCpp.Add(method);
                }
            }
        }

        return result;
    }

    private static bool IsOxidizeType(SyntaxNode node, CancellationToken token)
    {
        var attributeNode = node as AttributeSyntax;
        if (attributeNode == null)
            return false;

        string? name = GetAttributeName(attributeNode);
        return name == "Oxidize" ||
            name == "OxidizeAttribute" ||
            name == "OxidizeNativeImplementation" ||
            name == "OxidizeNativeImplementationAttribute";
    }

    private static string? GetAttributeName(AttributeSyntax attribute)
    {
        NameSyntax? name = attribute.Name;
        SimpleNameSyntax? simpleName = name as SimpleNameSyntax;
        if (simpleName != null)
            return simpleName.Identifier.Text;

        QualifiedNameSyntax? qualifiedName = name as QualifiedNameSyntax;
        if (qualifiedName != null)
            return qualifiedName.Right.Identifier.Text;

        return null;
    }

    private static IEnumerable<TypeToGenerate> GetOxidizeClass(GeneratorSyntaxContext ctx, CancellationToken token)
    {
        SemanticModel semanticModel = ctx.SemanticModel;
        OxidizeWalker walker = new OxidizeWalker(semanticModel);

        var attributeSyntax = ctx.Node as AttributeSyntax;
        if (attributeSyntax == null)
            return Array.Empty<TypeToGenerate>();

        var classSyntax = attributeSyntax.Parent?.Parent as ClassDeclarationSyntax;
        if (classSyntax == null)
            return Array.Empty<TypeToGenerate>();

        string? attributeName = GetAttributeName(attributeSyntax);

        if (attributeName == "Oxidize" || attributeName == "OxidizeAttribute")
        {
            // A C# class containing a method that identifies what types, methods, properties, etc. should be accessible from C++.
            foreach (MemberDeclarationSyntax memberSyntax in classSyntax.Members)
            {
                MethodDeclarationSyntax? methodSyntax = memberSyntax as MethodDeclarationSyntax;
                if (methodSyntax == null)
                    continue;

                if (string.Equals(methodSyntax.Identifier.Text, "ExposeToCPP", StringComparison.InvariantCultureIgnoreCase))
                    walker.Visit(methodSyntax);
            }
        }
        else if (attributeName == "OxidizeNativeImplementation" || attributeName == "OxidizeNativeImplementationAttribute")
        {
            var args = attributeSyntax.ArgumentList!.Arguments;
            if (args.Count < 2)
                // TODO: report insufficient arguments. Can this even happen?
                return walker.GenerationItems.Values;

            var implClassName = (args[0]?.Expression as LiteralExpressionSyntax)?.Token.ValueText;
            var implHeaderName = (args[1]?.Expression as LiteralExpressionSyntax)?.Token.ValueText;

            // A C# class that is meant to be implemented in C++.
            ITypeSymbol? type = semanticModel.GetDeclaredSymbol(classSyntax) as ITypeSymbol;

            if (type != null)
            {
                TypeToGenerate item;
                if (!walker.GenerationItems.TryGetValue(type, out item))
                {
                    item = new TypeToGenerate(type);
                    walker.GenerationItems.Add(type, item);
                }

                item.ImplementationClassName = implClassName;
                item.ImplementationHeaderName = implHeaderName;

                foreach (MemberDeclarationSyntax memberSyntax in classSyntax.Members)
                {
                    MethodDeclarationSyntax? methodSyntax = memberSyntax as MethodDeclarationSyntax;
                    if (methodSyntax == null)
                        continue;

                    if (methodSyntax.Modifiers.IndexOf(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword) >= 0)
                    {
                        IMethodSymbol? symbol = semanticModel.GetDeclaredSymbol(methodSyntax) as IMethodSymbol;
                        if (symbol != null)
                        {
                            item.MethodsImplementedInCpp.Add(symbol);
                        }
                    }
                }
            }
        }

        return walker.GenerationItems.Values;
    }

    private string? GetName(NameSyntax? name)
    {
        if (name == null)
            return null;

        var simpleName = name as SimpleNameSyntax;
        if (simpleName != null)
            return simpleName.Identifier.Text;

        var qualifiedName = name as QualifiedNameSyntax;
        if (qualifiedName != null)
            return qualifiedName.Right.Identifier.Text;

        return null;
    }

    private static IEnumerable<TypeToGenerate> Process(Dictionary<ITypeSymbol, TypeToGenerate> items, CancellationToken token)
    {
        foreach (TypeToGenerate item in items.Values)
        {
            InheritanceChainer.Chain(item, items);
        }

        return items.Values;
    }

    private static CodeGenerator CreateCppGenerator(Compilation compilation, AnalyzerConfigOptionsProvider options)
    {
        CppGenerationContext cppContext = new CppGenerationContext(compilation);
        
        string? projectDir;
        if (!options.GlobalOptions.TryGetValue("build_property.projectdir", out projectDir))
            projectDir = "";

        string? cppHeaderPath;
        if (!options.GlobalOptions.TryGetValue("cpp_header_path", out cppHeaderPath))
            cppHeaderPath = "generated/include";

        string? cppSourcePath;
        if (!options.GlobalOptions.TryGetValue("cpp_source_path", out cppSourcePath))
            cppSourcePath = "generated/src";

        cppContext.OutputHeaderDirectory = Path.GetFullPath(Path.Combine(projectDir, cppHeaderPath));
        cppContext.OutputSourceDirectory = Path.GetFullPath(Path.Combine(projectDir, cppSourcePath));

        string? baseNamespace;
        if (!options.GlobalOptions.TryGetValue("base_namespace", out baseNamespace))
            baseNamespace = "";

        cppContext.BaseNamespace = baseNamespace;

        cppContext.CustomGenerators.Add(compilation.GetSpecialType(SpecialType.System_String), new CustomStringGenerator());

        return new CodeGenerator(cppContext);
    }
}
