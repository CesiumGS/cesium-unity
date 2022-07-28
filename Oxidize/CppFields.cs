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

        public static void Generate(CppGenerationContext context, GenerationItem item, GeneratedResult result)
        {
            // Only blittable value types need fields.
            if (result.CppDeclaration.Type.Kind != CppTypeKind.BlittableStruct)
                return;

            ImmutableArray<ISymbol> members = item.type.GetMembers();
            foreach (ISymbol member in members)
            {
                IFieldSymbol? field = member as IFieldSymbol;
                if (field == null)
                    continue;

                GenerateField(context, item, field, result);
            }
        }

        private static void GenerateField(CppGenerationContext context, GenerationItem item, IFieldSymbol field, GeneratedResult result)
        {
            // Skip static fields
            // TODO: Implement these as functions that call into the C#?
            if (field.IsStatic)
                return;

            CppType fieldType = CppType.FromCSharp(context, field.Type);

            result.CppDeclaration.Elements.Add(new(
                Content: $"{fieldType.GetFullyQualifiedName()} {field.Name};",
                IsPrivate: field.DeclaredAccessibility != Accessibility.Public,
                TypeDefinitionsReferenced: new[] { fieldType }));
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
