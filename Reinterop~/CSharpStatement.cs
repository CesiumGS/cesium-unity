namespace Reinterop
{
    /// <summary>Base class for the restricted C# statement tree used by the generator.</summary>
    internal abstract record CSharpStatement;

    /// <summary>A local variable declaration, optionally with an initializer.</summary>
    internal record CSharpVariableDeclaration(string TypeName, string Name, CSharpExpression? Initializer = null) : CSharpStatement;

    /// <summary>An expression evaluated for its side effects.</summary>
    internal record CSharpExpressionStatement(CSharpExpression Expression) : CSharpStatement;

    /// <summary>An <c>if</c> statement with optional <c>else</c> statements.</summary>
    internal record CSharpIf(CSharpExpression Condition, IReadOnlyList<CSharpStatement> Then, IReadOnlyList<CSharpStatement>? Else = null) : CSharpStatement;

    /// <summary>A <c>throw</c> statement.</summary>
    internal record CSharpThrow(CSharpExpression Exception) : CSharpStatement;

    /// <summary>A <c>return</c> statement.</summary>
    internal record CSharpReturn(CSharpExpression? Value = null) : CSharpStatement;

    /// <summary>A try block with its managed exception catch body.</summary>
    internal record CSharpTryCatch(IReadOnlyList<CSharpStatement> TryBody, IReadOnlyList<CSharpStatement> CatchBody) : CSharpStatement;
}
