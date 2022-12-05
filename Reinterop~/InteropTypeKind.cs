namespace Reinterop
{
    internal enum InteropTypeKind
    {
        Unknown,
        Primitive,
        BlittableStruct,
        NonBlittableStructWrapper,
        ClassWrapper,
        Enum,
        EnumFlags,
        GenericParameter,
        Delegate,
        Nullable
    }
}
