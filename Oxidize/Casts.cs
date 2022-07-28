using System;
using System.Collections.Generic;
using System.Text;

namespace Oxidize
{
    internal class Casts
    {
        public static void Generate(CppGenerationContext context, GenerationItem item, GeneratedResult result)
        {
            // It only makes sense to cast instances, so static class need not apply.
            if (item.type.IsStatic)
                return;

            // Don't allow conversion of value types
            // TODO: but we could, by boxing them
            if (item.type.IsValueType)
                return;

            CppType objectHandleType = CppObjectHandle.GetCppType(context);

            // Generate implicit conversions to all base classes.
            GenerationItem? baseClass = item.baseClass;
            while (baseClass != null)
            {
                CppType baseType = CppType.FromCSharp(context, baseClass.type);

                string baseTypeName = baseType.GetFullyQualifiedName();

                result.CppDeclaration.Elements.Add(new(
                    Content: $"operator {baseTypeName}() const;",
                    TypeDeclarationsReferenced: new[] { baseType }));

                result.CppDefinition.Elements.Add(new(
                    Content:
                        $$"""
                        {{result.CppDefinition.Type.Name}}::operator {{baseTypeName}}() const {
                            return {{baseTypeName}}({{objectHandleType.GetFullyQualifiedName()}}(this->_handle));
                        }
                        """,
                    TypeDefinitionsReferenced: new[] { baseType, objectHandleType }));
                baseClass = baseClass.baseClass;
            }

            // Generate implicit conversions to all interfaces.
            foreach (GenerationItem anInterface in item.interfaces)
            {
                CppType interfaceType = CppType.FromCSharp(context, anInterface.type);
                string interfaceTypeName = interfaceType.GetFullyQualifiedName();

                result.CppDeclaration.Elements.Add(new(
                    Content: $"operator {interfaceTypeName}() const;",
                    TypeDeclarationsReferenced: new[] { interfaceType }));

                result.CppDefinition.Elements.Add(new(
                    Content:
                        $$"""
                        {{result.CppDefinition.Type.Name}}::operator {{interfaceTypeName}}() const {
                            return {{interfaceTypeName}}({{objectHandleType.GetFullyQualifiedName()}}(this->_handle));
                        }
                        """,
                    TypeDefinitionsReferenced: new[] { interfaceType, objectHandleType }));
            }
        }
    }
}
