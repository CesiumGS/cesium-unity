using Microsoft.CodeAnalysis;

namespace Reinterop
{
    internal class CppHandleManagement
    {
        internal static void Generate(CppGenerationContext context, TypeToGenerate item, GeneratedResult result)
        {
            GeneratedCppDeclaration declaration = result.CppDeclaration;
            GeneratedCppDefinition definition = result.CppDefinition;

            // We only need handle management for non-static classes.
            if (item.Type.IsStatic)
                return;

            // Only classes, delegates, and non-blittable struct wrappers have handles.
            if (declaration.Type.Kind != InteropTypeKind.ClassWrapper &&
                declaration.Type.Kind != InteropTypeKind.NonBlittableStructWrapper &&
                declaration.Type.Kind != InteropTypeKind.Delegate)
            {
                return;
            }

            CppType type = CppType.FromCSharp(context, item.Type);
            CppType objectHandleType = CppObjectHandle.GetCppType(context);

            string templateSpecialization = "";
            if (declaration.Type.GenericArguments != null && declaration.Type.GenericArguments.Count > 0)
            {
                templateSpecialization = $"<{string.Join(", ", declaration.Type.GenericArguments.Select(arg => arg.GetFullyQualifiedName()))}>";
            }

            // The handle to the managed object
            declaration.Elements.Add(new(
                Content: $"{objectHandleType.GetFullyQualifiedName()} _handle;",
                IsPrivate: true,
                TypeDefinitionsReferenced: new[] { objectHandleType }
            ));

            // Construct from an object handle
            declaration.Elements.Add(new(
                Content: $"explicit {type.Name}({objectHandleType.GetFullyQualifiedName()}&& handle) noexcept;",
                TypeDeclarationsReferenced: new[] { objectHandleType }
            ));

            definition.Elements.Add(new(
                Content:
                    $$"""
                    {{type.Name}}{{templateSpecialization}}::{{type.Name}}({{objectHandleType.GetFullyQualifiedName()}}&& handle) noexcept :
                        _handle(std::move(handle)) {}
                    """,
                AdditionalIncludes: new[] { "<utility> " }, // for std::move
                TypeDefinitionsReferenced: new[]
                {
                    objectHandleType,
                    result.CppDefinition.Type
                }
            ));

            // Construct from a null reference
            declaration.Elements.Add(new(
                Content: $"{type.Name}(std::nullptr_t) noexcept;",
                TypeDeclarationsReferenced: new[] { CppType.NullPointer }
            ));

            definition.Elements.Add(new(
                Content:
                    $$"""
                    {{type.Name}}{{templateSpecialization}}::{{type.Name}}(std::nullptr_t) noexcept : _handle(nullptr) {
                    }
                    """,
                TypeDefinitionsReferenced: new[]
                {
                    result.CppDefinition.Type,
                    CppType.NullPointer,
                }
            ));

            bool hasOverloadedOperatorEquals = CSharpTypeUtility
                .FindMembers(item.Type, "op_Equality")
                .Where(
                    op => op is IMethodSymbol method &&
                    method.ReturnType.SpecialType == SpecialType.System_Boolean &&
                    method.Parameters.Length == 2 &&
                    SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, method.ContainingType) &&
                    SymbolEqualityComparer.Default.Equals(method.Parameters[1].Type, method.ContainingType))
                .Any();

            // For simple types without an overloaded operator==, we can check
            // to see if a wrapper represents a null reference without leaving
            // C++ land.
            //
            // But if such an operator does exist, we have to use it, even if
            // that means a call into C#.
            if (!hasOverloadedOperatorEquals)
            {
                declaration.Elements.Add(new(
                    Content: $"bool operator==(std::nullptr_t) const noexcept;",
                    TypeDeclarationsReferenced: new[] { CppType.NullPointer }
                ));
                definition.Elements.Add(new(
                    Content:
                        $$"""
                    bool {{type.Name}}{{templateSpecialization}}::operator==(std::nullptr_t) const noexcept {
                        return this->_handle.GetRaw() == nullptr;
                    }
                    """,
                    TypeDefinitionsReferenced: new[] { result.CppDefinition.Type, objectHandleType }
                ));

                declaration.Elements.Add(new(
                    Content: $"bool operator!=(std::nullptr_t) const noexcept;",
                    TypeDeclarationsReferenced: new[] { CppType.NullPointer }
                ));
                definition.Elements.Add(new(
                    Content:
                        $$"""
                    bool {{type.Name}}{{templateSpecialization}}::operator!=(std::nullptr_t) const noexcept {
                        return this->_handle.GetRaw() != nullptr;
                    }
                    """,
                    TypeDefinitionsReferenced: new[] { result.CppDefinition.Type, objectHandleType }
                ));
            }

            // Get handle
            declaration.Elements.Add(new(
                Content: $"const {objectHandleType.GetFullyQualifiedName()}& GetHandle() const;",
                TypeDeclarationsReferenced: new[] { objectHandleType }
            ));
            declaration.Elements.Add(new(
                Content: $"{objectHandleType.GetFullyQualifiedName()}& GetHandle();",
                TypeDeclarationsReferenced: new[] { objectHandleType }
            ));

            definition.Elements.Add(new(
                Content:
                    $$"""
                    const {{objectHandleType.GetFullyQualifiedName()}}& {{type.Name}}{{templateSpecialization}}::GetHandle() const {
                        return this->_handle;
                    }
                    """,
                TypeDefinitionsReferenced: new[] { result.CppDefinition.Type }
            ));
            definition.Elements.Add(new(
                Content:
                    $$"""
                    {{objectHandleType.GetFullyQualifiedName()}}& {{type.Name}}{{templateSpecialization}}::GetHandle() {
                        return this->_handle;
                    }
                    """,
                TypeDefinitionsReferenced: new[] { result.CppDefinition.Type }
            ));
        }
    }
}
