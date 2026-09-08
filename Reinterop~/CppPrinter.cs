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
                CppVariableDeclaration { Initializer: null } d => $"{d.Type.GetFullyQualifiedName()} {d.Name};",
                CppVariableDeclaration { UseBracedInitialization: true } d => $"{d.Type.GetFullyQualifiedName()} {d.Name}{{{Print(d.Initializer!)}}};",
                CppVariableDeclaration d => $"{d.Type.GetFullyQualifiedName()} {d.Name} = {Print(d.Initializer!)};",
                CppExpressionStatement e => $"{Print(e.Expression)};",
                CppAssignment a => $"{Print(a.Target)} = {Print(a.Value)};",
                CppIf i => PrintIf(i),
                CppThrow { Exception: null } => "throw;",
                CppThrow t => $"throw {Print(t.Exception!)};",
                CppReturn { Value: null } => "return;",
                CppReturn r => $"return {Print(r.Value!)};",
                CppTry t => PrintTry(t),
                _ => throw new NotSupportedException($"Unsupported {nameof(CppStatement)}: {statement.GetType().Name}")
            };
        }

        private static string PrintIf(CppIf i)
        {
            string condition = $"if ({Print(i.Condition)})";

            // Match the codebase's existing convention of omitting braces around a single-statement body.
            if (i.Else != null)
            {
                string thenBody = i.Then.Select(Print).JoinAndIndent("    ");
                string elseBody = i.Else.Select(Print).JoinAndIndent("    ");
                return condition + " {" + Environment.NewLine + "    " + thenBody + Environment.NewLine + "} else {" + Environment.NewLine + "    " + elseBody + Environment.NewLine + "}";
            }

            string result = i.Then.Count == 1
                ? condition + Environment.NewLine + "    " + Print(i.Then[0])
                : condition + " {" + Environment.NewLine + "    " + i.Then.Select(Print).JoinAndIndent("    ") + Environment.NewLine + "}";
            return result;
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
                CppLiteral literal => literal.Value,
                CppCall c => $"{PrintParenthesized(c.Callee)}({string.Join(", ", c.Arguments.Select(Print))})",
                CppNew n => $"new {n.Type.GetFullyQualifiedName()}({string.Join(", ", n.Arguments.Select(Print))})",
                CppTernary t => $"{PrintParenthesized(t.Condition)} ? {PrintParenthesized(t.Then)} : {PrintParenthesized(t.Else)}",
                CppBinary b => $"{PrintParenthesized(b.Left)} {b.Op} {PrintParenthesized(b.Right)}",
                CppUnary u => $"{u.Op}{PrintParenthesized(u.Operand)}",
                CppCast c => c.Kind switch
                {
                    CppCastKind.CStyle => $"({c.TargetType.GetFullyQualifiedName()})({Print(c.Expression)})",
                    CppCastKind.Static => $"static_cast<{c.TargetType.GetFullyQualifiedName()}>({Print(c.Expression)})",
                    CppCastKind.Const => $"const_cast<{c.TargetType.GetFullyQualifiedName()}>({Print(c.Expression)})",
                    CppCastKind.Reinterpret => $"reinterpret_cast<{c.TargetType.GetFullyQualifiedName()}>({Print(c.Expression)})",
                    _ => throw new NotSupportedException($"Unsupported {nameof(CppCastKind)}: {c.Kind}")
                },
                CppMove m => $"std::move({Print(m.Expression)})",
                CppMemberAccess m => $"{PrintParenthesized(m.Target)}.{m.MemberName}",
                CppPointerMemberAccess m => $"{PrintParenthesized(m.Target)}->{m.MemberName}",
                _ => throw new NotSupportedException($"Unsupported {nameof(CppExpression)}: {expression.GetType().Name}")
            };
        }

        private static string PrintParenthesized(CppExpression expression)
        {
            return expression switch
            {
                CppIdentifier or CppLiteral or CppCall or CppNew or CppCast or CppMove or CppMemberAccess or CppPointerMemberAccess => Print(expression),
                _ => $"({Print(expression)})"
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
                    d.Type.AddSourceIncludesToSet(result);
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
                    if (i.Else != null)
                        foreach (CppStatement s in i.Else)
                            GetRequiredIncludes(s, result);
                    break;
                case CppThrow t:
                    if (t.Exception != null)
                        GetRequiredIncludes(t.Exception, result);
                    break;
                case CppReturn r:
                    if (r.Value != null)
                        GetRequiredIncludes(r.Value, result);
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
                case CppLiteral l:
                    break;
                case CppCall c:
                    GetRequiredIncludes(c.Callee, result);
                    foreach (CppExpression arg in c.Arguments)
                        GetRequiredIncludes(arg, result);
                    break;
                case CppNew n:
                    n.Type.AddSourceIncludesToSet(result);
                    foreach (CppExpression arg in n.Arguments)
                        GetRequiredIncludes(arg, result);
                    break;
                case CppTernary t:
                    GetRequiredIncludes(t.Condition, result);
                    GetRequiredIncludes(t.Then, result);
                    GetRequiredIncludes(t.Else, result);
                    break;
                case CppBinary b:
                    GetRequiredIncludes(b.Left, result);
                    GetRequiredIncludes(b.Right, result);
                    break;
                case CppUnary u:
                    GetRequiredIncludes(u.Operand, result);
                    break;
                case CppCast c:
                    c.TargetType.AddSourceIncludesToSet(result);
                    GetRequiredIncludes(c.Expression, result);
                    break;
                case CppMove m:
                    result.Add("<utility>"); // for std::move
                    GetRequiredIncludes(m.Expression, result);
                    break;
                case CppMemberAccess m:
                    GetRequiredIncludes(m.Target, result);
                    break;
                case CppPointerMemberAccess m:
                    GetRequiredIncludes(m.Target, result);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported {nameof(CppExpression)}: {expression.GetType().Name}");
            }
        }
    }
}
