using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Text;

namespace Oxidize
{
    internal class CSharpCodeGenerator
    {
        public static void Generate(SourceProductionContext context, Compilation compilation, ImmutableArray<GeneratedResult?> results)
        {
            GeneratedCSharpInit combined = GeneratedCSharpInit.Merge(results.Select(result => result == null ? new GeneratedCSharpInit() : result.CSharpInit));
            Console.WriteLine(combined.ToSourceFileString());
            context.AddSource("OxidizeInitializer", combined.ToSourceFileString());
        }
    }
}
