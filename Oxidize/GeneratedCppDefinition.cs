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

        public void AddToSourceFile(CppSourceFile sourceFile)
        {
            if (Type == null)
                return;

            sourceFile.Includes.UnionWith(GetIncludes());
            sourceFile.ForwardDeclarations.UnionWith(GetForwardDeclarations());
            CppSourceFileNamespace ns = sourceFile.GetNamespace(Type.GetFullyQualifiedNamespace(false));
            ns.Members.AddRange(GetElements());
        }

        private IEnumerable<string> GetIncludes()
        {
            HashSet<string> result = new HashSet<string>();

            this.Type.AddSourceIncludesToSet(result);

            foreach (GeneratedCppDefinitionElement element in Elements)
            {
                element.AddIncludesToSet(result);
            }

            return result.Select(include => $"#include {include}");
        }

        private IEnumerable<string> GetForwardDeclarations()
        {
            HashSet<string> result = new HashSet<string>();

            foreach (GeneratedCppDefinitionElement element in Elements)
            {
                element.AddForwardDeclarationsToSet(result);
            }

            return result;
        }

        private IEnumerable<string> GetElements()
        {
            return Elements.Select(element => element.Content + Environment.NewLine);
        }
    }
}
