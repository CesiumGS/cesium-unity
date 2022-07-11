using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxidize
{
    internal class CppCodeGenerator
    {
        private string _headerPath;
        private string _implementationPath;
        private string _baseNamespace;

        public CppCodeGenerator(string headerPath, string implementationPath, string baseNamespace = "")
        {
            this._headerPath = headerPath;
            this._implementationPath = implementationPath;
            this._baseNamespace = baseNamespace;
        }

        const string baseNamespace = "";

        public void Generate(SourceProductionContext context, Compilation compilation, GenerationItem item)
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
                this.GenerateClass(context, item);
            }
        }
        
        private void GenerateClass(SourceProductionContext context, GenerationItem item)
        {
            INamedTypeSymbol? named = item.type as INamedTypeSymbol;
            if (named == null || named.IsGenericType)
            {
                Console.WriteLine("Skipping generation for generic type (for now).");
                return;
            }

            string cppNamespace = CppTypes.GetNamespace(this._baseNamespace, item.type);
            string className = item.type.Name;

            TypeDefinition definition = new TypeDefinition();

            CppCasts.GenerateDowncasts(this._baseNamespace, item, definition);
            GenerateUpcasts(item, definition);

            string header =
                $$"""
                #pragma once

                {{string.Join(Environment.NewLine, definition.headerIncludes)}}

                namespace {{cppNamespace}} {

                class {{className}} {
                public:
                  {{className}}();
                  explicit {{className}}(void* handle);

                  {{string.Join(Environment.NewLine + "  ", definition.declarations)}}

                private:
                   void* _handle;
                };

                }
                """;

            File.WriteAllText(className + ".h", header, Encoding.UTF8);

            string cpp =
                $$"""
                namespace {{cppNamespace}} {

                {{string.Join(Environment.NewLine + Environment.NewLine, definition.definitions)}}

                }
                """;

            File.WriteAllText(className + ".cpp", cpp, Encoding.UTF8);
        }

        private void GenerateUpcasts(GenerationItem item, TypeDefinition definition)
        {

        }
    }
}
