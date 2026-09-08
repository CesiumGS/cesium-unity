namespace Reinterop
{
    /// <summary>
    /// Base class for the small, restricted representation of C++ expressions needed to express
    /// Reinterop's generated interop bodies. This is not a general-purpose C++ AST - it models the
    /// fixed set of expression shapes used by the generator and is rendered by
    /// <see cref="CppPrinter"/>.
    /// </summary>
    internal abstract record CppExpression
    {
        /// <summary>
        /// `#includes` that are required in order for this expression to be compilable. These are
        /// gathered up by <see cref="CppPrinter.GetRequiredIncludes(CppExpression)"/> along with any
        /// includes implied by the node type itself.
        /// </summary>
        public string[]? RequiredIncludes { get; init; } = null;
    }

    /// <summary>
    /// A bare name reference - a variable, or a (possibly qualified) function/type name used as a
    /// callee, e.g. <c>x</c> or <c>::DotNet::System::String</c>.
    /// </summary>
    internal record CppIdentifier(string Name) : CppExpression
    {
        /// <summary>The <c>this</c> pointer of the enclosing member function.</summary>
        public static CppIdentifier This { get; } = new CppIdentifier("this");

        /// <summary>
        /// Creates an identifier naming <paramref name="type"/>, automatically requiring the includes
        /// needed to use that type.
        /// </summary>
        public CppIdentifier(CppType type) : this(type.GetFullyQualifiedName())
        {
            HashSet<string> includes = new();
            type.AddSourceIncludesToSet(includes);
            if (includes.Count > 0)
                RequiredIncludes = includes.ToArray();
        }
    }

    /// <summary>
    /// A literal value written exactly as it appears in the source, e.g. a number, <c>nullptr</c>, or
    /// a quoted string. Use <see cref="String"/> to build a correctly-quoted string literal.
    /// </summary>
    internal record CppLiteral(string Value) : CppExpression
    {
        /// <summary>The <c>nullptr</c> literal.</summary>
        public static CppLiteral Nullptr { get; } = new CppLiteral("nullptr");

        /// <summary>The <c>true</c> literal.</summary>
        public static CppLiteral True { get; } = new CppLiteral("true");

        /// <summary>The <c>false</c> literal.</summary>
        public static CppLiteral False { get; } = new CppLiteral("false");

        /// <summary>
        /// Creates a double-quoted C++ string literal holding <paramref name="value"/>, escaping the
        /// characters that must not appear literally inside one.
        /// </summary>
        public static CppLiteral String(string value)
        {
            System.Text.StringBuilder builder = new();
            builder.Append('"');
            foreach (char c in value)
            {
                switch (c)
                {
                    case '\\': builder.Append("\\\\"); break;
                    case '"': builder.Append("\\\""); break;
                    case '\n': builder.Append("\\n"); break;
                    case '\r': builder.Append("\\r"); break;
                    case '\t': builder.Append("\\t"); break;
                    default: builder.Append(c); break;
                }
            }
            builder.Append('"');
            return new CppLiteral(builder.ToString());
        }
    }

    /// <summary>
    /// A function call, <c>Callee(Arguments...)</c>. Also used for functional-style construction and
    /// conversions, e.g. <c>std::int32_t(x)</c>.
    /// </summary>
    internal record CppCall(CppExpression Callee, IReadOnlyList<CppExpression> Arguments) : CppExpression;

    /// <summary>
    /// Access to a member of a value or reference, <c>Target.MemberName</c>.
    /// </summary>
    internal record CppMemberAccess(CppExpression Target, string MemberName) : CppExpression;

    /// <summary>
    /// Access to a member through a pointer, <c>Target-&gt;MemberName</c>.
    /// </summary>
    internal record CppPointerMemberAccess(CppExpression Target, string MemberName) : CppExpression;

    /// <summary>
    /// The kind of cast being performed. Used by <see cref="CppCast"/>.
    /// </summary>
    internal enum CppCastKind
    {
        CStyle,
        Static,
        Const,
        Reinterpret
    }

    /// <summary>
    /// A cast, <c>(TargetType)(Expression)</c>.
    /// </summary>
    internal record CppCast(CppCastKind Kind, CppType TargetType, CppExpression Expression) : CppExpression
    {
        public static CppCast CStyle(CppType targetType, CppExpression expression) => new(CppCastKind.CStyle, targetType, expression);
        public static CppCast Static(CppType targetType, CppExpression expression) => new(CppCastKind.Static, targetType, expression);
        public static CppCast Const(CppType targetType, CppExpression expression) => new(CppCastKind.Const, targetType, expression);
        public static CppCast Reinterpret(CppType targetType, CppExpression expression) => new(CppCastKind.Reinterpret, targetType, expression);
    }

    /// <summary>
    /// A heap allocation, <c>new TypeName(Arguments...)</c>. Ownership of the result is always handed
    /// across the interop boundary, so there is deliberately no matching <c>delete</c> expression.
    /// </summary>
    internal record CppNew(CppType Type, IReadOnlyList<CppExpression> Arguments) : CppExpression;

    /// <summary>
    /// A <c>std::move(Expression)</c>. Automatically requires <c>&lt;utility&gt;</c>.
    /// </summary>
    internal record CppMove(CppExpression Expression) : CppExpression;

    /// <summary>
    /// An infix binary operator, e.g. <c>CppBinary("==", left, right)</c> renders as
    /// <c>left == right</c>.
    /// </summary>
    internal record CppBinary(string Op, CppExpression Left, CppExpression Right) : CppExpression;

    /// <summary>
    /// A prefix unary operator, e.g. <c>CppUnary("&amp;", CppIdentifier("x"))</c> renders as
    /// <c>&amp;x</c>.
    /// </summary>
    internal record CppUnary(string Op, CppExpression Operand) : CppExpression;

    /// <summary>
    /// A conditional expression, <c>Condition ? Then : Else</c>.
    /// </summary>
    internal record CppTernary(CppExpression Condition, CppExpression Then, CppExpression Else) : CppExpression;
}
