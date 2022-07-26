namespace Oxidize
{
    internal class GeneratedCppDeclarationElement : GeneratedCppElement
    {
        public GeneratedCppDeclarationElement(string content, IEnumerable<CppTypeReference> typesReferenced, bool isPrivate)
            : base(content, typesReferenced)
        {
            this.IsPrivate = isPrivate;
        }

        public bool IsPrivate;
    }
}
