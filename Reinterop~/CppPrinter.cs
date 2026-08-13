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
                CppCast c => $"{c.targetType.GetFullyQualifiedName()}({Print(c.Expression)})",
                CppMove m => $"std::move({Print(m.Expression)})",
                _ => throw new NotSupportedException($"Unsupported {nameof(CppExpression)}: {expression.GetType().Name}")
            };
        }

        public static HashSet<string> GetRequiredIncludes(IEnumerable<CppStatement> statements)
        {
            HashSet<string> result = new();
            foreach (CppStatement statement in statements)
                GetRequiredIncludes(statement, result);
            return result;
        }

        public static HashSet<string> GetRequiredIncludes(IEnumerable<CppExpression> expressions)
        {
            HashSet<string> result = new();
            foreach (CppExpression expression in expressions)
                GetRequiredIncludes(expression, result);
            return result;
        }

        public static HashSet<string> GetRequiredIncludes(CppExpression expression)
        {
            HashSet<string> result = new();
            GetRequiredIncludes(expression, result);
            return result;
        }

        private static void GetRequiredIncludes(CppStatement statement, HashSet<string> result)
        {
            switch(statement)
            {
                case CppVariableDeclaration d:
                    if (d.Initializer != null)
                        GetRequiredIncludes(d.Initializer, result);
                    break;
                case CppExpressionStatement e:
                    GetRequiredIncludes(e.Expression, result);
                    break;
                case CppAssignment a:
                    GetRequiredIncludes(a.Target, result);
                    GetRequiredIncludes(a.Value, result);
                    break;
                case CppIf i:
                    GetRequiredIncludes(i.Condition, result);
                    foreach (CppStatement s in i.Then)
                        GetRequiredIncludes(s, result);
                    break;
                case CppThrow t:
                    GetRequiredIncludes(t.Exception, result);
                    break;
                case CppReturn r:
                    if (r.Value != null)
                        GetRequiredIncludes(r.Value, result);
                    break;
                case CppRawStatement r:
                    break;
                case CppTry t:
                    foreach (CppStatement s in t.Body)
                        GetRequiredIncludes(s, result);
                    foreach (CppCatch c in t.Catches)
                        foreach (CppStatement s in c.Body)
                            GetRequiredIncludes(s, result);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported {nameof(CppStatement)}: {statement.GetType().Name}");
            };
        }

        private static void GetRequiredIncludes(CppExpression expression, HashSet<string> result)
        {
            if (expression.RequiredIncludes != null)
            {
                foreach (string include in expression.RequiredIncludes)
                    result.Add(include);
            }

            switch(expression)
            {
                case CppIdentifier id:
                    break;
                case CppRaw raw:
                    break;
                case CppCall c:
                    GetRequiredIncludes(c.Callee, result);
                    foreach (CppExpression arg in c.Arguments)
                        GetRequiredIncludes(arg, result);
                    break;
                case CppBinary b:
                    GetRequiredIncludes(b.Left, result);
                    GetRequiredIncludes(b.Right, result);
                    break;
                case CppUnary u:
                    GetRequiredIncludes(u.Operand, result);
                    break;
                case CppCast c:
                    GetRequiredIncludes(c.Expression, result);
                    break;
                case CppMove m:
                    result.Add("<utility>"); // for std::move
                    GetRequiredIncludes(m.Expression, result);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported {nameof(CppExpression)}: {expression.GetType().Name}");
            }
        }
    }
}
