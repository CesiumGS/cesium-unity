using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oxidize
{
    internal class CppMethods
    {
        public static void GenerateMethods(CppGenerationContext context, GenerationItem mainItem, GenerationItem currentItem, TypeDefinition definition)
        {
            CppType cppType = CppType.FromCSharp(context, mainItem.type);

            foreach (IMethodSymbol method in currentItem.methods)
            {
                GenerateMethod(context, mainItem.type, cppType, method, definition);
            }
        }

        public static void GenerateMethod(CppGenerationContext context, ITypeSymbol managedType, CppType cppType, IMethodSymbol method, TypeDefinition definition)
        {
            string modifiers = "";
            string afterModifiers = "";
            if (method.IsStatic)
            {
                modifiers += "static ";
            }
            else
            {
                afterModifiers += " const";
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

            // Add a private, static field of a function pointer that will call
            // into a managed delegate for this method.
            CppType interopReturnType = returnType.AsInteropType();
            var interopParameters = parameters.Select(parameter => (ParameterName: parameter.Name, CallSiteName: parameter.Name, Type: parameter.Type, InteropType: parameter.Type.AsInteropType()));
            
            // If this is an instance method, pass the current object as the first parameter.
            if (!method.IsStatic)
            {
                interopParameters = new[] { (ParameterName: "thiz", CallSiteName: "(*this)", Type: cppType, InteropType: cppType.AsInteropType()) }.Concat(interopParameters);
            }

            var interopParameterStrings = interopParameters.Select(parameter => $"{parameter.InteropType.GetFullyQualifiedName()} {parameter.ParameterName}");

            definition.privateDeclarations.Add($"static {interopReturnType.GetFullyQualifiedName()} (*Call{method.Name})({string.Join(", ", interopParameterStrings)});");

            // And also define it in the CPP file
            definition.definitions.Add($"{interopReturnType.GetFullyQualifiedName()} (*{cppType.GetFullyQualifiedName(false)}::Call{method.Name})({string.Join(", ", interopParameterStrings)}) = nullptr;");

            // Add the method declaration
            var parameterStrings = parameters.Select(parameter => $"{parameter.Type.GetFullyQualifiedName()} {parameter.Name}");
            definition.declarations.Add($"{modifiers}{returnType.GetFullyQualifiedName()} {method.Name}({string.Join(", ", parameterStrings)}){afterModifiers};");

            // Add the method definition
            var parameterPassStrings = interopParameters.Select(parameter => parameter.Type.GetConversionToInteropType(context, parameter.CallSiteName));

            if (returnType.Name == "void" && !returnType.Flags.HasFlag(CppTypeFlags.Pointer))
            {
                definition.definitions.Add(
                    $$"""
                    {{returnType.GetFullyQualifiedName()}} {{cppType.GetFullyQualifiedName(false)}}::{{method.Name}}({{string.Join(", ", parameterStrings)}}){{afterModifiers}} {
                        Call{{method.Name}}({{string.Join(", ", parameterPassStrings)}});
                    }
                    """);
            }
            else
            {
                definition.definitions.Add(
                    $$"""
                    {{returnType.GetFullyQualifiedName()}} {{cppType.GetFullyQualifiedName(false)}}::{{method.Name}}({{string.Join(", ", parameterStrings)}}){{afterModifiers}} {
                        auto result = Call{{method.Name}}({{string.Join(", ", parameterPassStrings)}});
                        return {{returnType.GetConversionFromInteropType(context, "result")}};
                    }
                    """);
            }

            // Prepare a C# delegate and C++ function pointer to be used to call this method.
            definition.interopMethods.Add(new InteropMethod(
                managedType,
                cppType,
                method,
                $"{cppType.GetFullyQualifiedName()}::Call{method.Name}",
                $"{interopReturnType.GetFullyQualifiedName()} (*)({string.Join(", ", interopParameters.Select(parameter => parameter.InteropType.GetFullyQualifiedName()))})"));
        }
    }
}
