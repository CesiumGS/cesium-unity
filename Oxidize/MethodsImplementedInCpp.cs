using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace Oxidize
{
    internal class MethodsImplementedInCpp
    {
        public static void Generate(CppGenerationContext context, GenerationItem item, GeneratedResult result)
        {
            Debug.Assert(result.CppImplementationInvoker != null);

            foreach (IMethodSymbol method in item.methodsImplementedInCpp)
            {
                GenerateMethod(context, item, result, method);
            }
        }

        private static void GenerateMethod(CppGenerationContext context, GenerationItem item, GeneratedResult result, IMethodSymbol method)
        {
            if (result.CppImplementationInvoker == null)
                return;

            CppType wrapperType = result.CppDefinition.Type;
            CppType implType = result.CppImplementationInvoker.ImplementationType;

            // TODO: support static methods
            // TODO: support return values

            CppType returnType = CppType.FromCSharp(context, method.ReturnType).AsReturnType();
            CppType interopReturnType = returnType.AsInteropType();
            var parameters = method.Parameters.Select(parameter =>
            {
                CppType type = CppType.FromCSharp(context, parameter.Type).AsParameterType();
                return (Name: parameter.Name, Type: type, InteropType: type.AsInteropType());
            });

            string name = $"{wrapperType.GetFullyQualifiedName(false).Replace("::", "_")}_{method.Name}";
            
            var parameterList = new[] { "void* handle", "void* pImpl" }.Concat(parameters.Select(parameter => $"{parameter.InteropType.GetFullyQualifiedName()} {parameter.Name}"));
            string parameterListString = string.Join(", ", parameterList);

            var callParameterList = new[] { "wrapper" }.Concat(parameters.Select(parameter => parameter.Type.GetConversionFromInteropType(context, parameter.Name)));
            string callParameterListString = string.Join(", ", callParameterList);

            result.CppImplementationInvoker.Functions.Add(new(
                content: $$"""
                    {{interopReturnType.GetFullyQualifiedName()}} {{name}}({{parameterListString}}) {
                        const {{wrapperType.GetFullyQualifiedName()}} wrapper(ObjectHandle(handle));
                        auto pImplTyped = reinterpret_cast<{{implType.GetFullyQualifiedName()}}*>(pImpl);
                        pImplTyped->{{method.Name}}({{callParameterListString}});
                    }
                    """,
                typesReferenced: new[]
                {
                    new CppTypeReference(wrapperType, true),
                    new CppTypeReference(implType, true),
                    new CppTypeReference(returnType, true)
                }.Concat(parameters.Select(parameter => new CppTypeReference(parameter.Type)))
                .Concat(parameters.Select(parameter => new CppTypeReference(parameter.InteropType)))
            ));
        }
    }
}