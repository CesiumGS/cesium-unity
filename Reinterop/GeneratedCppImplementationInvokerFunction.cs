namespace Reinterop
{
    internal record GeneratedCppImplementationInvokerFunction(
        string Content,
        IEnumerable<CppType>? TypeDeclarationsReferenced = null,
        IEnumerable<CppType>? TypeDefinitionsReferenced = null,
        IEnumerable<string>? AdditionalIncludes = null)
        : GeneratedCppElement(TypeDeclarationsReferenced, TypeDefinitionsReferenced, AdditionalIncludes);
}