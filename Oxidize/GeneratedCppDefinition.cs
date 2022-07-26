namespace Oxidize
{
    internal class GeneratedCppDefinition
    {
        public GeneratedCppDefinition(CppType type)
        {
            this.Type = type;
        }

        public CppType Type;
        public List<GeneratedCppDefinitionElement> Elements = new List<GeneratedCppDefinitionElement>();

        public string ToSourceFileString()
        {
            if (Type == null)
                return "";

            return
                $$"""
                {{GetIncludes().JoinAndIndent("")}}

                {{GetForwardDeclarations().JoinAndIndent("")}}

                namespace {{Type.GetFullyQualifiedNamespace(false)}} {

                {{GetElements().JoinAndIndent("")}}

                }
                """;
        }

        private IEnumerable<string> GetIncludes()
        {
            HashSet<string> result = new HashSet<string>();

            foreach (GeneratedCppDefinitionElement element in Elements)
            {
                result.UnionWith(element.AdditionalIncludes);

                foreach (CppTypeReference reference in element.TypesReferenced)
                {
                    if (reference.Type != null && reference.RequiresCompleteDefinition)
                        reference.Type.AddSourceIncludesToSet(result);
                }
            }

            return result.Select(include => $"#include {include}");
        }

        private IEnumerable<string> GetForwardDeclarations()
        {
            HashSet<string> result = new HashSet<string>();

            foreach (GeneratedCppDefinitionElement element in Elements)
            {
                foreach (CppTypeReference reference in element.TypesReferenced)
                {
                    if (reference.Type != null && !reference.RequiresCompleteDefinition)
                        reference.Type.AddForwardDeclarationsToSet(result);
                }
            }

            return result;
        }

        private IEnumerable<string> GetElements()
        {
            return Elements.Select(element => element.Content);
        }
    }
}
