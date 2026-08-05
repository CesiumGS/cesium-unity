using Microsoft.CodeAnalysis;

namespace Reinterop
{
    internal class Properties
    {
        public static void Generate(CppGenerationContext context, TypeToGenerate mainItem, TypeToGenerate currentItem, GeneratedResult result)
        {
            foreach (IPropertySymbol property in currentItem.Properties)
            {
                GenerateSingleProperty(context, mainItem, result, property);
            }
        }

        private static void GenerateSingleProperty(CppGenerationContext context, TypeToGenerate item, GeneratedResult result, IPropertySymbol property)
        {
            if (property.GetMethod != null)
                GenerateSingleMethod(context, item, result, property, property.GetMethod);

            // TODO: support element setters (i.e. obj[0] = x)
            if (property.SetMethod != null && !property.IsIndexer)
                GenerateSingleMethod(context, item, result, property, property.SetMethod);
        }

        private static void GenerateSingleMethod(CppGenerationContext context, TypeToGenerate item, GeneratedResult result, IPropertySymbol property, IMethodSymbol method)
        {
            GeneratedCppDeclaration declaration = result.CppDeclaration;
            GeneratedCppDefinition definition = result.CppDefinition;
            GeneratedInit init = result.Init;

            CppType returnType = CppType.FromCSharp(context, method.ReturnType).AsReturnType();
            CppType interopReturnType = returnType.AsInteropType();
            var parameters = method.Parameters.Select(parameter =>
            {
                CppType type = CppType.FromCSharp(context, parameter.Type).AsParameterType();
                return (ParameterName: parameter.Name, CallSiteName: parameter.Name, Type: type, InteropType: type.AsInteropType());
            });
            var interopParameters = parameters;

            // If this is an instance method, pass the current object as the first parameter.
            if (!method.IsStatic)
            {
                interopParameters = new[] { (ParameterName: "thiz", CallSiteName: "(*this)", Type: result.CppDefinition.Type.AsParameterType(), InteropType: result.CppDefinition.Type.AsParameterType().AsInteropType()) }.Concat(interopParameters);
            }

            bool hasStructRewrite = Interop.RewriteStructReturn(ref interopParameters, ref returnType, ref interopReturnType);

            // Add a parameter in which to return the exception, if there is one.
            interopParameters = interopParameters.Concat(new[] { (ParameterName: "reinteropException", CallSiteName: "", Type: CppType.VoidPointerPointer, InteropType: CppType.VoidPointerPointer) });

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
            var (csName, csContent) = Interop.CreateCSharpDelegateInit(context, item.Type, method, $"Property_{method.Name}");
            init.Functions.Add(new(
                CppName: $"{definition.Type.GetFullyQualifiedName()}::Property_{method.Name}",
                CppTypeSignature: $"{interopReturnType.GetFullyQualifiedName()} (*)({string.Join(", ", interopParameters.Select(parameter => parameter.InteropType.GetFullyQualifiedName()))})",
                CppTypeDefinitionsReferenced: new[] { definition.Type },
                CppTypeDeclarationsReferenced: new[] { interopReturnType }.Concat(parameters.Select(parameter => parameter.Type)),
                CSharpName: csName,
                CSharpContent: csContent
            ));

            string modifiers = "";
            string afterModifiers = "";
            if (method.IsStatic)
                modifiers += "static ";
            else
                afterModifiers += " const";

            string propertyName = property.Name;
            if (property.IsIndexer)
                propertyName = "operator[]";

            // Method declaration
            var parameterStrings = parameters.Select(parameter => $"{parameter.Type.GetFullyQualifiedName()} {parameter.ParameterName}");
            declaration.Elements.Add(new(
                Content: $"{modifiers}{returnType.GetFullyQualifiedName()} {propertyName}({string.Join(", ", parameterStrings)}){afterModifiers};",
                TypeDeclarationsReferenced: new[] { returnType }.Concat(parameters.Select(parameter => parameter.Type))
            ));

            string typeTemplateSpecialization = "";
            if (definition.Type.GenericArguments != null && definition.Type.GenericArguments.Count > 0)
            {
                typeTemplateSpecialization = "<" + string.Join(", ", definition.Type.GenericArguments.Select(t => t.GetFullyQualifiedName())) + ">";
            }

            // Method definition
            IReadOnlyList<CppArgument> callArguments = interopParameters
                .Where(parameter => parameter.ParameterName != "reinteropException")
                .Select(parameter => parameter.ParameterName == "pReturnValue"
                    ? CppArgument.OutParameter(returnType.GetFullyQualifiedName(), "result")
                    : CppArgument.Value(parameter.Type.GetConversionToInteropType(context, parameter.CallSiteName)))
                .ToArray();
            if (returnType.Name == "void" && !returnType.Flags.HasFlag(CppTypeFlags.Pointer))
            {
                IReadOnlyList<CppStatement> body = CppInterop.CallManagedFunction(new CppIdentifier($"Property_{method.Name}"), callArguments);
                definition.Elements.Add(new(
                    Content:
                        $$"""
                        {{returnType.GetFullyQualifiedName()}} {{definition.Type.Name}}{{typeTemplateSpecialization}}::{{propertyName}}({{string.Join(", ", parameterStrings)}}){{afterModifiers}} {
                            {{new[] { CppPrinter.Print(body) }.JoinAndIndent("    ")}}
                        }
                        """,
                    TypeDefinitionsReferenced: new[]
                    {
                        definition.Type,
                        returnType,
                        CppObjectHandle.GetCppType(context),
                        CppReinteropException.GetCppType(context)
                    }.Concat(parameters.Select(parameter => parameter.Type))
                ));
            }
            else
            {
                IReadOnlyList<CppStatement> body = CppInterop.CallManagedFunction(
                    new CppIdentifier($"Property_{method.Name}"), callArguments,
                    resultTypeName: hasStructRewrite ? null : "auto",
                    returnExpression: new CppRaw(returnType.GetConversionFromInteropType(context, "result")));
                definition.Elements.Add(new(
                    Content:
                        $$"""
                        {{returnType.GetFullyQualifiedName()}} {{definition.Type.Name}}{{typeTemplateSpecialization}}::{{propertyName}}({{string.Join(", ", parameterStrings)}}){{afterModifiers}} {
                            {{new[] { CppPrinter.Print(body) }.JoinAndIndent("    ")}}
                        }
                        """,
                    TypeDefinitionsReferenced: new[]
                    {
                        definition.Type,
                        returnType,
                        CppObjectHandle.GetCppType(context),
                        CppReinteropException.GetCppType(context)
                    }.Concat(parameters.Select(parameter => parameter.Type))
                ));
            }
        }
    }
}
