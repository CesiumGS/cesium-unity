namespace Oxidize
{
    public static class GenerationUtility
    {
        public static string JoinAndIndent(this IEnumerable<string> lines, string indent)
        {
            return string.Join(Environment.NewLine + indent, lines.Select(line => line.Replace(Environment.NewLine, Environment.NewLine + indent)));
        }
    }
}
