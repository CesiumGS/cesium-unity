using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Oxidize
{
    internal class GeneratedCSharpPartialMethod
    {
        public GeneratedCSharpPartialMethod(string methodDefinition, string interopFunctionDeclaration)
        {
            this.MethodDefinition = methodDefinition;
            this.InteropFunctionDeclaration = interopFunctionDeclaration;
        }

        /// <summary>
        /// The definintion of the partial method that calls into C++.
        /// </summary>
        public string MethodDefinition;

        /// <summary>
        /// The interop function declaration of the corresponding C++ function, suitable for use
        /// with `static extern` as a `DllImport` or with a delegate marked with `UnmanagedFunctionPointer`.
        /// </summary>
        public string InteropFunctionDeclaration;
    }

    internal class GeneratedCSharpPartialMethodDefinitions
    {
        public readonly CSharpType Type;
        public List<GeneratedCSharpPartialMethod> Methods = new List<GeneratedCSharpPartialMethod>();

        public GeneratedCSharpPartialMethodDefinitions(CSharpType type)
        {
            this.Type = type;
        }
        
        public string ToSourceFileString()
        {
            // TODO: support structs
            string kind = "class";
            return
                $$"""
                namespace {{Type.GetFullyQualifiedNamespace()}}
                {
                    {{CSharpTypeUtility.GetAccessString(Type.Symbol.DeclaredAccessibility)}} partial {{kind}} {{Type.Symbol.Name}} : System.IDisposable
                    {
                        {{GetMethods().JoinAndIndent("        ")}}
                    }
                }
                """;
        }

        private IEnumerable<string> GetMethods()
        {
            return Methods.Select(method => method.MethodDefinition);
        }
    }
}