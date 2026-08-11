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
            // Delete the default constructor so this static class can't be constructed.
            result.InteropFunctions.Add(
                new CppInteropFunction(context, result.CppDefinition.Type, item.Type.Name).Deleted(true));
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

            // For blittable structs, add static "Construct" functions rather than C++ constructors.
            // This way we can use default construction and member initialization and avoid a call into C# to
            // construct simple blittable types, but can still call explicit C# constructors when necessary.
            string functionName = declaration.Type.Kind == InteropTypeKind.BlittableStruct ? "Construct" : declaration.Type.Name;

            CppInteropFunction recipe = new CppInteropFunction(context, result.CppDefinition.Type, functionName)
                .Parameters(constructor.Parameters)
                .ReturnType(declaration.Type)
                .Static(true)
                .CSharp(item.Type, constructor);

            if (declaration.Type.Kind == InteropTypeKind.BlittableStruct)
            {
                result.InteropFunctions.Add(recipe);
            }
            else
            {
                IReadOnlyList<CppStatement> lambdaBody = CppInterop.CallManagedFunction(
                    new CppIdentifier(recipe.FunctionPointerName), recipe.CallArguments(),
                    resultTypeName: "void*",
                    returnExpression: new CppRaw("handle"),
                    resultVariableName: "handle");

                recipe
                    .MemberInitializers([
                        new CppMemberInitializer("_handle", new CppRaw($"[&]() mutable {{ {CppPrinter.Print(lambdaBody)} }}()"))
                    ])
                    .DefinitionBody([]);

                result.InteropFunctions.Add(recipe);
            }
        }
    }
}
