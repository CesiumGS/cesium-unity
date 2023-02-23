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
            this.EnumValues = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
            this.Interfaces = new List<TypeToGenerate>();
            this.MethodsImplementedInCpp = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        }

        public ITypeSymbol Type;
        public HashSet<IMethodSymbol> Constructors;
        public HashSet<IMethodSymbol> Methods;
        public HashSet<IPropertySymbol> Properties;
        public HashSet<IFieldSymbol> Fields;
        public HashSet<IEventSymbol> Events;
        public HashSet<IFieldSymbol> EnumValues;
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

        public bool ImplementationStaticOnly = true;

        public static Dictionary<ITypeSymbol, TypeToGenerate> Combine(IEnumerable<IEnumerable<TypeToGenerate>> listOfItems)
        {
            Dictionary<ITypeSymbol, TypeToGenerate> result = new Dictionary<ITypeSymbol, TypeToGenerate>(SymbolEqualityComparer.Default);

            foreach (IEnumerable<TypeToGenerate> items in listOfItems)
            {
                foreach (TypeToGenerate item in items)
                {
                    TypeToGenerate current;
                    if (!result.TryGetValue(item.Type, out current))
                    {
                        current = new TypeToGenerate(item.Type);
                        result.Add(item.Type, current);
                    }

                    if (current.ImplementationClassName == null)
                    {
                        current.ImplementationClassName = item.ImplementationClassName;
                    }
                    else if (item.ImplementationClassName != null && item.ImplementationClassName != current.ImplementationClassName)
                    {
                        // TODO: report conflicting implementation class name
                    }

                    if (current.ImplementationHeaderName == null)
                    {
                        current.ImplementationHeaderName = item.ImplementationHeaderName;
                    }
                    else if (item.ImplementationHeaderName != null && item.ImplementationHeaderName != current.ImplementationHeaderName)
                    {
                        // TODO: report conflicting implementation header name
                    }

                    current.ImplementationStaticOnly = current.ImplementationStaticOnly && item.ImplementationStaticOnly;

                    foreach (IMethodSymbol method in item.Constructors)
                    {
                        current.Constructors.Add(method);
                    }

                    foreach (IMethodSymbol method in item.Methods)
                    {
                        current.Methods.Add(method);
                    }

                    foreach (IPropertySymbol property in item.Properties)
                    {
                        current.Properties.Add(property);
                    }

                    foreach (IFieldSymbol field in item.Fields)
                    {
                        current.Fields.Add(field);
                    }

                    foreach (IEventSymbol evt in item.Events)
                    {
                        current.Events.Add(evt);
                    }

                    foreach (IFieldSymbol enumValue in item.EnumValues)
                    {
                        current.EnumValues.Add(enumValue);
                    }

                    foreach (IMethodSymbol method in item.MethodsImplementedInCpp)
                    {
                        current.MethodsImplementedInCpp.Add(method);
                    }
                }
            }

            return result;
        }
    }
}
