using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxidize
{
    internal class CppProperties
    {
        public static void GenerateProperties(CppGenerationContext context, GenerationItem mainItem, GenerationItem currentItem, TypeDefinition definition)
        {
            CppType cppType = CppType.FromCSharp(context, mainItem.type);

            foreach (IPropertySymbol property in currentItem.properties)
            {
                GenerateProperty(context, mainItem.type, cppType, property, definition);
            }
        }

        public static void Generate(CppGenerationContext context, GenerationItem mainItem, GenerationItem currentItem, GeneratedResult result)
        {
            foreach (IPropertySymbol property in currentItem.properties)
            {
                GenerateSingleProperty(context, mainItem, result, property);
            }
        }

        private static void GenerateSingleProperty(CppGenerationContext context, GenerationItem item, GeneratedResult result, IPropertySymbol property)
        {
            if (property.GetMethod != null)
                GenerateSingleMethod(context, item, result, property, property.GetMethod);

            if (property.SetMethod != null)
                GenerateSingleMethod(context, item, result, property, property.SetMethod);
        }

        private static void GenerateSingleMethod(CppGenerationContext context, GenerationItem item, GeneratedResult result, IPropertySymbol property, IMethodSymbol method)
        {
            GeneratedCppDeclaration declaration = result.CppDeclaration;
            GeneratedCppDefinition definition = result.CppDefinition;
            GeneratedCppInit cppInit = result.CppInit;
            GeneratedCSharpInit csharpInit = result.CSharpInit;

            CppType returnType = CppType.FromCSharp(context, method.ReturnType);
            CppType interopReturnType = returnType.AsInteropType();
            var parameters = method.Parameters.Select(parameter => {
                CppType type = CppType.FromCSharp(context, parameter.Type);
                return (ParameterName: parameter.Name, CallSiteName: parameter.Name, Type: type, InteropType: type.AsInteropType());
            });
            var interopParameters = parameters;

            // If this is an instance method, pass the current object as the first parameter.
            if (!method.IsStatic)
            {
                interopParameters = new[] { (ParameterName: "thiz", CallSiteName: "(*this)", Type: result.CppDefinition.Type, InteropType: result.CppDefinition.Type.AsInteropType()) }.Concat(interopParameters);
            }

            var interopParameterStrings = interopParameters.Select(parameter => $"{parameter.InteropType.GetFullyQualifiedName()} {parameter.ParameterName}");

            // A private, static field of function pointer type that will call
            // into a managed delegate for this method.
            declaration.Elements.Add(new(
                Content: $"static {interopReturnType.GetFullyQualifiedName()} (*Property_{method.Name})({string.Join(", ", interopParameterStrings)});",
                IsPrivate: true,
                TypeDeclarationsReferenced: new[] { interopReturnType }.Concat(parameters.Select(parameter => parameter.InteropType))
            ));

            definition.Elements.Add(new(
                Content: $"{interopReturnType.GetFullyQualifiedName()} (*{definition.Type.GetFullyQualifiedName(false)}::Property_{method.Name})({string.Join(", ", interopParameterStrings)}) = nullptr;",
                TypeDeclarationsReferenced: new[] { interopReturnType }.Concat(parameters.Select(parameter => parameter.InteropType))
            ));

            // The static field should be initialized at startup.
            cppInit.Fields.Add(new(
                Name: $"{definition.Type.GetFullyQualifiedName()}::Property_{method.Name}",
                TypeSignature: $"{interopReturnType.GetFullyQualifiedName()} (*)({string.Join(", ", interopParameters.Select(parameter => parameter.InteropType.GetFullyQualifiedName()))})",
                TypeDefinitionsReferenced: new[] { definition.Type },
                TypeDeclarationsReferenced: new[] { interopReturnType }.Concat(parameters.Select(parameter => parameter.Type))
            ));

            // And passed from the C# init method
            csharpInit.Delegates.Add(Interop.CreateCSharpDelegateInit(context.Compilation, item.type, method));

            string modifiers = "";
            string afterModifiers = "";
            if (method.IsStatic)
                modifiers += "static ";
            else
                afterModifiers += " const";

            // Method declaration
            var parameterStrings = parameters.Select(parameter => $"{parameter.Type.GetFullyQualifiedName()} {parameter.ParameterName}");
            declaration.Elements.Add(new(
                Content: $"{modifiers}{returnType.GetFullyQualifiedName()} {property.Name}({string.Join(", ", parameterStrings)}){afterModifiers};",
                TypeDeclarationsReferenced: new[] { returnType }.Concat(parameters.Select(parameter => parameter.Type))
            ));

            // Method definition
            var parameterPassStrings = interopParameters.Select(parameter => parameter.Type.GetConversionToInteropType(context, parameter.CallSiteName));
            if (returnType.Name == "void" && !returnType.Flags.HasFlag(CppTypeFlags.Pointer))
            {
                definition.Elements.Add(new(
                    Content:
                        $$"""
                        {{returnType.GetFullyQualifiedName()}} {{definition.Type.Name}}::{{property.Name}}({{string.Join(", ", parameterStrings)}}){{afterModifiers}} {
                            Property_{{method.Name}}({{string.Join(", ", parameterPassStrings)}});
                        }
                        """,
                    TypeDefinitionsReferenced: new[]
                    {
                        definition.Type,
                        returnType,
                        CppObjectHandle.GetCppType(context)
                    }.Concat(parameters.Select(parameter => parameter.Type))
                ));
            }
            else
            {
                definition.Elements.Add(new(
                    Content:
                        $$"""
                        {{returnType.GetFullyQualifiedName()}} {{definition.Type.Name}}::{{property.Name}}({{string.Join(", ", parameterStrings)}}){{afterModifiers}} {
                            auto result = Property_{{method.Name}}({{string.Join(", ", parameterPassStrings)}});
                            return {{returnType.GetConversionFromInteropType(context, "result")}};
                        }
                        """,
                    TypeDefinitionsReferenced: new[]
                    {
                        definition.Type,
                        returnType,
                        CppObjectHandle.GetCppType(context)
                    }.Concat(parameters.Select(parameter => parameter.Type))
                ));
            }
        }

        public static void GenerateProperty(CppGenerationContext context, ITypeSymbol itemType, CppType cppType, IPropertySymbol property, TypeDefinition definition)
        {
            if (property.GetMethod != null)
                GenerateMethod(context, itemType, cppType, property, property.GetMethod, definition);

            if (property.SetMethod != null)
                GenerateMethod(context, itemType, cppType, property, property.SetMethod, definition);
        }

        private static void GenerateMethod(CppGenerationContext context, ITypeSymbol managedType, CppType cppType, IPropertySymbol property, IMethodSymbol method, TypeDefinition definition)
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

            definition.privateDeclarations.Add($"static {interopReturnType.GetFullyQualifiedName()} (*Property_{method.Name})({string.Join(", ", interopParameterStrings)});");

            // And also define it in the CPP file
            definition.definitions.Add($"{interopReturnType.GetFullyQualifiedName()} (*{cppType.GetFullyQualifiedName(false)}::Property_{method.Name})({string.Join(", ", interopParameterStrings)}) = nullptr;");

            // Add the method declaration
            var parameterStrings = parameters.Select(parameter => $"{parameter.Type.GetFullyQualifiedName()} {parameter.Name}");
            definition.declarations.Add($"{modifiers}{returnType.GetFullyQualifiedName()} {property.Name}({string.Join(", ", parameterStrings)}){afterModifiers};");

            // Add the method definition
            var parameterPassStrings = interopParameters.Select(parameter => parameter.Type.GetConversionToInteropType(context, parameter.CallSiteName));

            if (returnType.Name == "void" && !returnType.Flags.HasFlag(CppTypeFlags.Pointer))
            {
                definition.definitions.Add(
                    $$"""
                    {{returnType.GetFullyQualifiedName()}} {{cppType.GetFullyQualifiedName(false)}}::{{property.Name}}({{string.Join(", ", parameterStrings)}}){{afterModifiers}} {
                        Property_{{method.Name}}({{string.Join(", ", parameterPassStrings)}});
                    }
                    """);
            }
            else
            {
                definition.definitions.Add(
                    $$"""
                    {{returnType.GetFullyQualifiedName()}} {{cppType.GetFullyQualifiedName(false)}}::{{property.Name}}({{string.Join(", ", parameterStrings)}}){{afterModifiers}} {
                        auto result = Property_{{method.Name}}({{string.Join(", ", parameterPassStrings)}});
                        return {{returnType.GetConversionFromInteropType(context, "result")}};
                    }
                    """);
            }

            // Prepare a C# delegate and C++ function pointer to be used to call this method.
            definition.interopMethods.Add(new InteropMethod(
                managedType,
                cppType,
                method,
                $"{cppType.GetFullyQualifiedName()}::Property_{method.Name}",
                $"{interopReturnType.GetFullyQualifiedName()} (*)({string.Join(", ", interopParameters.Select(parameter => parameter.InteropType.GetFullyQualifiedName()))})"));
        }
    }
}
