using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxidize
{
    internal class CppCodeGenerator
    {
        private CppGenerationContext _options;

        public CppCodeGenerator(CppGenerationContext options)
        {
            this._options = options;
        }

        public void Generate(SourceProductionContext context, GenerationItem item)
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

            CppType itemType = CppType.FromCSharp(this._options, item.type);

            string cppNamespace = itemType.GetFullyQualifiedNamespace();
            string className = item.type.Name;

            TypeDefinition definition = new TypeDefinition();

            if (!item.type.IsStatic)
            {
                // Instances of this class may exist, so we need a field to hold the object handle and
                // a constructor to create this wrapper from a handle.
                definition.privateDeclarations.Add("::Oxidize::ObjectHandle _handle;");
                definition.declarations.Add($"explicit {className}(::Oxidize::ObjectHandle&& handle) noexcept;");
                definition.definitions.Add(
                    $$"""
                    explicit {{className}}::{{className}}(::Oxidize::ObjectHandle&& handle) noexcept :
                      _handle(std::move(handle)) {}
                    """);

                // Also allow up- and down-casting this instance to related types.
                CppCasts.GenerateDowncasts(this._options, item, definition);
                CppCasts.GenerateUpcasts(this._options, item, definition);
            }
            else
            {
                // Instances of wrappers for static classes can never be constructed.
                definition.declarations.Add($"{className}() = delete;");
            }

            CppProperties.GenerateProperties(this._options, item, definition);
            CppMethods.GenerateMethods(this._options, item, definition);

            string header =
                $$"""
                #pragma once

                {{string.Join(Environment.NewLine, definition.headerIncludes.Select(include => "#include " + include))}}
                {{string.Join(Environment.NewLine, definition.forwardDeclarations)}}

                namespace {{cppNamespace}} {

                class {{className}} {
                public:
                  {{string.Join(Environment.NewLine + "  ", definition.declarations)}}

                  const ObjectHandle& GetHandle() const { return this->_handle; }

                private:
                  {{string.Join(Environment.NewLine + "  ", definition.privateDeclarations)}}
                };

                }
                """;

            Directory.CreateDirectory("generated/include");
            File.WriteAllText("generated/include/" + className + ".h", header, Encoding.UTF8);

            string cpp =
                $$"""
                {{string.Join(Environment.NewLine, definition.cppIncludes.Select(include => "#include " + include))}}

                namespace {{cppNamespace}} {

                {{string.Join(Environment.NewLine + Environment.NewLine, definition.definitions)}}

                }
                """;

            Directory.CreateDirectory("generated/src");
            File.WriteAllText("generated/src/" + className + ".cpp", cpp, Encoding.UTF8);
        }
    }
}
