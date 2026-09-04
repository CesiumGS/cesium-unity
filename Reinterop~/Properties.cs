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
            CSharpFunctionCallableFromCpp recipe = new CSharpFunctionCallableFromCpp(context, item.Type)
                .Name(propertyName)
                .Parameters(method.Parameters)
                .ReturnType(method.ReturnType)
                .Static(property.IsStatic)
                .Body(new CSharpFunctionCallableFromCpp.CSharpBodyInvokePropertyAccessor(property, method.MethodKind == MethodKind.PropertyGet));

            result.InteropFunctions2.Add(recipe);
        }
    }
}
