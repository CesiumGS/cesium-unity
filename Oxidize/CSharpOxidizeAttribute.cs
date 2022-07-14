using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxidize
{
    /// <summary>
    /// Inserts the "OxidizeAttribute" class into an assembly as it is compiled.
    /// This is intended to be used from a RegisterPostInitializationOutput
    /// callback on an IIncrementalGenerator.
    /// </summary>
    internal class CSharpOxidizeAttribute
    {
        public static void Generate(IncrementalGeneratorPostInitializationContext context)
        {
            context.AddSource("OxidizeAttribute", source);
        }

        private const string source =
            """
            namespace Oxidize
            {
                [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
                internal class OxidizeAttribute : Attribute
                {
                }
            }
            """;
    }
}
