using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace Oxidize
{
    internal class Constructors
    {
        public static void Generate(CppGenerationContext context, TypeToGenerate item, GeneratedResult result)
        {
            // TODO: We're not currently generating constructors for blittable value types. They'll need to be slightly different (no handle).
            // We only need handle management for non-static classes.

            GeneratedCppDeclaration declaration = result.CppDeclaration;
            if (declaration.Type.Kind != CppTypeKind.ClassWrapper && declaration.Type.Kind != CppTypeKind.NonBlittableStructWrapper)
                return;

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
            GeneratedCppInit cppInit = result.CppInit;
            GeneratedCSharpInit csharpInit = result.CSharpInit;

            var parameters = constructor.Parameters.Select(parameter => (Name: parameter.Name, Type: CppType.FromCSharp(context, parameter.Type).AsParameterType()));
            var interopReturnType = declaration.Type.AsInteropType();
            var interopParameters = parameters.Select(parameter => (ParameterName: parameter.Name, CallSiteName: parameter.Name, Type: parameter.Type, InteropType: parameter.Type.AsInteropType()));
            var interopParameterStrings = interopParameters.Select(parameter => $"{parameter.InteropType.GetFullyQualifiedName()} {parameter.ParameterName}");

            string interopFunctionName = $"Construct_{Interop.HashParameters(constructor.Parameters)}";

            string templateSpecialization = "";
            if (declaration.Type.GenericArguments != null && declaration.Type.GenericArguments.Count > 0)
            {
                templateSpecialization = $"<{string.Join(", ", declaration.Type.GenericArguments.Select(arg => arg.GetFullyQualifiedName()))}>";
            }

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
            cppInit.Fields.Add(new(
                Name: $"{definition.Type.GetFullyQualifiedName()}::{interopFunctionName}",
                TypeSignature: $"{interopReturnType.GetFullyQualifiedName()} (*)({string.Join(", ", interopParameters.Select(parameter => parameter.InteropType.GetFullyQualifiedName()))})",
                TypeDefinitionsReferenced: new[] { definition.Type },
                TypeDeclarationsReferenced: new[] { interopReturnType }.Concat(interopParameters.Select(parameter => parameter.Type))
            ));

            // And passed from the C# init method
            csharpInit.Delegates.Add(Interop.CreateCSharpDelegateInit(context.Compilation, item.Type, constructor));

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
