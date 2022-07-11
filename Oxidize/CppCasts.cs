using System;
using System.Collections.Generic;
using System.Text;

namespace Oxidize
{
    internal class CppCasts
    {
        public static void GenerateDowncasts(string baseNamespace, GenerationItem item, TypeDefinition definition)
        {
            string typeName = CppTypes.GetFullyQualifiedTypeName(baseNamespace, item.type);

            GenerationItem? baseClass = item.baseClass;
            while (baseClass != null)
            {
                string baseTypeName = CppTypes.GetFullyQualifiedTypeName(baseNamespace, baseClass.type);
                definition.declarations.Add($$"""operator {{baseTypeName}}() const;""");
                definition.definitions.Add(
                    $$"""
                    {{typeName}}::operator {{baseTypeName}}() const {
                        return {{baseTypeName}}(this->_handle);
                    }
                    """);
                baseClass = baseClass.baseClass;
            }

            foreach (GenerationItem anInterface in item.interfaces)
            {
                string interfaceTypeName = CppTypes.GetFullyQualifiedTypeName(baseNamespace, anInterface.type);
                definition.declarations.Add($$"""operator {{interfaceTypeName}}() const;""");
                definition.definitions.Add(
                    $$"""
                    {{typeName}}::operator {{interfaceTypeName}}() const {
                        return {{interfaceTypeName}}(this->_handle);
                    }
                    """);
            }
        }
    }
}
