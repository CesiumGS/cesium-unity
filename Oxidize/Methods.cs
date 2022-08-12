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

        public static void GenerateSingleMethod(CppGenerationContext context, TypeToGenerate item, GeneratedResult result, IMethodSymbol method)
        {
            GeneratedCppDeclaration declaration = result.CppDeclaration;
            GeneratedCppDefinition definition = result.CppDefinition;
            GeneratedInit init = result.Init;

            CppType returnType = CppType.FromCSharp(context, method.ReturnType).AsReturnType();
            CppType interopReturnType = returnType.AsInteropType();
            var parameters = method.Parameters.Select(parameter => {
                CppType type = CppType.FromCSharp(context, parameter.Type).AsParameterType();
                return (ParameterName: parameter.Name, CallSiteName: parameter.Name, Type: type, InteropType: type.AsInteropType());
            });
            var interopParameters = parameters;

            string interopName = $"Call{method.Name}_{Interop.HashParameters(method.Parameters, method.TypeArguments)}";

            string modifiers = "";
            string afterModifiers = "";
            if (method.IsStatic)
                modifiers += "static ";
            else
                afterModifiers += " const";

            string templateSpecialization = "";
            string templatePrefix = "";
            if (method.IsGenericMethod)
            {
                // Add the template which will be specialized by this method.
                IMethodSymbol genericMethod = method.ConstructedFrom;

                // Only add the template declaration if this is the first method constructed from this template.
                IMethodSymbol? first = item.Methods.First(m => SymbolEqualityComparer.Default.Equals(m.ConstructedFrom, genericMethod));
                if (SymbolEqualityComparer.Default.Equals(first, method))
                {
                    CppType genericReturn = CppType.FromCSharp(context, genericMethod.ReturnType).AsReturnType();
                    var genericParameters = genericMethod.Parameters.Select(parameter => CppType.FromCSharp(context, parameter.Type).AsParameterType().GetFullyQualifiedName() + " " + parameter.Name);
                    string genericParametersString = string.Join(", ", genericParameters);
                    declaration.Elements.Add(new(
                        Content:
                            $$"""
                            template <{{string.Join(", ", method.TypeParameters.Select(parameter => "typename " + parameter.Name))}}>
                            {{modifiers}}{{genericReturn.GetFullyQualifiedName()}} {{method.Name}}({{genericParametersString}}){{afterModifiers}};
                            """,
                        TypeDeclarationsReferenced: genericMethod.Parameters.Select(p => CppType.FromCSharp(context, p.Type))
                        ));
                }

                templatePrefix = "template <> ";
                var templateParameters = method.TypeArguments.Select(typeArgument => CppType.FromCSharp(context, typeArgument).GetFullyQualifiedName());
                templateSpecialization = $"<{string.Join(", ", templateParameters)}>";
            }

            // If this is an instance method, pass the current object as the first parameter.
            if (!method.IsStatic)
            {
                interopParameters = new[] { (ParameterName: "thiz", CallSiteName: "(*this)", Type: result.CppDefinition.Type, InteropType: result.CppDefinition.Type.AsInteropType()) }.Concat(interopParameters);
            }

            var interopParameterStrings = interopParameters.Select(parameter => $"{parameter.InteropType.GetFullyQualifiedName()} {parameter.ParameterName}");

            // A private, static field of function pointer type that will call
            // into a managed delegate for this method.
            declaration.Elements.Add(new(
                Content: $"static {interopReturnType.GetFullyQualifiedName()} (*{interopName})({string.Join(", ", interopParameterStrings)});",
                IsPrivate: true,
                TypeDeclarationsReferenced: new[] { interopReturnType }.Concat(parameters.Select(parameter => parameter.InteropType))
            ));

            definition.Elements.Add(new(
                Content: $"{interopReturnType.GetFullyQualifiedName()} (*{definition.Type.GetFullyQualifiedName(false)}::{interopName})({string.Join(", ", interopParameterStrings)}) = nullptr;",
                TypeDeclarationsReferenced: new[] { interopReturnType }.Concat(parameters.Select(parameter => parameter.InteropType))
            ));

            // The static field should be initialized at startup.
            var (csName, csContent) = Interop.CreateCSharpDelegateInit(context.Compilation, item.Type, method, interopName);
            init.Functions.Add(new(
                CppName: $"{definition.Type.GetFullyQualifiedName()}::{interopName}",
                CppTypeSignature: $"{interopReturnType.GetFullyQualifiedName()} (*)({string.Join(", ", interopParameters.Select(parameter => parameter.InteropType.GetFullyQualifiedName()))})",
                CppTypeDefinitionsReferenced: new[] { definition.Type },
                CppTypeDeclarationsReferenced: new[] { interopReturnType }.Concat(parameters.Select(parameter => parameter.Type)),
                CSharpName: csName,
                CSharpContent: csContent
            ));

            var parameterStrings = parameters.Select(parameter => $"{parameter.Type.GetFullyQualifiedName()} {parameter.ParameterName}");

            // Method declaration
            // Skip method declaration for generic methods, because we only need the generic version above.
            if (!method.IsGenericMethod)
            {
                declaration.Elements.Add(new(
                    Content: $"{templatePrefix}{modifiers}{returnType.GetFullyQualifiedName()} {method.Name}{templateSpecialization}({string.Join(", ", parameterStrings)}){afterModifiers};",
                    TypeDeclarationsReferenced: new[] { returnType }.Concat(parameters.Select(parameter => parameter.Type))
                ));
            }

            string typeTemplateSpecialization = "";
            if (definition.Type.GenericArguments != null && definition.Type.GenericArguments.Count > 0)
            {
                typeTemplateSpecialization = "<" + string.Join(", ", definition.Type.GenericArguments.Select(t => t.GetFullyQualifiedName())) + ">";
            }

            // Method definition
            var parameterPassStrings = interopParameters.Select(parameter => parameter.Type.GetConversionToInteropType(context, parameter.CallSiteName));
            if (returnType.Name == "void" && !returnType.Flags.HasFlag(CppTypeFlags.Pointer))
            {
                definition.Elements.Add(new(
                    Content:
                        $$"""
                        {{templatePrefix}}{{returnType.GetFullyQualifiedName()}} {{definition.Type.Name}}{{typeTemplateSpecialization}}::{{method.Name}}{{templateSpecialization}}({{string.Join(", ", parameterStrings)}}){{afterModifiers}} {
                            {{interopName}}({{string.Join(", ", parameterPassStrings)}});
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
                        {{templatePrefix}}{{returnType.GetFullyQualifiedName()}} {{definition.Type.Name}}{{typeTemplateSpecialization}}::{{method.Name}}{{templateSpecialization}}({{string.Join(", ", parameterStrings)}}){{afterModifiers}} {
                            auto result = {{interopName}}({{string.Join(", ", parameterPassStrings)}});
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
