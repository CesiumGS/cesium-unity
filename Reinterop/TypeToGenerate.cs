using Microsoft.CodeAnalysis;

namespace Reinterop
{
    internal class TypeToGenerate
    {
        public TypeToGenerate(ITypeSymbol type)
        {
            this.Type = type;
            this.Constructors = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
            this.Methods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
            this.Properties = new HashSet<IPropertySymbol>(SymbolEqualityComparer.Default);
            this.Fields = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
            this.Events = new HashSet<IEventSymbol>(SymbolEqualityComparer.Default);
            this.EnumValues = new List<IFieldSymbol>();
            this.Interfaces = new List<TypeToGenerate>();
            this.MethodsImplementedInCpp = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        }

        public ITypeSymbol Type;
        public HashSet<IMethodSymbol> Constructors;
        public HashSet<IMethodSymbol> Methods;
        public HashSet<IPropertySymbol> Properties;
        public HashSet<IFieldSymbol> Fields;
        public HashSet<IEventSymbol> Events;
        public List<IFieldSymbol> EnumValues;
        public TypeToGenerate? BaseClass;
        public List<TypeToGenerate> Interfaces;
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
