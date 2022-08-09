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

            headerFile.Includes.UnionWith(GetIncludes());
            headerFile.ForwardDeclarations.UnionWith(GetForwardDeclarations());

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

        private IEnumerable<string> GetIncludes()
        {
            HashSet<string> result = new HashSet<string>();

            foreach (GeneratedCppDeclarationElement element in Elements)
            {
                element.AddIncludesToSet(result);
            }

            return result.Select(include => $"#include {include}");
        }

        private IEnumerable<string> GetForwardDeclarations()
        {
            HashSet<string> result = new HashSet<string>();

            if (Type.GenericArguments != null)
            {
                foreach (CppType genericArg in Type.GenericArguments)
                {
                    genericArg.AddForwardDeclarationsToSet(result);
                }
            }

            foreach (GeneratedCppDeclarationElement element in Elements)
            {
                element.AddForwardDeclarationsToSet(result);
            }

            return result;
        }

        private string GetTypeKind()
        {
            if (Type == null)
                // TODO: report a compiler error instead.
                return "TypeIsNull";

            if (Type.Kind == InteropTypeKind.ClassWrapper)
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
