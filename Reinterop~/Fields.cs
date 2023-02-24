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

            string fieldName = field.Name;
            bool isPrivate = field.DeclaredAccessibility != Accessibility.Public;

            // If this is a backing field for an automatic property, use the property name instead.
            IPropertySymbol? autoProperty = field.AssociatedSymbol as IPropertySymbol;
            
            // Unfortunately, the above will only work if the C# compiler is looking at the source code for the property.
            // So for backing fields in referenced assemblies, we need to do this more manually.
            if (autoProperty == null && fieldName.EndsWith("__BackingField"))
            {
                Match match = new Regex("<(.+)>k__BackingField").Match(fieldName);
                if (match.Success && match.Groups.Count > 1)
                {
                    autoProperty = CSharpTypeUtility.FindMember(field.ContainingType, match.Groups[1].Value) as IPropertySymbol;
                }
            }

            if (autoProperty != null)
            {
                fieldName = autoProperty.Name;
                isPrivate = autoProperty.DeclaredAccessibility != Accessibility.Public;
            }

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
            CppType setInteropType = setType.AsInteropType();
            CppType getInteropType = getType.AsInteropType();

            string interopSetParameters = $"{setInteropType.GetFullyQualifiedName()} value";
            string interopSetParametersCall = $"{setType.GetConversionToInteropType(context, "value")}";
            if (!field.IsStatic)
            {
                interopSetParameters = $"void* thiz, " + interopSetParameters;
                interopSetParametersCall = $"{definition.Type.AsParameterType().GetConversionToInteropType(context, "(*this)")}, {interopSetParametersCall}";
            }

            IEnumerable<(string ParameterName, string CallSiteName, CppType Type, CppType InteropType)> getParameters = new List<(string ParameterName, string CallSiteName, CppType Type, CppType InteropType)>();
            if (!field.IsStatic)
            {
                getParameters = new[] { (ParameterName: "thiz", CallSiteName: "(*this)", Type: result.CppDefinition.Type.AsParameterType(), InteropType: result.CppDefinition.Type.AsInteropType()) }.Concat(getParameters);
            }

            bool hasStructRewrite = Interop.RewriteStructReturn(ref getParameters, ref getType, ref getInteropType);

            var interopGetParameters = getParameters.Select(parameter => $"{parameter.InteropType.GetFullyQualifiedName()} {parameter.ParameterName}");
            var interopGetParametersCall = getParameters.Select(parameter => parameter.Type.GetConversionToInteropType(context, parameter.CallSiteName));

            // Add the static fields for the get/set functions
            declaration.Elements.Add(new(
                Content: $"static {getInteropType.GetFullyQualifiedName()} (*Field_get_{field.Name})({string.Join(", ", interopGetParameters)});",
                IsPrivate: true,
                TypeDeclarationsReferenced: new[] { getInteropType }));
            definition.Elements.Add(new(
                Content: $"{getInteropType.GetFullyQualifiedName()} (*{definition.Type.GetFullyQualifiedName(false)}::Field_get_{field.Name})({string.Join(", ", interopGetParameters)}) = nullptr;",
                TypeDeclarationsReferenced: new[] { getInteropType }
            ));

            declaration.Elements.Add(new(
                Content: $"static void (*Field_set_{field.Name})({interopSetParameters});",
                IsPrivate: true,
                TypeDeclarationsReferenced: new[] { setInteropType }));
            definition.Elements.Add(new(
                Content: $"void (*{definition.Type.GetFullyQualifiedName(false)}::Field_set_{field.Name})({interopSetParameters}) = nullptr;",
                TypeDeclarationsReferenced: new[] { setInteropType }
            ));

            // Initialize the fields at startup
            var (csName, csContent) = Interop.CreateCSharpDelegateInit(context, item.Type, field, isGet: true);
            init.Functions.Add(new(
                CppName: $"{definition.Type.GetFullyQualifiedName()}::Field_get_{field.Name}",
                CppTypeSignature: $"{getInteropType.GetFullyQualifiedName()} (*)({string.Join(", ", interopGetParameters)})",
                CppTypeDefinitionsReferenced: new[] { definition.Type },
                CppTypeDeclarationsReferenced: new[] { getInteropType },
                CSharpName: csName,
                CSharpContent: csContent
            ));

            (csName, csContent) = Interop.CreateCSharpDelegateInit(context, item.Type, field, isGet: false);
            init.Functions.Add(new(
                CppName: $"{definition.Type.GetFullyQualifiedName()}::Field_set_{field.Name}",
                CppTypeSignature: $"void (*)({interopSetParameters})",
                CppTypeDefinitionsReferenced: new[] { definition.Type },
                CppTypeDeclarationsReferenced: new[] { setInteropType },
                CSharpName: csName,
                CSharpContent: csContent
            ));

            // Method declaration
            declaration.Elements.Add(new(
                Content: $"{(field.IsStatic ? "static " : "")}{getType.GetFullyQualifiedName()} {field.Name}(){(field.IsStatic ? "" : " const")};",
                TypeDeclarationsReferenced: new[] { getType }
            ));
            declaration.Elements.Add(new(
                Content: $"{(field.IsStatic ? "static " : "")}void {field.Name}({setType.GetFullyQualifiedName()} value){(field.IsStatic ? "" : " const")};",
                TypeDeclarationsReferenced: new[] { setType }
            ));

            string[] invocation = new[]
            {
                $"auto result = Field_get_{field.Name}({string.Join(", ", interopGetParametersCall)});",
                $"return {getType.GetConversionFromInteropType(context, "result")};"
            };
            if (hasStructRewrite)
            {
                if (fieldType.Kind == InteropTypeKind.Nullable)
                {
                    invocation = new[]
                    {
                        $"{getType.GenericArguments.FirstOrDefault().GetFullyQualifiedName()} result;",
                        $"std::uint8_t resultIsValid = Field_get_{field.Name}({string.Join(", ", interopGetParametersCall)});",
                        $"return resultIsValid ? std::make_optional(std::move({getType.GetConversionFromInteropType(context, "result")})) : std::nullopt;"
                    };
                }
                else
                {
                    invocation = new[]
                    {
                        $"{getType.GetFullyQualifiedName()} result;",
                        $"Field_get_{field.Name}({string.Join(", ", interopGetParametersCall)});",
                        $"return {getType.GetConversionFromInteropType(context, "result")};"
                    };
                }
            }

            definition.Elements.Add(new(
                Content:
                    $$"""
                    {{getType.GetFullyQualifiedName()}} {{definition.Type.Name}}::{{field.Name}}(){{(field.IsStatic ? "" : " const")}} {
                        {{GenerationUtility.JoinAndIndent(invocation, "    ")}}
                    }
                    """,
                TypeDefinitionsReferenced: new[]
                {
                    definition.Type,
                    getType,
                    CppObjectHandle.GetCppType(context)
                }
            ));

            definition.Elements.Add(new(
                Content:
                    $$"""
                    void {{definition.Type.Name}}::{{field.Name}}({{setType.GetFullyQualifiedName()}} value){{(field.IsStatic ? "" : " const")}} {
                        Field_set_{{field.Name}}({{interopSetParametersCall}});
                    }
                    """,
                TypeDefinitionsReferenced: new[]
                {
                    definition.Type,
                    setType,
                    CppObjectHandle.GetCppType(context)
                }
            ));
        }
    }
}
