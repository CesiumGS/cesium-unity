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

        public static CppType GetCppWrapperType(CppGenerationContext context)
        {
            List<string> ns = new List<string>();
            if (context.BaseNamespace.Length > 0)
                ns.Add(context.BaseNamespace);
            ns.Add("Reinterop");

            // If the first two namespaces are identical, remove the duplication.
            // This is to avoid `Reinterop::Reinterop`.
            if (ns.Count >= 2 && ns[0] == ns[1])
                ns.RemoveAt(0);

            return new CppType(InteropTypeKind.ClassWrapper, ns, "ReinteropException", null, 0);
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
                        string s = e.Message;
                    }
                }
            }
            """;
    }
}
