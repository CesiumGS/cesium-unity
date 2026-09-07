using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Concurrent;

namespace Reinterop
{
    /// <summary>
    /// Catches exceptions thrown while running generator pipeline steps and reports them as
    /// compiler diagnostics (including the full call stack) instead of letting an unhandled
    /// exception silently prevent the generator from producing any output.
    /// </summary>
    internal static class GeneratorDiagnostics
    {
        public static readonly DiagnosticDescriptor GeneratorCrashed = new DiagnosticDescriptor(
#pragma warning disable RS2008
            id: "REINTEROP001",
#pragma warning restore RS2008
            title: "Reinterop generator crashed",
            messageFormat: "The Reinterop source generator threw an exception and could not finish generating code: {0}",
            category: "Reinterop",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        // Unity's build output parser expects a file/line prefix and silently drops diagnostics
        // reported with Location.None, so callers without a more specific location should use this.
        public static Location GetFallbackLocation(Compilation compilation)
        {
            SyntaxTree? tree = compilation.SyntaxTrees.FirstOrDefault();
            if (tree == null)
                return Location.None;

            return Location.Create(tree, new TextSpan(0, 0));
        }

        // Unity's build output parser also treats each line of output separately and only
        // recognizes lines starting with "path(line,col):", so a multi-line exception message
        // (e.g. one with a stack trace) gets truncated to just its first line. Flatten it to a
        // single line so the full stack trace survives.
        public static string FormatException(Exception ex)
        {
            return ex.ToString().Replace("\r\n", " | ").Replace("\n", " | ");
        }

        // Shared across pipeline steps because Roslyn's incremental generator infrastructure
        // gives most steps no way to report a diagnostic directly; only the terminal
        // RegisterSourceOutput/RegisterImplementationSourceOutput callbacks have a context that can.
        private static readonly ConcurrentQueue<Exception> _caught = new ConcurrentQueue<Exception>();

        /// <summary>
        /// Runs <paramref name="func"/>, catching any exception it throws so the generator
        /// pipeline can keep running. The exception is queued to be reported by the next call
        /// to <see cref="ReportCaughtExceptions"/>.
        /// </summary>
        public static T? Try<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                _caught.Enqueue(ex);
                return default;
            }
        }

        /// <summary>
        /// Runs <paramref name="action"/>, catching any exception it throws so the generator
        /// pipeline can keep running. The exception is queued to be reported by the next call
        /// to <see cref="ReportCaughtExceptions"/>.
        /// </summary>
        public static void Try(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                _caught.Enqueue(ex);
            }
        }

        /// <summary>
        /// Reports any exceptions previously caught by <see cref="Try"/> as compiler errors,
        /// with the exception's full call stack included in the diagnostic message.
        /// </summary>
        public static void ReportCaughtExceptions(Action<Diagnostic> reportDiagnostic)
        {
            while (_caught.TryDequeue(out Exception? ex))
            {
                reportDiagnostic(Diagnostic.Create(GeneratorCrashed, Location.None, FormatException(ex)));
            }
        }
    }
}
