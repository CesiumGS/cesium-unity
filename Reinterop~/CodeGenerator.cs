using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;

namespace Reinterop
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
            if (itemType.Kind == InteropTypeKind.Enum || itemType.Kind == InteropTypeKind.EnumFlags)
                result = GenerateEnum(item, itemType);
            else if (itemType.Kind == InteropTypeKind.ClassWrapper || itemType.Kind == InteropTypeKind.BlittableStruct || itemType.Kind == InteropTypeKind.NonBlittableStructWrapper || itemType.Kind == InteropTypeKind.Delegate)
                result = GenerateClassOrStruct(item, itemType);
            else
                result = null;

            foreach (ICustomGenerator customGenerator in this.Options.CustomGenerators)
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

            // Generate properties and methods throughout the whole inheritance hierarchy.
            TypeToGenerate? current = item;
            while (current != null)
            {
                Properties.Generate(this.Options, item, current, result);
                Methods.Generate(this.Options, item, current, result);
                Events.Generate(this.Options, item, current, result);
                Fields.Generate(this.Options, item, current, result);
                current = current.BaseClass;
            }

            // If this class has partial methods that are meant to be implemented in C++,
            // generate the necessary bindings.
            if (item.ImplementationClassName != null)
            {
                // TODO: parse out namespaces? Require user to specify them separately?
                CppType implementationType = new CppType(InteropTypeKind.Unknown, Array.Empty<string>(), item.ImplementationClassName, null, 0, item.ImplementationHeaderName);
                result.CppImplementationInvoker = new GeneratedCppImplementationInvoker(implementationType);
                result.CSharpPartialMethodDefinitions = new GeneratedCSharpPartialMethodDefinitions(CSharpType.FromSymbol(this.Options, item.Type));

                MethodsImplementedInCpp.Generate(this.Options, item, result);
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

        public static void WriteCSharpCode(GeneratorExecutionContext context, CppGenerationContext cppContext, IEnumerable<GeneratedResult?> results)
        {
            GeneratedInit combinedInit = GeneratedInit.Merge(results.Select(result => result == null ? new GeneratedInit() : result.Init));
            context.AddSource("ReinteropInitializer", combinedInit.ToCSharpSourceFileString(cppContext));

            foreach (GeneratedResult? result in results)
            {
                if (result == null)
                    continue;

                GeneratedCSharpPartialMethodDefinitions? partialMethods = result.CSharpPartialMethodDefinitions;
                if (partialMethods == null || partialMethods.Methods.Count == 0)
                    continue;

                context.AddSource(partialMethods.Type.Name + "-generated", partialMethods.ToSourceFileString());
            }
        }

        public IEnumerable<CppSourceFile> DistributeToSourceFiles(IEnumerable<GeneratedResult?> generatedResults)
        {
            // Don't emit C++ code if the C# code has compiler errors.
            Dictionary<string, CppSourceFile> sourceFiles = new Dictionary<string, CppSourceFile>();

            // Create source files for the standard types.
            CppObjectHandle.Generate(this.Options, sourceFiles);

            // Create source files for the generated types.
            foreach (GeneratedResult? generated in generatedResults)
            {
                if (generated == null)
                    continue;

                CppType declarationType = generated.CppDeclaration.Type;
                string headerPath = Path.Combine(new string[] { "include" }.Concat(declarationType.Namespaces).ToArray());
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
                string sourcePath = Path.Combine(new string[] { "src" }.Concat(declarationType.Namespaces).ToArray());
                sourcePath = Path.Combine(sourcePath, generated.CppDefinition.Type.Name + ".cpp");

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

            // Read the previous inventory, and delete any files that don't exist in the new one.
            Directory.CreateDirectory(this.Options.OutputDirectory);
            using (FileStream f = File.Open(Path.Combine(this.Options.OutputDirectory, "reinterop-inventory.txt"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                f.Seek(0, SeekOrigin.Begin);

                // Create an inventory of all the files to be written. Sorted. Use forward slashes.
                string[] files = sourceFiles.Values.Select(f => f.Filename.Replace("\\", "/")).ToArray();
                Array.Sort(files);

                string[] previousInventory;
                using (StreamReader reader = new StreamReader(f, Encoding.UTF8, false, 16384, true))
                {
                    string previousInventoryText = reader.ReadToEnd();
                    previousInventory = previousInventoryText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string inventoryItem in previousInventory)
                    {
                        if (Array.BinarySearch(files, inventoryItem) < 0)
                        {
                            try
                            {
                                File.Delete(Path.Combine(Options.OutputDirectory, inventoryItem));
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }

                if (previousInventory.Length != files.Length || !previousInventory.SequenceEqual(files))
                {
                    f.Seek(0, SeekOrigin.Begin);
                    f.SetLength(0);

                    using (StreamWriter writer = new StreamWriter(f))
                    {
                        writer.Write(string.Join(Environment.NewLine, files));
                    }
                }
            }

            return sourceFiles.Values;
        }
    }
}
