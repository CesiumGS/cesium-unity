using Microsoft.CodeAnalysis;

namespace Reinterop
{
    internal class CSharpType
    {
        public readonly CppGenerationContext Context;
        public readonly InteropTypeKind Kind;
        public readonly IReadOnlyCollection<string> Namespaces;
        public readonly ITypeSymbol Symbol;

        public Compilation Compilation
        {
            get { return Context.Compilation; }
        }

        public CSharpType(CppGenerationContext context, InteropTypeKind kind, IReadOnlyCollection<string> namespaces, ITypeSymbol symbol)
        {
            this.Context = context;
            this.Kind = kind;
            this.Namespaces = new List<string>(namespaces);
            this.Symbol = symbol;
        }

        public static CSharpType FromSymbol(CppGenerationContext context, ITypeSymbol symbol)
        {
            InteropTypeKind kind = Interop.DetermineTypeKind(context, symbol);

            List<string> namespaces = new List<string>();

            INamespaceSymbol ns = symbol.ContainingNamespace;
            while (ns != null)
            {
                if (ns.Name.Length > 0)
                    namespaces.Add(ns.Name);
                ns = ns.ContainingNamespace;
            }

            namespaces.Reverse();

            return new CSharpType(context, kind, namespaces, symbol);
        }

        public string GetFullyQualifiedNamespace()
        {
            IArrayTypeSymbol? arraySymbol = this.Symbol as IArrayTypeSymbol;
            if (arraySymbol != null)
                return CSharpType.FromSymbol(this.Context, arraySymbol.ElementType).GetFullyQualifiedNamespace();
            else
                return Symbol.ContainingNamespace.ToDisplayString();
        }

        public string GetFullyQualifiedName()
        {
            return Symbol.ToDisplayString();
        }

        private CSharpType AsInteropTypeCommon()
        {
            // C++ doesn't specify the size of a bool, and C# uses different sizes in different contexts.
            // So we explicitly marshal bools as uint8_t / System.Byte.
            if (this.Symbol.SpecialType == SpecialType.System_Boolean)
                return CSharpType.FromSymbol(Context, Compilation.GetSpecialType(SpecialType.System_Byte));
            else if (this.Kind == InteropTypeKind.ClassWrapper || this.Kind == InteropTypeKind.NonBlittableStructWrapper || this.Kind == InteropTypeKind.Delegate)
                return new CSharpType(Context, InteropTypeKind.Primitive, new string[] { "System" }, Compilation.GetSpecialType(SpecialType.System_IntPtr));
            else
                return this;
        }

        public CSharpType AsInteropTypeParameter()
        {
            if (this.Kind == InteropTypeKind.BlittableStruct)
                return this.AsPointer();
            else if (this.Kind == InteropTypeKind.Nullable && this.Symbol is INamedTypeSymbol named)
            {
                ITypeSymbol? nullabledTypeSymbol = named.TypeArguments.FirstOrDefault();
                if (nullabledTypeSymbol != null)
                    return CSharpType.FromSymbol(this.Context, nullabledTypeSymbol).AsPointer();
                else
                    return this.AsPointer();
            }
            else
                return this.AsInteropTypeCommon();
        }

        public CSharpType AsInteropTypeReturn()
        {
            if (this.Kind == InteropTypeKind.Nullable && this.Symbol is INamedTypeSymbol named)
            {
                ITypeSymbol? nullabledTypeSymbol = named.TypeArguments.FirstOrDefault();
                if (nullabledTypeSymbol != null)
                    return CSharpType.FromSymbol(this.Context, nullabledTypeSymbol);
            }
            return this.AsInteropTypeCommon();
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
            else if (this.Kind == InteropTypeKind.BlittableStruct)
                return $"&{variableName}";
            else if (this.Kind == InteropTypeKind.Nullable)
                return $"{variableName} is {this.AsInteropTypeReturn().GetFullyQualifiedName()} {variableName}NonNull ? &{variableName}NonNull : null";
            else
                return variableName;
        }

        public string GetParameterConversionFromInteropType(string variableName)
        {
            if (this.Symbol.SpecialType == SpecialType.System_Boolean)
                return $"{variableName} != 0";
            else if (this.Kind == InteropTypeKind.ClassWrapper || this.Kind == InteropTypeKind.NonBlittableStructWrapper || this.Kind == InteropTypeKind.Delegate)
                return $"({this.GetFullyQualifiedName()})Reinterop.ObjectHandleUtility.GetObjectFromHandle({variableName})!";
            else if (this.Kind == InteropTypeKind.BlittableStruct)
                return $"*{variableName}";
            else if (this.Kind == InteropTypeKind.Nullable)
                return $"{variableName} == null ? null : *{variableName}";
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

        public CSharpType AsPointer()
        {
            return new CSharpType(this.Context, InteropTypeKind.Primitive, this.Namespaces, this.Compilation.CreatePointerTypeSymbol(this.Symbol));            
        }
    }
}
