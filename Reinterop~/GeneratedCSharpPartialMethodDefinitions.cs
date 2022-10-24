using Microsoft.CodeAnalysis;

namespace Reinterop
{
    internal class GeneratedCSharpPartialMethod
    {
        public GeneratedCSharpPartialMethod(string methodDefinition, string interopFunctionDeclaration = "")
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

        /// <summary>
        /// True if this C# class needs to be backed by a C++ class instance. False if all of its methods
        /// implemented in C++ are static so no instance is required.
        /// </summary>
        public bool needsInstance = true;

        public GeneratedCSharpPartialMethodDefinitions(CSharpType type)
        {
            this.Type = type;
        }

        public string ToSourceFileString()
        {
            // TODO: support structs
            string kind = "class";

            if (needsInstance)
            {
                return
                    $$"""
                    using System;
                    using System.Runtime.InteropServices;

                    namespace {{Type.GetFullyQualifiedNamespace()}}
                    {
                        {{CSharpTypeUtility.GetAccessString(Type.Symbol.DeclaredAccessibility)}} partial {{kind}} {{Type.Symbol.Name}} : System.IDisposable
                        {
                            [System.NonSerialized]
                            private System.IntPtr _implementation = System.IntPtr.Zero;

                            public IntPtr NativeImplementation
                            {
                                get { return _implementation; }
                            }

                            {{GetMethods().JoinAndIndent("        ")}}

                            {{GetInteropFunctionDeclarations().JoinAndIndent("        ")}}
                        }
                    }
                    """;
            }
            else
            {
                return
                    $$"""
                    using System;
                    using System.Runtime.InteropServices;

                    namespace {{Type.GetFullyQualifiedNamespace()}}
                    {
                        {{CSharpTypeUtility.GetAccessString(Type.Symbol.DeclaredAccessibility)}} partial {{kind}} {{Type.Symbol.Name}}
                        {
                            {{GetMethods().JoinAndIndent("        ")}}

                            {{GetInteropFunctionDeclarations().JoinAndIndent("        ")}}
                        }
                    }
                    """;
            }
        }

        private IEnumerable<string> GetMethods()
        {
            return Methods.Select(method => method.MethodDefinition);
        }

        private IEnumerable<string> GetInteropFunctionDeclarations()
        {
            return Methods.Select(method => method.InteropFunctionDeclaration).Where(method => !string.IsNullOrEmpty(method));
        }
    }
}