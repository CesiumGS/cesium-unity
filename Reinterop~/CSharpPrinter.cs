namespace Reinterop
{
    // The C# mirror of CppPrinter.cs - mechanically renders CSharpStatement/CSharpExpression trees
    // (see CSharpSyntax.cs) to C# source text. This is the only place that formatting decisions for
    // these nodes are made.
    internal static class CSharpPrinter
    {
        public static string Print(IReadOnlyList<CSharpStatement> statements, string indent = "")
        {
            return statements.Select(Print).JoinAndIndent(indent);
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
                CSharpTryCatch t => PrintTryCatch(t),
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

        private static string PrintTryCatch(CSharpTryCatch t)
        {
            string tryBody = t.TryBody.Select(Print).JoinAndIndent("    ");
            string catchBody = t.CatchBody.Select(Print).JoinAndIndent("    ");

            return "try" + Environment.NewLine + "{" + Environment.NewLine + "    " + tryBody + Environment.NewLine + "}" + Environment.NewLine +
                   "catch (System.Exception reinteropManagedException)" + Environment.NewLine + "{" + Environment.NewLine + "    " + catchBody + Environment.NewLine + "}";
        }

        public static string Print(CSharpExpression expression)
        {
            return expression switch
            {
                CSharpIdentifier id => id.Name,
                CSharpRaw raw => raw.Text,
                CSharpLiteral literal => literal.Value,
                CSharpCall c => $"{PrintParenthesized(c.Callee)}({string.Join(", ", c.Arguments.Select(Print))})",
                CSharpBinary b => $"{PrintParenthesized(b.Left)} {b.Op} {PrintParenthesized(b.Right)}",
                CSharpUnary u => $"{u.Op}{PrintParenthesized(u.Operand)}",
                CSharpCast c => $"({c.TypeName}){PrintParenthesized(c.Expression)}",
                CSharpMemberAccess m => $"{PrintParenthesized(m.Target)}.{m.MemberName}",
                CSharpNew n => $"new {n.TypeName}({string.Join(", ", n.Arguments.Select(Print))})",
                CSharpTernary t => $"{PrintParenthesized(t.Condition)} ? {PrintParenthesized(t.Then)} : {PrintParenthesized(t.Else)}",
                CSharpIs i => $"{PrintParenthesized(i.Expression)} is {i.TypeName} {i.castedVariableName ?? ""}",
                _ => throw new NotSupportedException($"Unsupported {nameof(CSharpExpression)}: {expression.GetType().Name}")
            };
        }

        public static string PrintParenthesized(CSharpExpression expression)
        {
            // Only parenthesize complex expressions
            return expression switch
            {
                CSharpIdentifier id => Print(id),
                CSharpLiteral literal => Print(literal),
                CSharpRaw raw => Print(raw),
                CSharpMemberAccess access => Print(access),
                CSharpCall call => Print(call),
                _ => $"({Print(expression)})"
            };
        }
    }
}
