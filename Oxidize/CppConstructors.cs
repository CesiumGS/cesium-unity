using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace Oxidize
{
    internal class CppConstructors
    {
        public static void GenerateConstructors(CppGenerationContext context, GenerationItem item, TypeDefinition definition)
        {
            CppType cppType = CppType.FromCSharp(context, item.type);

            foreach (IMethodSymbol method in item.constructors)
            {
                GenerateConstructor(context, item.type, cppType, method, definition);
            }
        }

        public static void Generate(CppGenerationContext context, GenerationItem item, GeneratedResult result)
        {
            if (item.type.IsStatic)
                GenerateStatic(context, item, result);
            else
                GenerateNonStatic(context, item, result);
        }

        private static void GenerateStatic(CppGenerationContext context, GenerationItem item, GeneratedResult result)
        {
            GeneratedCppDeclaration declaration = result.CppDeclaration;

            // Delete the default constructor so this static class can't be constructed.
            declaration.Elements.Add(new(
                Content: $"{declaration.Type.Name}() = delete;",
                IsPrivate: false
            ));
        }

        private static void GenerateNonStatic(CppGenerationContext context, GenerationItem item, GeneratedResult result)
        {
            foreach (IMethodSymbol constructor in item.constructors)
            {
                GenerateSingleNonStatic(context, item, result, constructor);
            }
        }

        private static void GenerateSingleNonStatic(CppGenerationContext context, GenerationItem item, GeneratedResult result, IMethodSymbol constructor)
        {
            GeneratedCppDeclaration declaration = result.CppDeclaration;
            GeneratedCppDefinition definition = result.CppDefinition;
            GeneratedCppInit cppInit = result.CppInit;
            GeneratedCSharpInit csharpInit = result.CSharpInit;

            var parameters = constructor.Parameters.Select(parameter => (Name: parameter.Name, Type: CppType.FromCSharp(context, parameter.Type).AsParameterType()));
            var interopReturnType = declaration.Type.AsInteropType();
            var interopParameters = parameters.Select(parameter => (ParameterName: parameter.Name, CallSiteName: parameter.Name, Type: parameter.Type, InteropType: parameter.Type.AsInteropType()));
            var interopParameterStrings = interopParameters.Select(parameter => $"{parameter.InteropType.GetFullyQualifiedName()} {parameter.ParameterName}");

            // A private, static field of function pointer type that will call
            // into a managed delegate for this constructor.
            declaration.Elements.Add(new(
                Content: $"static {interopReturnType.GetFullyQualifiedName()} (*Construct)({string.Join(", ", interopParameterStrings)});",
                IsPrivate: true,
                TypeDeclarationsReferenced: new[] { interopReturnType }.Concat(interopParameters.Select(parameter => parameter.InteropType))

            ));

            definition.Elements.Add(new(
                Content: $"{interopReturnType.GetFullyQualifiedName()} (*{definition.Type.GetFullyQualifiedName(false)}::Construct)({string.Join(", ", interopParameterStrings)}) = nullptr;",
                TypeDefinitionsReferenced: new[] { interopReturnType }.Concat(interopParameters.Select(parameter => parameter.InteropType))
            ));

            // The static field should be initialized at startup.
            cppInit.Fields.Add(new(
                Name: $"{definition.Type.GetFullyQualifiedName()}::Construct",
                TypeSignature: $"{interopReturnType.GetFullyQualifiedName()} (*)({string.Join(", ", interopParameters.Select(parameter => parameter.InteropType.GetFullyQualifiedName()))})",
                TypeDefinitionsReferenced: new[] { definition.Type },
                TypeDeclarationsReferenced: new[] { interopReturnType }.Concat(interopParameters.Select(parameter => parameter.Type))
            ));

            // And passed from the C# init method
            csharpInit.Delegates.Add(Interop.CreateCSharpDelegateInit(context.Compilation, item.type, constructor));

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
                    {{definition.Type.GetFullyQualifiedName(false)}}::{{definition.Type.Name}}({{string.Join(", ", parameterStrings)}})
                        : _handle(Construct({{string.Join(", ", parameterPassStrings)}}))
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

        public static void GenerateConstructor(CppGenerationContext context, ITypeSymbol managedType, CppType cppType, IMethodSymbol constructor, TypeDefinition definition)
        {
            var parameters = constructor.Parameters.Select(parameter => (Name: parameter.Name, Type: CppType.FromCSharp(context, parameter.Type).AsParameterType()));

            foreach (var (_, type) in parameters)
            {
                type.AddHeaderIncludesToSet(definition.headerIncludes);
                type.AddSourceIncludesToSet(definition.cppIncludes);
                type.AddForwardDeclarationsToSet(definition.forwardDeclarations);
            }

            // Add a private, static field of a function pointer that will call
            // into a managed delegate for this constructor.
            var interopReturnType = cppType.AsInteropType();
            var interopParameters = parameters.Select(parameter => (ParameterName: parameter.Name, CallSiteName: parameter.Name, Type: parameter.Type, InteropType: parameter.Type.AsInteropType()));
            var interopParameterStrings = interopParameters.Select(parameter => $"{parameter.InteropType.GetFullyQualifiedName()} {parameter.ParameterName}");

            definition.privateDeclarations.Add($"static {interopReturnType.GetFullyQualifiedName()} (*Construct)({string.Join(", ", interopParameterStrings)});");

            // And also define it in the CPP file
            definition.definitions.Add($"{interopReturnType.GetFullyQualifiedName()} (*{cppType.GetFullyQualifiedName(false)}::Construct)({string.Join(", ", interopParameterStrings)}) = nullptr;");

            // Add the constructor declaration
            var parameterStrings = parameters.Select(parameter => $"{parameter.Type.GetFullyQualifiedName()} {parameter.Name}");
            definition.declarations.Add($"{cppType.Name}({string.Join(", ", parameterStrings)});");

            // Add the constructor definition
            var parameterPassStrings = interopParameters.Select(parameter => parameter.Type.GetConversionToInteropType(context, parameter.CallSiteName));

            definition.definitions.Add(
                $$"""
                {{cppType.GetFullyQualifiedName(false)}}::{{cppType.Name}}({{string.Join(", ", parameterStrings)}})
                    : _handle(Construct({{string.Join(", ", parameterPassStrings)}}))
                {
                }
                """);

            // Prepare a C# delegate and C++ function pointer to be used to call this method.
            definition.interopConstructors.Add(new InteropConstructor(
                managedType,
                cppType,
                constructor,
                $"{cppType.GetFullyQualifiedName()}::Construct",
                $"{interopReturnType.GetFullyQualifiedName()} (*)({string.Join(", ", interopParameters.Select(parameter => parameter.InteropType.GetFullyQualifiedName()))})"));
        }
    }
}
