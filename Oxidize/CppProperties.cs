using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxidize
{
    internal class CppProperties
    {
        public static void GenerateProperties(CppGenerationContext context, GenerationItem item, TypeDefinition definition)
        {
            CppType itemType = CppType.FromCSharp(context, item.type);
            string typeName = itemType.GetFullyQualifiedName();

            foreach (IPropertySymbol property in item.properties)
            {
                GenerateProperty(context, typeName, property, definition);
            }
        }

        public static void GenerateProperty(CppGenerationContext context, string typeName, IPropertySymbol property, TypeDefinition definition)
        {
            string modifiers = "";
            if (property.IsStatic)
            {
                modifiers += "static ";
            }

            if (property.GetMethod != null)
            {
                CppType returnType = CppType.FromCSharp(context, property.Type).AsReturnType();
                definition.declarations.Add($"{modifiers}{returnType.GetFullyQualifiedName()} {property.Name}() const;");
                returnType.AddHeaderIncludesToSet(definition.headerIncludes);
                returnType.AddSourceIncludesToSet(definition.cppIncludes);
                returnType.AddForwardDeclarationsToSet(definition.forwardDeclarations);
            }

            if (property.SetMethod != null)
            {
                CppType valueType = CppType.FromCSharp(context, property.Type).AsParameterType();
                definition.declarations.Add($"{modifiers}void {property.Name}({valueType.GetFullyQualifiedName()} value);");
                valueType.AddHeaderIncludesToSet(definition.headerIncludes);
                valueType.AddSourceIncludesToSet(definition.cppIncludes);
                valueType.AddForwardDeclarationsToSet(definition.forwardDeclarations);
            }
        }
    }
}
