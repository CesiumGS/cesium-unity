namespace Oxidize
{
    internal enum InteropTypeKind
    {
        Unknown,
        Primitive,
        BlittableStruct,
        NonBlittableStructWrapper,
        ClassWrapper,
        Enum,
        GenericParameter,
        Delegate
    }
}
