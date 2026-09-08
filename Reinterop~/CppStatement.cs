namespace Reinterop
{
    /// <summary>
    /// Base class for the small, restricted representation of C++ statements needed to express
    /// Reinterop's interop bodies. See <see cref="CppExpression"/> for the expression counterpart.
    /// Rendered to source text by <see cref="CppPrinter"/>.
    /// </summary>
    internal abstract record CppStatement;

    /// <summary>
    /// A local variable declaration, either <c>TypeName Name = Initializer;</c> or
    /// <c>TypeName Name{Initializer};</c>. <see cref="Initializer"/> is null for a bare declaration
    /// (e.g. <c>MyStruct result;</c>) that's filled in by a later statement, such as a call with an
    /// out parameter.
    /// </summary>
    internal record CppVariableDeclaration(
        CppType Type,
        string Name,
        CppExpression? Initializer = null,
        bool UseBracedInitialization = false) : CppStatement;

    /// <summary>
    /// An expression evaluated for its side effects, <c>Expression;</c>.
    /// </summary>
    internal record CppExpressionStatement(CppExpression Expression) : CppStatement;

    /// <summary>
    /// An assignment, <c>Target = Value;</c>.
    /// </summary>
    internal record CppAssignment(CppExpression Target, CppExpression Value) : CppStatement;

    /// <summary>
    /// An <c>if</c> statement with no <c>else</c> clause. A single-statement body is printed without
    /// braces, matching the convention of the surrounding generated code.
    /// </summary>
    internal record CppIf(CppExpression Condition, IReadOnlyList<CppStatement> Then, IReadOnlyList<CppStatement>? Else = null) : CppStatement;

    /// <summary>
    /// A <c>throw</c> statement. <see cref="Exception"/> is null for a rethrow (<c>throw;</c>) of the
    /// exception currently being handled, which is only valid inside a catch block.
    /// </summary>
    internal record CppThrow(CppExpression? Exception = null) : CppStatement;

    /// <summary>
    /// A <c>return</c> statement. <see cref="Value"/> is null for a return from a void function.
    /// </summary>
    internal record CppReturn(CppExpression? Value = null) : CppStatement;

    /// <summary>
    /// A single <c>catch</c> clause of a <see cref="CppTry"/>. The exception is always caught by
    /// reference. <see cref="ExceptionType"/> is null for a catch-all (<c>catch (...)</c>), in which
    /// case <see cref="VariableName"/> is unused.
    /// </summary>
    internal record CppCatch(string? ExceptionType, string? VariableName, IReadOnlyList<CppStatement> Body);

    /// <summary>
    /// A <c>try</c> block followed by one or more <see cref="CppCatch"/> clauses.
    /// </summary>
    internal record CppTry(IReadOnlyList<CppStatement> Body, IReadOnlyList<CppCatch> Catches) : CppStatement;
}
