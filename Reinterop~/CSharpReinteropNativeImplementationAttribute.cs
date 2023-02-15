using Microsoft.CodeAnalysis;

namespace Reinterop
{
    /// <summary>
    /// Inserts the "ReinteropNativeImplementationAttribute" class into an assembly as it is compiled.
    /// This is intended to be used from a RegisterPostInitializationOutput
    /// callback on an IIncrementalGenerator.
    /// </summary>
    internal class CSharpReinteropNativeImplementationAttribute
    {
        public static void Generate(GeneratorExecutionContext context)
        {
            context.AddSource("ReinteropNativeImplementationAttribute", source);
        }

        private const string source =
            """
            using System;

            namespace Reinterop
            {
                [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
                internal class ReinteropNativeImplementationAttribute : Attribute
                {
                    public readonly string ImplementationClassName;

                    public readonly string HeaderName;

                    public readonly bool StaticOnly;

                    public ReinteropNativeImplementationAttribute(string implementationClassName, string headerName, bool staticOnly = false) {
                        this.ImplementationClassName = implementationClassName;
                        this.HeaderName = headerName;
                        this.StaticOnly = staticOnly;
                    }
                }
            }
            """;
    }
}
