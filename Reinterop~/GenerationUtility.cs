namespace Reinterop
{
    public static class GenerationUtility
    {
        public static string JoinAndIndent(this IEnumerable<string> lines, string indent, bool newlineBetweenEach = false)
        {
            var formattedLines = lines.Select(line => line.Replace(Environment.NewLine, Environment.NewLine + indent));
            string separator = Environment.NewLine + indent;
            if (newlineBetweenEach)
                separator = Environment.NewLine + separator;
            return string.Join(separator, formattedLines);
        }
    }
}
