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
                        System.Diagnostics.Debug.Assert(_implementation == System.IntPtr.Zero, "Implementation is already created. Be sure to call CreateImplementation only once.");
                        _implementation = {{createName}}(Reinterop.ObjectHandleUtility.CreateHandle(this));
                    }
                    """,
                interopFunctionDeclaration:
                    $$"""
                    [DllImport("{{context.NativeLibraryName}}", CallingConvention=CallingConvention.Cdecl)]
                    private static extern System.IntPtr {{createName}}(System.IntPtr thiz);
                    """));

            string disposeName = $$"""{{wrapperType.GetFullyQualifiedName(false).Replace("::", "_")}}_Dispose""";
            result.CppImplementationInvoker.Functions.Add(new(
                Content:
                    $$"""
                    #if defined(_WIN32)
                    __declspec(dllexport)
                    #endif
                    void {{disposeName}}(void* handle, void* pImpl) {
                      const {{wrapperType.GetFullyQualifiedName()}} wrapper{{{objectHandleType.GetFullyQualifiedName()}}(handle)};
                      auto pImplTyped = reinterpret_cast<{{implType.GetFullyQualifiedName()}}*>(pImpl);
                      pImplTyped->JustBeforeDelete(wrapper);
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
            result.CSharpPartialMethodDefinitions.Methods.Add(new(
                methodDefinition:
                    $$"""
                    protected void DisposeImplementation()
                    {
                        if (_implementation != System.IntPtr.Zero)
                        {
                            {{disposeName}}(Reinterop.ObjectHandleUtility.CreateHandle(this), _implementation);
                            _implementation = System.IntPtr.Zero;
                        }
                    }
                    """,
                interopFunctionDeclaration:
                    $$"""
                    [DllImport("{{context.NativeLibraryName}}", CallingConvention=CallingConvention.Cdecl)]
                    private static extern void {{disposeName}}(System.IntPtr thiz, System.IntPtr implementation);
                    """));

            // If there is no finalizer, create one that calls DisposeImplementation.
            // But if the other half of this partial class has a finalizer already, we can't add one, so it's on
            // the implementer of that other class to call DisposeImplementation themselves.
            if (!item.Type.GetMembers("Finalize").Any())
            {
                result.CSharpPartialMethodDefinitions.Methods.Add(new(
                    methodDefinition:
                        $$"""
                        ~{{wrapperType.Name}}()
                        {
                            DisposeImplementation();
                        }
                        """));
            }

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
                                    ? "base.Dispose();" // let the base class suppress finalization if desired
                                    : "GC.SuppressFinalize(this);"
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

            // Add functions for other methods.
            foreach (IMethodSymbol method in item.MethodsImplementedInCpp)
            {
                // Do not generate CreateImplementation or Dispose methods.
                if (method.Name == "CreateImplementation" || method.Name == "Dispose")
                    continue;

                GenerateMethod(context, item, result, method);
            }

            // Add a method to the C++ wrapper that allows access to the C++ implementation.
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

            CSharpType csWrapperType = CSharpType.FromSymbol(context.Compilation, item.Type);

            string genericTypeHash = "";
            INamedTypeSymbol? named = csWrapperType.Symbol as INamedTypeSymbol;
            if (named != null && named.IsGenericType)
            {
                genericTypeHash = Interop.HashParameters(null, named.TypeArguments);
            }

            string baseName = $"{csWrapperType.GetFullyQualifiedNamespace().Replace(".", "_")}_{csWrapperType.Symbol.Name}{genericTypeHash}_Property_get_NativeImplementation";

            result.Init.Functions.Add(new(
                CppName: $"{wrapperType.GetFullyQualifiedName()}::Property_get_NativeImplementation",
                CppTypeSignature: $"void* (*)(void*)",
                CSharpName: $"{baseName}Delegate",
                CSharpContent:
                    $$"""
                    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
                    private unsafe delegate IntPtr {{baseName}}Type(IntPtr thiz);
                    private static unsafe readonly {{baseName}}Type {{baseName}}Delegate = new {{baseName}}Type({{baseName}});
                    [AOT.MonoPInvokeCallback(typeof({{baseName}}Type))]
                    private static unsafe IntPtr {{baseName}}(IntPtr thiz)
                    {
                        return ({{csWrapperType.GetParameterConversionFromInteropType("thiz")}}).NativeImplementation;
                    }
                    """
            ));
        }

        private static void GenerateMethod(CppGenerationContext context, TypeToGenerate item, GeneratedResult result, IMethodSymbol method)
        {
            if (result.CppImplementationInvoker == null)
                return;
            if (result.CSharpPartialMethodDefinitions == null)
                return;

            CppType wrapperType = result.CppDefinition.Type;
            CppType implType = result.CppImplementationInvoker.ImplementationType;

            // TODO: support static methods
            // TODO: support return values

            CppType returnType = CppType.FromCSharp(context, method.ReturnType).AsReturnType();
            CppType interopReturnType = returnType.AsInteropType();
            var parameters = method.Parameters.Select(parameter =>
            {
                CppType type = CppType.FromCSharp(context, parameter.Type).AsParameterType();
                return (Name: parameter.Name, Type: type, InteropType: type.AsInteropType());
            });

            string name = $"{wrapperType.GetFullyQualifiedName(false).Replace("::", "_")}_{method.Name}";

            var parameterList = new[] { "void* handle", "void* pImpl" }.Concat(parameters.Select(parameter => $"{parameter.InteropType.GetFullyQualifiedName()} {parameter.Name}"));
            string parameterListString = string.Join(", ", parameterList);

            var callParameterList = new[] { "wrapper" }.Concat(parameters.Select(parameter => parameter.Type.GetConversionFromInteropType(context, parameter.Name)));
            string callParameterListString = string.Join(", ", callParameterList);

            CppType objectHandleType = CppObjectHandle.GetCppType(context);

            string implementation;
            if (returnType == CppType.Void)
            {
                implementation =
                    $$"""
                    pImplTyped->{{method.Name}}({{callParameterListString}});
                    """;
            }
            else
            {
                implementation =
                    $$"""
                    auto result = pImplTyped->{{method.Name}}({{callParameterListString}});
                    return {{returnType.GetConversionToInteropType(context, "result")}};
                    """;
            }

            result.CppImplementationInvoker.Functions.Add(new(
                Content:
                    $$"""
                    #if defined(_WIN32)
                    __declspec(dllexport)
                    #endif
                    {{interopReturnType.GetFullyQualifiedName()}} {{name}}({{parameterListString}}) {
                      const {{wrapperType.GetFullyQualifiedName()}} wrapper{{{objectHandleType.GetFullyQualifiedName()}}(handle)};
                      auto pImplTyped = reinterpret_cast<{{implType.GetFullyQualifiedName()}}*>(pImpl);
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
                 .Concat(parameters.Select(parameter => parameter.InteropType))
            ));

            CSharpType csWrapperType = CSharpType.FromSymbol(context.Compilation, item.Type);
            CSharpType csReturnType = CSharpType.FromSymbol(context.Compilation, method.ReturnType);
            var csParameters = method.Parameters.Select(parameter => (Name: parameter.Name, CallName: parameter.Name, Type: CSharpType.FromSymbol(context.Compilation, parameter.Type)));
            var csParametersInterop = csParameters;
            var implementationPointer = CSharpType.FromSymbol(context.Compilation, context.Compilation.GetSpecialType(SpecialType.System_IntPtr));

            if (!method.IsStatic)
            {
                csParametersInterop = new[] { (Name: "thiz", CallName: "this", Type: csWrapperType), (Name: "implementation", CallName: "_implementation", Type: implementationPointer) }.Concat(csParametersInterop);
            }

            string csImplementation;
            if (csReturnType.Symbol.SpecialType == SpecialType.System_Void)
            {
                csImplementation =
                    $$"""
                    {{name}}({{string.Join(", ", csParametersInterop.Select(parameter => parameter.Type.GetConversionToInteropType(parameter.CallName)))}});
                    """;
            }
            else
            {
                csImplementation =
                    $$"""
                    var result = {{name}}({{string.Join(", ", csParametersInterop.Select(parameter => parameter.Type.GetConversionToInteropType(parameter.CallName)))}});
                    return {{csReturnType.GetReturnValueConversionFromInteropType("result")}};
                    """;
            }

            string modifiers = CSharpTypeUtility.GetAccessString(method.DeclaredAccessibility);
            if (method.IsStatic)
                modifiers += " static";

            if (method.IsOverride)
                modifiers += " override";
            else if (method.IsVirtual)
                modifiers += " virtual";

            result.CSharpPartialMethodDefinitions.Methods.Add(new(
                methodDefinition:
                    $$"""
                    {{modifiers}} partial {{csReturnType.GetFullyQualifiedName()}} {{method.Name}}({{string.Join(", ", csParameters.Select(parameter => $"{parameter.Type.GetFullyQualifiedName()} {parameter.Name}"))}})
                    {
                        System.Diagnostics.Debug.Assert(_implementation != System.IntPtr.Zero, "Implementation instance was not created or has already been destroyed. Check that all constructors call CreateImplementation, and that the object is not used after it is Disposed.");
                        {{new[] { csImplementation }.JoinAndIndent("    ")}}
                    }
                    """,
                interopFunctionDeclaration:
                    $$"""
                    [DllImport("{{context.NativeLibraryName}}", CallingConvention=CallingConvention.Cdecl)]
                    private static extern {{csReturnType.AsInteropType().GetFullyQualifiedName()}} {{name}}({{string.Join(", ", csParametersInterop.Select(parameter => parameter.Type.AsInteropType().GetFullyQualifiedName() + " " + parameter.Name))}});
                    """));
        }
    }
}