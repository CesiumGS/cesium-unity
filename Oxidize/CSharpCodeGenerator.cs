using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxidize
{
    internal class CSharpCodeGenerator
    {
        public static void Generate(SourceProductionContext context, GenerationItem item)
        {
            Console.WriteLine("Generating bindings for " + item.type.ToDisplayString());
            if (item.baseClass != null)
            {
                Console.WriteLine("  Base Class: " + item.baseClass.type.ToDisplayString());
            }
            Console.WriteLine("  Interfaces");
            foreach (GenerationItem anInterface in item.interfaces)
            {
                Console.WriteLine("    " + anInterface.type.ToDisplayString());
            }
            Console.WriteLine("  Properties");
            foreach (IPropertySymbol property in item.properties)
            {
                Console.WriteLine("    " + property.ToDisplayString());
            }
            Console.WriteLine("  Methods");
            foreach (IMethodSymbol method in item.methods)
            {
                Console.WriteLine("    " + method.ToDisplayString());
            }

            if (item.type.IsReferenceType)
            {
                GenerateClass(context, item);
            }

        }

        private static void GenerateClass(SourceProductionContext context, GenerationItem item)
        {
        }
    }
}
