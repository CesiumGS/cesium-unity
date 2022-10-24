using Microsoft.CodeAnalysis;

namespace Reinterop
{
    /// <summary>
    /// Inserts the "ReinteropAttribute" class into an assembly as it is compiled.
    /// This is intended to be used from a RegisterPostInitializationOutput
    /// callback on an IIncrementalGenerator.
    /// </summary>
    internal class CSharpReinteropAttribute
    {
        public static void Generate(GeneratorExecutionContext context)
        {
            context.AddSource("ReinteropAttribute", source);
        }

        private const string source =
            """
            using System;

            namespace Reinterop
            {
                [AttributeUsage(AttributeTargets.Class)]
                internal class ReinteropAttribute : Attribute
                {
                }
            }
            """;
    }
}
