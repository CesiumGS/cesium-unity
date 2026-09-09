using Microsoft.CodeAnalysis;

namespace Reinterop
{
    /// <summary>
    /// Options to the Reinterop code generation.
    /// </summary>
    internal class ReinteropGenerationContext
    {
        public ReinteropGenerationContext(Compilation compilation)
        {
            this.Compilation = compilation;
        }

        /// <summary>
        /// The base namespace in which to put all generated code. For example,
        /// if a C# class is in the `System.Threading` and the base namespace is
        /// `Foo`, then the namespace of the generated C++ wrapper will be
        /// `Foo::System::Threading`. If this property is empty, the C++
        /// namespace will match the C# namespace.
        /// </summary>
        public string BaseNamespace = "DotNet";

        /// <summary>
        /// The directory in which to generate output C++ files.
        /// </summary>
        public string OutputDirectory = "generated";

        /// <summary>
        /// The Roslyn compilation for which we're generating code.
        /// </summary>
        public Compilation Compilation;

        /// <summary>
        /// The name of the DLL or SO containing the C++ code.
        /// </summary>
        public string NativeLibraryName = "ReinteropNative";

        /// <summary>
        /// Types that should not be treated as blittable, even if they appear to be.
        /// </summary>
        public HashSet<string> NonBlittableTypes = new HashSet<string>();

        /// <summary>
        /// The custom generators to use for generating code for specific types. Each custom generator
        /// is invoked on each type. It can add additional dependencies to generate, and can modify
        /// the generated code for a type.
        /// </summary>
        public List<ICustomGenerator> CustomGenerators = new List<ICustomGenerator>();
    }
}
