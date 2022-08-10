using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;

namespace Oxidize
{
    internal class CodeGenerator
    {
        public readonly CppGenerationContext Options;

        public CodeGenerator(CppGenerationContext options)
        {
            this.Options = options;
        }

        public GeneratedResult? GenerateType(TypeToGenerate item)
        {
            GeneratedResult? result = null;

            CppType itemType = CppType.FromCSharp(this.Options, item.Type);
            if (itemType.Kind == InteropTypeKind.Enum)
                result = GenerateEnum(item, itemType);
            //else if (itemType.Kind == CppTypeKind.Delegate)
            //    result = GenerateDelegate(item, itemType);
            else if (itemType.Kind == InteropTypeKind.ClassWrapper || itemType.Kind == InteropTypeKind.BlittableStruct || itemType.Kind == InteropTypeKind.NonBlittableStructWrapper || itemType.Kind == InteropTypeKind.Delegate)
                result = GenerateClassOrStruct(item, itemType);
            else
                result = null;

            ICustomGenerator? customGenerator = null;
            if (this.Options.CustomGenerators.TryGetValue(item.Type, out customGenerator))
            {
                result = customGenerator.Generate(this.Options, item, result);
            }

            return result;
        }

        private GeneratedResult? GenerateClassOrStruct(TypeToGenerate item, CppType itemType)
        {
            GeneratedResult result = new GeneratedResult(itemType);

            Interop.GenerateForType(this.Options, item, result);
            CppHandleManagement.Generate(this.Options, item, result);
            Constructors.Generate(this.Options, item, result);
            Casts.Generate(this.Options, item, result);
            Fields.Generate(this.Options, item, result);

            // Generate properties and methods throughout the whole inheritance hierarchy.
            TypeToGenerate? current = item;
            while (current != null)
            {
                Properties.Generate(this.Options, item, current, result);
                Methods.Generate(this.Options, item, current, result);
                current = current.BaseClass;
            }

            // If this class has partial methods that are meant to be implemented in C++,
            // generate the necessary bindings.
            if (item.ImplementationClassName != null)
            {
                // TODO: parse out namespaces? Require user to specify them separately?
                CppType implementationType = new CppType(InteropTypeKind.Unknown, Array.Empty<string>(), item.ImplementationClassName, null, 0, item.ImplementationHeaderName);
                result.CppImplementationInvoker = new GeneratedCppImplementationInvoker(implementationType);
                result.CSharpPartialMethodDefinitions = new GeneratedCSharpPartialMethodDefinitions(CSharpType.FromSymbol(this.Options.Compilation, item.Type));
                
                MethodsImplementedInCpp.Generate(this.Options, item, result);
                Console.WriteLine(result.CSharpPartialMethodDefinitions.ToSourceFileString());
            }

            return result;
        }

        private GeneratedResult? GenerateEnum(TypeToGenerate item, CppType itemType)
        {
            GeneratedResult result = new GeneratedResult(itemType);

            foreach (IFieldSymbol enumValue in item.EnumValues)
            {
                result.CppDeclaration.Elements.Add(new(Content: $"{enumValue.Name} = {enumValue.ConstantValue},"));
            }

            return result;
        }

        public static void WriteCSharpCode(SourceProductionContext context, Compilation compilation, ImmutableArray<GeneratedResult?> results)
        {
            GeneratedInit combinedInit = GeneratedInit.Merge(results.Select(result => result == null ? new GeneratedInit() : result.Init));
            Console.WriteLine(combinedInit.ToCSharpSourceFileString());
            context.AddSource("OxidizeInitializer", combinedInit.ToCSharpSourceFileString());

            foreach (GeneratedResult? result in results)
            {
                if (result == null)
                    continue;

                GeneratedCSharpPartialMethodDefinitions? partialMethods = result.CSharpPartialMethodDefinitions;
                if (partialMethods == null)
                    continue;

                context.AddSource(partialMethods.Type.Symbol.Name + "-generated", partialMethods.ToSourceFileString());
            }
        }

        public IEnumerable<CppSourceFile> DistributeToSourceFiles(ImmutableArray<GeneratedResult?> generatedResults)
        {
            Dictionary<string, CppSourceFile> sourceFiles = new Dictionary<string, CppSourceFile>();

            // Create source files for the standard types.
            CppObjectHandle.Generate(this.Options, sourceFiles);
            
            // Create source files for the generated types.
            foreach (GeneratedResult? generated in generatedResults)
            {
                if (generated == null)
                    continue;

                CppType declarationType = generated.CppDeclaration.Type;
                string headerPath = Path.Combine(new string[] { Options.OutputHeaderDirectory }.Concat(declarationType.Namespaces).ToArray());
                headerPath = Path.Combine(headerPath, declarationType.Name + ".h");

                CppSourceFile? headerFile = null;
                if (!sourceFiles.TryGetValue(headerPath, out headerFile))
                {
                    headerFile = new CppSourceFile();
                    headerFile.IsHeaderFile = true;
                    headerFile.Filename = headerPath;
                    sourceFiles.Add(headerPath, headerFile);
                }

                generated.CppDeclaration.AddToHeaderFile(headerFile);

                CppType definitionType = generated.CppDefinition.Type;
                string sourcePath = Path.Combine(Options.OutputSourceDirectory, generated.CppDefinition.Type.Name + ".cpp");

                CppSourceFile? sourceFile = null;
                if (!sourceFiles.TryGetValue(sourcePath, out sourceFile))
                {
                    sourceFile = new CppSourceFile();
                    sourceFile.IsHeaderFile = false;
                    sourceFile.Filename = sourcePath;
                    sourceFiles.Add(sourcePath, sourceFile);
                }

                generated.CppDefinition.AddToSourceFile(sourceFile);

                if (generated.CppImplementationInvoker != null)
                    generated.CppImplementationInvoker.AddToSourceFile(sourceFile);
            }

            // Create source files for the initialization process.
            GeneratedInit init = GeneratedInit.Merge(generatedResults.Select(result => result == null ? new GeneratedInit() : result.Init));
            init.GenerateCpp(this.Options, sourceFiles);

            return sourceFiles.Values;
        }
    }
}
