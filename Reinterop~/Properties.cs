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
            GeneratedCppDeclaration declaration = result.CppDeclaration;
            GeneratedCppDefinition definition = result.CppDefinition;
            GeneratedInit init = result.Init;

            CppType returnType = CppType.FromCSharp(context, method.ReturnType).AsReturnType();
            CppInteropParameter[] parameters = method.Parameters
                .Select(parameter => new CppInteropParameter(parameter.Name, CppType.FromCSharp(context, parameter.Type).AsParameterType()))
                .ToArray();

            // If this is an instance method, pass the current object as the first (implicit "thiz") parameter.
            CppType? instanceType = method.IsStatic ? null : result.CppDefinition.Type.AsParameterType();
            CppInteropFunction recipe = new(context, parameters, returnType, instanceType);

            // A private, static field of function pointer type that will call
            // into a managed delegate for this method.
            declaration.Elements.Add(new(
                Content: $"static {recipe.InteropReturnType.GetFullyQualifiedName()} (*Property_{method.Name})({recipe.InteropParameterListDeclaration()});",
                IsPrivate: true,
                TypeDeclarationsReferenced: new[] { recipe.InteropReturnType }.Concat(recipe.ParameterInteropTypes)
            ));

            definition.Elements.Add(new(
                Content: $"{recipe.InteropReturnType.GetFullyQualifiedName()} (*{definition.Type.GetFullyQualifiedName(false)}::Property_{method.Name})({recipe.InteropParameterListDeclaration()}) = nullptr;",
                TypeDeclarationsReferenced: new[] { recipe.InteropReturnType }.Concat(recipe.ParameterInteropTypes)
            ));

            // The static field should be initialized at startup.
            var (csName, csContent) = Interop.CreateCSharpDelegateInit(context, item.Type, method, $"Property_{method.Name}");
            init.Functions.Add(new(
                CppName: $"{definition.Type.GetFullyQualifiedName()}::Property_{method.Name}",
                CppTypeSignature: $"{recipe.InteropReturnType.GetFullyQualifiedName()} (*)({recipe.InteropParameterTypeList()})",
                CppTypeDefinitionsReferenced: new[] { definition.Type },
                CppTypeDeclarationsReferenced: new[] { recipe.InteropReturnType }.Concat(recipe.ParameterTypes),
                CSharpName: csName,
                CSharpContent: csContent
            ));

            string modifiers = "";
            string afterModifiers = "";
            if (method.IsStatic)
                modifiers += "static ";
            else
                afterModifiers += " const";

            string propertyName = property.Name;
            if (property.IsIndexer)
                propertyName = "operator[]";

            // Method declaration
            declaration.Elements.Add(new(
                Content: $"{modifiers}{returnType.GetFullyQualifiedName()} {propertyName}({recipe.ParameterListDeclaration()}){afterModifiers};",
                TypeDeclarationsReferenced: new[] { returnType }.Concat(recipe.ParameterTypes)
            ));

            string typeTemplateSpecialization = "";
            if (definition.Type.GenericArguments != null && definition.Type.GenericArguments.Count > 0)
            {
                typeTemplateSpecialization = "<" + string.Join(", ", definition.Type.GenericArguments.Select(t => t.GetFullyQualifiedName())) + ">";
            }

            // Method definition
            IReadOnlyList<CppStatement> body = recipe.Body(new CppIdentifier($"Property_{method.Name}"))!;
            definition.Elements.Add(new(
                Content:
                    $$"""
                    {{returnType.GetFullyQualifiedName()}} {{definition.Type.Name}}{{typeTemplateSpecialization}}::{{propertyName}}({{recipe.ParameterListDeclaration()}}){{afterModifiers}} {
                        {{new[] { CppPrinter.Print(body) }.JoinAndIndent("    ")}}
                    }
                    """,
                TypeDefinitionsReferenced: new[]
                {
                    definition.Type,
                    returnType,
                    CppObjectHandle.GetCppType(context),
                    CppReinteropException.GetCppType(context)
                }.Concat(recipe.ParameterTypes)
            ));
        }
    }
}
