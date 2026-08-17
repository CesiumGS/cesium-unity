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
                CppType type = CppType.FromCSharp(context, p.Type);
                return (Name: p.Name, CsType: CSharpType.FromSymbol(context, p.Type), Type: type, InteropType: type.AsInteropType());
            });
            CppType returnType = CppType.FromCSharp(context, invokeMethod.ReturnType).AsReturnType();

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

            string createDelegateCSharpContent =
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
                                    resultTypeName: invokeMethod.ReturnType.SpecialType != SpecialType.System_Void ? "var" : null,
                                    returnExpression: invokeMethod.ReturnType.SpecialType != SpecialType.System_Void ? new CSharpRaw(csReturnType.GetReturnValueConversionFromInteropType("result")) : null)) }.JoinAndIndent("                                ")}}
                            }
                        }

                        [System.Runtime.InteropServices.DllImport("{{context.NativeLibraryName}}", CallingConvention=System.Runtime.InteropServices.CallingConvention.Cdecl)]
                        private static extern void {{disposeCallbackName}}(IntPtr callbackFunction);
                        [System.Runtime.InteropServices.DllImport("{{context.NativeLibraryName}}", CallingConvention=System.Runtime.InteropServices.CallingConvention.Cdecl)]
                        private static unsafe extern {{csReturnType.AsInteropTypeReturn().GetFullyQualifiedName()}} {{invokeCallbackName}}({{string.Join(", ", invokeInteropParameters)}}, IntPtr* reinteropException);
                    }
                    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
                    private unsafe delegate IntPtr {{csBaseName}}Type(IntPtr callbackFunction, IntPtr* reinteropException);
                    private static unsafe readonly {{csBaseName}}Type {{csBaseName}}Delegate = new {{csBaseName}}Type({{csBaseName}});
                    [AOT.MonoPInvokeCallback(typeof({{csBaseName}}Type))]
                    private static unsafe IntPtr {{csBaseName}}(IntPtr callbackFunction, IntPtr* reinteropException)
                    {
                        try
                        {
                            var receiver = new {{csType.Name}}{{genericTypeHash}}NativeFunction(callbackFunction);
                            return Reinterop.ObjectHandleUtility.CreateHandle(new {{csType.GetFullyQualifiedName()}}(receiver.Invoke));
                        }
                        catch (Exception reinteropManagedException)
                        {
                            *reinteropException = Reinterop.ObjectHandleUtility.CreateHandle(reinteropManagedException);
                            return IntPtr.Zero;
                        }
                    }
                    """
                ;

            CppInteropFunction createDelegateRecipe = new CppInteropFunction(context, itemType, "CreateDelegate")
                .Parameters([new CppInteropParameter("pCallbackFunction", CppType.VoidPointer)])
                .ReturnType(itemType.AsReturnType())
                .Static(true)
                .Private(true)
                .CSharp(csBaseName + "Delegate", createDelegateCSharpContent);
            result.InteropFunctions.Add(createDelegateRecipe);

            CppInteropFunction constructorRecipe = new CppInteropFunction(context, itemType, itemType.Name)
                .Parameters([new CppInteropParameter("callback", functionType)])
                .MemberInitializers([
                    new CppMemberInitializer(
                        itemType.Name,
                        new CppCall(
                            new CppIdentifier("CreateDelegate"),
                            [new CppRaw("reinterpret_cast<void*>(new std::function<FunctionSignature>(std::move(callback)))")]))
                ])
                .DefinitionBody([]);
            result.InteropFunctions.Add(constructorRecipe);

            var interopParameters = new[] { (Name: "pCallbackFunction", CsType: CSharpType.FromSymbol(context, context.Compilation.GetSpecialType(SpecialType.System_IntPtr)), Type: CppType.VoidPointer, InteropType: CppType.VoidPointer) }.Concat(callbackParameters);
            var callParameters = callbackParameters.Select(p => p.Type.GetConversionFromInteropType(context, p.Name));

            CppType interopReturnType = returnType.AsInteropType();

            string resultImplementation = "";
            string returnImplementation = "return;";
            string returnDefault = "return;";
            if (invokeMethod.ReturnType.SpecialType != SpecialType.System_Void)
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
                    CppType.FromCSharp(context, context.Compilation.GetSpecialType(SpecialType.System_String))
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
            string csTypeName = Interop.GetUniqueNameForType(csType);
            string csCombineDelegatesName = csTypeName + "_CombineDelegates";
            string csRemoveDelegateName = csTypeName + "_RemoveDelegate";

            CppInteropFunction combineDelegatesRecipe = new CppInteropFunction(context, itemType, "operator+")
                .Parameters([new CppInteropParameter("rhs", itemType.AsParameterType())])
                .ReturnType(itemType.AsReturnType())
                .CSharp(csCombineDelegatesName + "Delegate",
                    $$"""
                    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
                    private unsafe delegate System.IntPtr {{csCombineDelegatesName}}Type(System.IntPtr thiz, System.IntPtr rhs, System.IntPtr* reinteropException);
                    private static unsafe readonly {{csCombineDelegatesName}}Type {{csCombineDelegatesName}}Delegate = new {{csCombineDelegatesName}}Type({{csCombineDelegatesName}});
                    [AOT.MonoPInvokeCallback(typeof({{csCombineDelegatesName}}Type))]
                    private static unsafe System.IntPtr {{csCombineDelegatesName}}(System.IntPtr thiz, System.IntPtr rhs, System.IntPtr* reinteropException)
                    {
                        try
                        {
                            {{csType.GetFullyQualifiedName()}} left = ({{csType.GetFullyQualifiedName()}})ObjectHandleUtility.GetObjectFromHandle(thiz)!;
                            {{csType.GetFullyQualifiedName()}} right = ({{csType.GetFullyQualifiedName()}})ObjectHandleUtility.GetObjectFromHandle(rhs)!;
                            return ObjectHandleUtility.CreateHandle(left + right);
                        }
                        catch (Exception reinteropManagedException)
                        {
                            *reinteropException = Reinterop.ObjectHandleUtility.CreateHandle(reinteropManagedException);
                            return System.IntPtr.Zero;
                        }
                    }
                    """);
            result.InteropFunctions.Add(combineDelegatesRecipe);

            CppInteropFunction removeDelegateRecipe = new CppInteropFunction(context, itemType, "operator-")
                .Parameters([new CppInteropParameter("rhs", itemType.AsParameterType())])
                .ReturnType(itemType.AsReturnType())
                .CSharp(csRemoveDelegateName + "Delegate",
                    $$"""
                    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
                    private unsafe delegate System.IntPtr {{csRemoveDelegateName}}Type(System.IntPtr thiz, System.IntPtr rhs, System.IntPtr* reinteropException);
                    private static unsafe readonly {{csRemoveDelegateName}}Type {{csRemoveDelegateName}}Delegate = new {{csRemoveDelegateName}}Type({{csRemoveDelegateName}});
                    [AOT.MonoPInvokeCallback(typeof({{csRemoveDelegateName}}Type))]
                    private static unsafe System.IntPtr {{csRemoveDelegateName}}(System.IntPtr thiz, System.IntPtr rhs, System.IntPtr* reinteropException)
                    {
                        try
                        {
                            {{csType.GetFullyQualifiedName()}} left = ({{csType.GetFullyQualifiedName()}})ObjectHandleUtility.GetObjectFromHandle(thiz)!;
                            {{csType.GetFullyQualifiedName()}} right = ({{csType.GetFullyQualifiedName()}})ObjectHandleUtility.GetObjectFromHandle(rhs)!;
                            return ObjectHandleUtility.CreateHandle(left - right);
                        }
                        catch (Exception reinteropManagedException)
                        {
                            *reinteropException = Reinterop.ObjectHandleUtility.CreateHandle(reinteropManagedException);
                            return System.IntPtr.Zero;
                        }
                    }
                    """);
            result.InteropFunctions.Add(removeDelegateRecipe);

            // Add a Dispose method to free the native function without waiting for the finalizer.
            CppInteropFunction disposeRecipe = new CppInteropFunction(context, itemType, "Dispose")
                .ReturnType(CppType.Void)
                .CSharp(csTypeName + "_DisposeDelegateDelegate",
                    $$"""
                    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
                    private unsafe delegate void {{csTypeName}}_DisposeDelegateType(System.IntPtr thiz, System.IntPtr* reinteropException);
                    private static unsafe readonly {{csTypeName}}_DisposeDelegateType {{csTypeName}}_DisposeDelegateDelegate = new {{csTypeName}}_DisposeDelegateType({{csTypeName}}_DisposeDelegate);
                    [AOT.MonoPInvokeCallback(typeof({{csTypeName}}_DisposeDelegateType))]
                    private static unsafe void {{csTypeName}}_DisposeDelegate(System.IntPtr thiz, System.IntPtr* reinteropException)
                    {
                        try
                        {
                            var delegateObject = ({{csType.GetFullyQualifiedName()}})ObjectHandleUtility.GetObjectFromHandle(thiz)!;
                            var nativeFunction = delegateObject.Target as {{csType.Name}}{{genericTypeHash}}NativeFunction;
                            if (nativeFunction != null)
                            {
                                nativeFunction.Dispose();
                            }
                        }
                        catch (Exception reinteropManagedException)
                        {
                            *reinteropException = Reinterop.ObjectHandleUtility.CreateHandle(reinteropManagedException);
                        }
                    }
                    """);
            result.InteropFunctions.Add(disposeRecipe);
        }
    }
}
