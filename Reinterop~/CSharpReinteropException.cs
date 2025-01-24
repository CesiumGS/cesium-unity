using Microsoft.CodeAnalysis;

namespace Reinterop
{
    /// <summary>
    /// Inserts the "ReinteropException" class into an assembly as it is compiled.
    /// This is intended to be used from a RegisterPostInitializationOutput
    /// callback on an IIncrementalGenerator.
    /// </summary>
    internal class CSharpReinteropException
    {
        public static void Generate(GeneratorExecutionContext context)
        {
            context.AddSource("ReinteropException", Source);
        }

        public const string Source =
            """
            namespace Reinterop
            {
                [Reinterop]
                internal class ReinteropException : System.Exception
                {
                    public ReinteropException(string message) : base(message) {}

                    internal static void ExposeToCPP()
                    {
                        ReinteropException e = new ReinteropException("message");
                    }
                }
            }
            """;
    }
}
