using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;

namespace Reinterop
{
    internal class Fields
    {
        public static void Generate(CppGenerationContext context, TypeToGenerate mainItem, TypeToGenerate currentItem, GeneratedResult result)
        {
            if (result.CppDeclaration.Type.Kind == InteropTypeKind.BlittableStruct)
            {
                // For blittable value types, include every field (not just the used ones).
                // But don't generate fields for base classes.
                if (mainItem == currentItem)
                    GenerateEveryField(context, mainItem, result);
            }
            else
            {
                // For other types, generate accessors for the fields we actually need.
                GenerateFieldAccessors(context, mainItem, currentItem, result);
            }
        }

        private static void GenerateEveryField(CppGenerationContext context, TypeToGenerate item, GeneratedResult result)
        {
            ImmutableArray<ISymbol> members = item.Type.GetMembers();
            foreach (ISymbol member in members)
            {
                IFieldSymbol? field = member as IFieldSymbol;
                if (field == null)
                    continue;

                GenerateField(context, item, field, result);
            }
        }

        private static void GenerateField(CppGenerationContext context, TypeToGenerate item, IFieldSymbol field, GeneratedResult result)
        {
            // Skip static fields
            // TODO: Implement these as functions that call into the C#?
            if (field.IsStatic)
                return;
                
            // Skip any fields with an "Ignore" attribute.
            if (field.GetAttributes().Where(attrib => attrib.AttributeClass != null && (attrib.AttributeClass.Name == "IgnoreAttribute")).Any())
                return;

            string fieldName = CSharpTypeUtility.GetFieldName(field);
            bool isPrivate = CSharpTypeUtility.GetFieldIsPrivate(field);

            CppType fieldType = CppType.FromCSharp(context, field.Type);

            result.CppDeclaration.Elements.Add(new(
                Content: $"{fieldType.GetFullyQualifiedName()} {fieldName};",
                IsPrivate: isPrivate,
                TypeDefinitionsReferenced: new[] { fieldType }));
        }

        private static void GenerateFieldAccessors(CppGenerationContext context, TypeToGenerate mainItem, TypeToGenerate currentItem, GeneratedResult result)
        {
            foreach (IFieldSymbol field in currentItem.Fields)
            {
                GenerateSingleFieldAccessors(context, mainItem, field, result);
            }
        }

        private static void GenerateSingleFieldAccessors(CppGenerationContext context, TypeToGenerate item, IFieldSymbol field, GeneratedResult result)
        {
            CppType fieldType = CppType.FromCSharp(context, field.Type);
            CppType setType = fieldType.AsParameterType();
            CppType getType = fieldType.AsReturnType();

            CppType? instanceType = !field.IsStatic ? result.CppDefinition.Type.AsParameterType() : null;

            CppInteropFunction getRecipe = new(context, $"Field_get_{field.Name}", Array.Empty<CppInteropParameter>(), getType, instanceType);
            CppInteropFunction setRecipe = new(context, $"Field_set_{field.Name}", new[] { new CppInteropParameter("value", setType) }, CppType.Void, instanceType);

            var (getCsName, getCsContent) = Interop.CreateCSharpDelegateInit(context, item.Type, field, isGet: true);
            var (setCsName, setCsContent) = Interop.CreateCSharpDelegateInit(context, item.Type, field, isGet: false);

            getRecipe.AddToGeneration(result, field.Name, getCsName, getCsContent, getRecipe.Body());

            IReadOnlyList<CppStatement> setterBody = CppInterop.CallManagedFunction(
                new CppIdentifier($"Field_set_{field.Name}"), setRecipe.CallArguments());
            setRecipe.AddToGeneration(result, field.Name, setCsName, setCsContent, setterBody);
        }
    }
}
