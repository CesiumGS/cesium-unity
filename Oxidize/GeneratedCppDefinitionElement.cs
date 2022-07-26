namespace Oxidize
{
    internal class GeneratedCppDefinitionElement : GeneratedCppElement
    {
        public GeneratedCppDefinitionElement(string content, IEnumerable<CppTypeReference> typesReferenced, IEnumerable<string>? additionalIncludes = null) : base(content, typesReferenced)
        {
            this.AdditionalIncludes = additionalIncludes == null ? new List<string>() : new List<string>(additionalIncludes);
        }

        public List<string> AdditionalIncludes;
    }
}
