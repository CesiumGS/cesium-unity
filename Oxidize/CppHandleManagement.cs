using System;
using System.Collections.Generic;
using System.Text;

namespace Oxidize
{
    internal class CppHandleManagement
    {
        internal static void Generate(CppGenerationContext context, GenerationItem item, GeneratedResult result)
        {
            GeneratedCppDeclaration declaration = result.CppDeclaration;
            GeneratedCppDefinition definition = result.CppDefinition;

            // We only need handle management for non-static classes.
            if (item.type.IsStatic || declaration.Type.Kind != CppTypeKind.ClassWrapper)
                return;

            CppType objectHandleType = CppObjectHandle.GetCppType(context);

            // The handle to the managed object
            declaration.Elements.Add(new(
                Content: $"{objectHandleType.GetFullyQualifiedName()} _handle;",
                IsPrivate: true,
                TypeDefinitionsReferenced: new[] { objectHandleType }
            ));

            // Construct from an object handle
            declaration.Elements.Add(new(
                Content: $"explicit {item.type.Name}({objectHandleType.GetFullyQualifiedName()}&& handle) noexcept;",
                TypeDeclarationsReferenced: new[] { objectHandleType }
            ));

            definition.Elements.Add(new(
                Content:
                    $$"""
                    {{item.type.Name}}::{{item.type.Name}}({{objectHandleType.GetFullyQualifiedName()}}&& handle) noexcept :
                        _handle(std::move(handle)) {}
                    """,
                AdditionalIncludes: new[] { "<utility> "}, // for std::move
                TypeDefinitionsReferenced: new[]
                {
                    objectHandleType,
                    result.CppDefinition.Type
                }
            ));

            // Construct from a null reference
            declaration.Elements.Add(new(
                Content: $"{item.type.Name}(std::nullptr_t) noexcept;",
                TypeDeclarationsReferenced: new[]  { CppType.NullPointer }
            ));

            definition.Elements.Add(new(
                Content:
                    $$"""
                    {{item.type.Name}}::{{item.type.Name}}(std::nullptr_t) noexcept : _handle(nullptr) {
                    }
                    """,
                TypeDefinitionsReferenced: new[]
                {
                    result.CppDefinition.Type,
                    CppType.NullPointer,
                }
            ));

            // Comparisons to null reference
            declaration.Elements.Add(new(
                Content: $"bool operator==(std::nullptr_t) const noexcept;",
                TypeDeclarationsReferenced: new[] { CppType.NullPointer }
            ));
            definition.Elements.Add(new(
                Content:
                    $$"""
                    bool {{item.type.Name}}::operator==(std::nullptr_t) const noexcept {
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
                    bool {{item.type.Name}}::operator!=(std::nullptr_t) const noexcept {
                        return this->_handle.GetRaw() != nullptr;
                    }
                    """,
                TypeDefinitionsReferenced: new[] { result.CppDefinition.Type, objectHandleType }
            ));

            // Get handle
            declaration.Elements.Add(new(
                Content: $"const {objectHandleType.GetFullyQualifiedName()}& GetHandle() const;",
                TypeDeclarationsReferenced: new[] { objectHandleType }
            ));

            definition.Elements.Add(new(
                Content:
                    $$"""
                    const {{objectHandleType.GetFullyQualifiedName()}}& {{item.type.Name}}::GetHandle() const {
                        return this->_handle;
                    }
                    """,
                TypeDefinitionsReferenced: new[] { result.CppDefinition.Type }
            ));
        }
    }
}
