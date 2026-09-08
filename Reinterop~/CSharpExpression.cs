namespace Reinterop
{
    /// <summary>
    /// Base class for the small, restricted representation of C# expressions needed to express
    /// Reinterop's generated interop bodies. This is not a general-purpose C# AST - it models the
    /// fixed set of expression shapes used by the generator and is rendered by
    /// <see cref="CSharpPrinter"/>.
    /// </summary>
    internal abstract record CSharpExpression;

    /// <summary>
    /// A bare name reference: a variable, or a possibly qualified method or type name used as a
    /// callee, such as <c>result</c> or <c>Reinterop.ObjectHandleUtility</c>.
    /// </summary>
    internal record CSharpIdentifier(string Name) : CSharpExpression
    {
        /// <summary>The <c>thiz</c> pointer representing `this` in an interop function.</summary>
        public static CSharpIdentifier Thiz { get; } = new CSharpIdentifier("thiz");

        /// <summary>
        /// Creates an identifier naming <paramref name="type"/>.
        /// </summary>
        public CSharpIdentifier(CSharpType type) : this(type.GetFullyQualifiedName()) {}
    }

    /// <summary>
    /// A literal value written exactly as it should appear in C# source, such as <c>null</c>,
    /// <c>0</c>, <c>true</c>, or a quoted string literal.
    /// </summary>
    internal record CSharpLiteral(string Value) : CSharpExpression;

    /// <summary>
    /// A method or constructor call, <c>Callee(Arguments...)</c>. The callee may itself be a member
    /// access or another expression; the printer adds parentheses when C# precedence requires them.
    /// </summary>
    internal record CSharpCall(CSharpExpression Callee, IReadOnlyList<CSharpExpression> Arguments) : CSharpExpression;

    /// <summary>
    /// An infix binary operator, for example <c>CSharpBinary("!=", left, right)</c> renders as
    /// <c>left != right</c>. Operands remain separate expression nodes so the printer can preserve
    /// their grouping with parentheses.
    /// </summary>
    internal record CSharpBinary(string Op, CSharpExpression Left, CSharpExpression Right) : CSharpExpression;

    /// <summary>
    /// A conditional expression, <c>Condition ? Then : Else</c>, used for compact conversions such
    /// as nullable values and Boolean interop values.
    /// </summary>
    internal record CSharpTernary(CSharpExpression Condition, CSharpExpression Then, CSharpExpression Else) : CSharpExpression;

    /// <summary>
    /// An <c>is</c> type-test expression. When <paramref name="CastedVariableName"/> is supplied,
    /// this is a type pattern that both tests the value and declares a variable, for example
    /// <c>value is SomeType ValueNonNull</c>.
    /// </summary>
    internal record CSharpIs(CSharpExpression Expression, CSharpType Type, string? CastedVariableName = null) : CSharpExpression;

    /// <summary>
    /// A prefix unary operator, for example <c>CSharpUnary("&amp;", CSharpIdentifier("value"))</c>
    /// renders as <c>&amp;value</c>. The operand is parenthesized by the printer when needed to
    /// preserve the AST's grouping.
    /// </summary>
    internal record CSharpUnary(string Op, CSharpExpression Operand) : CSharpExpression;

    /// <summary>
    /// Access to a member of a value or reference, <c>Target.MemberName</c>, such as
    /// <c>reinteropException.IsInvalid</c> or <c>ObjectHandleUtility.CreateHandle</c>.
    /// </summary>
    internal record CSharpMemberAccess(CSharpExpression Target, string MemberName) : CSharpExpression;

    /// <summary>
    /// Indexing an array or other indexable value, <c>Target[Arguments...]</c>.
    /// </summary>
    internal record CSharpElementAccess(CSharpExpression Target, IReadOnlyList<CSharpExpression> Arguments) : CSharpExpression;

    /// <summary>
    /// Object construction with <c>new</c>, <c>new Type(Arguments...)</c>.
    /// </summary>
    internal record CSharpNew(CSharpType Type, IReadOnlyList<CSharpExpression> Arguments) : CSharpExpression;

    /// <summary>
    /// Array construction with one or more dimensions, <c>new ElementType[Dimensions...]</c>.
    /// </summary>
    internal record CSharpArrayNew(CSharpType ElementType, IReadOnlyList<CSharpExpression> Dimensions) : CSharpExpression;

    /// <summary>
    /// A C-style cast, <c>(Type)Expression</c>.
    /// </summary>
    internal record CSharpCast(CSharpType Type, CSharpExpression Expression) : CSharpExpression;
}
