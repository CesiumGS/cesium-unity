using Microsoft.CodeAnalysis;
using System.Xml.Linq;

namespace Oxidize
{
    internal enum CSharpTypeKind
    {
        Unknown,
        Primitive,
        BlittableStruct,
        NonBlittableStruct,
        Class
    }

    internal class CSharpType
    {
        public readonly Compilation Compilation;
        public readonly CSharpTypeKind Kind;
        public readonly IReadOnlyCollection<string> Namespaces;
        public readonly ITypeSymbol Symbol;

        public CSharpType(Compilation compilation, CSharpTypeKind kind, IReadOnlyCollection<string> namespaces, ITypeSymbol symbol)
        {
            this.Compilation = compilation;
            this.Kind = kind;
            this.Namespaces = new List<string>(namespaces);
            this.Symbol = symbol;
        }

        public static CSharpType FromSymbol(Compilation compilation, ITypeSymbol symbol)
        {
            CSharpTypeKind kind = CSharpTypeKind.Unknown;
            switch (symbol.SpecialType)
            {
                case SpecialType.System_Int16:
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_UInt16:
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                case SpecialType.System_IntPtr:
                case SpecialType.System_Void:
                    kind = CSharpTypeKind.Primitive;
                    break;
            }

            if (kind == CSharpTypeKind.Unknown)
            {
                if (symbol.IsReferenceType)
                {
                    kind = CSharpTypeKind.Class;
                }
                else
                {
                    // TODO: distinguish between blittable and non-blittable structs
                    kind = CSharpTypeKind.BlittableStruct;
                }
            }

            List<string> namespaces = new List<string>();
            INamespaceSymbol ns = symbol.ContainingNamespace;
            while (ns != null)
            {
                if (ns.Name.Length > 0)
{
                    namespaces.Add(ns.Name);
                }
                ns = ns.ContainingNamespace;
            }

            return new CSharpType(compilation, kind, namespaces, symbol);
        }

        public string GetFullyQualifiedNamespace()
        {
            if (Symbol.SpecialType == SpecialType.System_Void)
                return "";
            return string.Join(".", Namespaces);
        }

        public string GetFullyQualifiedName()
        {
            if (Symbol.SpecialType == SpecialType.System_Void)
                return "void";

            string name = Symbol.Name;
            INamedTypeSymbol containingType = Symbol.ContainingType;
            if (containingType != null)
            {
                name = containingType.Name + "." + name;
                containingType = containingType.ContainingType;
            }

            return $"{GetFullyQualifiedNamespace()}.{name}";
        }

        public CSharpType AsInteropType()
        {
            if (this.Kind == CSharpTypeKind.Primitive || this.Kind == CSharpTypeKind.BlittableStruct)
                return this;

            return new CSharpType(Compilation, CSharpTypeKind.Primitive, new string[] { "System" }, Compilation.GetSpecialType(SpecialType.System_IntPtr));
        }

        /// <summary>
        /// Gets an expression that converts this type to the
        /// {@link AsInteropType}.
        /// </summary>
        public string GetConversionToInteropType(string variableName)
        {
            if (this.Kind == CSharpTypeKind.Primitive || this.Kind == CSharpTypeKind.BlittableStruct)
                return variableName;

            return $"Oxidize.ObjectHandleUtility.CreateHandle({variableName})";
        }

        public string GetConversionFromInteropType(string variableName)
        {
            if (this.Kind == CSharpTypeKind.Primitive || this.Kind == CSharpTypeKind.BlittableStruct)
                return variableName;

            return $"({this.GetFullyQualifiedName()}?)Oxidize.ObjectHandleUtility.GetObjectFromHandle({variableName})";
        }
    }
}
