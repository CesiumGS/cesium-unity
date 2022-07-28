using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace Oxidize
{
    internal class Methods
    {
        public static void Generate(CppGenerationContext context, TypeToGenerate mainItem, TypeToGenerate currentItem, GeneratedResult result)
        {
            foreach (IMethodSymbol method in currentItem.Methods)
            {
                GenerateSingleMethod(context, mainItem, result, method);
            }
        }

        private static void GenerateSingleMethod(CppGenerationContext context, TypeToGenerate item, GeneratedResult result, IMethodSymbol method)
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

            // If this is an instance method, pass the current object as the first parameter.
            if (!method.IsStatic)
            {
                parameters = new[] { (ParameterName: "thiz", CallSiteName: "(*this)", Type: result.CppDefinition.Type, InteropType: result.CppDefinition.Type.AsInteropType()) }.Concat(parameters);
            }

            var interopParameterStrings = parameters.Select(parameter => $"{parameter.InteropType.GetFullyQualifiedName()} {parameter.ParameterName}");

            // A private, static field of function pointer type that will call
            // into a managed delegate for this method.
            declaration.Elements.Add(new(
                Content: $"static {interopReturnType.GetFullyQualifiedName()} (*Call{method.Name})({string.Join(", ", interopParameterStrings)});",
                IsPrivate: true,
                TypeDeclarationsReferenced: new[] { interopReturnType }.Concat(parameters.Select(parameter => parameter.InteropType))
            ));

            definition.Elements.Add(new(
                Content: $"{interopReturnType.GetFullyQualifiedName()} (*{definition.Type.GetFullyQualifiedName(false)}::Call{method.Name})({string.Join(", ", interopParameterStrings)}) = nullptr;",
                TypeDeclarationsReferenced: new[] { interopReturnType }.Concat(parameters.Select(parameter => parameter.InteropType))
            ));

            // The static field should be initialized at startup.
            cppInit.Fields.Add(new(
                Name: $"{definition.Type.GetFullyQualifiedName()}::Call{method.Name}",
                TypeSignature: $"{interopReturnType.GetFullyQualifiedName()} (*)({string.Join(", ", parameters.Select(parameter => parameter.InteropType.GetFullyQualifiedName()))})",
                TypeDefinitionsReferenced: new[] { definition.Type },
                TypeDeclarationsReferenced: new[] { interopReturnType }.Concat(parameters.Select(parameter => parameter.Type))
            ));

            // And passed from the C# init method
            csharpInit.Delegates.Add(Interop.CreateCSharpDelegateInit(context.Compilation, item.Type, method));

            string modifiers = "";
            string afterModifiers = "";
            if (method.IsStatic)
                modifiers += "static ";
            else
                afterModifiers += " const";

            // Method declaration
            var parameterStrings = parameters.Select(parameter => $"{parameter.Type.GetFullyQualifiedName()} {parameter.ParameterName}");
            declaration.Elements.Add(new(
                Content: $"{modifiers}{returnType.GetFullyQualifiedName()} {method.Name}({string.Join(", ", parameterStrings)}){afterModifiers};",
                TypeDeclarationsReferenced: new[] { returnType }.Concat(parameters.Select(parameter => parameter.Type))
            ));

            // Constructor definition
            var parameterPassStrings = parameters.Select(parameter => parameter.Type.GetConversionToInteropType(context, parameter.CallSiteName));
            if (returnType.Name == "void" && !returnType.Flags.HasFlag(CppTypeFlags.Pointer))
            {
                definition.Elements.Add(new(
                    Content:
                        $$"""
                        {{returnType.GetFullyQualifiedName()}} {{definition.Type.Name}}::{{method.Name}}({{string.Join(", ", parameterStrings)}}){{afterModifiers}} {
                            Call{{method.Name}}({{string.Join(", ", parameterPassStrings)}});
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
                        {{returnType.GetFullyQualifiedName()}} {{definition.Type.Name}}::{{method.Name}}({{string.Join(", ", parameterStrings)}}){{afterModifiers}} {
                            auto result = Call{{method.Name}}({{string.Join(", ", parameterPassStrings)}});
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
    }
}
