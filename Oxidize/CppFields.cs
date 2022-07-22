using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Oxidize
{
    internal class CppFields
    {
        public static void GenerateFields(CppGenerationContext context, GenerationItem item, TypeDefinition definition)
        {
            CppType cppType = CppType.FromCSharp(context, item.type);

            ImmutableArray<ISymbol> members = item.type.GetMembers();
            foreach (ISymbol member in members)
            {
                IFieldSymbol? field = member as IFieldSymbol;
                if (field == null)
                    continue;

                GenerateField(context, item.type, cppType, field, definition);
            }
        }

        private static void GenerateField(CppGenerationContext context, ITypeSymbol type, CppType cppType, IFieldSymbol field, TypeDefinition definition)
        {
            string modifier = field.IsStatic ? "static " : "";

            // The order of fields is important, so always put them in the
            // public section and prefix them with the accessibility if it's different.
            bool isPrivate = field.DeclaredAccessibility != Accessibility.Public;
            if (isPrivate)
                modifier = "private: " + modifier;

            CppType fieldType = CppType.FromCSharp(context, field.Type);
            fieldType.AddHeaderIncludesToSet(definition.headerIncludes);
            fieldType.AddSourceIncludesToSet(definition.cppIncludes);
            fieldType.AddForwardDeclarationsToSet(definition.forwardDeclarations);

            string fieldDeclaration = $"{modifier}{fieldType.GetFullyQualifiedName()} {field.Name};";
            definition.declarations.Add(fieldDeclaration);

            // Switch back to the proper accessibility if necessary.
            if (isPrivate)
                definition.declarations.Add("public:");
        }
    }
}
