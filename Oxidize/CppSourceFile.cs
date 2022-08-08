using System.Text;

namespace Oxidize
{
    internal class CppSourceFileNamespace
    {
        public List<string> Members = new List<string>();
    }

    internal class CppSourceFile
    {
        public string Filename = "";
        public bool IsHeaderFile = true;
        public HashSet<string> Includes = new HashSet<string>();
        public HashSet<string> ForwardDeclarations = new HashSet<string>();
        public Dictionary<string, CppSourceFileNamespace> Namespaces = new Dictionary<string, CppSourceFileNamespace>();

        public CppSourceFileNamespace GetNamespace(string name)
        {
            CppSourceFileNamespace ns;
            if (!this.Namespaces.TryGetValue(name, out ns))
            {
                ns = new CppSourceFileNamespace();
                this.Namespaces.Add(name, ns);
            }

            return ns;
        }

        public string ToContentString()
        {
            string content =
                $$"""
                {{Includes.JoinAndIndent("")}}
                
                {{ForwardDeclarations.JoinAndIndent("")}}

                {{Namespaces.Select(kvp => GetNamespace(kvp.Key, kvp.Value)).JoinAndIndent("")}}
                """;
            if (IsHeaderFile)
                return "#pragma once" + Environment.NewLine + Environment.NewLine + content;
            else
                return content;
        }

        public void Write()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Filename));
            File.WriteAllText(Filename, this.ToContentString(), Encoding.UTF8);
        }

        private string GetNamespace(string name, CppSourceFileNamespace content)
        {
            return
                $$"""
                namespace {{name}} {

                {{content.Members.JoinAndIndent(indent: "", newlineBetweenEach: true)}}

                } // namespace {{name}}

                """;
        }
    }
}
