namespace Reinterop
{
    internal class GeneratedCppImplementationInvoker
    {
        public GeneratedCppImplementationInvoker(CppType implementationType)
        {
            this.ImplementationType = implementationType;
        }

        public CppType ImplementationType;
        public List<GeneratedCppImplementationInvokerFunction> Functions = new List<GeneratedCppImplementationInvokerFunction>();

        public void AddToSourceFile(CppSourceFile sourceFile)
        {
            if (ImplementationType == null)
                return;

            AddIncludes(sourceFile.Includes);
            AddForwardDeclarations(sourceFile.ForwardDeclarations);

            CppSourceFileNamespace ns = sourceFile.GetNamespace("");
            ns.Members.Add(
                $$"""
                extern "C" {
                
                {{GetFunctions().JoinAndIndent("")}}
                
                } // extern "C"
                """);
        }

        private void AddIncludes(ISet<string> includes)
        {
            foreach (GeneratedCppImplementationInvokerFunction function in Functions)
            {
                function.AddIncludesToSet(includes);
            }
        }

        private void AddForwardDeclarations(ISet<string> forwardDeclarations)
        {
            foreach (GeneratedCppImplementationInvokerFunction function in Functions)
            {
                function.AddForwardDeclarationsToSet(forwardDeclarations);
            }
        }

        private IEnumerable<string> GetFunctions()
        {
            return Functions.Select(f => f.Content);
        }
    }
}