using Microsoft.CodeAnalysis;

namespace Reinterop
{
    internal class CppHandleManagement
    {
        internal static void Generate(CppGenerationContext context, TypeToGenerate item, GeneratedResult result)
        {
            // We only need handle management for non-static classes.
            if (item.Type.IsStatic)
                return;

            // Only classes, delegates, and non-blittable struct wrappers have handles.
            if (result.Type.Kind != InteropTypeKind.ClassWrapper &&
                result.Type.Kind != InteropTypeKind.NonBlittableStructWrapper &&
                result.Type.Kind != InteropTypeKind.Delegate)
            {
                return;
            }

            CppType objectHandleType = CppObjectHandle.GetCppType(context);

            // The handle to the managed object
            result.CppDeclaration.Elements.Add(new(
                Content: $"{objectHandleType.GetFullyQualifiedName()} _handle;",
                IsPrivate: true,
                TypeDefinitionsReferenced: new[] { objectHandleType }
            ));

            // Construct from an object handle
            CppInteropFunction objectHandleConstructorRecipe = new CppInteropFunction(context, result.Type, result.Type.Name)
                .Parameters([new CppInteropParameter("handle", objectHandleType.AsMovableParameterType())])
                .Explicit(true)
                .NoExcept(true)
                .Static(true)
                .MemberInitializers([ new CppMemberInitializer("_handle", new CppMove(new CppIdentifier("handle"))) ])
                .DefinitionBody([]);
            result.InteropFunctions.Add(objectHandleConstructorRecipe);

            // Construct from a null pointer
            CppInteropFunction nullConstructorRecipe = new CppInteropFunction(context, result.Type, result.Type.Name)
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
                .Any(
                    op => op is IMethodSymbol method &&
                    method.ReturnType.SpecialType == SpecialType.System_Boolean &&
                    method.Parameters.Length == 2 &&
                    SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, method.ContainingType) &&
                    SymbolEqualityComparer.Default.Equals(method.Parameters[1].Type, method.ContainingType));

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
