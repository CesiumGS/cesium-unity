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
            Compilation compilation,
            ITypeSymbol ownerType,
            IMethodSymbol method,
            string interopFunctionName)
        {
            CSharpType csType = CSharpType.FromSymbol(compilation, ownerType);
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

            var callParameterDetails = parameters.Select(parameter => (Name: parameter.Name, Type: CSharpType.FromSymbol(compilation, parameter.Type), InteropType: CSharpType.FromSymbol(compilation, parameter.Type).AsInteropType()));
            var interopParameterDetails = callParameterDetails;

            string invocationTarget;
            if (method.Name == ".ctor")
            {
                // New object construction
                invocationTarget = $"new {csType.GetFullyQualifiedName()}";
                returnType = ownerType;
            }
            else if (!method.IsStatic)
            {
                // Instance method or property
                interopParameterDetails = new[] { (Name: "thiz", Type: csType, InteropType: csType.AsInteropType()) }.Concat(interopParameterDetails);
                invocationTarget = $"(({csType.GetFullyQualifiedName()})ObjectHandleUtility.GetObjectFromHandle(thiz)!){accessName}";
            }
            else
            {
                // Static method or property
                invocationTarget = $"{csType.GetFullyQualifiedName()}{accessName}";
            }

            CSharpType csReturnType = CSharpType.FromSymbol(compilation, returnType);
            CSharpType csInteropReturnType = csReturnType.AsInteropType();
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
                        var result = {{invocationTarget}}[{{callParameterList}}];
                        return {{csReturnType.GetConversionToInteropType("result")}};
                        """;
                }
                else
                {
                    // A property getter.
                    implementation =
                        $$"""
                        var result = {{invocationTarget}};
                        return {{csReturnType.GetConversionToInteropType("result")}};
                        """;
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
                        {{invocationTarget}}[{{indexParameter.Type.GetParameterConversionFromInteropType(indexParameter.Name)}}] = {{valueParameter.Type.GetParameterConversionFromInteropType(valueParameter.Name)}};
                        """;
                }
                else
                {
                    // A property setter.
                    implementation =
                        $$"""
                        {{invocationTarget}} = {{callParameterList}};
                        """;
                }
            }
            else if (evt != null && SymbolEqualityComparer.Default.Equals(evt.AddMethod, method))
            {
                // An event adder
                implementation =
                    $$"""
                    {{invocationTarget}} += {{callParameterList}};
                    """;
            }
            else if (evt != null && SymbolEqualityComparer.Default.Equals(evt.RemoveMethod, method))
            {
                // An event adder
                implementation =
                    $$"""
                    {{invocationTarget}} -= {{callParameterList}};
                    """;
            }
            else if (returnType.SpecialType == SpecialType.System_Void)
            {
                // Regular method returning void.
                implementation =
                    $$"""
                    {{invocationTarget}}({{callParameterList}});
                    """;
            }
            else
            {
                // Either a constructor or a regular method with a return value.
                implementation =
                    $$"""
                    var result = {{invocationTarget}}({{callParameterList}});
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
                        {{implementation.Replace(Environment.NewLine, Environment.NewLine + "  ")}}
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
            Compilation compilation,
            ITypeSymbol ownerType,
            IFieldSymbol field,
            bool isGet)
        {
            CSharpType csType = CSharpType.FromSymbol(compilation, ownerType);

            var parameters = isGet ? new (string Name, ITypeSymbol Type)[] { } : new[] { (Name: "value", Type: field.Type ) };
            var returnType = isGet ? field.Type : compilation.GetSpecialType(SpecialType.System_Void);

            string accessName = field.Name;

            var callParameterDetails = parameters.Select(parameter => (Name: parameter.Name, Type: CSharpType.FromSymbol(compilation, parameter.Type), InteropType: CSharpType.FromSymbol(compilation, parameter.Type).AsInteropType()));
            var interopParameterDetails = callParameterDetails;

            string invocationTarget;
            if (!field.IsStatic)
            {
                // Instance method or property
                interopParameterDetails = new[] { (Name: "thiz", Type: csType, InteropType: csType.AsInteropType()) }.Concat(interopParameterDetails);
                invocationTarget = $"(({csType.GetFullyQualifiedName()})ObjectHandleUtility.GetObjectFromHandle(thiz)!).{accessName}";
            }
            else
            {
                // Static method or property
                invocationTarget = $"{csType.GetFullyQualifiedName()}.{accessName}";
            }

            CSharpType csReturnType = CSharpType.FromSymbol(compilation, returnType);
            CSharpType csInteropReturnType = csReturnType.AsInteropType();
            string interopReturnTypeString = csInteropReturnType.GetFullyQualifiedName();

            string callParameterList = string.Join(", ", callParameterDetails.Select(parameter => parameter.Type.GetParameterConversionFromInteropType(parameter.Name)));
            string interopParameterList = string.Join(", ", interopParameterDetails.Select(parameter => $"{parameter.InteropType.GetFullyQualifiedName()} {parameter.Name}"));

            string implementation;
            if (isGet)
            {
                implementation =
                    $$"""
                    var result = {{invocationTarget}};
                    return {{csReturnType.GetConversionToInteropType("result")}};
                    """;
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
            string genericTypeHash = "";
            INamedTypeSymbol? named = type.Symbol as INamedTypeSymbol;
            if (named != null && named.IsGenericType)
            {
                genericTypeHash = HashParameters(null, named.TypeArguments);
            }

            return $"{type.GetFullyQualifiedNamespace().Replace(".", "_")}_{type.Symbol.Name}{genericTypeHash}";
        }

        public static void GenerateForType(CppGenerationContext context, TypeToGenerate item, GeneratedResult result)
        {
            string initializeReinteropHeader = context.BaseNamespace == null ? "<initializeReinterop.h>" : $"<{context.BaseNamespace.Replace("::", "/")}/initializeReinterop.h>";
            result.CppDeclaration.Elements.Add(new(
                Content: "friend void ::initializeReinterop(void** functionPointers, std::int32_t count);",
                IsPrivate: true,
                TypeDeclarationsReferenced: new[] { CppType.Int32 },
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

        public static InteropTypeKind DetermineTypeKind(Compilation compilation, ITypeSymbol type)
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

            if (SymbolEqualityComparer.Default.Equals(type.BaseType, compilation.GetSpecialType(SpecialType.System_Enum)))
                return InteropTypeKind.Enum;
            else if (type.TypeKind == TypeKind.Delegate)
                return InteropTypeKind.Delegate;
            else if (type.IsReferenceType)
                return InteropTypeKind.ClassWrapper;
            else if (IsBlittableStruct(compilation, type))
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
        private static bool IsBlittableStruct(Compilation compilation, ITypeSymbol type)
        {
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
                if (field == null)
                    continue;

                if (!IsBlittableStruct(compilation, field.Type))
                    return false;
            }

            return true;
        }

        private static bool IsBlittablePrimitive(ITypeSymbol type)
        {
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
    }
}
