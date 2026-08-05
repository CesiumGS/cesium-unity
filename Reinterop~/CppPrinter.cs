namespace Reinterop
{
    // Mechanically renders CppStatement/CppExpression trees (see CppSyntax.cs) to C++ source text. This
    // is the only place that formatting decisions for these nodes are made.
    internal static class CppPrinter
    {
        public static string Print(IReadOnlyList<CppStatement> statements)
        {
            return string.Join(Environment.NewLine, statements.Select(Print));
        }

        public static string Print(CppStatement statement)
        {
            return statement switch
            {
                CppVariableDeclaration { Initializer: null } d => $"{d.TypeName} {d.Name};",
                CppVariableDeclaration d => $"{d.TypeName} {d.Name} = {Print(d.Initializer!)};",
                CppExpressionStatement e => $"{Print(e.Expression)};",
                CppAssignment a => $"{Print(a.Target)} = {Print(a.Value)};",
                CppIf i => PrintIf(i),
                CppThrow t => $"throw {Print(t.Exception)};",
                CppReturn { Value: null } => "return;",
                CppReturn r => $"return {Print(r.Value!)};",
                CppRawStatement r => r.Text,
                CppTry t => PrintTry(t),
                _ => throw new NotSupportedException($"Unsupported {nameof(CppStatement)}: {statement.GetType().Name}")
            };
        }

        private static string PrintIf(CppIf i)
        {
            string condition = $"if ({Print(i.Condition)})";

            // Match the codebase's existing convention of omitting braces around a single-statement body.
            if (i.Then.Count == 1)
                return condition + Environment.NewLine + "    " + Print(i.Then[0]);

            string body = i.Then.Select(Print).JoinAndIndent("    ");
            return condition + " {" + Environment.NewLine + "    " + body + Environment.NewLine + "}";
        }

        private static string PrintTry(CppTry t)
        {
            string result = "try {" + Environment.NewLine + "    " + t.Body.Select(Print).JoinAndIndent("    ");
            foreach (CppCatch c in t.Catches)
            {
                string header = c.ExceptionType == null ? "catch (...)" : $"catch ({c.ExceptionType}& {c.VariableName})";
                string catchBody = c.Body.Select(Print).JoinAndIndent("    ");
                result += Environment.NewLine + "} " + header + " {" + Environment.NewLine + "    " + catchBody;
            }
            result += Environment.NewLine + "}";
            return result;
        }

        public static string Print(CppExpression expression)
        {
            return expression switch
            {
                CppIdentifier id => id.Name,
                CppRaw raw => raw.Text,
                CppCall c => $"{Print(c.Callee)}({string.Join(", ", c.Arguments.Select(Print))})",
                CppBinary b => $"{Print(b.Left)} {b.Op} {Print(b.Right)}",
                CppUnary u => $"{u.Op}{Print(u.Operand)}",
                _ => throw new NotSupportedException($"Unsupported {nameof(CppExpression)}: {expression.GetType().Name}")
            };
        }
    }
}
