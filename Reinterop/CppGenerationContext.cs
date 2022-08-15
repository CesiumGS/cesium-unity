using Microsoft.CodeAnalysis;

namespace Reinterop
{
    /// <summary>
    /// Options to the C++ code generation.
    /// </summary>
    internal class CppGenerationContext
    {
        public CppGenerationContext(Compilation compilation)
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
        public string BaseNamespace = "";

        /// <summary>
        /// The directory in which to generate output C++ files.
        /// </summary>
        public string OutputDirectory = "generated";

        /// <summary>
        /// The compilation for which we're generating C++ code.
        /// </summary>
        public Compilation Compilation;

        public Dictionary<ITypeSymbol, ICustomGenerator> CustomGenerators = new Dictionary<ITypeSymbol, ICustomGenerator>(SymbolEqualityComparer.Default);
    }
}
