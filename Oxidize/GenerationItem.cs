using Microsoft.CodeAnalysis;

namespace Oxidize
{
    internal class GenerationItem
    {
        public GenerationItem(ITypeSymbol type)
        {
            this.Type = type;
            this.Constructors = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
            this.Methods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
            this.Properties = new HashSet<IPropertySymbol>(SymbolEqualityComparer.Default);
            this.Interfaces = new List<GenerationItem>();
            this.MethodsImplementedInCpp = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        }

        public ITypeSymbol Type;
        public HashSet<IMethodSymbol> Constructors;
        public HashSet<IMethodSymbol> Methods;
        public HashSet<IPropertySymbol> Properties;
        public GenerationItem? BaseClass;
        public List<GenerationItem> Interfaces;
        public HashSet<IMethodSymbol> MethodsImplementedInCpp;

        /// <summary>
        /// If this C# class has a C++ implementation, this is the name of the
        /// C++ implementation class.
        /// </summary>
        public string? ImplementationClassName;

        /// <summary>
        /// If this C# class has a C++ implementation, this is the name of the
        /// C++ header file declaring the class.
        /// </summary>
        public string? ImplementationHeaderName;
    }
}
