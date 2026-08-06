using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Xml.Linq;

namespace Reinterop
{
    internal class Constructors
    {
        public static void Generate(CppGenerationContext context, TypeToGenerate item, GeneratedResult result)
        {
            if (item.Type.IsStatic)
                GenerateStatic(context, item, result);
            else
                GenerateNonStatic(context, item, result);
        }

        private static void GenerateStatic(CppGenerationContext context, TypeToGenerate item, GeneratedResult result)
        {
            GeneratedCppDeclaration declaration = result.CppDeclaration;

            // Delete the default constructor so this static class can't be constructed.
            declaration.Elements.Add(new(
                Content: $"{declaration.Type.Name}() = delete;",
                IsPrivate: false
            ));
        }

        private static void GenerateNonStatic(CppGenerationContext context, TypeToGenerate item, GeneratedResult result)
        {
            foreach (IMethodSymbol constructor in item.Constructors)
            {
                GenerateSingleNonStatic(context, item, result, constructor);
            }
        }

        private static void GenerateSingleNonStatic(CppGenerationContext context, TypeToGenerate item, GeneratedResult result, IMethodSymbol constructor)
        {
            GeneratedCppDeclaration declaration = result.CppDeclaration;
            GeneratedCppDefinition definition = result.CppDefinition;
            GeneratedInit init = result.Init;

            var parameters = constructor.Parameters
                .Select(parameter => new CppInteropParameter(parameter.Name, CppType.FromCSharp(context, parameter.Type).AsParameterType()))
                .ToArray();
            string interopFunctionName = $"Construct_{Interop.HashParameters(constructor.Parameters)}";
            CppInteropFunction recipe = new(context, interopFunctionName, parameters, declaration.Type);

            string templateSpecialization = Interop.GetTemplateSpecialization(declaration.Type);

            // A private, static field of function pointer type that will call
            // into a managed delegate for this constructor, initialized at startup.
            var (csName, csContent) = Interop.CreateCSharpDelegateInit(context, item.Type, constructor, interopFunctionName);
            recipe.AddInteropFunctionPointer(result, $"{definition.Type.Name}{templateSpecialization}", csName, csContent);

            // For blittable structs, add static "Construct" functions rather than C++ constructors.
            // This way we can use default construction and member initialization and avoid a call into C# to
            // construct simple blittable types, but can still call explicit C# constructors when necessary.
            if (declaration.Type.Kind == InteropTypeKind.BlittableStruct)
            {
                // Constructor declaration
                declaration.Elements.Add(new(
                    Content: $"static {declaration.Type.Name} Construct({recipe.ParameterListDeclaration()});",
                    TypeDeclarationsReferenced: recipe.ParameterTypes
                ));

                // Constructor definition
                IReadOnlyList<CppStatement> body = recipe.Body(outParameterTypeName: definition.Type.Name)!;
                definition.Elements.Add(new(
                    Content:
                        $$"""
                        {{definition.Type.Name}} {{definition.Type.Name}}{{templateSpecialization}}::Construct({{recipe.ParameterListDeclaration()}})
                        {
                            {{new[] { CppPrinter.Print(body) }.JoinAndIndent("    ")}}
                        }
                        """,
                    TypeDefinitionsReferenced: new[]
                    {
                        definition.Type,
                        recipe.InteropReturnType,
                        CppObjectHandle.GetCppType(context),
                        CppReinteropException.GetCppType(context)
                    }.Concat(recipe.ParameterTypes)
                ));
            }
            else
            {
                // Constructor declaration
                declaration.Elements.Add(new(
                    Content: $"{declaration.Type.Name}({recipe.ParameterListDeclaration()});",
                    TypeDeclarationsReferenced: recipe.ParameterTypes
                ));

                // Constructor definition
                IReadOnlyList<CppArgument> callArguments = recipe.CallArguments();
                IReadOnlyList<CppStatement> body = CppInterop.CallManagedFunction(
                    new CppIdentifier(interopFunctionName), callArguments,
                    resultTypeName: "void*",
                    returnExpression: new CppRaw("handle"),
                    resultVariableName: "handle");
                definition.Elements.Add(new(
                    Content:
                        $$"""
                        {{definition.Type.Name}}{{templateSpecialization}}::{{definition.Type.Name}}({{recipe.ParameterListDeclaration()}})
                            : _handle([&]() mutable {
                                {{new[] { CppPrinter.Print(body) }.JoinAndIndent("        ")}}
                            }())
                        {
                        }
                        """,
                    TypeDefinitionsReferenced: new[]
                    {
                        definition.Type,
                        recipe.InteropReturnType,
                        CppObjectHandle.GetCppType(context),
                        CppReinteropException.GetCppType(context)
                    }.Concat(recipe.ParameterTypes)
                ));
            }
        }
    }
}
