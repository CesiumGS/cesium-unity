using Microsoft.CodeAnalysis;

namespace Oxidize
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
        /// The directory in which to generate .cpp files.
        /// </summary>
        public string OutputSourceDirectory = "c:\\Dev\\cesium-unity-samples\\cesium-unity\\TestOxidize\\generated\\src";

        /// <summary>
        /// The directory in which to generate .h files.
        /// </summary>
        public string OutputHeaderDirectory = "c:\\Dev\\cesium-unity-samples\\cesium-unity\\TestOxidize\\generated\\include";

        /// <summary>
        /// The compilation for which we're generating C++ code.
        /// </summary>
        public Compilation Compilation;

        public Dictionary<ITypeSymbol, ICustomGenerator> CustomGenerators = new Dictionary<ITypeSymbol, ICustomGenerator>();
    }
}
