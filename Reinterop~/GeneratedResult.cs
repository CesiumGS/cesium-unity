namespace Reinterop
{
    internal class GeneratedResult
    {
        public GeneratedResult(CppType type)
        {
            this.Type = type;
            this.Init = new GeneratedInit();
            this.CppDeclaration = new GeneratedCppDeclaration(type);
            this.CppDefinition = new GeneratedCppDefinition(type);
            this.CppImplementationInvoker = null;
            this.CSharpPartialMethodDefinitions = null;
        }

        /// <summary>
        /// The C++ type that is being generated. This is the type that will be declared in the .h file and defined in the .cpp file.
        /// </summary>
        public CppType Type;

        /// <summary>
        /// The functions created as delegates on the C# side and passed to the C++
        /// side as function pointers during the init process.
        /// </summary>
        public GeneratedInit Init;

        /// <summary>
        /// The interop-backed C++ functions (methods, property accessors, constructors, field
        /// accessors) recipe'd for this type. Populated during generation; <see cref="CppInteropFunction.AddToGeneration"/>
        /// is called on each of these later, once generation of this type is otherwise complete, to
        /// append their declarations/definitions/init registrations to this result. Kept as a list
        /// (rather than being applied immediately) so tests can inspect the recipes directly instead
        /// of the generated C++/C# text.
        /// </summary>
        public List<CppInteropFunction> InteropFunctions = new List<CppInteropFunction>();

        public List<CSharpFunctionCallableFromCpp> InteropFunctions2 = new List<CSharpFunctionCallableFromCpp>();

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
