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
            if (Type.Symbol == null)
                throw new Exception("Type with partial method definitions must have a Symbol.");

            // TODO: support structs
            string kind = "class";

            string interopPrefix = Type.GetFullyQualifiedName().Replace(".", "_");
            if (!string.IsNullOrEmpty(this.Type.Context.BaseNamespace))
                interopPrefix = this.Type.Context.BaseNamespace + "_" + interopPrefix;

            if (needsInstance)
            {
                return
                    $$"""
                    using Microsoft.Win32.SafeHandles;
                    using System;
                    using System.Runtime.ConstrainedExecution;
                    using System.Runtime.InteropServices;

                    namespace {{Type.GetFullyQualifiedNamespace()}}
                    {
                        {{CSharpTypeUtility.GetAccessString(Type.Symbol.DeclaredAccessibility)}} partial {{kind}} {{Type.Symbol.Name}} : System.IDisposable
                        {
                            internal class ImplementationHandle : SafeHandleZeroOrMinusOneIsInvalid
                            {
                                public ImplementationHandle({{Type.Symbol.Name}} managed) : base(true)
                                {
                                    SetHandle({{interopPrefix}}_CreateImplementation(Reinterop.ObjectHandleUtility.CreateHandle(managed)));
                                }
                    
                                [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
                                protected override bool ReleaseHandle()
                                {
                                    {{interopPrefix}}_DestroyImplementation(this.handle);
                                    return true;
                                }
                            }
                    
                            [System.NonSerialized]
                            private ImplementationHandle _implementation = null;

                            internal ImplementationHandle NativeImplementation
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