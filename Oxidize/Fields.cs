using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Oxidize
{
    internal class Fields
    {
        public static void Generate(CppGenerationContext context, GenerationItem item, GeneratedResult result)
        {
            // Only blittable value types need fields.
            if (result.CppDeclaration.Type.Kind != CppTypeKind.BlittableStruct)
                return;

            ImmutableArray<ISymbol> members = item.Type.GetMembers();
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
    }
}
