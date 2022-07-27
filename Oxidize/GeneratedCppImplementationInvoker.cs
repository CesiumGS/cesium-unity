using System.Xml.Linq;

namespace Oxidize
{
    internal class GeneratedCppImplementationInvoker
    {
        public GeneratedCppImplementationInvoker(CppType implementationType)
        {
            this.ImplementationType = implementationType;
        }

        public CppType ImplementationType;
        public List<GeneratedCppImplementationInvokerFunction> Functions = new List<GeneratedCppImplementationInvokerFunction>();

        public string ToSourceFileString()
        {
            return
                $$"""
                {{GetIncludes().JoinAndIndent("")}}
                
                {{GetForwardDeclarations().JoinAndIndent("")}}

                extern "C" {
                
                {{GetFunctions().JoinAndIndent("")}}

                } // extern "C"
                """;
        }

        private IEnumerable<string> GetIncludes()
        {
            HashSet<string> result = new HashSet<string>();

            foreach (GeneratedCppImplementationInvokerFunction function in Functions)
            {
                function.AddIncludesToSet(result);
            }

            return result.Select(include => $"#include {include}");
        }

        private IEnumerable<string> GetForwardDeclarations()
        {
            HashSet<string> result = new HashSet<string>();

            foreach (GeneratedCppImplementationInvokerFunction function in Functions)
            {
                function.AddForwardDeclarationsToSet(result);
            }

            return result;
        }

        private IEnumerable<string> GetFunctions()
        {
            return Functions.Select(f => f.Content);
        }
    }
}