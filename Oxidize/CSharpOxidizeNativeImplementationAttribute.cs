using Microsoft.CodeAnalysis;

namespace Oxidize
{
    /// <summary>
    /// Inserts the "OxidizeNativeImplementationAttribute" class into an assembly as it is compiled.
    /// This is intended to be used from a RegisterPostInitializationOutput
    /// callback on an IIncrementalGenerator.
    /// </summary>
    internal class CSharpOxidizeNativeImplementationAttribute
    {
        public static void Generate(IncrementalGeneratorPostInitializationContext context)
        {
            context.AddSource("OxidizeNativeImplementationAttribute", source);
        }

        private const string source =
            """
            using System;

            namespace Oxidize
            {
                [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
                internal class OxidizeNativeImplementationAttribute : Attribute
                {
                    public readonly string ImplementationClassName;

                    public readonly string HeaderName;

                    public OxidizeNativeImplementationAttribute(string implementationClassName, string headerName) {
                        this.ImplementationClassName = implementationClassName;
                        this.HeaderName = headerName;
                    }
                }
            }
            """;
    }
}
