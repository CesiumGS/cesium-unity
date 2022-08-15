namespace Reinterop
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

            AddIncludes(sourceFile.Includes);
            AddForwardDeclarations(sourceFile.ForwardDeclarations);
            CppSourceFileNamespace ns = sourceFile.GetNamespace(Type.GetFullyQualifiedNamespace(false));
            ns.Members.AddRange(GetElements());
        }

        private void AddIncludes(ISet<string> includes)
        {
            this.Type.AddSourceIncludesToSet(includes);

            foreach (GeneratedCppDefinitionElement element in Elements)
            {
                element.AddIncludesToSet(includes);
            }
        }

        private void AddForwardDeclarations(ISet<string> forwardDeclarations)
        {
            foreach (GeneratedCppDefinitionElement element in Elements)
            {
                element.AddForwardDeclarationsToSet(forwardDeclarations);
            }
        }

        private IEnumerable<string> GetElements()
        {
            return Elements.Select(element => element.Content + Environment.NewLine);
        }
    }
}
