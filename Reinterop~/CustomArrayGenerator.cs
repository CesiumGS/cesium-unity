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
            GeneratedCppDeclaration declaration = result.CppDeclaration;
            GeneratedCppDefinition definition = result.CppDefinition;
            GeneratedInit init = result.Init;

            string createBySizeName = $"Construct_Size";

            declaration.Elements.Add(new(
                Content: $"static void* (*{createBySizeName})(std::int32_t size);",
                IsPrivate: true
            ));
            declaration.Elements.Add(new(
                Content: $"{declaration.Type.Name}(std::int32_t size);",
                TypeDeclarationsReferenced: new[] { CppType.Int32 }
            ));

            string templateSpecialization = Interop.GetTemplateSpecialization(declaration.Type);

            definition.Elements.Add(new(
                Content: $"void* (*{definition.Type.Name}{templateSpecialization}::{createBySizeName})(std::int32_t) = nullptr;"
            ));
            definition.Elements.Add(new(
                Content:
                    $$"""
                    {{definition.Type.Name}}{{templateSpecialization}}::{{definition.Type.Name}}(std::int32_t size)
                      : _handle({{createBySizeName}}(size))
                    {
                    }
                    """
            ));

            CSharpType csType = CSharpType.FromSymbol(context, arrayType);
            string baseName = $"{Interop.GetUniqueNameForType(csType)}_Constructor_Size";
            init.Functions.Add(new(
                CppName: $"{definition.Type.GetFullyQualifiedName()}::{createBySizeName}",
                CppTypeSignature: $"void* (*)(std::int32_t size)",
                CppTypeDefinitionsReferenced: new[] { definition.Type },
                CppTypeDeclarationsReferenced: new[] { CppType.Int32 },
                CSharpName: baseName + "Delegate",
                CSharpContent:
                    $$"""
                    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
                    private unsafe delegate IntPtr {{baseName}}Type(System.Int32 size);
                    private static unsafe readonly {{baseName}}Type {{baseName}}Delegate = new {{baseName}}Type({{baseName}});
                    [AOT.MonoPInvokeCallback(typeof({{baseName}}Type))]
                    private static unsafe IntPtr {{baseName}}(System.Int32 size)
                    {
                        var result = new {{arrayType.ElementType.ToDisplayString()}}[size];
                        return {{csType.GetConversionToInteropType("result")}};
                    }
                    """
            ));
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

            GeneratedCppDeclaration declaration = result.CppDeclaration;
            GeneratedCppDefinition definition = result.CppDefinition;
            GeneratedInit init = result.Init;

            CppType elementType = CppType.FromCSharp(context, arrayType.ElementType);
            CppType elementTypeParameter = elementType.AsParameterType();
            CppType elementInteropTypeParameter = elementTypeParameter.AsInteropType();

            string setItem = $"SetItem";

            declaration.Elements.Add(new(
                Content: $"static void (*{setItem})(void* thiz, std::int32_t index, {elementInteropTypeParameter.GetFullyQualifiedName()} value);",
                IsPrivate: true,
                TypeDeclarationsReferenced: new[] { CppType.Int32, elementInteropTypeParameter }
            ));
            declaration.Elements.Add(new(
                Content: $"void Item(std::int32_t index, {elementTypeParameter.GetFullyQualifiedName()} value);",
                TypeDeclarationsReferenced: new[] { CppType.Int32, elementTypeParameter }
            ));

            string templateSpecialization = Interop.GetTemplateSpecialization(declaration.Type);

            definition.Elements.Add(new(
                Content: $"void (*{definition.Type.Name}{templateSpecialization}::{setItem})(void* thiz, std::int32_t, {elementInteropTypeParameter.GetFullyQualifiedName()} value) = nullptr;"
            ));
            definition.Elements.Add(new(
                Content:
                    $$"""
                    void {{definition.Type.Name}}{{templateSpecialization}}::Item(std::int32_t index, {{elementTypeParameter.GetFullyQualifiedName()}} value) {
                      {{setItem}}({{definition.Type.AsParameterType().GetConversionToInteropType(context, "(*this)")}}, index, {{elementTypeParameter.GetConversionToInteropType(context, "value")}});
                    }
                    """
            ));

            CSharpType csType = CSharpType.FromSymbol(context, arrayType);
            CSharpType csElementType = CSharpType.FromSymbol(context, arrayType.ElementType);
            CSharpType csElementInteropType = csElementType.AsInteropTypeParameter();

            string baseName = $"{Interop.GetUniqueNameForType(csType)}_SetItem";
            init.Functions.Add(new(
                CppName: $"{definition.Type.GetFullyQualifiedName()}::{setItem}",
                CppTypeSignature: $"void (*)(void*, std::int32_t, {elementInteropTypeParameter.GetFullyQualifiedName()})",
                CppTypeDefinitionsReferenced: new[] { definition.Type, CppType.Int32, elementInteropTypeParameter },
                CSharpName: baseName + "Delegate",
                CSharpContent:
                    $$"""
                    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
                    private unsafe delegate void {{baseName}}Type(System.IntPtr thiz, System.Int32 index, {{csElementInteropType.GetFullyQualifiedName()}} value);
                    private static unsafe readonly {{baseName}}Type {{baseName}}Delegate = new {{baseName}}Type({{baseName}});
                    [AOT.MonoPInvokeCallback(typeof({{baseName}}Type))]
                    private static unsafe void {{baseName}}(System.IntPtr thiz, System.Int32 index, {{csElementInteropType.GetFullyQualifiedName()}} value)
                    {
                        ({{csType.GetParameterConversionFromInteropType("thiz")}})[index] = {{csElementType.GetParameterConversionFromInteropType("value")}};
                    }
                    """
            ));

        }
    }
}
