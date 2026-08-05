using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;

namespace Reinterop
{
    internal class Fields
    {
        public static void Generate(CppGenerationContext context, TypeToGenerate mainItem, TypeToGenerate currentItem, GeneratedResult result)
        {
            if (result.CppDeclaration.Type.Kind == InteropTypeKind.BlittableStruct)
            {
                // For blittable value types, include every field (not just the used ones).
                // But don't generate fields for base classes.
                if (mainItem == currentItem)
                    GenerateEveryField(context, mainItem, result);
            }
            else
            {
                // For other types, generate accessors for the fields we actually need.
                GenerateFieldAccessors(context, mainItem, currentItem, result);
            }
        }

        private static void GenerateEveryField(CppGenerationContext context, TypeToGenerate item, GeneratedResult result)
        {
            ImmutableArray<ISymbol> members = item.Type.GetMembers();
            foreach (ISymbol member in members)
            {
                IFieldSymbol? field = member as IFieldSymbol;
                if (field == null)
                    continue;

                GenerateField(context, item, field, result);
            }
        }

        private static void GenerateField(CppGenerationContext context, TypeToGenerate item, IFieldSymbol field, GeneratedResult result)
        {
            // Skip static fields
            // TODO: Implement these as functions that call into the C#?
            if (field.IsStatic)
                return;
                
            // Skip any fields with an "Ignore" attribute.
            if (field.GetAttributes().Where(attrib => attrib.AttributeClass != null && (attrib.AttributeClass.Name == "IgnoreAttribute")).Any())
                return;

            string fieldName = CSharpTypeUtility.GetFieldName(field);
            bool isPrivate = CSharpTypeUtility.GetFieldIsPrivate(field);

            CppType fieldType = CppType.FromCSharp(context, field.Type);

            result.CppDeclaration.Elements.Add(new(
                Content: $"{fieldType.GetFullyQualifiedName()} {fieldName};",
                IsPrivate: isPrivate,
                TypeDefinitionsReferenced: new[] { fieldType }));
        }

        private static void GenerateFieldAccessors(CppGenerationContext context, TypeToGenerate mainItem, TypeToGenerate currentItem, GeneratedResult result)
        {
            foreach (IFieldSymbol field in currentItem.Fields)
            {
                GenerateSingleFieldAccessors(context, mainItem, field, result);
            }
        }

        private static void GenerateSingleFieldAccessors(CppGenerationContext context, TypeToGenerate item, IFieldSymbol field, GeneratedResult result)
        {
            GeneratedCppDeclaration declaration = result.CppDeclaration;
            GeneratedCppDefinition definition = result.CppDefinition;
            GeneratedInit init = result.Init;

            CppType fieldType = CppType.FromCSharp(context, field.Type);
            CppType setType = fieldType.AsParameterType();
            CppType getType = fieldType.AsReturnType();

            CppType? instanceType = !field.IsStatic ? result.CppDefinition.Type.AsParameterType() : null;

            CppInteropFunction getRecipe = new(context, $"Field_get_{field.Name}", Array.Empty<CppInteropParameter>(), getType, instanceType);
            CppInteropFunction setRecipe = new(context, $"Field_set_{field.Name}", new[] { new CppInteropParameter("value", setType) }, CppType.Void, instanceType);

            // Add the static fields for the get/set functions, initialized at startup.
            string qualifiedDefinitionName = definition.Type.GetFullyQualifiedName(false);
            var (getCsName, getCsContent) = Interop.CreateCSharpDelegateInit(context, item.Type, field, isGet: true);
            getRecipe.AddToGeneration(result, qualifiedDefinitionName, getCsName, getCsContent);

            var (setCsName, setCsContent) = Interop.CreateCSharpDelegateInit(context, item.Type, field, isGet: false);
            setRecipe.AddToGeneration(result, qualifiedDefinitionName, setCsName, setCsContent);

            // Method declaration
            declaration.Elements.Add(new(
                Content: $"{(field.IsStatic ? "static " : "")}{getType.GetFullyQualifiedName()} {field.Name}(){(field.IsStatic ? "" : " const")};",
                TypeDeclarationsReferenced: new[] { getType }
            ));
            declaration.Elements.Add(new(
                Content: $"{(field.IsStatic ? "static " : "")}void {field.Name}({setType.GetFullyQualifiedName()} value){(field.IsStatic ? "" : " const")};",
                TypeDeclarationsReferenced: new[] { setType }
            ));

            // The Nullable-with-struct-rewrite case (getter returns a bool "is valid" flag alongside an
            // out-parameter) isn't modeled by CppInteropFunction.Body, so it's still built as a plain
            // string template - same deliberate scope exclusion as Methods.cs.
            IReadOnlyList<CppStatement>? body = getRecipe.Body(new CppIdentifier($"Field_get_{field.Name}"));
            string[]? invocation = null;
            if (body == null)
            {
                var parameterPassStrings = getRecipe.InteropParameters.Select(parameter => parameter.Type.GetConversionToInteropType(context, parameter.CallSiteName));
                parameterPassStrings = parameterPassStrings.Concat(new[] { "&reinteropException" }).Where(s => !string.IsNullOrEmpty(s));

                invocation = new[]
                {
                    $"void* reinteropException = nullptr;",
                    $"{getType.GenericArguments.FirstOrDefault().GetFullyQualifiedName()} result;",
                    $"std::uint8_t resultIsValid = Field_get_{field.Name}({string.Join(", ", parameterPassStrings)});",
                    $"if (reinteropException != nullptr) {{",
                    $"  throw Reinterop::ReinteropNativeException(::DotNet::System::Exception(::DotNet::Reinterop::ObjectHandle(reinteropException)));",
                    $"}}",
                    $"return resultIsValid ? std::make_optional(std::move({getType.GetConversionFromInteropType(context, "result")})) : std::nullopt;"
                };
            }

            definition.Elements.Add(new(
                Content:
                    body != null
                    ? $$"""
                        {{getType.GetFullyQualifiedName()}} {{definition.Type.Name}}::{{field.Name}}(){{(field.IsStatic ? "" : " const")}} {
                            {{new[] { CppPrinter.Print(body) }.JoinAndIndent("    ")}}
                        }
                        """
                    : $$"""
                        {{getType.GetFullyQualifiedName()}} {{definition.Type.Name}}::{{field.Name}}(){{(field.IsStatic ? "" : " const")}} {
                            {{GenerationUtility.JoinAndIndent(invocation!, "    ")}}
                        }
                        """,
                TypeDefinitionsReferenced: new[]
                {
                    definition.Type,
                    getType,
                    CppObjectHandle.GetCppType(context),
                    CppReinteropException.GetCppType(context)
                }
            ));

            IReadOnlyList<CppStatement> setterBody = CppInterop.CallManagedFunction(
                new CppIdentifier($"Field_set_{field.Name}"), setRecipe.CallArguments());
            definition.Elements.Add(new(
                Content:
                    $$"""
                    void {{definition.Type.Name}}::{{field.Name}}({{setType.GetFullyQualifiedName()}} value){{(field.IsStatic ? "" : " const")}} {
                        {{new[] { CppPrinter.Print(setterBody) }.JoinAndIndent("    ")}}
                    }
                    """,
                TypeDefinitionsReferenced: new[]
                {
                    definition.Type,
                    setType,
                    CppObjectHandle.GetCppType(context),
                    CppReinteropException.GetCppType(context)
                }
            ));
        }
    }
}
