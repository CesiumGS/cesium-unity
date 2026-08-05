namespace Reinterop
{
    // The C# mirror of CppPrinter.cs - mechanically renders CSharpStatement/CSharpExpression trees
    // (see CSharpSyntax.cs) to C# source text. This is the only place that formatting decisions for
    // these nodes are made.
    internal static class CSharpPrinter
    {
        public static string Print(IReadOnlyList<CSharpStatement> statements)
        {
            return string.Join(Environment.NewLine, statements.Select(Print));
        }

        public static string Print(CSharpStatement statement)
        {
            return statement switch
            {
                CSharpVariableDeclaration { Initializer: null } d => $"{d.TypeName} {d.Name};",
                CSharpVariableDeclaration d => $"{d.TypeName} {d.Name} = {Print(d.Initializer!)};",
                CSharpExpressionStatement e => $"{Print(e.Expression)};",
                CSharpIf i => PrintIf(i),
                CSharpThrow t => $"throw {Print(t.Exception)};",
                CSharpReturn { Value: null } => "return;",
                CSharpReturn r => $"return {Print(r.Value!)};",
                CSharpRawStatement r => r.Text,
                _ => throw new NotSupportedException($"Unsupported {nameof(CSharpStatement)}: {statement.GetType().Name}")
            };
        }

        private static string PrintIf(CSharpIf i)
        {
            string condition = $"if ({Print(i.Condition)})";

            // Match the codebase's existing convention of omitting braces around a single-statement body.
            if (i.Then.Count == 1)
                return condition + Environment.NewLine + "    " + Print(i.Then[0]);

            string body = i.Then.Select(Print).JoinAndIndent("    ");
            return condition + Environment.NewLine + "{" + Environment.NewLine + "    " + body + Environment.NewLine + "}";
        }

        public static string Print(CSharpExpression expression)
        {
            return expression switch
            {
                CSharpIdentifier id => id.Name,
                CSharpRaw raw => raw.Text,
                CSharpCall c => $"{Print(c.Callee)}({string.Join(", ", c.Arguments.Select(Print))})",
                CSharpBinary b => $"{Print(b.Left)} {b.Op} {Print(b.Right)}",
                CSharpUnary u => $"{u.Op}{Print(u.Operand)}",
                CSharpCast c => $"({c.TypeName}){Print(c.Expression)}",
                _ => throw new NotSupportedException($"Unsupported {nameof(CSharpExpression)}: {expression.GetType().Name}")
            };
        }
    }
}
