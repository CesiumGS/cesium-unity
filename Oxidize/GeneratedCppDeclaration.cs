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

        public string ToHeaderFileString()
        {
            if (Type == null)
                return "";

            string templateDeclaration = "";
            string templateSpecialization = "";
            if (Type.GenericArguments != null && Type.GenericArguments.Count > 0)
            {
                templateDeclaration =
                    $$"""
                    template <{{string.Join(", ", Type.GenericArguments.Select((CppType type, int index) => "typename T" + index))}}>
                    {{GetTypeKind()}} {{Type.Name}} {};

                    template <>

                    """;
                templateSpecialization = $"<{string.Join(", ", Type.GenericArguments.Select(arg => arg.GetFullyQualifiedName()))}>";
            }

            return
                $$"""
                #pragma once

                {{GetIncludes().JoinAndIndent("")}}
                
                {{GetForwardDeclarations().JoinAndIndent("")}}

                namespace {{Type.GetFullyQualifiedNamespace(false)}} {

                {{templateDeclaration}}{{GetTypeKind()}} {{Type.Name}}{{templateSpecialization}} {
                  {{GetElements().JoinAndIndent("  ")}}
                };

                } // namespace {{Type.GetFullyQualifiedNamespace(false)}}
                """;
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

            if (Type.Kind == CppTypeKind.ClassWrapper)
                return "class";
            else if (Type.Kind == CppTypeKind.Enum)
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
                if (this.Type.Kind == CppTypeKind.Enum)
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
