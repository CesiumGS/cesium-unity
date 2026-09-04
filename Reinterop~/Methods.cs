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
            return new CppInteropFunction(context, result.CppDefinition.Type, method.Name)
                .TypeParameters(method.TypeParameters)
                .TypeArguments(method.TypeArguments)
                .ReturnType(method.ReturnType)
                .Parameters(method.Parameters)
                .Static(method.IsStatic)
                .CSharp(item.Type, method);
        }

        public static void GenerateSingleMethod(CppGenerationContext context, GenerateTypeState state, TypeToGenerate item, GeneratedResult result, IMethodSymbol method)
        {
            CSharpFunctionCallableFromCpp interop = new CSharpFunctionCallableFromCpp(context, item.Type)
                .Name(method.Name)
                .TypeArguments(method.TypeArguments)
                .ReturnType(method.ReturnType)
                .Parameters(method.Parameters)
                .Static(method.IsStatic);

            if (method.MethodKind == MethodKind.UserDefinedOperator && method.Parameters.Length == 2)
                interop.Body(new CSharpFunctionCallableFromCpp.CSharpBodyInvokeBinaryOperator());
            else
                interop.Body(new CSharpFunctionCallableFromCpp.CSharpBodyInvokeMethod());

            // For op_Equality/op_Inequality, the interop function itself is private, and a public operator==/!= is added below to call it.
            bool addOperator = method.MethodKind == MethodKind.UserDefinedOperator && (method.Name == "op_Equality" || method.Name == "op_Inequality");
            interop.Private(addOperator);

            result.InteropFunctions2.Add(interop);

            if (method.IsGenericMethod)
            {
                // Add the template which will be specialized by this method.
                // We only need to do this once for all specializations.
                IMethodSymbol genericMethod = method.ConstructedFrom;
                CppInteropFunction genericRecipe = CreateCppInteropFunction(context, item, result, genericMethod);
                genericRecipe.Private(addOperator);
                if (state.MethodCache.TryGetValue(genericRecipe.FunctionPointerName, out CppInteropFunction? cachedRecipe))
                {
                    genericRecipe = cachedRecipe;
                }
                else
                {
                    state.MethodCache[genericRecipe.FunctionPointerName] = genericRecipe;
                    result.InteropFunctions.Add(genericRecipe);
                }

                // Declare that this recipe is a specialization.
                // recipe.Specializes(genericRecipe);
            }

            if (addOperator)
            {
                var functions = interop.CreatePairedInteropFunctions();

                string op = Interop.MethodNameToOperator(method.Name);
                CppParameter rhs = functions.cpp.Parameters()[1];
                CppFunction operatorRecipe = new CppFunction(context, result.CppDefinition.Type, "operator" + op)
                    .Parameters([rhs])
                    .ReturnType(CppType.Boolean.AsReturnType())
                    .DefinitionBody([
                        new CppReturn(
                            new CppCall(
                                new CppIdentifier(method.Name),
                                [new CppRaw("*this"), new CppIdentifier(rhs.Name)]
                            )
                        )
                    ]);
                result.InteropFunctions3.Add(operatorRecipe);

                // If this operator is on a base type and that base type is the right-hand side, also add a
                // version that takes this type, and a version that takes nullptr. This is a nice convenience
                // so that the user doesn't need to include the base class header file in order to compare
                // instances of this type.
                // Only do this for the first such operator, though, or we'll have multiply-defined symbols.
                if (!SymbolEqualityComparer.Default.Equals(method.ContainingType, item.Type) &&
                    SymbolEqualityComparer.Default.Equals(method.ContainingType, method.Parameters[1].Type) &&
                    IsMostDerivedVersionOfOperator(item.Type, method))
                {
                    CppType baseType = CppType.FromCSharp(context, CSharpType.FromSymbol(context, method.ContainingType));
                    CppFunction baseTypeRecipe = operatorRecipe.Clone()
                        .Parameters([new CppParameter(result.CppDefinition.Type.AsParameterType(), "rhs")])
                        .DefinitionBody([
                            new CppReturn(
                                new CppCall(
                                    new CppIdentifier(method.Name),
                                    [
                                        new CppRaw("*this"),
                                        new CppCast(baseType, new CppIdentifier("rhs"))
                                    ]
                                )
                            )
                        ]);
                    result.InteropFunctions3.Add(baseTypeRecipe);

                    CppFunction nullPtrRecipe = operatorRecipe.Clone()
                        .Parameters([new CppParameter(CppType.NullPointer.AsParameterType(), "")])
                        .DefinitionBody([
                            new CppReturn(
                                new CppCall(
                                    new CppIdentifier(method.Name),
                                    [
                                        new CppRaw("*this"),
                                        new CppCast(baseType, new CppIdentifier("nullptr"))
                                    ]
                                )
                            )
                        ]);
                    result.InteropFunctions3.Add(nullPtrRecipe);
                }
            }
        }

        private static bool IsMostDerivedVersionOfOperator(ITypeSymbol type, IMethodSymbol method)
        {
            ISymbol? first = CSharpTypeUtility
                .FindMembers(type, method.Name)
                .FirstOrDefault(
                    member => member is IMethodSymbol method &&
                    method.Parameters.Length == 2 &&
                    CSharpType.IsFirstDerivedFromSecond(type, method.Parameters[0].Type) &&
                    CSharpType.IsFirstDerivedFromSecond(type, method.Parameters[1].Type));
            return SymbolEqualityComparer.Default.Equals(first, method);
        }
    }
}
