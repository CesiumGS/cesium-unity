namespace Oxidize
{
    internal enum CppTypeKind
    {
        Unknown,
        Primitive,
        BlittableStruct,
        NonBlittableStructWrapper,
        ClassWrapper,
        Enum
    }
}
