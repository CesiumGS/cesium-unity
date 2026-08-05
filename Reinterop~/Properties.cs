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
            GeneratedCppDefinition definition = result.CppDefinition;

            CppType returnType = CppType.FromCSharp(context, method.ReturnType).AsReturnType();
            CppInteropParameter[] parameters = method.Parameters
                .Select(parameter => new CppInteropParameter(parameter.Name, CppType.FromCSharp(context, parameter.Type).AsParameterType()))
                .ToArray();

            // If this is an instance method, pass the current object as the first (implicit "thiz") parameter.
            CppType? instanceType = method.IsStatic ? null : result.CppDefinition.Type.AsParameterType();
            CppInteropFunction recipe = new(context, $"Property_{method.Name}", parameters, returnType, instanceType);

            // A private, static field of function pointer type that will call
            // into a managed delegate for this method, initialized at startup.
            var (csName, csContent) = Interop.CreateCSharpDelegateInit(context, item.Type, method, $"Property_{method.Name}");
            recipe.AddToGeneration(result, definition.Type.GetFullyQualifiedName(false), csName, csContent);

            string propertyName = property.Name;
            if (property.IsIndexer)
                propertyName = "operator[]";

            string typeTemplateSpecialization = "";
            if (definition.Type.GenericArguments != null && definition.Type.GenericArguments.Count > 0)
            {
                typeTemplateSpecialization = "<" + string.Join(", ", definition.Type.GenericArguments.Select(t => t.GetFullyQualifiedName())) + ">";
            }

            IReadOnlyList<CppStatement> body = recipe.Body(new CppIdentifier($"Property_{method.Name}"))!;
            recipe.AddFunction(result, propertyName, body, method.IsStatic, typeTemplateSpecialization);
        }
    }
}
