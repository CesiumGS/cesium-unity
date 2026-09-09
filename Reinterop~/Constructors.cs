using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Xml.Linq;

namespace Reinterop
{
    internal class Constructors
    {
        public static void Generate(ReinteropGenerationContext context, TypeToGenerate item, GeneratedResult result)
        {
            if (item.Type.IsStatic)
                GenerateStatic(context, item, result);
            else
                GenerateNonStatic(context, item, result);
        }

        private static void GenerateStatic(ReinteropGenerationContext context, TypeToGenerate item, GeneratedResult result)
        {
            // Delete the default constructor so this static class can't be constructed.
            result.ExtraCppFunctions.Add(
                new CppFunction(context, result.CppDefinition.Type, item.Type.Name).Deleted(true));
        }

        private static void GenerateNonStatic(ReinteropGenerationContext context, TypeToGenerate item, GeneratedResult result)
        {
            foreach (IMethodSymbol constructor in item.Constructors)
            {
                GenerateSingleNonStatic(context, item, result, constructor);
            }
        }

        private static void GenerateSingleNonStatic(ReinteropGenerationContext context, TypeToGenerate item, GeneratedResult result, IMethodSymbol constructor)
        {
            // Create a static "Construct" function that calls this constructor.
            // For blittable structs, this will be public and it will be the only way to invoke this constructor.
            // For other types, this will be private and the public C++ constructor will call it.
            // We don't add constructors to blittable types so that we can use the default constructor and
            // member initialization to construct them without calling into C#.
            CSharpFunctionCallableFromCpp recipe = new CSharpFunctionCallableFromCpp(context, item.Type)
                .Name("Construct")
                .Parameters(constructor.Parameters)
                .ReturnType(item.Type)
                .Static(true)
                .Body(new CSharpBodyConstructInstance())
                .Private(result.Type.Kind != InteropTypeKind.BlittableStruct);

            result.InteropFunctions.Add(recipe);

            CppType cppType = result.Type;
            if (cppType.Kind != InteropTypeKind.BlittableStruct)
            {
                // The actual C++ constructor, which calls the Construct method.
                CppFunction constructorRecipe = new CppFunction(context, result.Type, cppType.Name)
                    .Parameters(constructor.Parameters)
                    .ReturnType(item.Type)
                    .Static(true)
                    .DefinitionBody([])
                    .MemberInitializers([
                        new CppMemberInitializer(
                            cppType.Name,
                            new CppCall(
                                new CppIdentifier(recipe.Name()!),
                                recipe.Parameters().Select(p => new CppIdentifier(p.Name)).ToArray()))
                    ]);
                result.ExtraCppFunctions.Add(constructorRecipe);
            }
        }
    }
}
