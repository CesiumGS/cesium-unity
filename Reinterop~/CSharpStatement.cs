namespace Reinterop
{
    /// <summary>
    /// Base class for the small, restricted representation of C# statements needed to express
    /// Reinterop's generated interop bodies. This is not a general-purpose C# AST - it models the
    /// fixed statement shapes used by the generator and is rendered by <see cref="CSharpPrinter"/>.
    /// </summary>
    internal abstract record CSharpStatement;

    /// <summary>
    /// A local variable declaration, either <c>Type Name;</c> or
    /// <c>Type Name = Initializer;</c>. <see cref="Initializer"/> is null for a bare
    /// declaration whose value is assigned later. <paramref name="Type"/> may be null
    /// for a declaration of an implicitly-typed variable, e.g. <c>var x = 3;</c>.
    /// </summary>
    internal record CSharpVariableDeclaration(CSharpType? Type, string Name, CSharpExpression? Initializer = null) : CSharpStatement;

    /// <summary>
    /// An expression evaluated for its side effects and terminated with a semicolon, such as a
    /// native interop call or a handle reset call.
    /// </summary>
    internal record CSharpExpressionStatement(CSharpExpression Expression) : CSharpStatement;

    /// <summary>
    /// An <c>if</c> statement with an optional <c>else</c> body. A single-statement body is rendered
    /// without braces to match the existing generated C# style; multiple statements are enclosed in
    /// braces by <see cref="CSharpPrinter"/>.
    /// </summary>
    internal record CSharpIf(CSharpExpression Condition, IReadOnlyList<CSharpStatement> Then, IReadOnlyList<CSharpStatement>? Else = null) : CSharpStatement;

    /// <summary>
    /// A <c>throw</c> statement that throws <paramref name="Exception"/>, typically the managed
    /// exception reconstructed from the native exception out-parameter.
    /// </summary>
    internal record CSharpThrow(CSharpExpression Exception) : CSharpStatement;

    /// <summary>
    /// A <c>return</c> statement. <see cref="Value"/> is null for a return from a void method.
    /// </summary>
    internal record CSharpReturn(CSharpExpression? Value = null) : CSharpStatement;

    /// <summary>
    /// A <c>try</c> block and its managed exception catch body. The printer emits the generated
    /// <c>catch (System.Exception reinteropManagedException)</c> header used by the interop wrapper;
    /// the catch variable is intentionally not configurable because this node models that fixed
    /// generator pattern.
    /// </summary>
    internal record CSharpTryCatch(IReadOnlyList<CSharpStatement> TryBody, IReadOnlyList<CSharpStatement> CatchBody) : CSharpStatement;
}
