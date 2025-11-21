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

            result.CppDeclaration.Elements.Add(new(
                Content: $"static void* (*CreateDelegate)(void* pCallbackFunction);",
                IsPrivate: true));
            result.CppDefinition.Elements.Add(new(
                Content: $"void* (*{itemType.Name}{templateSpecialization}::CreateDelegate)(void* pCallbackFunction) = nullptr;"));

            result.CppDeclaration.Elements.Add(new(
                Content: $"using FunctionSignature = {returnType.GetFullyQualifiedName()} ({string.Join(", ", callbackParameters.Select(p => p.Type.AsParameterType().GetFullyQualifiedName()))});"
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

            string csResultImplementation = "";
            string csReturnImplementation = "return;";
            if (invokeMethod.ReturnType.SpecialType != SpecialType.System_Void)
            {
                csResultImplementation = "var result = ";
                csReturnImplementation = $"return {csReturnType.GetReturnValueConversionFromInteropType("result")}";
            }

            result.Init.Functions.Add(new(
                CppName: $"{itemType.GetFullyQualifiedName()}::CreateDelegate",
                CppTypeSignature: $"void* (*)(void*)",
                CppTypeDefinitionsReferenced: new[] { itemType },
                CSharpName: csBaseName + "Delegate",
                CSharpContent:
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
                                System.IntPtr reinteropException = System.IntPtr.Zero;
                                {{csResultImplementation}}{{invokeCallbackName}}({{string.Join(", ", callInvokeInteropParameters)}}, &reinteropException);
                                if (reinteropException != System.IntPtr.Zero)
                                {
                                    throw (System.Exception)Reinterop.ObjectHandleUtility.GetObjectAndFreeHandle(reinteropException);
                                }
                                {{csReturnImplementation}};
                            }
                        }

                        [System.Runtime.InteropServices.DllImport("{{context.NativeLibraryName}}", CallingConvention=System.Runtime.InteropServices.CallingConvention.Cdecl)]
                        private static extern void {{disposeCallbackName}}(IntPtr callbackFunction);
                        [System.Runtime.InteropServices.DllImport("{{context.NativeLibraryName}}", CallingConvention=System.Runtime.InteropServices.CallingConvention.Cdecl)]
                        private static unsafe extern {{csReturnType.AsInteropTypeReturn().GetFullyQualifiedName()}} {{invokeCallbackName}}({{string.Join(", ", invokeInteropParameters)}}, IntPtr* reinteropException);
                    }
                    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
                    private unsafe delegate IntPtr {{csBaseName}}Type(IntPtr callbackFunction);
                    private static unsafe readonly {{csBaseName}}Type {{csBaseName}}Delegate = new {{csBaseName}}Type({{csBaseName}});
                    [AOT.MonoPInvokeCallback(typeof({{csBaseName}}Type))]
                    private static unsafe IntPtr {{csBaseName}}(IntPtr callbackFunction)
                    {
                        var receiver = new {{csType.Name}}{{genericTypeHash}}NativeFunction(callbackFunction);
                        return Reinterop.ObjectHandleUtility.CreateHandle(new {{csType.GetFullyQualifiedName()}}(receiver.Invoke));
                    }
                    """
            ));

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
                      try {
                        {{resultImplementation}}(*pFunc)({{string.Join(", ", callParameters)}});
                        {{returnImplementation}}
                      } catch (::DotNet::Reinterop::ReinteropNativeException& e) {
                        *reinteropException = ::DotNet::Reinterop::ObjectHandle(e.GetDotNetException().GetHandle()).Release();
                        {{returnDefault}}
                      } catch (std::exception& e) {
                        *reinteropException = ::DotNet::Reinterop::ReinteropException(::DotNet::System::String(e.what())).GetHandle().Release();
                        {{returnDefault}}
                      } catch (...) {
                        *reinteropException = ::DotNet::Reinterop::ReinteropException(::DotNet::System::String("An unknown native exception occurred.")).GetHandle().Release();
                        {{returnDefault}}
                      }                   
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
            result.CppDeclaration.Elements.Add(new(
                Content: $"static void* (*CombineDelegates)(void* thiz, void* rhs);",
                IsPrivate: true));
            result.CppDeclaration.Elements.Add(new(
                Content: $"static void* (*RemoveDelegate)(void* thiz, void* rhs);",
                IsPrivate: true));

            result.CppDefinition.Elements.Add(new(
                Content: $"void* (*{itemType.GetFullyQualifiedName()}::CombineDelegates)(void* thiz, void* rhs) = nullptr;"));
            result.CppDefinition.Elements.Add(new(
                Content: $"void* (*{itemType.GetFullyQualifiedName()}::RemoveDelegate)(void* thiz, void* rhs) = nullptr;"));

            result.CppDeclaration.Elements.Add(new(
                Content: $"{itemType.GetFullyQualifiedName()} operator+(const {itemType.GetFullyQualifiedName()}& rhs) const;"));
            result.CppDeclaration.Elements.Add(new(
                Content: $"{itemType.GetFullyQualifiedName()} operator-(const {itemType.GetFullyQualifiedName()}& rhs) const;"));

            CppType objectHandle = CppObjectHandle.GetCppType(context);

            result.CppDefinition.Elements.Add(new(
                Content:
                    $$"""
                    {{itemType.GetFullyQualifiedName()}} {{itemType.GetFullyQualifiedName(false)}}::operator+(const {{itemType.GetFullyQualifiedName()}}& rhs) const {
                      return {{itemType.GetFullyQualifiedName()}}({{objectHandle.GetFullyQualifiedName()}}(CombineDelegates(this->GetHandle().GetRaw(), rhs.GetHandle().GetRaw())));
                    }
                    """,
                TypeDefinitionsReferenced: new[] { objectHandle }));
            result.CppDefinition.Elements.Add(new(
                Content:
                    $$"""
                    {{itemType.GetFullyQualifiedName()}} {{itemType.GetFullyQualifiedName(false)}}::operator-(const {{itemType.GetFullyQualifiedName()}}& rhs) const {
                      return {{itemType.GetFullyQualifiedName()}}({{objectHandle.GetFullyQualifiedName()}}(RemoveDelegate(this->GetHandle().GetRaw(), rhs.GetHandle().GetRaw())));
                    }
                    """));

            string csTypeName = Interop.GetUniqueNameForType(csType);
            string csCombineDelegatesName = csTypeName + "_CombineDelegates";
            string csRemoveDelegateName = csTypeName + "_RemoveDelegate";

            result.Init.Functions.Add(new(
                CppName: $"{itemType.GetFullyQualifiedName()}::CombineDelegates",
                CppTypeSignature: $"void* (*)(void*, void*)",
                CppTypeDefinitionsReferenced: new[] { itemType, objectHandle },
                CSharpName: csCombineDelegatesName + "Delegate",
                CSharpContent:
                    $$"""
                    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
                    private unsafe delegate System.IntPtr {{csCombineDelegatesName}}Type(System.IntPtr thiz, System.IntPtr rhs);
                    private static unsafe readonly {{csCombineDelegatesName}}Type {{csCombineDelegatesName}}Delegate = new {{csCombineDelegatesName}}Type({{csCombineDelegatesName}});
                    [AOT.MonoPInvokeCallback(typeof({{csCombineDelegatesName}}Type))]
                    private static unsafe System.IntPtr {{csCombineDelegatesName}}(System.IntPtr thiz, System.IntPtr rhs)
                    {
                        {{csType.GetFullyQualifiedName()}} left = ({{csType.GetFullyQualifiedName()}})ObjectHandleUtility.GetObjectFromHandle(thiz)!;
                        {{csType.GetFullyQualifiedName()}} right = ({{csType.GetFullyQualifiedName()}})ObjectHandleUtility.GetObjectFromHandle(rhs)!;
                        return ObjectHandleUtility.CreateHandle(left + right);
                    }
                    """
            ));

            result.Init.Functions.Add(new(
                CppName: $"{itemType.GetFullyQualifiedName()}::RemoveDelegate",
                CppTypeSignature: $"void* (*)(void*, void*)",
                CppTypeDefinitionsReferenced: new[] { itemType, objectHandle },
                CSharpName: csRemoveDelegateName + "Delegate",
                CSharpContent:
                    $$"""
                    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
                    private unsafe delegate System.IntPtr {{csRemoveDelegateName}}Type(System.IntPtr thiz, System.IntPtr rhs);
                    private static unsafe readonly {{csRemoveDelegateName}}Type {{csRemoveDelegateName}}Delegate = new {{csRemoveDelegateName}}Type({{csRemoveDelegateName}});
                    [AOT.MonoPInvokeCallback(typeof({{csRemoveDelegateName}}Type))]
                    private static unsafe System.IntPtr {{csRemoveDelegateName}}(System.IntPtr thiz, System.IntPtr rhs)
                    {
                        {{csType.GetFullyQualifiedName()}} left = ({{csType.GetFullyQualifiedName()}})ObjectHandleUtility.GetObjectFromHandle(thiz)!;
                        {{csType.GetFullyQualifiedName()}} right = ({{csType.GetFullyQualifiedName()}})ObjectHandleUtility.GetObjectFromHandle(rhs)!;
                        return ObjectHandleUtility.CreateHandle(left - right);
                    }
                    """
            ));
        }
    }
}
