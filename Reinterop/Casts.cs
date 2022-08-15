namespace Reinterop
{
    internal class Casts
    {
        public static void Generate(CppGenerationContext context, TypeToGenerate item, GeneratedResult result)
        {
            // It only makes sense to cast instances, so static class need not apply.
            if (item.Type.IsStatic)
                return;

            // Don't allow conversion of value types
            // TODO: but we could, by boxing them
            if (item.Type.IsValueType)
                return;

            GeneratedCppDeclaration declaration = result.CppDeclaration;
            if (declaration.Type.Kind != InteropTypeKind.ClassWrapper &&
                declaration.Type.Kind != InteropTypeKind.NonBlittableStructWrapper &&
                declaration.Type.Kind != InteropTypeKind.Delegate)
            {
                return;
            }

            CppType objectHandleType = CppObjectHandle.GetCppType(context);

            string templateSpecialization = "";
            if (declaration.Type.GenericArguments != null && declaration.Type.GenericArguments.Count > 0)
            {
                templateSpecialization = $"<{string.Join(", ", declaration.Type.GenericArguments.Select(arg => arg.GetFullyQualifiedName()))}>";
            }

            // Generate implicit conversions to all base classes.
            TypeToGenerate? baseClass = item.BaseClass;
            while (baseClass != null)
            {
                CppType baseType = CppType.FromCSharp(context, baseClass.Type);

                string baseTypeName = baseType.GetFullyQualifiedName();

                result.CppDeclaration.Elements.Add(new(
                    Content: $"operator {baseTypeName}() const;",
                    TypeDeclarationsReferenced: new[] { baseType }));

                result.CppDefinition.Elements.Add(new(
                    Content:
                        $$"""
                        {{result.CppDefinition.Type.Name}}{{templateSpecialization}}::operator {{baseTypeName}}() const {
                            return {{baseTypeName}}({{objectHandleType.GetFullyQualifiedName()}}(this->_handle));
                        }
                        """,
                    TypeDefinitionsReferenced: new[] { baseType, objectHandleType }));
                baseClass = baseClass.BaseClass;
            }

            // Generate implicit conversions to all interfaces.
            foreach (TypeToGenerate anInterface in item.Interfaces)
            {
                CppType interfaceType = CppType.FromCSharp(context, anInterface.Type);
                string interfaceTypeName = interfaceType.GetFullyQualifiedName();

                result.CppDeclaration.Elements.Add(new(
                    Content: $"operator {interfaceTypeName}() const;",
                    TypeDeclarationsReferenced: new[] { interfaceType }));

                result.CppDefinition.Elements.Add(new(
                    Content:
                        $$"""
                        {{result.CppDefinition.Type.Name}}{{templateSpecialization}}::operator {{interfaceTypeName}}() const {
                            return {{interfaceTypeName}}({{objectHandleType.GetFullyQualifiedName()}}(this->_handle));
                        }
                        """,
                    TypeDefinitionsReferenced: new[] { interfaceType, objectHandleType }));
            }
        }
    }
}
