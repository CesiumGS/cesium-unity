using Microsoft.CodeAnalysis;
using System.Xml.Linq;

namespace Oxidize
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
            return Symbol.ContainingNamespace.ToDisplayString();
        }

        public string GetFullyQualifiedName()
        {
            return Symbol.ToDisplayString();
        }

        public CSharpType AsInteropType()
        {
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
            if (this.Kind == InteropTypeKind.ClassWrapper || this.Kind == InteropTypeKind.NonBlittableStructWrapper || this.Kind == InteropTypeKind.Delegate)
                return $"Oxidize.ObjectHandleUtility.CreateHandle({variableName})";

            return variableName;
        }

        public string GetConversionFromInteropType(string variableName)
        {
            if (this.Kind == InteropTypeKind.ClassWrapper || this.Kind == InteropTypeKind.NonBlittableStructWrapper || this.Kind == InteropTypeKind.Delegate)
                return $"({this.GetFullyQualifiedName()})Oxidize.ObjectHandleUtility.GetObjectFromHandle({variableName})!";

            return variableName;
        }
    }
}
