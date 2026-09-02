using Microsoft.CodeAnalysis;

namespace Reinterop
{
    internal enum CSharpTypeFlags
    {
        None = 0,
        Pointer = 1,
        Array = 2
    }

    internal class CSharpType
    {
        public readonly CppGenerationContext Context;
        public readonly InteropTypeKind Kind;
        public readonly IReadOnlyList<string> Namespaces;
        public readonly string Name;
        public readonly SpecialType SpecialType;
        public readonly CSharpType? ArrayElementType;
        public readonly IReadOnlyList<CSharpType> TypeArguments;
        public readonly CSharpTypeFlags Flags;
        public readonly ITypeSymbol? Symbol;
        public readonly CSharpType? ContainingType;

        public Compilation Compilation
        {
            get { return Context.Compilation; }
        }

        public CSharpType(
            CppGenerationContext context,
            InteropTypeKind kind,
            IReadOnlyCollection<string> namespaces,
            string name,
            SpecialType specialType,
            CSharpType? arrayElementType = null,
            IReadOnlyList<CSharpType>? typeArguments = null,
            CSharpTypeFlags flags = CSharpTypeFlags.None,
            CSharpType? containingType = null,
            ITypeSymbol? symbol = null)
        {
            this.Context = context;
            this.Kind = kind;
            this.Namespaces = new List<string>(namespaces);
            this.Name = name;
            this.SpecialType = specialType;
            this.Symbol = symbol;
            this.ArrayElementType = arrayElementType;
            this.TypeArguments = typeArguments ?? new List<CSharpType>();
            this.Flags = flags;
            this.ContainingType = containingType;
        }

        public static CSharpType? FromSymbolOrNull(CppGenerationContext context, ITypeSymbol? symbol)
        {
            if (symbol == null)
                return null;
            return FromSymbol(context, symbol!)!;
        }

        public static CSharpType FromSymbol(CppGenerationContext context, ITypeSymbol symbol)
        {
            if (symbol is IPointerTypeSymbol pointer)
            {
                CSharpType original = FromSymbol(context, pointer.PointedAtType);
                return original.AsPointer();
            }

            if (symbol is IArrayTypeSymbol arrayType)
            {
                CSharpType original = FromSymbol(context, arrayType.ElementType);
                return original.AsArray();
            }

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

            return new CSharpType(
                context,
                kind,
                namespaces,
                symbol.Name,
                symbol.SpecialType,
                null,
                (symbol as INamedTypeSymbol)?.TypeArguments.Select(t => CSharpType.FromSymbol(context, t))?.ToList(),
                CSharpTypeFlags.None,
                CSharpType.FromSymbolOrNull(context, symbol.ContainingType),
                symbol
            );
        }

        public string GetFullyQualifiedNamespace()
        {
            if (this.ArrayElementType != null)
                return this.ArrayElementType.GetFullyQualifiedNamespace();
            else
                return string.Join(".", this.Namespaces);
        }

        public string GetFullyQualifiedName()
        {
            string suffix = "";
            if (this.Flags.HasFlag(CSharpTypeFlags.Array)) suffix += "[]";
            if (this.Flags.HasFlag(CSharpTypeFlags.Pointer)) suffix += "*";

            switch (this.SpecialType)
            {
                case SpecialType.System_Void:
                    return "void" + suffix;
                default:
                    string generics = "";
                    if (this.TypeArguments.Count > 0)
                    {
                        generics = "<" + string.Join(", ", this.TypeArguments.Select(t => t.GetFullyQualifiedName())) + ">";
                    }
                    if (this.ContainingType != null)
                        return this.ContainingType.GetFullyQualifiedName() + "." + this.Name + generics + suffix;
                    else
                        return this.GetFullyQualifiedNamespace() + "." + this.Name + generics + suffix;
            }            
        }

        private CSharpType AsInteropTypeCommon()
        {
            // C++ doesn't specify the size of a bool, and C# uses different sizes in different contexts.
            // So we explicitly marshal bools as uint8_t / System.Byte.
            if (this.SpecialType == SpecialType.System_Boolean)
                return CSharpType.FromSymbol(Context, Compilation.GetSpecialType(SpecialType.System_Byte));
            else if (this.Kind == InteropTypeKind.ClassWrapper || this.Kind == InteropTypeKind.NonBlittableStructWrapper || this.Kind == InteropTypeKind.Delegate)
                return CSharpType.FromSymbol(Context, Compilation.GetSpecialType(SpecialType.System_IntPtr));
            else
                return this;
        }

        public CSharpType AsInteropTypeParameter()
        {
            if (this.Kind == InteropTypeKind.BlittableStruct)
                return this.AsPointer();
            else if (this.Kind == InteropTypeKind.Nullable)
                return this.TypeArguments.FirstOrDefault()?.AsPointer() ?? this.AsPointer();
            else
                return this.AsInteropTypeCommon();
        }

        public CSharpType AsInteropTypeReturn()
        {
            if (this.Kind == InteropTypeKind.Nullable && this.TypeArguments.Count > 0)
                return this.TypeArguments.First();
            return this.AsInteropTypeCommon();
        }

        /// <summary>
        /// Gets an expression that converts this type to the
        /// {@link AsInteropType}.
        /// </summary>
        public string GetConversionToInteropType(string variableName)
        {
            if (this.SpecialType == SpecialType.System_Boolean)
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

        /// <summary>
        /// Gets an expression that converts this type to the
        /// {@link AsInteropType}.
        /// </summary>
        public CSharpExpression GetConversionToInteropTypeExpression(CSharpExpression originalExpression)
        {
            if (this.SpecialType == SpecialType.System_Boolean)
                return new CSharpCast("byte", new CSharpTernary(originalExpression, new CSharpLiteral("1"), new CSharpLiteral("0")));
            else if (this.Kind == InteropTypeKind.ClassWrapper || this.Kind == InteropTypeKind.NonBlittableStructWrapper || this.Kind == InteropTypeKind.Delegate)
                return new CSharpCall(
                    new CSharpMemberAccess(new CSharpIdentifier("Reinterop.ObjectHandleUtility"), "CreateHandle"),
                    [ originalExpression ]);
            else if (this.Kind == InteropTypeKind.BlittableStruct)
                return new CSharpUnary("&", originalExpression);
            else if (this.Kind == InteropTypeKind.Nullable)
                return new CSharpTernary(
                    new CSharpIs(originalExpression, this.AsInteropTypeReturn().GetFullyQualifiedName(), "ValueNonNull"),
                    new CSharpUnary("&", new CSharpIdentifier("ValueNonNull")),
                    new CSharpLiteral("null")
                );
            else
                return originalExpression;
        }

        public string GetParameterConversionFromInteropType(string variableName)
        {
            if (this.SpecialType == SpecialType.System_Boolean)
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

        public CSharpExpression GetParameterConversionFromInteropTypeExpression(CSharpExpression interopExpression)
        {
            if (this.SpecialType == SpecialType.System_Boolean)
                return new CSharpBinary("!=", interopExpression, new CSharpLiteral("0"));
            else if (this.Kind == InteropTypeKind.ClassWrapper || this.Kind == InteropTypeKind.NonBlittableStructWrapper || this.Kind == InteropTypeKind.Delegate)
                return new CSharpCast(this.GetFullyQualifiedName(), new CSharpCall(new CSharpIdentifier("Reinterop.ObjectHandleUtility.GetObjectFromHandle"), [ interopExpression ]));
            else if (this.Kind == InteropTypeKind.BlittableStruct)
                return new CSharpUnary("*", interopExpression);
            else if (this.Kind == InteropTypeKind.Nullable)
                return new CSharpTernary(
                    new CSharpBinary("==", interopExpression, new CSharpLiteral("null")),
                    new CSharpLiteral("null"),
                    new CSharpUnary("*", interopExpression)
                );
            else
                return interopExpression;
        }

        public string GetReturnValueConversionFromInteropType(string variableName)
        {
            if (this.SpecialType == SpecialType.System_Boolean)
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
            return new CSharpType(
                this.Context,
                InteropTypeKind.Primitive,
                this.Namespaces,
                this.Name,
                this.SpecialType,
                this.ArrayElementType,
                this.TypeArguments,
                this.Flags | CSharpTypeFlags.Pointer);
        }

        public CSharpType AsArray()
        {
            return new CSharpType(
                this.Context,
                InteropTypeKind.ClassWrapper,
                this.Namespaces,
                this.Name,
                SpecialType.System_Array,
                this,
                this.TypeArguments,
                this.Flags | CSharpTypeFlags.Array);
        }
    }
}
