namespace Oxidize
{
    internal class GeneratedResult
    {
        public GeneratedResult(CppType type)
        {
            this.CppInit = new GeneratedCppInit();
            this.CSharpInit = new GeneratedCSharpInit();
            this.CppDeclaration = new GeneratedCppDeclaration(type);
            this.CppDefinition = new GeneratedCppDefinition(type);
            this.CppImplementationInvoker = null;
            this.CSharpPartialMethodDefinitions = null;
        }

        /// <summary>
        /// The C++ code that initializes the C++ side of the interop layer with function
        /// pointers passed from the C# side.
        /// </summary>
        public GeneratedCppInit CppInit;

        /// <summary>
        /// The C# code that initializes the C++ side of the interop layer by passing function
        /// pointers from the C# side.
        /// </summary>
        public GeneratedCSharpInit CSharpInit;

        /// <summary>
        /// The C++ type declaration, i.e. the contents of the .h file.
        /// </summary>
        public GeneratedCppDeclaration CppDeclaration;

        /// <summary>
        /// The C++ type definition, i.e. the contents of the .cpp file.
        /// </summary>
        public GeneratedCppDefinition CppDefinition;

        /// <summary>
        /// The generated extern "C" functions that are called by the C# side
        /// to invoke a partial method declared on a class with the `OxidizeNativeImplementation`
        /// attribute. These functions call corresponding methods on a user-specified implementation
        /// class.
        /// </summary>
        public GeneratedCppImplementationInvoker? CppImplementationInvoker;

        /// <summary>
        /// The C# implementations for any partial methods on this class that are intended to be
        /// implemented by the user in C++ code.
        /// </summary>
        public GeneratedCSharpPartialMethodDefinition? CSharpPartialMethodDefinitions;
    }
}
