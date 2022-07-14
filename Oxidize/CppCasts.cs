using System;
using System.Collections.Generic;
using System.Text;

namespace Oxidize
{
    internal class CppCasts
    {
        public static void GenerateDowncasts(CppGenerationContext context, GenerationItem item, TypeDefinition definition)
        {
            CppType itemType = CppType.FromCSharp(context, item.type);
            string typeName = itemType.GetFullyQualifiedName();

            // Generate implicit conversions to all base classes.
            GenerationItem? baseClass = item.baseClass;
            while (baseClass != null)
            {
                CppType baseType = CppType.FromCSharp(context, baseClass.type);
                string baseTypeName = baseType.GetFullyQualifiedName();
                definition.declarations.Add($"operator {baseTypeName}() const;");
                definition.definitions.Add(
                    $$"""
                    {{typeName}}::operator {{baseTypeName}}() const {
                        return {{baseTypeName}}(::Oxidize::ObjectHandle(this->_handle));
                    }
                    """);
                baseClass = baseClass.baseClass;
            }

            // Generate implicit conversions to all interfaces.
            foreach (GenerationItem anInterface in item.interfaces)
            {
                CppType interfaceType = CppType.FromCSharp(context, anInterface.type);
                string interfaceTypeName = interfaceType.GetFullyQualifiedName();
                definition.declarations.Add($"operator {interfaceTypeName}() const;");
                definition.definitions.Add(
                    $$"""
                    {{typeName}}::operator {{interfaceTypeName}}() const {
                        return {{interfaceTypeName}}(::Oxidize::ObjectHandle(this->_handle));
                    }
                    """);
            }
        }

        public static void GenerateUpcasts(CppGenerationContext options, GenerationItem item, TypeDefinition definition)
        {
            // TODO
        }
    }
}
