namespace Reinterop
{
    internal class Casts
    {
        public static void Generate(CppGenerationContext context, TypeToGenerate item, GeneratedResult result)
        {
            // It only makes sense to cast instances, so static class need not apply.
            if (item.Type.IsStatic)
                return;

            // Don't allow conversion of value types
            // TODO: but we could, by boxing them
            if (item.Type.IsValueType)
                return;

            if (result.Type.Kind != InteropTypeKind.ClassWrapper &&
                result.Type.Kind != InteropTypeKind.NonBlittableStructWrapper &&
                result.Type.Kind != InteropTypeKind.Delegate)
            {
                return;
            }

            // Generate implicit conversions to all base classes.
            TypeToGenerate? baseClass = item.BaseClass;
            while (baseClass != null)
            {
                CSharpType csBaseType = CSharpType.FromSymbol(context, baseClass.Type);
                CppType baseType = CppType.FromCSharp(context, csBaseType);
                result.InteropFunctions.Add(CreateCast(context, result, baseType));

                baseClass = baseClass.BaseClass;
            }

            // Generate implicit conversions to all interfaces.
            foreach (TypeToGenerate anInterface in item.Interfaces)
            {
                CSharpType csInterfaceType = CSharpType.FromSymbol(context, anInterface.Type);
                CppType interfaceType = CppType.FromCSharp(context, csInterfaceType);
                result.InteropFunctions.Add(CreateCast(context, result, interfaceType));
            }
        }

        private static CppInteropFunction CreateCast(CppGenerationContext context, GeneratedResult result, CppType targetType)
        {
            CppType objectHandleType = CppObjectHandle.GetCppType(context);
            return new CppInteropFunction(context, result.Type, $"operator {targetType.GetFullyQualifiedName()}")
                .ReturnType(targetType)
                .Static(false)
                .DefinitionBody([
                    new CppReturn(new CppCast(
                        targetType,
                        new CppCall(
                            new CppIdentifier(objectHandleType.GetFullyQualifiedName()),
                            [new CppRaw("this->_handle")])
                        )
                    )
                ]);
        }
    }
}
