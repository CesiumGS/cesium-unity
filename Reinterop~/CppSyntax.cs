namespace Reinterop
{
    // A small, restricted representation of the C++ statements and expressions actually needed to
    // express Reinterop's interop bodies. This is not a general-purpose C++ AST - it only models the
    // fixed set of shapes used by the "recipes" in CppInterop.cs, so that those bodies can be built
    // and tested structurally instead of via hand-written, easily-diverging string interpolation.

    internal abstract record CppExpression
    {
        // `#includes` that are required in order for this expression to be compilable.
        public string[]? RequiredIncludes { get; init; } = null;
    }

    // A bare name reference - a variable, or a (possibly qualified) function/type name used as a callee.
    internal record CppIdentifier(string Name) : CppExpression;

    // An already-rendered expression, used as an escape hatch for pieces (e.g. argument conversions)
    // that haven't been converted to this DSL yet.
    internal record CppRaw(string Text) : CppExpression;

    internal record CppCall(CppExpression Callee, IReadOnlyList<CppExpression> Arguments) : CppExpression;

    internal record CppCast(CppType targetType, CppExpression Expression) : CppExpression;

    // A std::move() expression.
    internal record CppMove(CppExpression Expression) : CppExpression;

    internal record CppBinary(string Op, CppExpression Left, CppExpression Right) : CppExpression;

    // A prefix unary operator, e.g. CppUnary("&", CppIdentifier("x")) renders as "&x".
    internal record CppUnary(string Op, CppExpression Operand) : CppExpression;

    // An argument to a call - see CppOutParameterArgument for the case that needs a supporting
    // declaration statement before the call, rather than being a plain value.
    internal abstract record CppArgument
    {
        public static CppArgument Value(string rawExpressionText) => new CppValueArgument(new CppRaw(rawExpressionText));
        public static CppArgument Value(CppExpression expression) => new CppValueArgument(expression);
        public static CppArgument OutParameter(string typeName, string name) => new CppOutParameterArgument(typeName, name);
    }

    // An already-computed value expression, passed to the call as-is.
    internal record CppValueArgument(CppExpression Expression) : CppArgument;

    // A variable of TypeName named Name is declared (with no initializer) immediately before the
    // call, and its address is passed as this argument. Name can then be referenced afterwards (e.g.
    // in a return expression) via a CppIdentifier.
    internal record CppOutParameterArgument(string TypeName, string Name) : CppArgument;

    internal abstract record CppStatement;

    // TypeName is rendered as-is (e.g. "void*", "auto") since not every C++ spelling used here
    // (notably "auto") corresponds to a real CppType. Initializer is null for a bare declaration
    // (e.g. "MyStruct result;") that's filled in by a later statement (e.g. an out-parameter call).
    internal record CppVariableDeclaration(string TypeName, string Name, CppExpression? Initializer = null) : CppStatement;

    internal record CppExpressionStatement(CppExpression Expression) : CppStatement;

    internal record CppAssignment(CppExpression Target, CppExpression Value) : CppStatement;

    internal record CppIf(CppExpression Condition, IReadOnlyList<CppStatement> Then) : CppStatement;

    internal record CppThrow(CppExpression Exception) : CppStatement;

    internal record CppReturn(CppExpression? Value = null) : CppStatement;

    // An already-rendered statement (or block of statements), used as an escape hatch for bodies
    // (e.g. the call implementation preceding a catch block) that haven't been converted to this
    // DSL yet. Printed as-is, with no added punctuation.
    internal record CppRawStatement(string Text) : CppStatement;

    // ExceptionType is null for a catch-all ("catch (...)"), in which case VariableName is unused.
    internal record CppCatch(string? ExceptionType, string? VariableName, IReadOnlyList<CppStatement> Body);

    internal record CppTry(IReadOnlyList<CppStatement> Body, IReadOnlyList<CppCatch> Catches) : CppStatement;

    // A constructor member initializer.
    internal record CppMemberInitializer(string MemberName, CppExpression Value);
}
