using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace Reinterop
{
    internal class MethodsImplementedInCpp
    {
        public static void Generate(CppGenerationContext context, TypeToGenerate item, GeneratedResult result)
        {
            Debug.Assert(result.CppImplementationInvoker != null);
            if (result.CppImplementationInvoker == null)
                return;
            if (result.CSharpPartialMethodDefinitions == null)
                return;

            // Add functions to create and destroy the implementation class instance.
            CppType wrapperType = result.CppDefinition.Type;
            CppType implType = result.CppImplementationInvoker.ImplementationType;
            CppType objectHandleType = CppObjectHandle.GetCppType(context);

            // We only need a C++ instance if any partial methods are non-static
            bool needsInstance = !item.ImplementationStaticOnly && item.MethodsImplementedInCpp.Any(m => !m.IsStatic);
            result.CSharpPartialMethodDefinitions.needsInstance = needsInstance;

            if (needsInstance)
            {
                string createName = $$"""{{wrapperType.GetFullyQualifiedName(false).Replace("::", "_")}}_CreateImplementation""";
                result.CppImplementationInvoker.Functions.Add(new(
                    Content:
                        $$"""
                    #if defined(_WIN32)
                    __declspec(dllexport)
                    #endif
                    void* {{createName}}(void* handle) {
                      const {{wrapperType.GetFullyQualifiedName()}} wrapper{{{objectHandleType.GetFullyQualifiedName()}}(handle)};
                      return reinterpret_cast<void*>(new {{implType.GetFullyQualifiedName()}}(wrapper));
                    }
                    """,
                    TypeDefinitionsReferenced: new[]
                    {
                    wrapperType,
                    implType,
                    objectHandleType,
                    }));

                result.CSharpPartialMethodDefinitions.Methods.Add(new(
                    methodDefinition:
                        $$"""
                    private void CreateImplementation()
                    {
                        Reinterop.ReinteropInitializer.Initialize();
                        System.Diagnostics.Debug.Assert(this._implementation == null, "Implementation is already created. Be sure to call CreateImplementation only once.");
                        this._implementation = new ImplementationHandle(this);
                    }
                    """,
                    interopFunctionDeclaration:
                        $$"""
                    [DllImport("{{context.NativeLibraryName}}", CallingConvention=CallingConvention.Cdecl)]
                    private static extern System.IntPtr {{createName}}(System.IntPtr thiz);
                    """));

                string destroyName = $$"""{{wrapperType.GetFullyQualifiedName(false).Replace("::", "_")}}_DestroyImplementation""";
                result.CppImplementationInvoker.Functions.Add(new(
                    Content:
                        $$"""
                    #if defined(_WIN32)
                    __declspec(dllexport)
                    #endif
                    void {{destroyName}}(void* pImpl) {
                      auto pImplTyped = reinterpret_cast<{{implType.GetFullyQualifiedName()}}*>(pImpl);
                      delete pImplTyped;
                    }
                    """,
                    TypeDefinitionsReferenced: new[]
                    {
                    wrapperType,
                    implType,
                    objectHandleType
                    }));

                // Always add a protected DisposeImplementation method.
                // This method must take a raw IntPtr to the implementation, rather than an ImplementationHandle.
                // Otherwise .NET (or at least Unity's Mono) will complain that the handle has already been closed
                // when we try to call the destroy method using it.
                result.CSharpPartialMethodDefinitions.Methods.Add(new(
                    methodDefinition:
                        $$"""
                    protected void DisposeImplementation()
                    {
                        if (this._implementation != null && !this._implementation.IsInvalid)
                            this._implementation.Dispose();
                        this._implementation = null;
                    }
                    """,
                    interopFunctionDeclaration:
                        $$"""
                    [DllImport("{{context.NativeLibraryName}}", CallingConvention=CallingConvention.Cdecl)]
                    private static extern void {{destroyName}}(System.IntPtr implementation);
                    """));

                // If there is no parameterless Dispose Method, add one. If the base class has one, call it (and mark it override if needed).
                // But if the other half of this partial class implements Dispose, it's on the implementer of that other class to
                // arrange for DisposeImplementation to be called themselves.
                if (!item.Type.GetMembers("Dispose").Where(symbol => symbol is IMethodSymbol method && method.Parameters.Length == 0).Any())
                {
                    IMethodSymbol? baseDisposeMethod = CSharpTypeUtility.FindMembers(item.Type, "Dispose").Where(symbol => symbol is IMethodSymbol method && method.Parameters.Length == 0).FirstOrDefault() as IMethodSymbol;
                    result.CSharpPartialMethodDefinitions.Methods.Add(new(
                        methodDefinition:
                            $$"""
                        public {{(baseDisposeMethod != null && baseDisposeMethod.IsVirtual ? "override " : "")}}void Dispose()
                        {
                            {{(baseDisposeMethod != null
                                        ? "base.Dispose();" // call the base class dispose if there is one.
                                        : ""
                                    )}}
                            this.DisposeImplementation();
                        }
                        """));
                }

                // If the class has no explicit constructors at all, add a default one.
                bool hasConstructor = item.Type.GetMembers().Where(m =>
                {
                    IMethodSymbol? method = m as IMethodSymbol;
                    if (method == null)
                        return false;

                    if (method.MethodKind != MethodKind.Constructor)
                        return false;

                    if (method.IsImplicitlyDeclared)
                        return false;

                    return true;
                }).Any();

                if (!hasConstructor)
                {
                    result.CSharpPartialMethodDefinitions.Methods.Add(new(
                        methodDefinition:
                            $$"""
                        public {{wrapperType.Name}}()
                        {
                            CreateImplementation();
                        }
                        """,
                        interopFunctionDeclaration: ""));
                }
            }

            // Add functions for other methods.
            foreach (IMethodSymbol method in item.MethodsImplementedInCpp)
            {
                // Do not generate CreateImplementation or Dispose methods.
                if (method.Name == "CreateImplementation" || method.Name == "Dispose")
                    continue;

                GenerateMethod(context, item, result, method);
            }

            // Add a method to the C++ wrapper that allows access to the C++ implementation.
            if (needsInstance)
            {
                result.CppDeclaration.Elements.Add(new(
                    Content: $"static void* (*Property_get_NativeImplementation)(void*);",
                    IsPrivate: true));
                result.CppDeclaration.Elements.Add(new(
                    Content: $"::{implType.AsReference().GetFullyQualifiedName()} NativeImplementation() const noexcept;",
                    TypeDeclarationsReferenced: new[] { implType.AsReference() }
                ));

                result.CppDefinition.Elements.Add(new(
                    Content: $"void* (*{wrapperType.Name}::Property_get_NativeImplementation)(void*) = nullptr;"));
                result.CppDefinition.Elements.Add(new(
                    Content:
                        $$"""
                    ::{{implType.AsReference().GetFullyQualifiedName()}} {{wrapperType.Name}}::NativeImplementation() const noexcept {
                      return *reinterpret_cast<::{{implType.GetFullyQualifiedName()}}*>(Property_get_NativeImplementation(this->_handle.GetRaw()));
                    }
                    """
                ));

                CSharpType csWrapperType = CSharpType.FromSymbol(context, item.Type);

                string genericTypeHash = "";
                INamedTypeSymbol? named = csWrapperType.Symbol as INamedTypeSymbol;
                if (named != null && named.IsGenericType)
                {
                    genericTypeHash = Interop.HashParameters(null, named.TypeArguments);
                }

                string baseName = $"{csWrapperType.GetFullyQualifiedNamespace().Replace(".", "_")}_{csWrapperType.Name}{genericTypeHash}_Property_get_NativeImplementation";

                // Ideally the wrapper for NativeImplementation would return an ImplementationHandle.
                // But Mono explodes, saying a managed function returning a SafeHandle is not implemented.
                // So just use an IntPtr here instead.
                result.Init.Functions.Add(new(
                    CppName: $"{wrapperType.GetFullyQualifiedName()}::Property_get_NativeImplementation",
                    CppTypeSignature: $"void* (*)(void*)",
                    CSharpName: $"{baseName}Delegate",
                    CSharpContent:
                        $$"""
                        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
                        private unsafe delegate System.IntPtr {{baseName}}Type(IntPtr thiz);
                        private static unsafe readonly {{baseName}}Type {{baseName}}Delegate = new {{baseName}}Type({{baseName}});
                        [AOT.MonoPInvokeCallback(typeof({{baseName}}Type))]
                        private static unsafe System.IntPtr {{baseName}}(IntPtr thiz)
                        {
                            return ({{csWrapperType.GetParameterConversionFromInteropType("thiz")}}).NativeImplementation.DangerousGetHandle();
                        }
                        """
                ));
            }
        }

        private static void GenerateMethod(CppGenerationContext context, TypeToGenerate item, GeneratedResult result, IMethodSymbol method)
        {
            if (result.CppImplementationInvoker == null)
                return;
            if (result.CSharpPartialMethodDefinitions == null)
                return;

            CppType wrapperType = result.CppDefinition.Type;
            CppType implType = result.CppImplementationInvoker.ImplementationType;

            CppType returnType = CppType.FromCSharp(context, method.ReturnType).AsReturnType();
            CppType interopReturnType = returnType.AsInteropType();
            var parameters = method.Parameters.Select(parameter =>
            {
                CppType type = CppType.FromCSharp(context, parameter.Type).AsParameterType();
                return (ParameterName: parameter.Name, CallSiteName: parameter.Name, Type: type, InteropType: type.AsInteropType());
            });

            string name = $"{wrapperType.GetFullyQualifiedName(false).Replace("::", "_")}_{method.Name}";

            CppType objectHandleType = CppObjectHandle.GetCppType(context);

            // Start off assuming a static method
            string callTarget = $"{implType.GetFullyQualifiedName()}::";
            string getCallTarget = "";

            // If it's an instance method, we need some extra parameters.
            if (!method.IsStatic)
            {
                // Only add the C++ instance pointer if we're using a C++ instance.
                if (!item.ImplementationStaticOnly)
                {
                    parameters = new[]
                    {
                        (ParameterName: "pImpl", CallSiteName: "", Type: CppType.VoidPointer, InteropType: CppType.VoidPointer.AsInteropType())
                    }.Concat(parameters);
                    callTarget = "pImplTyped->";
                    getCallTarget =
                        $$"""
                        auto pImplTyped = reinterpret_cast<{{implType.GetFullyQualifiedName()}}*>(pImpl);
                        """;
                }

                parameters = new[]
                {
                    (ParameterName: "handle", CallSiteName: "wrapper", Type: CppType.VoidPointer, InteropType: CppType.VoidPointer.AsInteropType()),
                }.Concat(parameters);
                getCallTarget +=
                    $$"""
                    const {{wrapperType.GetFullyQualifiedName()}} wrapper{{{objectHandleType.GetFullyQualifiedName()}}(handle)};
                    """;
            }

            bool hasStructRewrite = false;
            string returnResult = $"return {returnType.GetConversionToInteropType(context, "result")};";
            if (returnType.Kind == InteropTypeKind.BlittableStruct)
            {
                CppType originalInteropReturnType = interopReturnType;
                interopReturnType = CppType.Void;

                parameters = parameters.Concat(new[]
                {
                    (ParameterName: "pReturnValue", CallSiteName: "", Type: returnType.AsReference(), InteropType: originalInteropReturnType.AsPointer())
                });

                returnResult = "*pReturnValue = std::move(result);";
                hasStructRewrite = true;
            }
            else if (returnType.Kind == InteropTypeKind.Nullable && interopReturnType.Kind == InteropTypeKind.BlittableStruct)
            {
                CppType originalInteropReturnType = interopReturnType;
                interopReturnType = CppType.UInt8;

                parameters = parameters.Concat(new[]
                {
                    (ParameterName: "pReturnValue", CallSiteName: "", Type: originalInteropReturnType.AsReference(), InteropType: originalInteropReturnType.AsPointer())
                });

                returnResult = "if (result.has_value()) { *pReturnValue = std::move(result.value()); return true; } else { return false; }";
                hasStructRewrite = true;
            }

            var parameterList = parameters.Select(parameter => $"{parameter.InteropType.GetFullyQualifiedName()} {parameter.ParameterName}");
            var callParameterList = parameters.Where(parameter => parameter.CallSiteName.Length > 0).Select(parameter => parameter.Type.GetConversionFromInteropType(context, parameter.CallSiteName));

            string parameterListString = string.Join(", ", parameterList);
            string callParameterListString = string.Join(", ", callParameterList);

            string implementation;
            if (returnType == CppType.Void)
            {
                implementation =
                    $$"""
                    {{callTarget}}{{method.Name}}({{callParameterListString}});
                    """;
            }
            else
            {
                implementation =
                    $$"""
                    auto result = {{callTarget}}{{method.Name}}({{callParameterListString}});
                    {{returnResult}}
                    """;
            }

            result.CppImplementationInvoker.Functions.Add(new(
                Content:
                    $$"""
                    #if defined(_WIN32)
                    __declspec(dllexport)
                    #endif
                    {{interopReturnType.GetFullyQualifiedName()}} {{name}}({{parameterListString}}) {
                      {{GenerationUtility.JoinAndIndent(new[] { getCallTarget }, "  ")}}
                      {{new[] { implementation }.JoinAndIndent("  ")}}
                    }
                    """,
                TypeDefinitionsReferenced: new[]
                {
                    wrapperType,
                    implType,
                    returnType,
                    objectHandleType
                }.Concat(parameters.Select(parameter => parameter.Type))
                 .Concat(parameters.Select(parameter => parameter.InteropType)),
                AdditionalIncludes: hasStructRewrite ? new[] { "<utility>" } : null // for std::move
            ));

            CSharpType csWrapperType = CSharpType.FromSymbol(context, item.Type);
            CSharpType csReturnType = CSharpType.FromSymbol(context, method.ReturnType);
            var csParameters = method.Parameters.Select(parameter => (Name: parameter.Name, CallName: parameter.Name, Type: CSharpType.FromSymbol(context, parameter.Type)));
            var csParametersInterop = csParameters;
            var implementationPointer = new CSharpType(context, InteropTypeKind.Primitive, csWrapperType.Namespaces, csWrapperType.Name + ".ImplementationHandle", csWrapperType.SpecialType, null);

            if (!method.IsStatic)
            {
                if (!item.ImplementationStaticOnly)
                {
                    csParametersInterop = new[]
                    {
                        (Name: "implementation", CallName: "_implementation", Type: implementationPointer)
                    }.Concat(csParametersInterop);
                }

                csParametersInterop = new[]
                {
                    (Name: "thiz", CallName: "this", Type: csWrapperType),
                }.Concat(csParametersInterop);
            }

            CSharpType csInteropReturnType = csReturnType.AsInteropTypeReturn();
            CSharpType csOriginalInteropReturnType = csInteropReturnType;
            if (hasStructRewrite)
            {
                csParametersInterop = csParametersInterop.Concat(new[]
                {
                    (Name: "pReturnValue", CallName: "&returnValue", Type: csInteropReturnType.AsPointer())
                });
                csInteropReturnType = CSharpType.FromSymbol(context, returnType.Kind == InteropTypeKind.Nullable ? context.Compilation.GetSpecialType(SpecialType.System_Byte) : context.Compilation.GetSpecialType(SpecialType.System_Void));
            }

            List<string> csImplementationLines = new List<string>();
            if (hasStructRewrite)
            {
                csImplementationLines.Add($"var returnValue = new {csOriginalInteropReturnType.AsInteropTypeReturn().GetFullyQualifiedName()}();");
            }

            if (csInteropReturnType.SpecialType == SpecialType.System_Void)
                csImplementationLines.Add($"{name}({string.Join(", ", csParametersInterop.Select(parameter => parameter.Type.GetConversionToInteropType(parameter.CallName)))});");
            else
                csImplementationLines.Add($"var result = {name}({string.Join(", ", csParametersInterop.Select(parameter => parameter.Type.GetConversionToInteropType(parameter.CallName)))});");

            if (csReturnType.SpecialType != SpecialType.System_Void)
            {
                if (hasStructRewrite)
                {
                    if (csReturnType.Kind == InteropTypeKind.Nullable)
                        csImplementationLines.Add("return result == 1 ? returnValue : null;");
                    else
                        csImplementationLines.Add("return returnValue;");
                }
                else
                {
                    csImplementationLines.Add($"return {csReturnType.GetReturnValueConversionFromInteropType("result")};");
                }
            }

            string modifiers = CSharpTypeUtility.GetAccessString(method.DeclaredAccessibility);
            string implementationCheck = "";
            if (method.IsStatic)
            {
                modifiers += " static";
            }
            else if (!item.ImplementationStaticOnly)
            {
                implementationCheck =
                    $$"""
                    if (this._implementation == null || this._implementation.IsInvalid)
                        throw new NotImplementedException("The native implementation is missing so {{method.Name}} cannot be invoked. This may be caused by a missing call to CreateImplementation in one of your constructors, or it may be that the entire native implementation shared library is missing or out of date.");
                    """;
            }
            else
            {
                implementationCheck = "Reinterop.ReinteropInitializer.Initialize();";
            }

            if (method.IsOverride)
                modifiers += " override";
            else if (method.IsVirtual)
                modifiers += " virtual";

            result.CSharpPartialMethodDefinitions.Methods.Add(new(
                methodDefinition:
                    $$"""
                    {{modifiers}} partial {{csReturnType.GetFullyQualifiedName()}} {{method.Name}}({{string.Join(", ", csParameters.Select(parameter => $"{parameter.Type.GetFullyQualifiedName()} {parameter.Name}"))}})
                    {
                        unsafe
                        {
                            {{GenerationUtility.JoinAndIndent(new[] { implementationCheck }, "        ")}}
                            {{csImplementationLines.JoinAndIndent("        ")}}
                        }
                    }
                    """,
                interopFunctionDeclaration:
                    $$"""
                    [DllImport("{{context.NativeLibraryName}}", CallingConvention=CallingConvention.Cdecl)]
                    private static unsafe extern {{csInteropReturnType.GetFullyQualifiedName()}} {{name}}({{string.Join(", ", csParametersInterop.Select(parameter => parameter.Type.AsInteropTypeParameter().GetFullyQualifiedName() + " " + parameter.Name))}});
                    """));
        }
    }
}