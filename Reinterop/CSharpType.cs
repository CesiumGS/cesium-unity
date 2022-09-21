using Microsoft.CodeAnalysis;

namespace Reinterop
{
    internal class CSharpType
    {
        public readonly Compilation Compilation;
        public readonly InteropTypeKind Kind;
        public readonly IReadOnlyCollection<string> Namespaces;
        public readonly ITypeSymbol Symbol;

        public CSharpType(Compilation compilation, InteropTypeKind kind, IReadOnlyCollection<string> namespaces, ITypeSymbol symbol)
        {
            this.Compilation = compilation;
            this.Kind = kind;
            this.Namespaces = new List<string>(namespaces);
            this.Symbol = symbol;
        }

        public static CSharpType FromSymbol(Compilation compilation, ITypeSymbol symbol)
        {
            InteropTypeKind kind = Interop.DetermineTypeKind(compilation, symbol);

            List<string> namespaces = new List<string>();

            INamespaceSymbol ns = symbol.ContainingNamespace;
            while (ns != null)
            {
                if (ns.Name.Length > 0)
                    namespaces.Add(ns.Name);
                ns = ns.ContainingNamespace;
            }

            namespaces.Reverse();

            return new CSharpType(compilation, kind, namespaces, symbol);
        }

        public string GetFullyQualifiedNamespace()
        {
            IArrayTypeSymbol? arraySymbol = this.Symbol as IArrayTypeSymbol;
            if (arraySymbol != null)
                return CSharpType.FromSymbol(this.Compilation, arraySymbol.ElementType).GetFullyQualifiedNamespace();
            else
                return Symbol.ContainingNamespace.ToDisplayString();
        }

        public string GetFullyQualifiedName()
        {
            return Symbol.ToDisplayString();
        }

        public CSharpType AsInteropType()
        {
            // C++ doesn't specify the size of a bool, and C# uses different sizes in different contexts.
            // So we explicitly marshal bools as uint8_t / System.Byte.
            if (this.Symbol.SpecialType == SpecialType.System_Boolean)
                return CSharpType.FromSymbol(Compilation, Compilation.GetSpecialType(SpecialType.System_Byte));
            if (this.Kind == InteropTypeKind.ClassWrapper || this.Kind == InteropTypeKind.NonBlittableStructWrapper || this.Kind == InteropTypeKind.Delegate)
                return new CSharpType(Compilation, InteropTypeKind.Primitive, new string[] { "System" }, Compilation.GetSpecialType(SpecialType.System_IntPtr));

            return this;
        }

        /// <summary>
        /// Gets an expression that converts this type to the
        /// {@link AsInteropType}.
        /// </summary>
        public string GetConversionToInteropType(string variableName)
        {
            if (this.Symbol.SpecialType == SpecialType.System_Boolean)
                return $"{variableName} ? (byte)1 : (byte)0";
            else if (this.Kind == InteropTypeKind.ClassWrapper || this.Kind == InteropTypeKind.NonBlittableStructWrapper || this.Kind == InteropTypeKind.Delegate)
                return $"Reinterop.ObjectHandleUtility.CreateHandle({variableName})";
            else
                return variableName;
        }

        public string GetParameterConversionFromInteropType(string variableName)
        {
            if (this.Symbol.SpecialType == SpecialType.System_Boolean)
                return $"{variableName} != 0";
            else if (this.Kind == InteropTypeKind.ClassWrapper || this.Kind == InteropTypeKind.NonBlittableStructWrapper || this.Kind == InteropTypeKind.Delegate)
                return $"({this.GetFullyQualifiedName()})Reinterop.ObjectHandleUtility.GetObjectFromHandle({variableName})!";
            else
                return variableName;
        }

        public string GetReturnValueConversionFromInteropType(string variableName)
        {
            if (this.Symbol.SpecialType == SpecialType.System_Boolean)
                return $"{variableName} != 0";
            else if (this.Kind == InteropTypeKind.ClassWrapper || this.Kind == InteropTypeKind.NonBlittableStructWrapper || this.Kind == InteropTypeKind.Delegate)
                return $"({this.GetFullyQualifiedName()})Reinterop.ObjectHandleUtility.GetObjectAndFreeHandle({variableName})!";
            else
                return variableName;
        }

        public static bool IsFirstDerivedFromSecond(ITypeSymbol first, ITypeSymbol second)
        {
            INamedTypeSymbol? namedSecond = second as INamedTypeSymbol;

            ITypeSymbol? toCheckFirst = first;
            while (toCheckFirst != null)
            {
                if (SymbolEqualityComparer.Default.Equals(second, toCheckFirst))
                    return true;

                if (namedSecond != null && toCheckFirst.AllInterfaces.Contains(namedSecond, SymbolEqualityComparer.Default))
                    return true;

                toCheckFirst = toCheckFirst.BaseType;
            }

            return false;
        }
    }
}
