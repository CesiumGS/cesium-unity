namespace Oxidize
{
    internal class GeneratedCppDeclaration
    {
        public GeneratedCppDeclaration(CppType type)
        {
            this.Type = type;
        }

        public CppType Type;
        public List<GeneratedCppDeclarationElement> Elements = new List<GeneratedCppDeclarationElement>();

        public void AddToHeaderFile(CppSourceFile headerFile)
        {
            if (Type == null)
                return;

            AddIncludes(headerFile.Includes);
            AddForwardDeclarations(headerFile.ForwardDeclarations);

            CppSourceFileNamespace ns = headerFile.GetNamespace(Type.GetFullyQualifiedNamespace(false));
            
            string templateDeclaration = "";
            string templateSpecialization = "";
            if (Type.GenericArguments != null && Type.GenericArguments.Count > 0)
            {
                ns.Members.Add(
                    $$"""
                    template <{{string.Join(", ", Type.GenericArguments.Select((CppType type, int index) => "typename T" + index))}}>
                    {{GetTypeKind()}} {{Type.Name}};
                    """);
                templateDeclaration = "template <> ";
                templateSpecialization = $"<{string.Join(", ", Type.GenericArguments.Select(arg => arg.GetFullyQualifiedName()))}>";
            }

            ns.Members.Add($$"""
                {{templateDeclaration}}{{GetTypeKind()}} {{Type.Name}}{{templateSpecialization}} {
                  {{GetElements().JoinAndIndent("  ")}}
                };
                """);
        }

        private void AddIncludes(ISet<string> includes)
        {
            foreach (GeneratedCppDeclarationElement element in Elements)
            {
                element.AddIncludesToSet(includes);
            }
        }

        private void AddForwardDeclarations(ISet<string> forwardDeclarations)
        {
            if (Type.GenericArguments != null)
            {
                foreach (CppType genericArg in Type.GenericArguments)
                {
                    genericArg.AddForwardDeclarationsToSet(forwardDeclarations);
                }
            }

            foreach (GeneratedCppDeclarationElement element in Elements)
            {
                element.AddForwardDeclarationsToSet(forwardDeclarations);
            }
        }

        private string GetTypeKind()
        {
            if (Type == null)
                // TODO: report a compiler error instead.
                return "TypeIsNull";

            if (Type.Kind == InteropTypeKind.ClassWrapper || Type.Kind == InteropTypeKind.Delegate)
                return "class";
            else if (Type.Kind == InteropTypeKind.Enum)
                return "enum class";
            else
                return "struct";
        }

        private IEnumerable<string> GetElements()
        {
            return Elements.Select(decl =>
            {
                if (decl.Content == null)
                    return "";

                string access;
                if (this.Type.Kind == InteropTypeKind.Enum)
                    access = "";
                else if (decl.IsPrivate)
                    access = "private: ";
                else
                    access = "public: ";

                return $"{access}{decl.Content}";
            });
        }
    }
}
