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
            CppType returnType = CppType.FromCSharp(context, method.ReturnType).AsReturnType();
            CppInteropParameter[] parameters = method.Parameters
                .Select(parameter => new CppInteropParameter(parameter.Name, CppType.FromCSharp(context, parameter.Type).AsParameterType()))
                .ToArray();

            CppInteropFunction recipe = new CppInteropFunction(context, result.CppDefinition.Type, $"Property_{method.Name}")
                .Parameters(parameters)
                .ReturnType(returnType)
                .Static(property.IsStatic);

            string propertyName = property.IsIndexer ? "operator[]" : property.Name;

            var (csName, csContent) = Interop.CreateCSharpDelegateInit(context, item.Type, method, $"Property_{method.Name}");
            recipe.AddToGeneration(result, propertyName, csName, csContent, recipe.Body());
        }
    }
}
