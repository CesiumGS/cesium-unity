using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oxidize
{
    internal class CppMethods
    {
        public static void GenerateMethods(CppGenerationContext context, GenerationItem item, TypeDefinition definition)
        {
            CppType itemType = CppType.FromCSharp(context, item.type);
            string typeName = itemType.GetFullyQualifiedName();

            foreach (IMethodSymbol method in item.methods)
            {
                GenerateMethod(context, typeName, method, definition);
            }
        }

        public static void GenerateMethod(CppGenerationContext context, string typeName, IMethodSymbol method, TypeDefinition definition)
        {
            string modifiers = "";
            if (method.IsStatic)
            {
                modifiers += "static ";
            }

            CppType returnType = CppType.FromCSharp(context, method.ReturnType).AsReturnType();
            returnType.AddHeaderIncludesToSet(definition.headerIncludes);
            returnType.AddSourceIncludesToSet(definition.cppIncludes);
            returnType.AddForwardDeclarationsToSet(definition.forwardDeclarations);

            var parameters = method.Parameters.Select(parameter => (Name: parameter.Name, Type: CppType.FromCSharp(context, parameter.Type).AsParameterType()));
            
            foreach (var (_, type) in parameters)
            {
                type.AddHeaderIncludesToSet(definition.headerIncludes);
                type.AddSourceIncludesToSet(definition.cppIncludes);
                type.AddForwardDeclarationsToSet(definition.forwardDeclarations);
            }

            // Add a private, static field to a function pointer that will call
            // into a managed delegate for this method.
            CppType interopReturnType = returnType.AsInteropType();
            var interopParameterStrings = parameters.Select(parameter => $"{parameter.Type.AsInteropType().GetFullyQualifiedName()} {parameter.Name}");
            definition.privateDeclarations.Add($"static {interopReturnType.GetFullyQualifiedName()} (*Call{method.Name})({string.Join(", ", interopParameterStrings)});");

            // Add the method declaration
            var parameterStrings = parameters.Select(parameter => $"{parameter.Type.GetFullyQualifiedName()} {parameter.Name}");
            definition.declarations.Add($"{modifiers}{returnType.GetFullyQualifiedName()} {method.Name}({string.Join(", ", parameterStrings)});");

            // Add the method definition
            var parameterPassStrings = parameters.Select(parameter => parameter.Type.GetConversionToInteropType(parameter.Name));
            definition.definitions.Add(
                $$"""
                {{returnType.GetFullyQualifiedName()}} {{typeName}}::{{method.Name}}({{string.Join(", ", parameterStrings)}}) {
                    auto result = Call{{method.Name}}({{string.Join(", ", parameterPassStrings)}});
                    return {{returnType.GetConversionFromInteropType("result")}};
                }
                """);
        }
    }
}
