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

        public void WriteInitializeFunction(ImmutableArray<GeneratedResult?> results)
        {
            GeneratedCppInit init = GeneratedCppInit.Merge(results.Select(result => result == null ? new GeneratedCppInit() : result.CppInit));

            Directory.CreateDirectory(Options.OutputSourceDirectory);
            File.WriteAllText(Path.Combine(Options.OutputSourceDirectory, "initializeOxidize.cpp"), init.ToSourceFileString(), Encoding.UTF8);

            string headerPath = Options.OutputHeaderDirectory;
            if (this.Options.BaseNamespace != null)
                headerPath = Path.Combine(headerPath, this.Options.BaseNamespace);

            Directory.CreateDirectory(headerPath);
            File.WriteAllText(Path.Combine(headerPath, "initializeOxidize.h"), init.ToHeaderFileString(), Encoding.UTF8);
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

        public void WriteCppCode(GeneratedResult? result)
        {
            if (result == null)
                return;

            CppType type = result.CppDefinition.Type;
            //string headerPath = Path.Combine(new string[] { Options.OutputHeaderDirectory }.Concat(type.Namespaces).ToArray());
            //Directory.CreateDirectory(headerPath);
            //File.WriteAllText(Path.Combine(headerPath, type.Name + ".h"), result.CppDeclaration.ToHeaderFileString(), Encoding.UTF8);

            //Directory.CreateDirectory(Options.OutputSourceDirectory);
            //File.WriteAllText(Path.Combine(Options.OutputSourceDirectory, type.Name + ".cpp"), result.CppDefinition.ToSourceFileString(), Encoding.UTF8);

            if (result.CppImplementationInvoker != null)
                File.WriteAllText(Path.Combine(Options.OutputSourceDirectory, type.Name + "Bindings.cpp"), result.CppImplementationInvoker.ToSourceFileString(), Encoding.UTF8);
        }

        public static void WriteCSharpCode(SourceProductionContext context, Compilation compilation, ImmutableArray<GeneratedResult?> results)
        {
            GeneratedCSharpInit combinedInit = GeneratedCSharpInit.Merge(results.Select(result => result == null ? new GeneratedCSharpInit() : result.CSharpInit));
            Console.WriteLine(combinedInit.ToSourceFileString());
            context.AddSource("OxidizeInitializer", combinedInit.ToSourceFileString());

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
            }

            return sourceFiles.Values;
        }
    }
}
