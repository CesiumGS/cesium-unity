using Microsoft.CodeAnalysis;

namespace Reinterop
{
    internal class CppHandleManagement
    {
        internal static void Generate(CppGenerationContext context, TypeToGenerate item, GeneratedResult result)
        {
            GeneratedCppDeclaration declaration = result.CppDeclaration;
            GeneratedCppDefinition definition = result.CppDefinition;

            // We only need handle management for non-static classes.
            if (item.Type.IsStatic)
                return;

            // Only classes, delegates, and non-blittable struct wrappers have handles.
            if (declaration.Type.Kind != InteropTypeKind.ClassWrapper &&
                declaration.Type.Kind != InteropTypeKind.NonBlittableStructWrapper &&
                declaration.Type.Kind != InteropTypeKind.Delegate)
            {
                return;
            }

            CppType type = CppType.FromCSharp(context, item.Type);
            CppType objectHandleType = CppObjectHandle.GetCppType(context);

            string templateSpecialization = "";
            if (declaration.Type.GenericArguments != null && declaration.Type.GenericArguments.Count > 0)
            {
                templateSpecialization = $"<{string.Join(", ", declaration.Type.GenericArguments.Select(arg => arg.GetFullyQualifiedName()))}>";
            }

            // The handle to the managed object
            declaration.Elements.Add(new(
                Content: $"{objectHandleType.GetFullyQualifiedName()} _handle;",
                IsPrivate: true,
                TypeDefinitionsReferenced: new[] { objectHandleType }
            ));

            // Construct from an object handle
            CppInteropFunction objectHandleConstructorRecipe = new CppInteropFunction(context, result.Type, type.Name)
                .Parameters([new CppInteropParameter("handle", objectHandleType.AsMovableParameterType())])
                .Explicit(true)
                .NoExcept(true)
                .Static(true)
                .MemberInitializers([ new CppMemberInitializer("_handle", new CppMove(new CppIdentifier("handle"))) ])
                .DefinitionBody([]);

            result.InteropFunctions.Add(objectHandleConstructorRecipe);

            CppInteropFunction nullConstructorRecipe = new CppInteropFunction(context, result.Type, type.Name)
                .Parameters([new CppInteropParameter("handle", CppType.NullPointer)])
                .NoExcept(true)
                .Static(true)
                .MemberInitializers([ new CppMemberInitializer("_handle", new CppIdentifier("handle")) ])
                .DefinitionBody([]);

            result.InteropFunctions.Add(nullConstructorRecipe);

            // For simple types without an overloaded operator==, we can check
            // to see if a wrapper represents a null reference without leaving
            // C++ land.
            //
            // But if such an operator does exist, we have to use it, even if
            // that means a call into C#.
            bool hasOverloadedOperatorEquals = CSharpTypeUtility
                .FindMembers(item.Type, "op_Equality")
                .Where(
                    op => op is IMethodSymbol method &&
                    method.ReturnType.SpecialType == SpecialType.System_Boolean &&
                    method.Parameters.Length == 2 &&
                    SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, method.ContainingType) &&
                    SymbolEqualityComparer.Default.Equals(method.Parameters[1].Type, method.ContainingType))
                .Any();

            if (!hasOverloadedOperatorEquals)
            {
                CppInteropFunction equalityOperatorRecipe = new CppInteropFunction(context, result.Type, "operator==")
                    .ReturnType(CppType.Boolean.AsReturnType())
                    .Parameters([new CppInteropParameter("", CppType.NullPointer)])
                    .NoExcept(true)
                    .DefinitionBody([
                        new CppReturn(new CppBinary("==", new CppCall(new CppRaw("_handle.GetRaw"), []), new CppIdentifier("nullptr")))
                    ]);
                result.InteropFunctions.Add(equalityOperatorRecipe);

                CppInteropFunction inequalityOperatorRecipe = new CppInteropFunction(context, result.Type, "operator!=")
                    .ReturnType(CppType.Boolean.AsReturnType())
                    .Parameters([new CppInteropParameter("", CppType.NullPointer)])
                    .NoExcept(true)
                    .DefinitionBody([
                        new CppReturn(new CppBinary("!=", new CppCall(new CppRaw("_handle.GetRaw"), []), new CppIdentifier("nullptr")))
                    ]);
                result.InteropFunctions.Add(inequalityOperatorRecipe);
            }

            // Get handle
            CppInteropFunction getHandleRecipe = new CppInteropFunction(context, result.Type, "GetHandle")
                .DefinitionBody([
                    new CppReturn(new CppIdentifier("_handle"))
                ]);
            result.InteropFunctions.Add(getHandleRecipe.Clone().Const(true).ReturnType(objectHandleType.AsConstReference()));
            result.InteropFunctions.Add(getHandleRecipe.Clone().Const(false).ReturnType(objectHandleType.AsReference()));
        }
    }
}
