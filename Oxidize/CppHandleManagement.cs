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

            // We only need handle management for non-static classes.
            if (item.type.IsStatic || declaration.Type.Kind != CppTypeKind.ClassWrapper)
                return;

            CppType objectHandleType = CppObjectHandle.GetCppType(context);

            // Construct from an object handle
            declaration.Elements.Add(new(
                content: $"explicit {item.type.Name}({objectHandleType.GetFullyQualifiedName()}&& handle) noexcept;",
                isPrivate: false,
                typesReferenced: new[] { new CppTypeReference(objectHandleType) }
            ));

            // Construct from a null reference
            declaration.Elements.Add(new(
                content: $"{item.type.Name}(std::nullptr_t) noexcept;",
                isPrivate: false,
                typesReferenced: new[]  { new CppTypeReference(CppType.NullPointer) }
            ));

            // Comparisons to null reference
            declaration.Elements.Add(new(
                content: $"bool operator==(std::nullptr_t) const noexcept;",
                isPrivate: false,
                typesReferenced: new[] { new CppTypeReference(CppType.NullPointer) }
            ));
            declaration.Elements.Add(new(
                content: $"bool operator!=(std::nullptr_t) const noexcept;",
                isPrivate: false,
                typesReferenced: new[] { new CppTypeReference(CppType.NullPointer) }
            ));

            // Get handle
            declaration.Elements.Add(new(
                content: $"const {objectHandleType.GetFullyQualifiedName()}& GetHandle() const;",
                isPrivate: false,
                typesReferenced: new[] { new CppTypeReference(objectHandleType) }
            ));
        }
    }
}
