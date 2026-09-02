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
            CSharpType csType = CSharpType.FromSymbol(context, arrayType);
            string baseName = $"{Interop.GetUniqueNameForType(csType)}_Constructor_Size";

            CppInteropFunction functionRecipe = new CppInteropFunction(context, result.Type, "Construct_Size")
                .Private(true)
                .Static(true)
                .Parameters([new CppInteropParameter("size", CppType.Int32)])
                .ReturnType(item.Type)
                .CSharp(
                    baseName + "Delegate",
                    $$"""
                    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
                    private unsafe delegate IntPtr {{baseName}}Type(System.Int32 size, System.IntPtr* reinteropException);
                    private static unsafe readonly {{baseName}}Type {{baseName}}Delegate = new {{baseName}}Type({{baseName}});
                    [AOT.MonoPInvokeCallback(typeof({{baseName}}Type))]
                    private static unsafe IntPtr {{baseName}}(System.Int32 size, System.IntPtr* reinteropException)
                    {
                        try
                        {
                            var result = new {{arrayType.ElementType.ToDisplayString()}}[size];
                            return {{csType.GetConversionToInteropType("result")}};
                        }
                        catch (Exception reinteropManagedException)
                        {
                            *reinteropException = Reinterop.ObjectHandleUtility.CreateHandle(reinteropManagedException);
                            return IntPtr.Zero;
                        }
                    }
                    """);
            result.InteropFunctions.Add(functionRecipe);

            CppInteropFunction constructorRecipe = new CppInteropFunction(context, result.Type, result.Type.Name)
                .Static(true)
                .Parameters([new CppInteropParameter("size", CppType.Int32.AsParameterType())])
                .ReturnType(item.Type)
                .DefinitionBody([])
                .MemberInitializers([
                    new CppMemberInitializer(
                            result.Type.Name,
                            new CppCall(
                                new CppIdentifier(functionRecipe.Name),
                                functionRecipe.Parameters().Select(p => new CppIdentifier(p.Name)).ToList()))
                ]);
            result.InteropFunctions.Add(constructorRecipe);
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

            CSharpType csElementType = CSharpType.FromSymbol(context, arrayType.ElementType);
            CppType elementType = CppType.FromCSharp(context, csElementType);
            CSharpType csType = CSharpType.FromSymbol(context, arrayType);
            CSharpType csElementInteropType = csElementType.AsInteropTypeParameter();
            string baseName = $"{Interop.GetUniqueNameForType(csType)}_SetItem";

            CppInteropFunction setItemRecipe = new CppInteropFunction(context, result.Type, "Item")
                .Parameters([
                    new CppInteropParameter("index", CppType.Int32.AsParameterType()),
                    new CppInteropParameter("value", elementType.AsParameterType())
                ])
                .ReturnType(CppType.Void)
                .CSharp(
                    baseName + "Delegate",
                    $$"""
                    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
                    private unsafe delegate void {{baseName}}Type(System.IntPtr thiz, System.Int32 index, {{csElementInteropType.GetFullyQualifiedName()}} value, System.IntPtr* reinteropException);
                    private static unsafe readonly {{baseName}}Type {{baseName}}Delegate = new {{baseName}}Type({{baseName}});
                    [AOT.MonoPInvokeCallback(typeof({{baseName}}Type))]
                    private static unsafe void {{baseName}}(System.IntPtr thiz, System.Int32 index, {{csElementInteropType.GetFullyQualifiedName()}} value, System.IntPtr* reinteropException)
                    {
                        try
                        {
                            ({{csType.GetParameterConversionFromInteropType("thiz")}})[index] = {{csElementType.GetParameterConversionFromInteropType("value")}};
                        }
                        catch (Exception reinteropManagedException)
                        {
                            *reinteropException = Reinterop.ObjectHandleUtility.CreateHandle(reinteropManagedException);
                        }
                    }
                    """
                );
            result.InteropFunctions.Add(setItemRecipe);
        }
    }
}
