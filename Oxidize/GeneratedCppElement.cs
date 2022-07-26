namespace Oxidize
{
    internal class GeneratedCppElement
    {
        public GeneratedCppElement(string content, IEnumerable<CppTypeReference> typesReferenced)
        {
            this.Content = content;
            this.TypesReferenced = new List<CppTypeReference>(typesReferenced);
        }

        public string Content;
        public List<CppTypeReference> TypesReferenced = new List<CppTypeReference>();
    }
}
