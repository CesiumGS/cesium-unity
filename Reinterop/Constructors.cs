using Microsoft.CodeAnalysis;
using System.Xml.Linq;

namespace Reinterop
{
    internal class Constructors
    {
        public static void Generate(CppGenerationContext context, TypeToGenerate item, GeneratedResult result)
        {
            // TODO: We're not currently generating constructors for blittable value types. They'll need to be slightly different (no handle).
            // We only need handle management for non-static classes.

            GeneratedCppDeclaration declaration = result.CppDeclaration;
            if (declaration.Type.Kind != InteropTypeKind.ClassWrapper &&
                declaration.Type.Kind != InteropTypeKind.NonBlittableStructWrapper &&
                declaration.Type.Kind != InteropTypeKind.Delegate)
            {
                return;
            }

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

            var parameters = constructor.Parameters.Select(parameter => (Name: parameter.Name, Type: CppType.FromCSharp(context, parameter.Type).AsParameterType()));
            var interopReturnType = declaration.Type.AsInteropType();
            var interopParameters = parameters.Select(parameter => (ParameterName: parameter.Name, CallSiteName: parameter.Name, Type: parameter.Type, InteropType: parameter.Type.AsInteropType()));
            var interopParameterStrings = interopParameters.Select(parameter => $"{parameter.InteropType.GetFullyQualifiedName()} {parameter.ParameterName}");

            string interopFunctionName = $"Construct_{Interop.HashParameters(constructor.Parameters)}";

            string templateSpecialization = Interop.GetTemplateSpecialization(declaration.Type);

            // A private, static field of function pointer type that will call
            // into a managed delegate for this constructor.
            declaration.Elements.Add(new(
                Content: $"static {interopReturnType.GetFullyQualifiedName()} (*{interopFunctionName})({string.Join(", ", interopParameterStrings)});",
                IsPrivate: true,
                TypeDeclarationsReferenced: new[] { interopReturnType }.Concat(interopParameters.Select(parameter => parameter.InteropType))

            ));

            definition.Elements.Add(new(
                Content: $"{interopReturnType.GetFullyQualifiedName()} (*{definition.Type.Name}{templateSpecialization}::{interopFunctionName})({string.Join(", ", interopParameterStrings)}) = nullptr;",
                TypeDeclarationsReferenced: new[] { interopReturnType }.Concat(interopParameters.Select(parameter => parameter.InteropType))
            ));

            // The static field should be initialized at startup.
            var (csName, csContent) = Interop.CreateCSharpDelegateInit(context, item.Type, constructor, interopFunctionName);
            init.Functions.Add(new(
                CppName: $"{definition.Type.GetFullyQualifiedName()}::{interopFunctionName}",
                CppTypeSignature: $"{interopReturnType.GetFullyQualifiedName()} (*)({string.Join(", ", interopParameters.Select(parameter => parameter.InteropType.GetFullyQualifiedName()))})",
                CppTypeDefinitionsReferenced: new[] { definition.Type },
                CppTypeDeclarationsReferenced: new[] { interopReturnType }.Concat(interopParameters.Select(parameter => parameter.Type)),
                CSharpName: csName,
                CSharpContent: csContent
            ));

            // Constructor declaration
            var parameterStrings = parameters.Select(parameter => $"{parameter.Type.GetFullyQualifiedName()} {parameter.Name}");
            declaration.Elements.Add(new(
                Content: $"{declaration.Type.Name}({string.Join(", ", parameterStrings)});",
                TypeDeclarationsReferenced: parameters.Select(parameter => parameter.Type)
            ));

            // Constructor definition
            var parameterPassStrings = interopParameters.Select(parameter => parameter.Type.GetConversionToInteropType(context, parameter.CallSiteName));
            definition.Elements.Add(new(
                Content:
                    $$"""
                    {{definition.Type.Name}}{{templateSpecialization}}::{{definition.Type.Name}}({{string.Join(", ", parameterStrings)}})
                        : _handle({{interopFunctionName}}({{string.Join(", ", parameterPassStrings)}}))
                    {
                    }
                    """,
                TypeDefinitionsReferenced: new[]
                {
                    definition.Type,
                    interopReturnType,
                    CppObjectHandle.GetCppType(context)
                }.Concat(parameters.Select(parameter => parameter.Type))
            ));
        }
    }
}
