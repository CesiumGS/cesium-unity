using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;

namespace Reinterop
{
    internal class Interop
    {
        public static string GetUniqueNameForType(CSharpType type)
        {
            string name = type.Name;
            string genericTypeHash = "";

            if (type.TypeArguments.Count > 0)
                genericTypeHash = HashParameters(null, type.TypeArguments);

            if (type.ArrayElementType != null)
            {
                name = "Array1";
                genericTypeHash = HashParameters(null, new[] { type.ArrayElementType });
            }

            return $"{type.GetFullyQualifiedNamespace().Replace(".", "_")}_{name}{genericTypeHash}";
        }

        public static void GenerateForType(ReinteropGenerationContext context, TypeToGenerate item, GeneratedResult result)
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

        public static string HashParameters(IEnumerable<CSharpParameter>? parameters = null, IEnumerable<CSharpType>? typeArguments = null)
        {
            IEnumerable<string>? formattedParameters = null;
            IEnumerable<string>? formattedTypeArguments = null;

            if (parameters != null)
                formattedParameters = parameters.Select(parameter => $"{parameter.Type.GetFullyQualifiedName()} {parameter.Name}");
            if (typeArguments != null)
                formattedTypeArguments = typeArguments.Select(arg => $"<{arg.GetFullyQualifiedName()}>");

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

        public static InteropTypeKind DetermineTypeKind(ReinteropGenerationContext context, ITypeSymbol type)
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
        public static bool IsBlittableStruct(ReinteropGenerationContext context, ITypeSymbol type, int depth = 0)
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
        /// Determines if a struct rewrite is required for a function with a given return type.
        /// See <see cref="RewriteStructReturn"/>.
        /// </summary>
        public static bool NeedsStructReturnRewrite(CSharpType returnType)
        {
            // All blittable structs require rewrite.
            if (returnType.Kind == InteropTypeKind.BlittableStruct)
                return true;

            // If it's not a blittable struct and not a nullable, it doesn't need rewrite.
            if (returnType.Kind != InteropTypeKind.Nullable)
                return false;
            
            // Only nullables of blittable structs and primitives require rewrite.
            // Because a nullable reference type can be accomodated by our normal interop approach.
            CSharpType interopReturnType = returnType.AsInteropTypeReturn();
            return interopReturnType.Kind == InteropTypeKind.BlittableStruct ||
                   interopReturnType.Kind == InteropTypeKind.Primitive;
        }

        public static string MakeSafeIdentifier(string s)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < s.Length; ++i)
            {
                char c = s[i];
                if (char.IsLetterOrDigit(c) || c == '_')
                    result.Append(c);
                else
                    result.Append(char.ConvertToUtf32(s, i).ToString());
            }

            return result.ToString();
        }
    }
}
