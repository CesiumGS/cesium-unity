using Microsoft.CodeAnalysis;

namespace Oxidize
{
    internal class GenerationItem
    {
        public GenerationItem(ITypeSymbol type)
        {
            this.type = type;
            this.constructors = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
            this.methods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
            this.properties = new HashSet<IPropertySymbol>(SymbolEqualityComparer.Default);
            this.interfaces = new List<GenerationItem>();
            this.methodsImplementedInCpp = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        }

        public ITypeSymbol type;
        public HashSet<IMethodSymbol> constructors;
        public HashSet<IMethodSymbol> methods;
        public HashSet<IPropertySymbol> properties;
        public GenerationItem? baseClass;
        public List<GenerationItem> interfaces;
        public HashSet<IMethodSymbol> methodsImplementedInCpp;

        /// <summary>
        /// If this C# class has a C++ implementation, this is the name of the
        /// C++ implementation class.
        /// </summary>
        public string? implClassName;

        /// <summary>
        /// If this C# class has a C++ implementation, this is the name of the
        /// C++ header file declaring the class.
        /// </summary>
        public string? implHeaderName;
    }
}
