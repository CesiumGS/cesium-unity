using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace Reinterop
{
    internal class Methods
    {
        public static void Generate(CppGenerationContext context, GenerateTypeState state, TypeToGenerate mainItem, TypeToGenerate currentItem, GeneratedResult result)
        {
            foreach (IMethodSymbol method in currentItem.Methods)
            {
                // Don't add static methods from base classes.
                // Unless they're operators, because operators become instance methods in C++.
                if (mainItem != currentItem && method.IsStatic && method.MethodKind != MethodKind.UserDefinedOperator)
                    continue;

                GenerateSingleMethod(context, state, mainItem, result, method);
            }
        }

        private static IMethodSymbol? FindMethod(TypeToGenerate item, Func<IMethodSymbol, bool> predicate)
        {
            IMethodSymbol? result = item.Methods.FirstOrDefault(predicate);
            if (result != null)
                return result;

            if (item.BaseClass != null)
                return FindMethod(item.BaseClass, predicate);

            return null;
        }

        private static CppInteropFunction CreateCppInteropFunction(CppGenerationContext context, TypeToGenerate item, GeneratedResult result, IMethodSymbol method)
        {
            string interopName = $"Call{method.Name}_{Interop.HashParameters(method.Parameters, method.TypeArguments)}";
            return new CppInteropFunction(context, result.CppDefinition.Type, interopName)
                .TypeParameters(method.TypeParameters.Select(parameter => new CppInteropParameter(parameter.Name, CppType.FromCSharp(context, parameter))))
                .TypeArguments(method.TypeArguments.Select(t => CppType.FromCSharp(context, t)))
                .ReturnType(CppType.FromCSharp(context, method.ReturnType).AsReturnType())
                .Parameters(method.Parameters.Select(parameter => new CppInteropParameter(parameter.Name, CppType.FromCSharp(context, parameter.Type).AsParameterType())))
                .Static(method.IsStatic);
        }

        public static void GenerateSingleMethod(CppGenerationContext context, GenerateTypeState state, TypeToGenerate item, GeneratedResult result, IMethodSymbol method)
        {
            CppInteropFunction recipe = CreateCppInteropFunction(context, item, result, method);

            GeneratedCppDeclaration declaration = result.CppDeclaration;
            GeneratedCppDefinition definition = result.CppDefinition;

            // For op_Equality/op_Inequality, the interop function itself is private, and a public operator==/!= is added below to call it.
            bool addOperator = method.MethodKind == MethodKind.UserDefinedOperator && (method.Name == "op_Equality" || method.Name == "op_Inequality");
            recipe.Private(addOperator);

            if (method.IsGenericMethod)
            {
                // Add the template which will be specialized by this method.
                // We only need to do this once for all specializations.
                IMethodSymbol genericMethod = method.ConstructedFrom;
                CppInteropFunction genericRecipe = CreateCppInteropFunction(context, item, result, genericMethod);
                genericRecipe.Private(addOperator);
                if (state.MethodCache.TryGetValue(genericRecipe.Name, out CppInteropFunction? cachedRecipe))
                {
                    genericRecipe = cachedRecipe;
                }
                else
                {
                    state.MethodCache[genericRecipe.Name] = genericRecipe;
                    genericRecipe.AddToGeneration(result, genericMethod.Name, null, null, null);
                }

                // Declare that this recipe is a specialization.
                recipe.Specializes(genericRecipe);
            }

            // A private, static field of function pointer type that will call into a managed delegate
            // for this method, initialized at startup, plus the method's own declaration and definition.
            var (csName, csContent) = Interop.CreateCSharpDelegateInit(context, item.Type, method, recipe.Name);
            recipe.AddToGeneration(result, method.Name, csName, csContent, recipe.Body());

            if (addOperator)
            {
                string typeTemplateSpecialization = CppInteropFunction.GetTypeTemplateSpecialization(definition.Type);
                string op = Interop.MethodNameToOperator(method.Name);
                CppInteropParameter rhs = recipe.Parameters()[1];
                declaration.Elements.Add(new(
                    Content: $"bool operator{op}({rhs.Type.GetFullyQualifiedName()} rhs) const;"
                ));
                definition.Elements.Add(new(
                    Content:
                        $$"""
                        bool {{definition.Type.Name}}{{typeTemplateSpecialization}}::operator{{op}}({{rhs.Type.GetFullyQualifiedName()}} rhs) const {
                          return {{method.Name}}(*this, rhs);
                        }
                        """,
                    TypeDefinitionsReferenced: recipe.ParameterTypes
                ));

                // If this operator is on a base type and that base type is the right-hand side, also add a
                // version that takes this type, and a version that takes nullptr. This is a nice convenience
                // so that the user doesn't need to include the base class header file in order to compare
                // instances of this type.
                // Only do this for the first such operator, though, or we'll have multiply-defined symbols.
                if (!SymbolEqualityComparer.Default.Equals(method.ContainingType, item.Type) &&
                    SymbolEqualityComparer.Default.Equals(method.ContainingType, method.Parameters[1].Type) &&
                    IsMostDerivedVersionOfOperator(item.Type, method))
                {
                    declaration.Elements.Add(new(
                        Content: $"bool operator{op}(const {declaration.Type.Name}& rhs) const;"
                    ));

                    CppType baseType = CppType.FromCSharp(context, method.ContainingType);
                    definition.Elements.Add(new(
                        Content:
                            $$"""
                            bool {{definition.Type.Name}}{{typeTemplateSpecialization}}::operator{{op}}(const {{declaration.Type.Name}}& rhs) const {
                            return {{method.Name}}(*this, {{baseType.GetFullyQualifiedName()}}(rhs));
                            }
                            """,
                        TypeDefinitionsReferenced: new[] { rhs.Type }
                    ));

                    declaration.Elements.Add(new(
                        Content: $"bool operator{op}(std::nullptr_t) const;"
                    ));

                    definition.Elements.Add(new(
                        Content:
                            $$"""
                            bool {{definition.Type.Name}}{{typeTemplateSpecialization}}::operator{{op}}(std::nullptr_t) const {
                            return {{method.Name}}(*this, {{baseType.GetFullyQualifiedName()}}(nullptr));
                            }
                            """,
                        TypeDefinitionsReferenced: new[] { rhs.Type }
                    ));
                }
            }
        }

        private static bool IsMostDerivedVersionOfOperator(ITypeSymbol type, IMethodSymbol method)
        {
            ISymbol? first = CSharpTypeUtility
                .FindMembers(type, method.Name)
                .Where(
                    member => member is IMethodSymbol method &&
                    method.Parameters.Length == 2 &&
                    CSharpType.IsFirstDerivedFromSecond(type, method.Parameters[0].Type) &&
                    CSharpType.IsFirstDerivedFromSecond(type, method.Parameters[1].Type)).FirstOrDefault();
            return SymbolEqualityComparer.Default.Equals(first, method);
        }
    }
}
