namespace Reinterop
{
    internal class GeneratedResult
    {
        public GeneratedResult(CppType type)
        {
            this.Init = new GeneratedInit();
            this.CppDeclaration = new GeneratedCppDeclaration(type);
            this.CppDefinition = new GeneratedCppDefinition(type);
            this.CppImplementationInvoker = null;
            this.CSharpPartialMethodDefinitions = null;
        }

        /// <summary>
        /// The functions created as delegates on the C# side and passed to the C++
        /// side as function pointers during the init process.
        /// </summary>
        public GeneratedInit Init;

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
        /// to invoke a partial method declared on a class with the `ReinteropNativeImplementation`
        /// attribute. These functions call corresponding methods on a user-specified implementation
        /// class.
        /// </summary>
        public GeneratedCppImplementationInvoker? CppImplementationInvoker;

        /// <summary>
        /// The C# implementations for any partial methods on this class that are intended to be
        /// implemented by the user in C++ code.
        /// </summary>
        public GeneratedCSharpPartialMethodDefinitions? CSharpPartialMethodDefinitions;
    }
}
