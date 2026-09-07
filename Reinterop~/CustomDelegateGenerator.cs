using Microsoft.CodeAnalysis;

namespace Reinterop
{
    internal class CustomDelegateGenerator : ICustomGenerator
    {
        public IEnumerable<TypeToGenerate> GetDependencies(CppGenerationContext context)
        {
            yield break;
        }

        public GeneratedResult? Generate(CppGenerationContext context, TypeToGenerate type, GeneratedResult? generated)
        {
            // A delegate is a class with some extras
            if (generated == null || generated.CppDeclaration.Type.Kind != InteropTypeKind.Delegate)
                return generated;

            this.GenerateDelegate(context, generated, type, generated.CppDefinition.Type);
            return generated;
        }

        private void GenerateDelegate(CppGenerationContext context, GeneratedResult result, TypeToGenerate item, CppType itemType)
        {
            CppType implementationType = new CppType(InteropTypeKind.Unknown, itemType.Namespaces, itemType.Name + "Native", null, 0, "<functional>");

            if (result.CppImplementationInvoker == null)
            {
                result.CppImplementationInvoker = new GeneratedCppImplementationInvoker(implementationType);
                result.CSharpPartialMethodDefinitions = new GeneratedCSharpPartialMethodDefinitions(CSharpType.FromSymbol(context, item.Type));
            }

            // Add a constructor taking a std::function
            IMethodSymbol? invokeMethod = item.Methods.FirstOrDefault(m => m.Name == "Invoke");
            if (invokeMethod == null)
                return;

            var callbackParameters = invokeMethod.Parameters.Select(p =>
            {
                CppType type = CppType.FromCSharp(context, CSharpType.FromSymbol(context, p.Type));
                return (Name: p.Name, CsType: CSharpType.FromSymbol(context, p.Type), Type: type, InteropType: type.AsInteropType());
            });
            CppType returnType = CppType.FromCSharp(context, CSharpType.FromSymbol(context, invokeMethod.ReturnType)).AsReturnType();

            string templateSpecialization = "";
            if (itemType.GenericArguments != null && itemType.GenericArguments.Count > 0)
            {
                templateSpecialization = $"<{string.Join(", ", itemType.GenericArguments.Select(arg => arg.GetFullyQualifiedName()))}>";
            }

            // Declare a function signature and then a CppType for std::function<FunctionSignature>.
            // The FunctionSignature is InteropTypeKind.Primitive because it will never cross the interop boundary,
            // and by declaring it a Primitive we avoid attempting to include a wrapper header file for it.
            result.CppDeclaration.Elements.Add(new(
                Content: $"using FunctionSignature = {returnType.GetFullyQualifiedName()} ({string.Join(", ", callbackParameters.Select(p => p.Type.AsParameterType().GetFullyQualifiedName()))});"
            ));

            CppType functionType = new CppType(
                InteropTypeKind.Unknown,
                [ "std" ], "function",
                [ new CppType(InteropTypeKind.Primitive, [], "FunctionSignature", null, 0) ],
                0, "<functional>");

            // A C# delegate type that wraps a std::function, and arranges for
            // the invoke and dispose to be implemented in C++.
            CSharpType csType = CSharpType.FromSymbol(context, item.Type);

            string genericTypeHash = "";
            INamedTypeSymbol? named = item.Type as INamedTypeSymbol;
            if (named != null && named.IsGenericType)
            {
                genericTypeHash = Interop.HashParameters(null, named.TypeArguments);
            }

            string csBaseName = $"{csType.GetFullyQualifiedNamespace().Replace(".", "_")}_{csType.Name}{genericTypeHash}_CreateDelegate";
            string invokeCallbackName = $"{csType.GetFullyQualifiedNamespace().Replace(".", "_")}_{item.Type.Name}{genericTypeHash}_InvokeCallback";
            string disposeCallbackName = $"{csType.GetFullyQualifiedNamespace().Replace(".", "_")}_{item.Type.Name}{genericTypeHash}_DisposeCallback";

            var invokeParameters = callbackParameters.Select(p => $"{p.CsType.GetFullyQualifiedName()} {p.Name}");
            var invokeInteropParameters = new[] { "ImplementationHandle callbackFunction" }.Concat(callbackParameters.Select(p => $"{p.CsType.AsInteropTypeParameter().GetFullyQualifiedName()} {p.Name}"));
            var callInvokeInteropParameters = new[] { "_callbackFunction" }.Concat(callbackParameters.Select(p => p.CsType.GetConversionToInteropType(p.Name)));
            var csReturnType = CSharpType.FromSymbol(context, invokeMethod.ReturnType);

                string nativeFunctionCSharpContent =
                    $$"""
                    private class {{csType.Name}}{{genericTypeHash}}NativeFunction : System.IDisposable
                    {
                        internal class ImplementationHandle : Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid
                        {
                            public ImplementationHandle(IntPtr nativeImplementation) : base(true)
                            {
                                SetHandle(nativeImplementation);
                            }

                            [System.Runtime.ConstrainedExecution.ReliabilityContract(System.Runtime.ConstrainedExecution.Consistency.WillNotCorruptState, System.Runtime.ConstrainedExecution.Cer.Success)]
                            protected override bool ReleaseHandle()
                            {
                                {{disposeCallbackName}}(this.handle);
                                return true;
                            }
                        }

                        [System.NonSerialized]
                        private ImplementationHandle _callbackFunction;

                        public {{csType.Name}}{{genericTypeHash}}NativeFunction(IntPtr callbackFunction)
                        {
                            _callbackFunction = new ImplementationHandle(callbackFunction);
                        }

                        public void Dispose()
                        {
                            if (this._callbackFunction != null && !this._callbackFunction.IsInvalid)
                                this._callbackFunction.Dispose();
                            this._callbackFunction = null;
                        }

                        public unsafe {{csReturnType.GetFullyQualifiedName()}} Invoke({{string.Join(", ", invokeParameters)}})
                        {
                            if (_callbackFunction == null)
                                throw new System.ObjectDisposedException("{{csType.Name}}");

                            unsafe
                            {
                                {{new[] { CSharpPrinter.Print(CSharpInterop.CallNativeFunction(
                                    new CSharpIdentifier(invokeCallbackName),
                                    callInvokeInteropParameters.Select(p => (CSharpExpression)new CSharpRaw(p)).ToArray(),
                                    resultTypeName: !csReturnType.IsVoid ? "var" : null,
                                    returnExpression: !csReturnType.IsVoid ? new CSharpRaw(csReturnType.GetReturnValueConversionFromInteropType("result")) : null)) }.JoinAndIndent("                                ")}}
                            }
                        }

                        [System.Runtime.InteropServices.DllImport("{{context.NativeLibraryName}}", CallingConvention=System.Runtime.InteropServices.CallingConvention.Cdecl)]
                        private static extern void {{disposeCallbackName}}(IntPtr callbackFunction);
                        [System.Runtime.InteropServices.DllImport("{{context.NativeLibraryName}}", CallingConvention=System.Runtime.InteropServices.CallingConvention.Cdecl)]
                        private static unsafe extern {{csReturnType.AsInteropTypeReturn().GetFullyQualifiedName()}} {{invokeCallbackName}}({{string.Join(", ", invokeInteropParameters)}}, IntPtr* reinteropException);
                    }
                    """
                ;

            string nativeFunctionTypeName = $"{csType.Name}{genericTypeHash}NativeFunction";

            CSharpFunctionCallableFromCpp createDelegateRecipe = new CSharpFunctionCallableFromCpp(context, item.Type)
                .Name("CreateDelegate")
                .Parameters([new CSharpParameter(CSharpType.FromSymbol(context, context.Compilation.GetSpecialType(SpecialType.System_IntPtr)), "pCallbackFunction")])
                .ReturnType(csType)
                .Static(true)
                .Private(true)
                .AdditionalCSharpContent(nativeFunctionCSharpContent)
                .Body(new CSharpBodyCreateDelegate(csType, nativeFunctionTypeName));
            result.InteropFunctions.Add(createDelegateRecipe);

            CppFunction constructorRecipe = new CppFunction(context, itemType, itemType.Name)
                .Parameters([new CppParameter(functionType, "callback")])
                .MemberInitializers([
                    new CppMemberInitializer(
                        itemType.Name,
                        new CppCall(
                            new CppIdentifier("CreateDelegate"),
                            [new CppRaw("reinterpret_cast<void*>(new std::function<FunctionSignature>(std::move(callback)))")]))
                ])
                .DefinitionBody([]);
            result.ExtraCppFunctions.Add(constructorRecipe);

            var interopParameters = new[] { (Name: "pCallbackFunction", CsType: CSharpType.FromSymbol(context, context.Compilation.GetSpecialType(SpecialType.System_IntPtr)), Type: CppType.VoidPointer, InteropType: CppType.VoidPointer) }.Concat(callbackParameters);
            var callParameters = callbackParameters.Select(p => p.Type.GetConversionFromInteropType(context, p.Name));

            CppType interopReturnType = returnType.AsInteropType();

            string resultImplementation = "";
            string returnImplementation = "return;";
            string returnDefault = "return;";
            if (!csReturnType.IsVoid)
            {
                resultImplementation = "auto result = ";
                returnImplementation = $"return {returnType.GetConversionToInteropType(context, "result")};";
                if (interopReturnType.Flags.HasFlag(CppTypeFlags.Pointer))
                    returnDefault = "return nullptr;";
                else
                    returnDefault = $$"""return {{interopReturnType.GetFullyQualifiedName()}}();""";
            }

            result.CppImplementationInvoker.Functions.Add(new(
                Content:
                    $$"""
                    #if defined(_WIN32)
                    __declspec(dllexport)
                    #endif
                    {{interopReturnType.GetFullyQualifiedName()}} {{invokeCallbackName}}({{string.Join(", ", interopParameters.Select(p => $"{p.InteropType.GetFullyQualifiedName()} {p.Name}").Concat(new[] { "void** reinteropException" }))}}) {
                        auto pFunc = reinterpret_cast<std::function<{{itemType.GetFullyQualifiedName()}}::FunctionSignature>*>(pCallbackFunction);
                        {{new[] { CppPrinter.Print(CppInterop.TranslateExceptionsToOutParameter(
                            new CppStatement[] { new CppRawStatement($"{resultImplementation}(*pFunc)({string.Join(", ", callParameters)});"), new CppRawStatement(returnImplementation) },
                            new CppStatement[] { new CppRawStatement(returnDefault) })) }.JoinAndIndent("    ")}}
                    }
                    """,
                TypeDefinitionsReferenced: new[]
                {
                    CppReinteropException.GetCppType(context),
                    CSharpReinteropException.GetCppWrapperType(context),
                    CppType.FromCSharp(context, CSharpType.FromSymbol(context, context.Compilation.GetSpecialType(SpecialType.System_String)))
                }));

            result.CppImplementationInvoker.Functions.Add(new(
                Content:
                    $$"""
                    #if defined(_WIN32)
                    __declspec(dllexport)
                    #endif
                    void {{disposeCallbackName}}(void* pCallbackFunction) {
                      auto pFunc = reinterpret_cast<std::function<{{itemType.GetFullyQualifiedName()}}::FunctionSignature>*>(pCallbackFunction);
                      delete pFunc;
                    }
                    """));

            // Add operator+ and operator- to combine and remove delegates, respectively.
            CSharpFunctionCallableFromCpp combineDelegatesRecipe = new CSharpFunctionCallableFromCpp(context, item.Type)
                .Name("operator+")
                .Parameters([new CSharpParameter(csType, "rhs")])
                .ReturnType(csType)
                .Body(new CSharpBodyAddRemoveDelegate("+"));
            result.InteropFunctions.Add(combineDelegatesRecipe);

            CSharpFunctionCallableFromCpp removeDelegateRecipe = new CSharpFunctionCallableFromCpp(context, item.Type)
                .Name("operator-")
                .Parameters([new CSharpParameter(csType, "rhs")])
                .ReturnType(csType)
                .Body(new CSharpBodyAddRemoveDelegate("-"));
            result.InteropFunctions.Add(removeDelegateRecipe);

            // Add a Dispose method to free the native function without waiting for the finalizer.
            CSharpFunctionCallableFromCpp disposeRecipe = new CSharpFunctionCallableFromCpp(context, item.Type)
                .Name("Dispose")
                .Body(new CSharpBodyDisposeDelegate(nativeFunctionTypeName));
            result.InteropFunctions.Add(disposeRecipe);
        }
    }
}
