namespace Reinterop
{
    // The C# mirror of CppSyntax.cs - a small, restricted representation of the C# statements and
    // expressions actually needed to express the "call into native code, check for an exception, and
    // optionally return a converted result" pattern used throughout the generated C# interop code.
    // This is not a general-purpose C# AST - it only models the fixed set of shapes used by the
    // recipes in CSharpInterop.cs.

    internal abstract record CSharpExpression;

    // A bare name reference - a variable, or a (possibly qualified) function name used as a callee.
    internal record CSharpIdentifier(string Name) : CSharpExpression;

    // An already-rendered expression, used as an escape hatch for pieces (e.g. argument conversions)
    // that haven't been converted to this DSL yet.
    internal record CSharpRaw(string Text) : CSharpExpression;

    // A literal value, e.g. a number or string.
    internal record CSharpLiteral(string Value) : CSharpExpression;

    internal record CSharpCall(CSharpExpression Callee, IReadOnlyList<CSharpExpression> Arguments) : CSharpExpression;

    internal record CSharpBinary(string Op, CSharpExpression Left, CSharpExpression Right) : CSharpExpression;

    internal record CSharpTernary(CSharpExpression Condition, CSharpExpression Then, CSharpExpression Else) : CSharpExpression;

    internal record CSharpIs(CSharpExpression Expression, string TypeName, string? castedVariableName = null) : CSharpExpression;

    // A prefix unary operator, e.g. CSharpUnary("&", CSharpIdentifier("x")) renders as "&x".
    internal record CSharpUnary(string Op, CSharpExpression Operand) : CSharpExpression;

    internal record CSharpMemberAccess(CSharpExpression Target, string MemberName) : CSharpExpression;

    internal record CSharpNew(string TypeName, IReadOnlyList<CSharpExpression> Arguments) : CSharpExpression;

    // A C-style cast, e.g. CSharpCast("System.Exception", ...) renders as "(System.Exception)...".
    internal record CSharpCast(string TypeName, CSharpExpression Expression) : CSharpExpression;

    internal abstract record CSharpStatement;

    // Initializer is null for a bare declaration (e.g. "int x;") that's filled in by a later statement.
    internal record CSharpVariableDeclaration(string TypeName, string Name, CSharpExpression? Initializer = null) : CSharpStatement;

    internal record CSharpExpressionStatement(CSharpExpression Expression) : CSharpStatement;

    internal record CSharpIf(CSharpExpression Condition, IReadOnlyList<CSharpStatement> Then, IReadOnlyList<CSharpStatement>? Else = null) : CSharpStatement;

    internal record CSharpThrow(CSharpExpression Exception) : CSharpStatement;

    internal record CSharpReturn(CSharpExpression? Value = null) : CSharpStatement;

    // An already-rendered statement, used as an escape hatch for statements that haven't been
    // converted to this DSL yet. Printed as-is, with no added punctuation.
    internal record CSharpRawStatement(string Text) : CSharpStatement;

    internal record CSharpTryCatch(IReadOnlyList<CSharpStatement> TryBody, IReadOnlyList<CSharpStatement> CatchBody) : CSharpStatement;
}
