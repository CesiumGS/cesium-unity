namespace Reinterop
{
    /// <summary>Base class for the restricted C# expression tree used by the generator.</summary>
    internal abstract record CSharpExpression;

    /// <summary>A variable, member, or qualified method/type name.</summary>
    internal record CSharpIdentifier(string Name) : CSharpExpression;

    /// <summary>A C# literal written in source form.</summary>
    internal record CSharpLiteral(string Value) : CSharpExpression;

    /// <summary>A method or constructor call.</summary>
    internal record CSharpCall(CSharpExpression Callee, IReadOnlyList<CSharpExpression> Arguments) : CSharpExpression;

    /// <summary>A binary operator expression.</summary>
    internal record CSharpBinary(string Op, CSharpExpression Left, CSharpExpression Right) : CSharpExpression;

    /// <summary>A conditional expression, <c>Condition ? Then : Else</c>.</summary>
    internal record CSharpTernary(CSharpExpression Condition, CSharpExpression Then, CSharpExpression Else) : CSharpExpression;

    /// <summary>An <c>is</c> type-test expression, optionally declaring a pattern variable.</summary>
    internal record CSharpIs(CSharpExpression Expression, string TypeName, string? CastedVariableName = null) : CSharpExpression;

    /// <summary>A prefix unary operator expression.</summary>
    internal record CSharpUnary(string Op, CSharpExpression Operand) : CSharpExpression;

    /// <summary>Access to a member of a value or reference.</summary>
    internal record CSharpMemberAccess(CSharpExpression Target, string MemberName) : CSharpExpression;

    /// <summary>Indexing an array or indexable value.</summary>
    internal record CSharpElementAccess(CSharpExpression Target, IReadOnlyList<CSharpExpression> Arguments) : CSharpExpression;

    /// <summary>Object construction with <c>new</c>.</summary>
    internal record CSharpNew(string TypeName, IReadOnlyList<CSharpExpression> Arguments) : CSharpExpression;

    /// <summary>Array construction with dimensions.</summary>
    internal record CSharpArrayNew(string ElementTypeName, IReadOnlyList<CSharpExpression> Dimensions) : CSharpExpression;

    /// <summary>A C-style cast expression.</summary>
    internal record CSharpCast(string TypeName, CSharpExpression Expression) : CSharpExpression;
}
