namespace Reinterop
{
    // A small, restricted representation of the C++ statements and expressions actually needed to
    // express Reinterop's interop bodies. This is not a general-purpose C++ AST - it only models the
    // fixed set of shapes used by the "recipes" in CppInterop.cs, so that those bodies can be built
    // and tested structurally instead of via hand-written, easily-diverging string interpolation.

    internal abstract record CppExpression;

    // A bare name reference - a variable, or a (possibly qualified) function/type name used as a callee.
    internal record CppIdentifier(string Name) : CppExpression;

    // An already-rendered expression, used as an escape hatch for pieces (e.g. argument conversions)
    // that haven't been converted to this DSL yet.
    internal record CppRaw(string Text) : CppExpression;

    internal record CppCall(CppExpression Callee, IReadOnlyList<CppExpression> Arguments) : CppExpression;

    internal record CppBinary(string Op, CppExpression Left, CppExpression Right) : CppExpression;

    internal abstract record CppStatement;

    // TypeName is rendered as-is (e.g. "void*", "auto") since not every C++ spelling used here
    // (notably "auto") corresponds to a real CppType.
    internal record CppVariableDeclaration(string TypeName, string Name, CppExpression Initializer) : CppStatement;

    internal record CppExpressionStatement(CppExpression Expression) : CppStatement;

    internal record CppIf(CppExpression Condition, IReadOnlyList<CppStatement> Then) : CppStatement;

    internal record CppThrow(CppExpression Exception) : CppStatement;

    internal record CppReturn(CppExpression? Value = null) : CppStatement;
}
