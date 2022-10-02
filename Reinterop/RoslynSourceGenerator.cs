﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;

namespace Reinterop
{
    [Generator]
    internal class RoslynSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}

            context.RegisterForSyntaxNotifications(() => new ReinteropSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            ReinteropSyntaxReceiver receiver = (ReinteropSyntaxReceiver)context.SyntaxReceiver!;

            // Don't generate any support code if there's nothing real to generate.
            if (receiver.ClassesImplementedInCpp.Count == 0 && receiver.ExposeToCppMethods.Count == 0)
                return;

            CSharpReinteropAttribute.Generate(context);
            CSharpReinteropNativeImplementationAttribute.Generate(context);
            CSharpObjectHandleUtility.Generate(context);

            // Create a new Compilation with the CSharpObjectHandleUtility created above.
            // Newer versions of Roslyn make this easy, but not the one in Unity.
            CSharpParseOptions options = (CSharpParseOptions)((CSharpCompilation)context.Compilation).SyntaxTrees[0].Options;
            Compilation compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(CSharpObjectHandleUtility.Source), options));

            // Add ObjectHandleUtility's ExposeToCPP to the receiver.
            INamedTypeSymbol? objectHandleUtilityType = compilation.GetTypeByMetadataName("Reinterop.ObjectHandleUtility");
            if (objectHandleUtilityType != null)
            {
                var exposeToCpp = CSharpTypeUtility.FindMembers(objectHandleUtilityType, "ExposeToCPP");
                foreach (ISymbol symbol in exposeToCpp)
                {
                    IMethodSymbol? method = symbol as IMethodSymbol;
                    if (method == null)
                        continue;

                    foreach (var reference in method.DeclaringSyntaxReferences)
                    {
                        if (reference.GetSyntax() is MethodDeclarationSyntax methodDeclaration)
                        {
                            receiver.ExposeToCppMethods.Add(methodDeclaration);
                        }
                    }
                }
            }

            Dictionary<string, object> properties = new Dictionary<string, object>();
            foreach (var property in receiver.Properties)
            {
                string name = property.Key;
                ExpressionSyntax expression = property.Value;

                SemanticModel semanticModel = compilation.GetSemanticModel(expression.SyntaxTree);
                Optional<object?> value = semanticModel.GetConstantValue(expression);
                if (value.HasValue && value.Value != null)
                {
                    properties.Add(name, value.Value);
                }
            }

            CodeGenerator codeGenerator = CreateCodeGenerator(context.AnalyzerConfigOptions, receiver.PropertiesPath, properties, compilation);

            List<IEnumerable<TypeToGenerate>> typesToGenerate = new List<IEnumerable<TypeToGenerate>>();

            foreach (MethodDeclarationSyntax exposeMethod in receiver.ExposeToCppMethods)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(exposeMethod.SyntaxTree);
                ExposeToCppSyntaxWalker walker = new ExposeToCppSyntaxWalker(codeGenerator.Options, semanticModel);
                walker.Visit(exposeMethod);
                typesToGenerate.Add(walker.GenerationItems.Values);
            }

            foreach (AttributeSyntax attributeSyntax in receiver.ClassesImplementedInCpp)
            {
                var args = attributeSyntax.ArgumentList!.Arguments;
                if (args.Count < 2)
                    // TODO: report insufficient arguments. Can this even happen?
                    continue;

                var classSyntax = attributeSyntax.Parent?.Parent as ClassDeclarationSyntax;
                if (classSyntax == null)
                    continue;

                var implClassName = (args[0]?.Expression as LiteralExpressionSyntax)?.Token.ValueText;
                var implHeaderName = (args[1]?.Expression as LiteralExpressionSyntax)?.Token.ValueText;

                // A C# class that is meant to be implemented in C++.
                SemanticModel semanticModel = compilation.GetSemanticModel(attributeSyntax.SyntaxTree);
                ITypeSymbol? type = semanticModel.GetDeclaredSymbol(classSyntax) as ITypeSymbol;

                ExposeToCppSyntaxWalker walker = new ExposeToCppSyntaxWalker(codeGenerator.Options, semanticModel);

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

                typesToGenerate.Add(walker.GenerationItems.Values);
            }

            // Create a unique entry for each type
            var typeDictionary = TypeToGenerate.Combine(typesToGenerate);

            // Process the generation items, for example, linking them together.
            foreach (TypeToGenerate item in typeDictionary.Values)
            {
                InheritanceChainer.Chain(item, typeDictionary);
            }

            List<GeneratedResult> generatedResults = new List<GeneratedResult>();
            foreach (TypeToGenerate item in typeDictionary.Values)
            {
                GeneratedResult? result = codeGenerator.GenerateType(item);
                if (result != null)
                    generatedResults.Add(result);
            }

            IEnumerable<CppSourceFile> sourceFiles = codeGenerator.DistributeToSourceFiles(generatedResults);
            foreach (CppSourceFile sourceFile in sourceFiles)
            {
                sourceFile.Write(codeGenerator.Options);
            }

            CodeGenerator.WriteCSharpCode(context, codeGenerator.Options, generatedResults);

            // Build the native library if requested and this is _not_ the Intellisense service.
            //string processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            //if (codeGenerator.Options.BuildNativeLibrary &&
            //    !processName.Contains("RoslynCodeAnalysisService") &&
            //    !processName.Contains("devenv"))
            //{
            //    if (!Debugger.IsAttached)
            //    {
            //        Debugger.Launch();
            //    }

            //    BuildNativeLibrary(codeGenerator.Options);
            //}
            //string processName2 = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            //string processName3 = System.AppDomain.CurrentDomain.FriendlyName;

            //string? foo = null;
            //if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("foo", out foo))
            //{
            //    Console.WriteLine("hi");
            //}

            //string[] symbols = context.ParseOptions.PreprocessorSymbolNames.ToArray();
            //var features = context.ParseOptions.Features.ToArray();
            //if (context.ParseOptions.PreprocessorSymbolNames.Contains("COMPILE_NATIVE_LIBRARY"))
            //{
            //    BuildNativeLibrary(codeGenerator.Options);
            //}
            //if (codeGenerator.Options.BuildNativeLibrary)
            //{

            //    string? designTimeBuild;
            //    if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.DesignTimeBuild", out designTimeBuild) || string.Compare(designTimeBuild, "true") == 0)
            //    {
            //        BuildNativeLibrary(codeGenerator.Options);
            //    }
            //}
        }

        private void BuildNativeLibrary(CppGenerationContext options)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.FileName = "cmake";
            startInfo.Arguments = "-B build -S .";
            startInfo.CreateNoWindow = false;
            startInfo.WorkingDirectory = Path.GetFullPath(Path.Combine(options.OutputDirectory, ".."));
            Process configure = Process.Start(startInfo);
            configure.WaitForExit();
        }

        private static readonly string[] ConfigurationPropertyNames = { "CppOutputPath", "BaseNamespace", "NativeLibraryName", "NonBlittableTypes", "BuildNativeLibrary" };

        private CodeGenerator CreateCodeGenerator(AnalyzerConfigOptionsProvider options, string? propertiesPath, IDictionary<string, object> properties, Compilation compilation)
        {
            Dictionary<string, object> mergedProperties = new Dictionary<string, object>(properties);

            CppGenerationContext cppContext = new CppGenerationContext(compilation);

            string? baseDir;
            if (!options.GlobalOptions.TryGetValue("build_property.projectdir", out baseDir))
            {
                // Use the directory of the file containing the properties as the base path.
                if (propertiesPath != null)
                    baseDir = Path.GetDirectoryName(propertiesPath);
                else
                    baseDir = "";
            }

            string? cppOutputPath;
            if (options.GlobalOptions.TryGetValue("cpp_output_path", out cppOutputPath))
                mergedProperties["CppOutputPath"] = cppOutputPath;
                
            string? baseNamespace;
            if (options.GlobalOptions.TryGetValue("base_namespace", out baseNamespace))
                mergedProperties["BaseNamespace"] = baseNamespace;

            string? nativeLibraryName;
            if (options.GlobalOptions.TryGetValue("native_library_name", out nativeLibraryName))
                mergedProperties["NativeLibraryName"] = nativeLibraryName;

            string? nonBlittableTypes;
            if (options.GlobalOptions.TryGetValue("non_blittable_types", out nonBlittableTypes))
                mergedProperties["NonBlittableTypes"] = nonBlittableTypes;

            string? buildNativeLibraryString;
            if (options.GlobalOptions.TryGetValue("build_native_library", out buildNativeLibraryString))
                mergedProperties["BuildNativeLibrary"] = String.Compare(buildNativeLibraryString, "true", ignoreCase: true) == 0;

            object? value;

            if (mergedProperties.TryGetValue("CppOutputPath", out value))
                cppOutputPath = value.ToString();
            else
                cppOutputPath = "generated";

            if (mergedProperties.TryGetValue("BaseNamespace", out value))
                baseNamespace = value.ToString();
            else
                baseNamespace = "DotNet";

            if (mergedProperties.TryGetValue("NativeLibraryName", out value))
                nativeLibraryName = value.ToString();
            else
                nativeLibraryName = "ReinteropNative";

            if (mergedProperties.TryGetValue("NonBlittableTypes", out value))
                nonBlittableTypes = value.ToString();
            else
                nonBlittableTypes = "";

            bool buildNativeLibrary;
            if (mergedProperties.TryGetValue("BuildNativeLibrary", out value))
                buildNativeLibrary = (bool)value;
            else
                buildNativeLibrary = false;

            cppContext.OutputDirectory = Path.GetFullPath(Path.Combine(baseDir, cppOutputPath));
            cppContext.BaseNamespace = baseNamespace;
            cppContext.NativeLibraryName = nativeLibraryName;
            cppContext.NonBlittableTypes.UnionWith(nonBlittableTypes.Split(',').Select(t => t.Trim()));
            cppContext.BuildNativeLibrary = buildNativeLibrary;

            cppContext.CustomGenerators.Add(new CustomStringGenerator());
            cppContext.CustomGenerators.Add(new CustomDelegateGenerator());
            cppContext.CustomGenerators.Add(new CustomArrayGenerator());

            return new CodeGenerator(cppContext);
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

        private class ReinteropSyntaxReceiver : ISyntaxReceiver
        {
            public readonly List<MethodDeclarationSyntax> ExposeToCppMethods = new List<MethodDeclarationSyntax>();
            public readonly List<AttributeSyntax> ClassesImplementedInCpp = new List<AttributeSyntax>();
            public readonly Dictionary<string, ExpressionSyntax> Properties = new Dictionary<string, ExpressionSyntax>();
            public string? PropertiesPath = null;

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                var attributeNode = syntaxNode as AttributeSyntax;
                if (attributeNode == null)
                    return;

                string? attributeName = GetAttributeName(attributeNode);
                if (attributeName != "Reinterop" &&
                    attributeName != "ReinteropAttribute" &&
                    attributeName != "ReinteropNativeImplementation" &&
                    attributeName != "ReinteropNativeImplementationAttribute")
                {
                    return;
                }

                var classSyntax = attributeNode.Parent?.Parent as ClassDeclarationSyntax;
                if (classSyntax == null)
                    return;

                if (attributeName == "Reinterop" || attributeName == "ReinteropAttribute")
                {
                    // A C# class containing a method that identifies what types, methods, properties, etc. should be accessible from C++.
                    foreach (MemberDeclarationSyntax memberSyntax in classSyntax.Members)
                    {
                        // Fields may be configuration properties, store them to be resolved later.
                        FieldDeclarationSyntax? fieldSyntax = memberSyntax as FieldDeclarationSyntax;
                        if (fieldSyntax != null)
                        {
                            foreach (var variable in fieldSyntax.Declaration.Variables)
                            {
                                string name = variable.Identifier.Text;
                                if (Array.IndexOf(ConfigurationPropertyNames, name) < 0)
                                    continue;

                                ExpressionSyntax? expression = variable.Initializer?.Value;
                                if (expression == null)
                                    continue;

                                string path = expression.SyntaxTree.FilePath;
                                if (PropertiesPath != null && PropertiesPath != path)
                                    throw new Exception("Reinterop configuration properties must be defined in only one class, but they were found in both " + PropertiesPath + " and " + path + ".");

                                PropertiesPath = path;
                                Properties.Add(name, expression);
                            }
                        }

                        MethodDeclarationSyntax? methodSyntax = memberSyntax as MethodDeclarationSyntax;
                        if (methodSyntax == null)
                            continue;

                        if (string.Equals(methodSyntax.Identifier.Text, "ExposeToCPP", StringComparison.InvariantCultureIgnoreCase))
                            ExposeToCppMethods.Add(methodSyntax);
                    }
                }
                else if (attributeName == "ReinteropNativeImplementation" || attributeName == "ReinteropNativeImplementationAttribute")
                {
                    // A class with partial methods intended to be implemented in C++.
                    ClassesImplementedInCpp.Add(attributeNode);
                }
            }
        }
    }
}