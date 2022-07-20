using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
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
