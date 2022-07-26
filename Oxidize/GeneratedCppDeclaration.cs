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

            return
                $$"""
                #pragma once

                {{GetIncludes().JoinAndIndent("")}}
                
                {{GetForwardDeclarations().JoinAndIndent("")}}

                namespace {{Type.GetFullyQualifiedNamespace(false)}} {

                {{GetTypeKind()}} {{Type.Name}} {
                  {{GetElements().JoinAndIndent("  ")}}
                };
                """;
        }

        private IEnumerable<string> GetIncludes()
        {
            HashSet<string> result = new HashSet<string>();

            foreach (GeneratedCppDeclarationElement element in Elements)
            {
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

            foreach (GeneratedCppDeclarationElement element in Elements)
            {
                foreach (CppTypeReference reference in element.TypesReferenced)
                {
                    if (reference.Type != null && !reference.RequiresCompleteDefinition)
                        reference.Type.AddForwardDeclarationsToSet(result);
                }
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
            else
                return "struct";
        }

        private IEnumerable<string> GetElements()
        {
            return Elements.Select(decl =>
            {
                if (decl.Content == null)
                    return "";

                string access = decl.IsPrivate ? "private" : "public";
                return $"{access}: {decl.Content}";
            });
        }
    }
}
