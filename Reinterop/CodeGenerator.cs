﻿using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

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
            if (itemType.Kind == InteropTypeKind.Enum)
                result = GenerateEnum(item, itemType);
            else if (itemType.Kind == InteropTypeKind.ClassWrapper || itemType.Kind == InteropTypeKind.BlittableStruct || itemType.Kind == InteropTypeKind.NonBlittableStructWrapper || itemType.Kind == InteropTypeKind.Delegate)
                result = GenerateClassOrStruct(item, itemType);
            else
                result = null;

            // A delegate is a class with some extras
            if (itemType.Kind == InteropTypeKind.Delegate && result != null)
                GenerateDelegate(result, item, itemType);

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
                Events.Generate(this.Options, item, current, result);
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

        private void GenerateDelegate(GeneratedResult result, TypeToGenerate item, CppType itemType)
        {
            // TODO: support delegates that return a value

            CppType implementationType = new CppType(InteropTypeKind.Unknown, itemType.Namespaces, itemType.Name + "Native", null, 0, "<functional>");

            if (result.CppImplementationInvoker == null)
            {
                result.CppImplementationInvoker = new GeneratedCppImplementationInvoker(implementationType);
                result.CSharpPartialMethodDefinitions = new GeneratedCSharpPartialMethodDefinitions(CSharpType.FromSymbol(this.Options.Compilation, item.Type));
            }

            // Add a constructor taking a std::function
            IMethodSymbol? invokeMethod = item.Methods.FirstOrDefault(m => m.Name == "Invoke");
            if (invokeMethod == null)
                return;

            var callbackParameters = invokeMethod.Parameters.Select(p =>
            {
                CppType type = CppType.FromCSharp(this.Options, p.Type);
                return (Name: p.Name, CsType: CSharpType.FromSymbol(this.Options.Compilation, p.Type), Type: type, InteropType: type.AsInteropType());
            });

            string templateSpecialization = "";
            if (itemType.GenericArguments != null && itemType.GenericArguments.Count > 0)
            {
                templateSpecialization = $"<{string.Join(", ", itemType.GenericArguments.Select(arg => arg.GetFullyQualifiedName()))}>";
            }

            result.CppDeclaration.Elements.Add(new(
                Content: $"static void* (*CreateDelegate)(void* pCallbackFunction);",
                IsPrivate: true));
            result.CppDefinition.Elements.Add(new(
                Content: $"void* (*{itemType.Name}{templateSpecialization}::CreateDelegate)(void* pCallbackFunction) = nullptr;"));

            result.CppDeclaration.Elements.Add(new(
                Content: $"using FunctionSignature = void ({string.Join(", ", callbackParameters.Select(p => p.Type.AsParameterType().GetFullyQualifiedName()))});"
            ));
            result.CppDeclaration.Elements.Add(new(
                Content: $"{itemType.Name}(std::function<FunctionSignature> callback);",
                TypeDeclarationsReferenced: callbackParameters.Select(p => p.Type.AsParameterType()),
                AdditionalIncludes: new[] { "<functional>" }
            ));

            result.CppDefinition.Elements.Add(new(
                Content:
                    $$"""
                    {{itemType.Name}}{{templateSpecialization}}::{{itemType.Name}}(std::function<FunctionSignature> callback) :
                        _handle(CreateDelegate(reinterpret_cast<void*>(new std::function<FunctionSignature>(std::move(callback)))))
                    {
                    }
                    """
                ));

            CSharpType csType = CSharpType.FromSymbol(this.Options.Compilation, item.Type);

            string genericTypeHash = "";
            INamedTypeSymbol? named = item.Type as INamedTypeSymbol;
            if (named != null && named.IsGenericType)
            {
                genericTypeHash = Interop.HashParameters(null, named.TypeArguments);
            }

            string csBaseName = $"{csType.GetFullyQualifiedNamespace().Replace(".", "_")}_{csType.Symbol.Name}{genericTypeHash}_CreateDelegate";
            string invokeCallbackName = $"{csType.GetFullyQualifiedNamespace().Replace(".", "_")}_{item.Type.Name}{genericTypeHash}_InvokeCallback";
            string disposeCallbackName = $"{csType.GetFullyQualifiedNamespace().Replace(".", "_")}_{item.Type.Name}{genericTypeHash}_DisposeCallback";

            var invokeParameters = callbackParameters.Select(p => $"{p.CsType.GetFullyQualifiedName()} {p.Name}");
            var invokeInteropParameters = new[] { "IntPtr callbackFunction" }.Concat(callbackParameters.Select(p => $"{p.CsType.AsInteropType().GetFullyQualifiedName()} {p.Name}"));
            var callInvokeInteropParameters = new[] { "_callbackFunction" }.Concat(callbackParameters.Select(p => p.CsType.GetConversionToInteropType(p.Name)));

            result.Init.Functions.Add(new(
                CppName: $"{itemType.GetFullyQualifiedName()}::CreateDelegate",
                CppTypeSignature: $"void* (*)(void*)",
                CppTypeDefinitionsReferenced: new[] { itemType },
                CSharpName: csBaseName + "Delegate",
                CSharpContent:
                    $$"""
                    private class {{csType.Symbol.Name}}{{genericTypeHash}}NativeFunction : System.IDisposable
                    {
                        private IntPtr _callbackFunction;

                        public {{csType.Symbol.Name}}{{genericTypeHash}}NativeFunction(IntPtr callbackFunction)
                        {
                            _callbackFunction = callbackFunction;
                        }

                        ~{{csType.Symbol.Name}}{{genericTypeHash}}NativeFunction()
                        {
                            Dispose(false);
                        }
                    
                        public void Dispose()
                        {
                            Dispose(true);
                            GC.SuppressFinalize(this);
                        }

                        private void Dispose(bool disposing)
                        {
                            if (_callbackFunction != IntPtr.Zero)
                            {
                                {{disposeCallbackName}}(_callbackFunction);
                                _callbackFunction = IntPtr.Zero;
                            }
                        }

                        public void Invoke({{string.Join(", ", invokeParameters)}})
                        {
                            if (_callbackFunction != null)
                                {{invokeCallbackName}}({{string.Join(", ", callInvokeInteropParameters)}});
                        }

                        [System.Runtime.InteropServices.DllImport("CesiumForUnityNative.dll")]
                        private static extern void {{disposeCallbackName}}(IntPtr callbackFunction);
                        [System.Runtime.InteropServices.DllImport("CesiumForUnityNative.dll")]
                        private static extern void {{invokeCallbackName}}({{string.Join(", ", invokeInteropParameters)}});
                    }
                    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
                    private unsafe delegate IntPtr {{csBaseName}}Type(IntPtr callbackFunction);
                    private static unsafe readonly {{csBaseName}}Type {{csBaseName}}Delegate = new {{csBaseName}}Type({{csBaseName}});
                    private static unsafe IntPtr {{csBaseName}}(IntPtr callbackFunction)
                    {
                        Reinterop.ReinteropInitializer.Initialize();
                        var receiver = new {{csType.Symbol.Name}}{{genericTypeHash}}NativeFunction(callbackFunction);
                        return Reinterop.ObjectHandleUtility.CreateHandle(new {{csType.GetFullyQualifiedName()}}(receiver.Invoke));
                    }
                    """
            ));

            var interopParameters = new[] { (Name: "pCallbackFunction", CsType: CSharpType.FromSymbol(this.Options.Compilation, this.Options.Compilation.GetSpecialType(SpecialType.System_IntPtr)), Type: CppType.VoidPointer, InteropType: CppType.VoidPointer) }.Concat(callbackParameters);
            var callParameters = callbackParameters.Select(p => p.Type.GetConversionFromInteropType(this.Options, p.Name));

            result.CppImplementationInvoker.Functions.Add(new(
                Content:
                    $$"""
                    __declspec(dllexport) void {{invokeCallbackName}}({{string.Join(", ", interopParameters.Select(p => $"{p.InteropType.GetFullyQualifiedName()} {p.Name}"))}}) {
                      auto pFunc = reinterpret_cast<std::function<{{itemType.GetFullyQualifiedName()}}::FunctionSignature>*>(pCallbackFunction);
                      (*pFunc)({{string.Join(", ", callParameters)}});
                    }
                    """));
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
            context.AddSource("ReinteropInitializer", combinedInit.ToCSharpSourceFileString());

            foreach (GeneratedResult? result in results)
            {
                if (result == null)
                    continue;

                GeneratedCSharpPartialMethodDefinitions? partialMethods = result.CSharpPartialMethodDefinitions;
                if (partialMethods == null || partialMethods.Methods.Count == 0)
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