using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

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
        }

        public ITypeSymbol type;
        public HashSet<IMethodSymbol> constructors;
        public HashSet<IMethodSymbol> methods;
        public HashSet<IPropertySymbol> properties;
        public GenerationItem? baseClass;
        public List<GenerationItem> interfaces;
    }
}
