using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;

namespace Reinterop
{
    /// <summary>
    /// Generates interop code for types, turning a <see cref="TypeToGenerate"/> into a <see cref="GeneratedResult"/>.
    /// Also knows how to write the code in a <see cref="GeneratedResult"/> to C++ and C# source files.
    /// </summary>
    internal static class ReinteropCodeGenerator
    {
        public static GeneratedResult? GenerateType(ReinteropGenerationContext context, TypeToGenerate item)
        {
            GenerateTypeState state = new();

            GeneratedResult? result = null;

            CSharpType csItemType = CSharpType.FromSymbol(context, item.Type);
            CppType itemType = CppType.FromCSharp(context, csItemType);
            if (itemType.Kind == InteropTypeKind.Enum || itemType.Kind == InteropTypeKind.EnumFlags)
                result = GenerateEnum(context, state, item, itemType);
            else if (itemType.Kind == InteropTypeKind.ClassWrapper || itemType.Kind == InteropTypeKind.BlittableStruct || itemType.Kind == InteropTypeKind.NonBlittableStructWrapper || itemType.Kind == InteropTypeKind.Delegate)
                result = GenerateClassOrStruct(context, state, item, itemType);
            else
                result = null;

            foreach (ICustomGenerator customGenerator in context.CustomGenerators)
            {
                result = customGenerator.Generate(context, item, result);
            }

            // Now that every method/property/constructor/field accessor recipe for this type has been
            // gathered, add each of them to the generation.
            if (result != null)
            {
                foreach (CSharpFunctionCallableFromCpp function in result.InteropFunctions)
                    function.GenerateCode(context, result);
                foreach (CppFunction function in result.ExtraCppFunctions)
                    function.AddToGeneration(result);
            }

            return result;
        }

        private static GeneratedResult? GenerateClassOrStruct(ReinteropGenerationContext context, GenerateTypeState state, TypeToGenerate item, CppType itemType)
        {
            GeneratedResult result = new GeneratedResult(itemType);

            Interop.GenerateForType(context, item, result);
            CppHandleManagement.Generate(context, item, result);
            Constructors.Generate(context, item, result);
            Casts.Generate(context, item, result);

            // Generate properties and methods throughout the whole inheritance hierarchy.
            TypeToGenerate? current = item;
            while (current != null)
            {
                Properties.Generate(context, item, current, result);
                Methods.Generate(context, state, item, current, result);
                Events.Generate(context, state, item, current, result);
                Fields.Generate(context, item, current, result);
                current = current.BaseClass;
            }

            // If this class has partial methods that are meant to be implemented in C++,
            // generate the necessary bindings.
            if (item.ImplementationClassName != null)
            {
                // TODO: parse out namespaces? Require user to specify them separately?
                CppType implementationType = new CppType(InteropTypeKind.Unknown, Array.Empty<string>(), item.ImplementationClassName, null, 0, item.ImplementationHeaderName);
                result.CppImplementationInvoker = new GeneratedCppImplementationInvoker(implementationType);
                result.CSharpPartialMethodDefinitions = new GeneratedCSharpPartialMethodDefinitions(CSharpType.FromSymbol(context, item.Type));

                MethodsImplementedInCpp.Generate(context, item, result);
            }

            return result;
        }

        private static GeneratedResult? GenerateEnum(ReinteropGenerationContext context, GenerateTypeState state, TypeToGenerate item, CppType itemType)
        {
            GeneratedResult result = new GeneratedResult(itemType);

            foreach (IFieldSymbol enumValue in item.EnumValues)
            {
                result.CppDeclaration.Elements.Add(new(Content: $"{enumValue.Name} = {enumValue.ConstantValue},"));
            }

            return result;
        }

        public static void WriteCSharpCode(GeneratorExecutionContext context, ReinteropGenerationContext cppContext, IEnumerable<GeneratedResult?> results)
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

        public static IEnumerable<CppSourceFile> DistributeToSourceFiles(ReinteropGenerationContext context, IEnumerable<GeneratedResult?> generatedResults)
        {
            // Don't emit C++ code if the C# code has compiler errors.
            Dictionary<string, CppSourceFile> sourceFiles = new Dictionary<string, CppSourceFile>();

            // Create source files for the standard types.
            CppObjectHandle.Generate(context, sourceFiles);
            CppReinteropException.Generate(context, sourceFiles);

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
            init.GenerateCpp(context, sourceFiles);

            // Read the previous inventory, and delete any files that don't exist in the new one.
            Directory.CreateDirectory(context.OutputDirectory);
            using (FileStream f = File.Open(Path.Combine(context.OutputDirectory, "reinterop-inventory.txt"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
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
                                File.Delete(Path.Combine(context.OutputDirectory, inventoryItem));
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
