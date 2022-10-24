namespace Reinterop
{
    internal record GeneratedCppDeclarationElement(
        string Content,
        bool IsPrivate = false,
        IEnumerable<CppType>? TypeDeclarationsReferenced = null,
        IEnumerable<CppType>? TypeDefinitionsReferenced = null,
        IEnumerable<string>? AdditionalIncludes = null)
        : GeneratedCppElement(TypeDeclarationsReferenced, TypeDefinitionsReferenced, AdditionalIncludes);
}
