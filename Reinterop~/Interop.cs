using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;

namespace Reinterop
{
    internal class Interop
    {
        /// <summary>
        /// Creates a C# delegate, field of delegate type, and method/constructor/property to be invoked
        /// by the delegate in order to provide a way for C++ code to call the method/constructor/property.
        /// </summary>
        /// <param name="compilation">The compilation context.</param>
        /// <param name="ownerType">The type that owns the method, constructor, or property.</param>
        /// <param name="method">The method, constructor, or property getter/setter.</param>
        /// <param name="interopFunctionName">The name to use for the function, which must be unique within its containing type.</param>
        /// <returns>The Name and Content to add to the C# GeneratedInit functions list.</returns>
        public static (string Name, string Content) CreateCSharpDelegateInit(
            CppGenerationContext context,
            ITypeSymbol ownerType,
            IMethodSymbol method,
            string interopFunctionName)
        {
            CSharpType csType = CSharpType.FromSymbol(context, ownerType);
            var parameters = method.Parameters;
            var returnType = method.ReturnType;

            IPropertySymbol? property = method.AssociatedSymbol as IPropertySymbol;
            IEventSymbol? evt = method.AssociatedSymbol as IEventSymbol;

            string accessName;
            if (method.Name == ".ctor")
                accessName = $"new {csType.GetFullyQualifiedName()}";
            else if (property != null && property.IsIndexer)
                accessName = "";
            else if (property != null)
                accessName = "." + property.Name;
            else if (evt != null)
                accessName = "." + evt.Name;
            else
                accessName = "." + method.Name;

            if (method.IsGenericMethod)
            {
                accessName += $"<{string.Join(", ", method.TypeArguments.Select(t => t.ToDisplayString()))}>";
            }

            var callParameterDetails = parameters.Select(parameter => (Name: parameter.Name, Type: CSharpType.FromSymbol(context, parameter.Type), InteropType: CSharpType.FromSymbol(context, parameter.Type).AsInteropTypeParameter()));
            var interopParameterDetails = callParameterDetails;

            string invocationTarget;
            string beforeInvocation = "";
            string afterInvocation = "";
            if (method.Name == ".ctor")
            {
                // New object construction
                invocationTarget = $"new {csType.GetFullyQualifiedName()}";
                returnType = ownerType;
            }
            else if (!method.IsStatic && csType.Kind == InteropTypeKind.NonBlittableStructWrapper)
            {
                // Structs must be unboxed, modified, and then reboxed.
                interopParameterDetails = new[] { (Name: "thiz", Type: csType, InteropType: csType.AsInteropTypeParameter()) }.Concat(interopParameterDetails);
                beforeInvocation = $"var thizUnboxed = {csType.GetParameterConversionFromInteropType("thiz")};";
                invocationTarget = $"thizUnboxed{accessName}";
                afterInvocation = $"Reinterop.ObjectHandleUtility.ResetHandleObject(thiz, thizUnboxed);";
            }
            else if (!method.IsStatic)
            {
                // Instance method or property
                interopParameterDetails = new[] { (Name: "thiz", Type: csType, InteropType: csType.AsInteropTypeParameter()) }.Concat(interopParameterDetails);
                invocationTarget = $"({csType.GetParameterConversionFromInteropType("thiz")}){accessName}";
            }
            else
            {
                // Static method or property
                invocationTarget = $"{csType.GetFullyQualifiedName()}{accessName}";
            }

            CSharpType csReturnType = CSharpType.FromSymbol(context, returnType);
            CSharpType csInteropReturnType = csReturnType.AsInteropTypeReturn();

            // Rewrite methods that return a blittable struct to instead taking a pointer to one.
            // See Interop.RewriteStructReturn in this file for the C++ side of this and more
            // explanation of why it's needed.
            bool hasStructRewrite = false;
            CSharpType csOriginalInteropReturnType = csInteropReturnType;
            if (csReturnType.Kind == InteropTypeKind.BlittableStruct || csReturnType.Kind == InteropTypeKind.Nullable)
            {
                hasStructRewrite = true;
                if (csReturnType.Kind == InteropTypeKind.Nullable)
                    csInteropReturnType = CSharpType.FromSymbol(context, context.Compilation.GetSpecialType(SpecialType.System_Byte));
                else
                    csInteropReturnType = CSharpType.FromSymbol(context, context.Compilation.GetSpecialType(SpecialType.System_Void));
                interopParameterDetails = interopParameterDetails.Concat(new[]
                {
                    (Name: "pReturnValue", Type: csReturnType, InteropType: csOriginalInteropReturnType.AsPointer())
                });
            }

            string interopReturnTypeString = csInteropReturnType.GetFullyQualifiedName();

            string callParameterList = string.Join(", ", callParameterDetails.Select(parameter => parameter.Type.GetParameterConversionFromInteropType(parameter.Name)));
            string interopParameterList = string.Join(", ", interopParameterDetails.Select(parameter => $"{parameter.InteropType.GetFullyQualifiedName()} {parameter.Name}"));

            string implementation;
            if (property != null && SymbolEqualityComparer.Default.Equals(property.GetMethod, method))
            {
                if (property.IsIndexer)
                {
                    // An element indexer (operator[])
                    implementation =
                        $$"""
                        {{beforeInvocation}}
                        var result = {{invocationTarget}}[{{callParameterList}}];
                        {{afterInvocation}}
                        """;
                }
                else
                {
                    // A property getter.
                    implementation =
                        $$"""
                        {{beforeInvocation}}
                        var result = {{invocationTarget}};
                        {{afterInvocation}}
                        """;
                }

                if (hasStructRewrite)
                {
                    if (csReturnType.Kind == InteropTypeKind.Nullable)
                        implementation += Environment.NewLine + $$"""if (result != null) { *pReturnValue = ({{csOriginalInteropReturnType.GetFullyQualifiedName()}})result; return 1; } else { return 0; }""";
                    else
                        implementation += Environment.NewLine + $"*pReturnValue = result;";
                }
                else
                {
                    implementation += Environment.NewLine + $"return {csReturnType.GetConversionToInteropType("result")};";
                }
            }
            else if (property != null && SymbolEqualityComparer.Default.Equals(property.SetMethod, method))
            {
                if (property.IsIndexer && callParameterDetails.Count() >= 2)
                {
                    // An element indexer (operator[])
                    var indexParameter = callParameterDetails.ElementAt(0);
                    var valueParameter = callParameterDetails.ElementAt(1);
                    implementation =
                        $$"""
                        {{beforeInvocation}}{{invocationTarget}}[{{indexParameter.Type.GetParameterConversionFromInteropType(indexParameter.Name)}}] = {{valueParameter.Type.GetParameterConversionFromInteropType(valueParameter.Name)}};{{afterInvocation}}
                        """;
                }
                else
                {
                    // A property setter.
                    implementation =
                        $$"""
                        {{beforeInvocation}}{{invocationTarget}} = {{callParameterList}};{{afterInvocation}}
                        """;
                }
            }
            else if (evt != null && SymbolEqualityComparer.Default.Equals(evt.AddMethod, method))
            {
                // An event adder
                implementation =
                    $$"""
                    {{beforeInvocation}}{{invocationTarget}} += {{callParameterList}};{{afterInvocation}}
                    """;
            }
            else if (evt != null && SymbolEqualityComparer.Default.Equals(evt.RemoveMethod, method))
            {
                // An event adder
                implementation =
                    $$"""
                    {{beforeInvocation}}{{invocationTarget}} -= {{callParameterList}};{{afterInvocation}}
                    """;
            }
            else if (method.MethodKind == MethodKind.UserDefinedOperator && method.Parameters.Length == 2)
            {
                // binary operator, like operator==
                var lhs = callParameterDetails.ElementAt(0);
                var rhs = callParameterDetails.ElementAt(1);
                implementation =
                    $$"""
                    {{beforeInvocation}}
                    var result = ({{lhs.Type.GetParameterConversionFromInteropType(lhs.Name)}}) {{Interop.MethodNameToOperator(method.Name)}} ({{rhs.Type.GetParameterConversionFromInteropType(rhs.Name)}});
                    {{afterInvocation}}
                    return {{csReturnType.GetConversionToInteropType("result")}};
                    """;
            }
            else if (returnType.SpecialType == SpecialType.System_Void)
            {
                // Regular method returning void.
                implementation =
                    $$"""
                    {{beforeInvocation}}{{invocationTarget}}({{callParameterList}});{{afterInvocation}}
                    """;
            }
            else if (hasStructRewrite)
            {
                // A method with a struct return value, rewritten to be a parameter instead for interop.
                implementation =
                    $$"""
                    {{beforeInvocation}}
                    var result = {{invocationTarget}}({{callParameterList}});
                    {{afterInvocation}}
                    """;
                if (csReturnType.Kind == InteropTypeKind.Nullable)
                    implementation += Environment.NewLine + $$"""if (result != null) { *pReturnValue = ({{csOriginalInteropReturnType.GetFullyQualifiedName()}})result; return 1; } else { return 0; }""";
                else
                    implementation += Environment.NewLine + $"*pReturnValue = result;";
            }
            else
            {
                // Either a constructor or a regular method with a return value.
                implementation =
                    $$"""
                    {{beforeInvocation}}
                    var result = {{invocationTarget}}({{callParameterList}});
                    {{afterInvocation}}
                    return {{csReturnType.GetConversionToInteropType("result")}};
                    """;
            }

            string baseName = GetUniqueNameForType(csType) + "_" + interopFunctionName;

            return (
                Name: $"{baseName}Delegate",
                Content:
                    $$"""
                    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
                    private unsafe delegate {{interopReturnTypeString}} {{baseName}}Type({{interopParameterList}});
                    private static unsafe readonly {{baseName}}Type {{baseName}}Delegate = new {{baseName}}Type({{baseName}});
                    [AOT.MonoPInvokeCallback(typeof({{baseName}}Type))]
                    private static unsafe {{interopReturnTypeString}} {{baseName}}({{interopParameterList}})
                    {
                        {{implementation.Replace(Environment.NewLine, Environment.NewLine + "    ")}}
                    }
                    """
            );
        }

        /// <summary>
        /// Creates a C# delegate, field of delegate type, and method/constructor/property to be invoked
        /// by the delegate in order to provide a way for C++ code to access the given field.
        /// </summary>
        /// <param name="compilation">The compilation context.</param>
        /// <param name="ownerType">The type that owns the method, constructor, or property.</param>
        /// <param name="method">The method, constructor, or property getter/setter.</param>
        /// <param name="isGet">True if this delegate provides the ability to GET the field, or false if it provides the ability to SET it.</param>
        /// <returns>The Name and Content to add to the C# GeneratedInit functions list.</returns>
        public static (string Name, string Content) CreateCSharpDelegateInit(
            CppGenerationContext context,
            ITypeSymbol ownerType,
            IFieldSymbol field,
            bool isGet)
        {
            CSharpType csType = CSharpType.FromSymbol(context, ownerType);

            var parameters = isGet ? new (string Name, ITypeSymbol Type)[] { } : new[] { (Name: "value", Type: field.Type ) };
            var returnType = isGet ? field.Type : context.Compilation.GetSpecialType(SpecialType.System_Void);

            string accessName = field.Name;

            var callParameterDetails = parameters.Select(parameter => (Name: parameter.Name, Type: CSharpType.FromSymbol(context, parameter.Type), InteropType: CSharpType.FromSymbol(context, parameter.Type).AsInteropTypeParameter()));
            var interopParameterDetails = callParameterDetails;

            string invocationTarget;
            if (!field.IsStatic)
            {
                // Instance method or property
                interopParameterDetails = new[] { (Name: "thiz", Type: csType, InteropType: csType.AsInteropTypeParameter()) }.Concat(interopParameterDetails);
                invocationTarget = $"(({csType.GetFullyQualifiedName()})ObjectHandleUtility.GetObjectFromHandle(thiz)!).{accessName}";
            }
            else
            {
                // Static method or property
                invocationTarget = $"{csType.GetFullyQualifiedName()}.{accessName}";
            }

            CSharpType csReturnType = CSharpType.FromSymbol(context, returnType);
            CSharpType csInteropReturnType = csReturnType.AsInteropTypeReturn();

            // Rewrite getters that return a blittable struct to instead taking a pointer to one.
            // See Interop.RewriteStructReturn in this file for the C++ side of this and more
            // explanation of why it's needed.
            bool hasStructRewrite = false;
            CSharpType csOriginalInteropReturnType = csInteropReturnType;
            if (csReturnType.Kind == InteropTypeKind.BlittableStruct || csReturnType.Kind == InteropTypeKind.Nullable)
            {
                hasStructRewrite = true;
                if (csReturnType.Kind == InteropTypeKind.Nullable)
                    csInteropReturnType = CSharpType.FromSymbol(context, context.Compilation.GetSpecialType(SpecialType.System_Byte));
                else
                    csInteropReturnType = CSharpType.FromSymbol(context, context.Compilation.GetSpecialType(SpecialType.System_Void));
                interopParameterDetails = interopParameterDetails.Concat(new[]
                {
                    (Name: "pReturnValue", Type: csReturnType, InteropType: csOriginalInteropReturnType.AsPointer())
                });
            }

            string interopReturnTypeString = csInteropReturnType.GetFullyQualifiedName();
            string callParameterList = string.Join(", ", callParameterDetails.Select(parameter => parameter.Type.GetParameterConversionFromInteropType(parameter.Name)));
            string interopParameterList = string.Join(", ", interopParameterDetails.Select(parameter => $"{parameter.InteropType.GetFullyQualifiedName()} {parameter.Name}"));

            string implementation;
            if (isGet)
            {
                implementation = $"var result = {invocationTarget};";
                if (hasStructRewrite)
                {
                    if (csReturnType.Kind == InteropTypeKind.Nullable)
                        implementation += Environment.NewLine + $$"""if (result != null) { *pReturnValue = ({{csOriginalInteropReturnType.GetFullyQualifiedName()}})result; return 1; } else { return 0; }""";
                    else
                        implementation += Environment.NewLine + $"*pReturnValue = result;";
                }
                else
                {
                    implementation += Environment.NewLine + $"return {csReturnType.GetConversionToInteropType("result")};";
                }
            }
            else
            {
                // A property setter.
                implementation =
                    $$"""
                    {{invocationTarget}} = {{callParameterList}};
                    """;
            }

            string baseName = $"{GetUniqueNameForType(csType)}_Field_{(isGet ? "get" : "set")}_{field.Name}";

            return (
                Name: $"{baseName}Delegate",
                Content:
                    $$"""
                    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
                    private unsafe delegate {{interopReturnTypeString}} {{baseName}}Type({{interopParameterList}});
                    private static unsafe readonly {{baseName}}Type {{baseName}}Delegate = new {{baseName}}Type({{baseName}});
                    [AOT.MonoPInvokeCallback(typeof({{baseName}}Type))]
                    private static unsafe {{interopReturnTypeString}} {{baseName}}({{interopParameterList}})
                    {
                        {{implementation.Replace(Environment.NewLine, Environment.NewLine + "  ")}}
                    }
                    """
            );
        }

        public static string GetUniqueNameForType(CSharpType type)
        {
            string name = type.Name;
            string genericTypeHash = "";
            INamedTypeSymbol? named = type.Symbol as INamedTypeSymbol;
            if (named != null && named.IsGenericType)
            {
                genericTypeHash = HashParameters(null, named.TypeArguments);
            }

            IArrayTypeSymbol? arraySymbol = type.Symbol as IArrayTypeSymbol;
            if (arraySymbol != null)
            {
                name = "Array1";
                genericTypeHash = HashParameters(null, new[] { arraySymbol.ElementType });
            }

            return $"{type.GetFullyQualifiedNamespace().Replace(".", "_")}_{name}{genericTypeHash}";
        }

        public static void GenerateForType(CppGenerationContext context, TypeToGenerate item, GeneratedResult result)
        {
            string initializeReinteropHeader = context.BaseNamespace == null ? "<initializeReinterop.h>" : $"<{context.BaseNamespace.Replace("::", "/")}/initializeReinterop.h>";
            result.CppDeclaration.Elements.Add(new(
                Content: "friend std::uint8_t (::initializeReinterop)(std::uint64_t validationHash, void** functionPointers, std::int32_t count);",
                IsPrivate: true,
                TypeDeclarationsReferenced: new[] { CppType.UInt8, CppType.Int32, CppType.UInt64 },
                AdditionalIncludes: new[] { initializeReinteropHeader }));
        }

        public static string InsecureHash(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(bytes);
                return System.Convert.ToBase64String(hash);
            }
        }

        public static ulong InsecureHash64bits(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(bytes);

                ulong result = 0;

                for (int i = 0; i < sizeof(ulong) && i < hash.Length; ++i)
                {
                    result <<= 8;
                    result |= hash[i];
                }

                return result;
            }
        }

        public static string HashParameters(IEnumerable<IParameterSymbol>? parameters = null, IEnumerable<ITypeSymbol>? typeArguments = null)
        {
            IEnumerable<string>? formattedParameters = null;
            IEnumerable<string>? formattedTypeArguments = null;

            if (parameters != null)
                formattedParameters = parameters.Select(parameter => $"{parameter.Type.ToDisplayString()} {parameter.Name}");
            if (typeArguments != null)
                formattedTypeArguments = typeArguments.Select(arg => $"<{arg.ToDisplayString()}>");

            IEnumerable<string>? allFormattedInput = null;
            if (formattedParameters != null && formattedTypeArguments != null)
                allFormattedInput = formattedTypeArguments.Concat(formattedParameters);
            else if (formattedParameters != null)
                allFormattedInput = formattedParameters;
            else if (formattedTypeArguments != null)
                allFormattedInput = formattedTypeArguments;
            else
                allFormattedInput = new string[] { };

            var allTogether = string.Join(", ", allFormattedInput);
            string hash = InsecureHash(allTogether);
            return hash.Replace("=", "").Replace("+", "_").Replace("/", "__");
        }

        public static InteropTypeKind DetermineTypeKind(CppGenerationContext context, ITypeSymbol type)
        {
            if (type.Kind == SymbolKind.TypeParameter)
                return InteropTypeKind.GenericParameter;

            switch (type.SpecialType)
            {
                case SpecialType.System_SByte:
                case SpecialType.System_Int16:
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_Byte:
                case SpecialType.System_UInt16:
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                case SpecialType.System_Boolean:
                case SpecialType.System_IntPtr:
                case SpecialType.System_Void:
                    return InteropTypeKind.Primitive;
            }

            INamedTypeSymbol? named = type as INamedTypeSymbol;
            if (named != null && named.Name == "Nullable" && named.TypeArguments.Length == 1)
                return InteropTypeKind.Nullable;
            else if (SymbolEqualityComparer.Default.Equals(type.BaseType, context.Compilation.GetSpecialType(SpecialType.System_Enum))) {
              if (type.GetAttributes().Where(attrib => attrib.AttributeClass != null && (attrib.AttributeClass.Name == "FlagsAttribute" || attrib.AttributeClass.Name == "Flags")).Any()) { 
                return InteropTypeKind.EnumFlags;
              } else {
                return InteropTypeKind.Enum;
              }
            } else if (type.TypeKind == TypeKind.Delegate)
                return InteropTypeKind.Delegate;
            else if (type.IsReferenceType)
                return InteropTypeKind.ClassWrapper;
            else if (type is IPointerTypeSymbol)
                return InteropTypeKind.Primitive;
            else if (IsBlittableStruct(context, type))
                return InteropTypeKind.BlittableStruct;
            else
                return InteropTypeKind.NonBlittableStructWrapper;

        }

        /// <summary>
        /// Determines if the given type is a blittable value type (struct).
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="type"></param>
        /// <returns>True if the struct is blittable.</returns>
        public static bool IsBlittableStruct(CppGenerationContext context, ITypeSymbol type, int depth = 0)
        {
            // Sanity test to avoid a stack overflow
            if (depth > 10)
                return false;

            // We construct a name rather than using `type.ToDisplayString()` here so that
            // entire generic types can be listed as unblittable.
            string name = type.ContainingNamespace != null ? type.ContainingNamespace.ToDisplayString() + "." : "";
            name += type.Name;
            if (name.Length > 0 && context.NonBlittableTypes.Contains(name))
                return false;

            if (!type.IsValueType)
                return false;

            if (IsBlittablePrimitive(type))
                return true;

            ImmutableArray<ISymbol> members = type.GetMembers();
            foreach (ISymbol member in members)
            {
                if (member.Kind != SymbolKind.Field)
                    continue;

                IFieldSymbol? field = member as IFieldSymbol;
                if (field == null || field.IsStatic)
                    continue;

                if (!IsBlittableStruct(context, field.Type, depth + 1))
                    return false;
            }

            return true;
        }

        private static bool IsBlittablePrimitive(ITypeSymbol type)
        {
            if (type.TypeKind == TypeKind.Enum)
                return true;

            switch (type.SpecialType)
            {
                case SpecialType.System_Byte:
                case SpecialType.System_SByte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                case SpecialType.System_IntPtr:
                case SpecialType.System_UIntPtr:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_Boolean:
                    return true;
                default:
                    return false;
            }
        }

        public static List<string> BuildNamespace(string? baseNamespace, params string[] namespaces)
        {
            List<string> result = new List<string>();
            if (!string.IsNullOrEmpty(baseNamespace) && (namespaces.Length == 0 || namespaces[0] != baseNamespace))
                result.Add(baseNamespace!);
            result.AddRange(namespaces);
            return result;
        }

        public static string GetTemplateSpecialization(CppType type)
        {
            string templateSpecialization = "";
            if (type.GenericArguments != null && type.GenericArguments.Count > 0)
            {
                templateSpecialization = $"<{string.Join(", ", type.GenericArguments.Select(arg => arg.GetFullyQualifiedName()))}>";
            }

            return templateSpecialization;
        }

        public static string MethodNameToOperator(string methodName)
        {
            switch (methodName)
            {
                case "op_Equality":
                    return "==";
                case "op_Inequality":
                    return "!=";
                default:
                    throw new Exception("Unsupported operator " + methodName);
            }
        }

        /// <summary>
        /// Mono has trouble return large structs from C# to C++. See https://github.com/CesiumGS/cesium-unity/issues/73.
        /// This rewrites the interop for such a method so that the C++ code instead passes a pointer to the
        /// struct which is filled in on the C# side.
        /// </summary>
        /// <param name="interopParameters">The parameters to the interop method.</param>
        /// <param name="interopReturnType">The return type of the interop method.</param>
        /// <returns>True if the method has been rewritten, otherwise false.</returns>
        public static bool RewriteStructReturn(ref IEnumerable<(string ParameterName, string CallSiteName, CppType Type, CppType InteropType)> interopParameters, ref CppType returnType, ref CppType interopReturnType)
        {
            if (returnType.Kind == InteropTypeKind.BlittableStruct)
            {
                CppType originalInteropReturnType = interopReturnType;
                interopReturnType = CppType.Void;

                interopParameters = interopParameters.Concat(new[]
                {
                    (ParameterName: "pReturnValue", CallSiteName: "result", Type: returnType.AsReference(), InteropType: originalInteropReturnType.AsPointer())
                });

                return true;
            }
            else if (returnType.Kind == InteropTypeKind.Nullable && interopReturnType.Kind == InteropTypeKind.BlittableStruct)
            {
                CppType originalInteropReturnType = interopReturnType;
                interopReturnType = CppType.UInt8;

                interopParameters = interopParameters.Concat(new[]
                {
                    (ParameterName: "pReturnValue", CallSiteName: "result", Type: originalInteropReturnType.AsReference(), InteropType: originalInteropReturnType.AsPointer())
                });

                return true;
            }

            return false;
        }
    }
}
