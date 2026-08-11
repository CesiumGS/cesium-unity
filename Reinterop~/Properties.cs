using Microsoft.CodeAnalysis;

namespace Reinterop
{
    internal class Properties
    {
        public static void Generate(CppGenerationContext context, TypeToGenerate mainItem, TypeToGenerate currentItem, GeneratedResult result)
        {
            foreach (IPropertySymbol property in currentItem.Properties)
            {
                GenerateSingleProperty(context, mainItem, result, property);
            }
        }

        private static void GenerateSingleProperty(CppGenerationContext context, TypeToGenerate item, GeneratedResult result, IPropertySymbol property)
        {
            if (property.GetMethod != null)
                GenerateSingleMethod(context, item, result, property, property.GetMethod);

            // TODO: support element setters (i.e. obj[0] = x)
            if (property.SetMethod != null && !property.IsIndexer)
                GenerateSingleMethod(context, item, result, property, property.SetMethod);
        }

        private static void GenerateSingleMethod(CppGenerationContext context, TypeToGenerate item, GeneratedResult result, IPropertySymbol property, IMethodSymbol method)
        {
            string propertyName = property.IsIndexer ? "operator[]" : property.Name;
            CppInteropFunction recipe = new CppInteropFunction(context, result.CppDefinition.Type, propertyName)
                .Parameters(method.Parameters)
                .ReturnType(method.ReturnType)
                .Static(property.IsStatic);

            recipe
                .CSharpDelegateInit(Interop.CreateCSharpDelegateInit(context, item.Type, method, recipe.FunctionPointerName));

            result.InteropFunctions.Add(recipe);
        }
    }
}
