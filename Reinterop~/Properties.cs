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

            // If this is an instance method, pass the current object as the first (implicit "thiz") parameter.
            CppType? instanceType = method.IsStatic ? null : result.CppDefinition.Type.AsParameterType();
            CppInteropFunction recipe = new CppInteropFunction(context, $"Property_{method.Name}", parameters, returnType, instanceType);

            string propertyName = property.IsIndexer ? "operator[]" : property.Name;

            var (csName, csContent) = Interop.CreateCSharpDelegateInit(context, item.Type, method, $"Property_{method.Name}");
            recipe.AddToGeneration(result, propertyName, csName, csContent, recipe.Body());
        }
    }
}
