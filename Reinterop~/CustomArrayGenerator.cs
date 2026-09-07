using Microsoft.CodeAnalysis;

namespace Reinterop
{
    internal class CustomArrayGenerator : ICustomGenerator
    {
        public IEnumerable<TypeToGenerate> GetDependencies(CppGenerationContext context)
        {
            yield break;
        }

        public GeneratedResult? Generate(CppGenerationContext context, TypeToGenerate type, GeneratedResult? generated)
        {
            // This generator only operates on arrays.
            if (generated == null || !(type.Type is IArrayTypeSymbol arrayType))
                return generated;

            GenerateSizeConstructor(context, type, generated, arrayType);
            GenerateItemMethod(context, type, generated, arrayType);

            return generated;
        }

        /// <summary>
        /// Add a constructor that can be used to create an array of a given size.
        /// </summary>
        private void GenerateSizeConstructor(CppGenerationContext context, TypeToGenerate item, GeneratedResult result, IArrayTypeSymbol arrayType)
        {
            CSharpFunctionCallableFromCpp functionRecipe = new CSharpFunctionCallableFromCpp(context, item.Type)
                .Name("Construct_Size")
                .Private(true)
                .Static(true)
                .Parameters([new CSharpParameter(CSharpType.FromSymbol(context, context.Compilation.GetSpecialType(SpecialType.System_Int32)), "size")])
                .ReturnType(item.Type)
                .Body(new CSharpBodyConstructArray());
            result.InteropFunctions2.Add(functionRecipe);

            CppFunction constructorRecipe = new CppFunction(context, result.Type, result.Type.Name)
                .Static(true)
                .Parameters([new CppParameter(CppType.Int32.AsParameterType(), "size")])
                .ReturnType(item.Type)
                .DefinitionBody([])
                .MemberInitializers([
                    new CppMemberInitializer(
                            result.Type.Name,
                            new CppCall(
                                new CppIdentifier(functionRecipe.Name()!),
                                functionRecipe.Parameters().Select(p => new CppIdentifier(p.Name)).ToList()))
                ]);
            result.InteropFunctions3.Add(constructorRecipe);
        }
 
        /// <summary>
        /// Add a method that can be used to assign a new value to an element of the array.
        /// </summary>
        private void GenerateItemMethod(CppGenerationContext context, TypeToGenerate item, GeneratedResult result, IArrayTypeSymbol arrayType)
        {
            // TODO: It would be nice to allow the user to use operator[] to assign a value to an array element.
            //       But to do that, we would need operator[] to return an object with an implicit conversion
            //       to the element type and an overloaded operator= to set the value. Here we take the
            //       simpler approach of adding an Item method instead.

            CSharpFunctionCallableFromCpp setItemRecipe = new CSharpFunctionCallableFromCpp(context, item.Type)
                .Name("Item")
                .Parameters([
                    new CSharpParameter(CSharpType.FromSymbol(context, context.Compilation.GetSpecialType(SpecialType.System_Int32)), "index"),
                    new CSharpParameter(CSharpType.FromSymbol(context, arrayType.ElementType), "value")
                ])
                .ReturnType(context.Compilation.GetSpecialType(SpecialType.System_Void))
                .Body(new CSharpBodySetArrayItem());
            result.InteropFunctions2.Add(setItemRecipe);
        }
    }
}
