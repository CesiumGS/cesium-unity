using System.Text;

namespace Reinterop
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

        public CppSourceFileNamespace GlobalNamespace = new CppSourceFileNamespace();

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

        public CppSourceFileNamespace GetGlobalNamespace() 
        {
          return this.GlobalNamespace;
        }

        public string ToContentString()
        {
            string content =
                $$"""
                {{Includes.Select(i => $"#include {i}").JoinAndIndent("")}}
                
                {{ForwardDeclarations.JoinAndIndent("")}}

                {{Namespaces.Select(kvp => GetNamespace(kvp.Key, kvp.Value)).JoinAndIndent("")}}

                {{GetNamespace(null, GetGlobalNamespace())}}
                """;
            if (IsHeaderFile)
                return "#pragma once" + Environment.NewLine + Environment.NewLine + content;
            else
                return content;
        }

        public void Write(CppGenerationContext options)
        {
            string path = Path.Combine(options.OutputDirectory, Filename);
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string newContent = this.ToContentString();

            if (File.Exists(path))
            {
                string existing = File.ReadAllText(path, Encoding.UTF8);

                // If the content hasn't changed, there's no need to rewrite it.
                if (existing == newContent)
                    return;
            }

            File.WriteAllText(path, newContent, Encoding.UTF8);
        }

        private string GetNamespace(string? name, CppSourceFileNamespace content)
        {
            if (string.IsNullOrEmpty(name))
                return content.Members.JoinAndIndent(indent: "", newlineBetweenEach: true);

            return
                $$"""
                namespace {{name}} {

                {{content.Members.JoinAndIndent(indent: "", newlineBetweenEach: true)}}

                } // namespace {{name}}

                """;
        }
    }
}
