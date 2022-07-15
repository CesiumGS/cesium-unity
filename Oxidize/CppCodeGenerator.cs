using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Oxidize
{
    internal class CppCodeGenerator
    {
        public readonly CppGenerationContext Options;

        public CppCodeGenerator(CppGenerationContext options)
        {
            this.Options = options;
        }

        //public void Generate(GenerationItem item)
        //{
        //    Console.WriteLine("Generating bindings for " + item.type.ToDisplayString());
        //    if (item.baseClass != null)
        //    {
        //        Console.WriteLine("  Base Class: " + item.baseClass.type.ToDisplayString());
        //    }
        //    Console.WriteLine("  Interfaces");
        //    foreach (GenerationItem anInterface in item.interfaces)
        //    {
        //        Console.WriteLine("    " + anInterface.type.ToDisplayString());
        //    }
        //    Console.WriteLine("  Properties");
        //    foreach (IPropertySymbol property in item.properties)
        //    {
        //        Console.WriteLine("    " + property.ToDisplayString());
        //    }
        //    Console.WriteLine("  Methods");
        //    foreach (IMethodSymbol method in item.methods)
        //    {
        //        Console.WriteLine("    " + method.ToDisplayString());
        //    }

        //    if (item.type.IsReferenceType)
        //    {
        //        this.GenerateClass(item);
        //    }
        //}

        public void WriteInitializeFunction(ImmutableArray<TypeDefinition?> typeDefinitions)
        {
            var interopFunctions = typeDefinitions.SelectMany(typeDefinition => typeDefinition == null ? new List<InteropFunction>() : typeDefinition.interopFunctions);
            var assignments = interopFunctions.Select(function => $"{function.CppTarget} = reinterpret_cast<{function.CppSignature}>(functionPointers[i++]);");

            string initialize = $$"""
                extern "C" {

                void initializeOxidize(void** functionPointers, int32_t count) {
                  if (count != {{interopFunctions.Count()}}) {
                    throw std::runtime_error("Incorrect number of function pointers provided. Are the C# and C++ layers in sync?");
                  }
                
                  std::int32_t i = 0;
                  {{string.Join(Environment.NewLine + "  ", assignments)}}
                }

                }
                """;

            Directory.CreateDirectory("generated/src");
            File.WriteAllText("generated/src/initializeOxidize.cpp", initialize, Encoding.UTF8);
        }

        public TypeDefinition? GenerateType(GenerationItem item)
        {
            INamedTypeSymbol? named = item.type as INamedTypeSymbol;
            if (named == null || named.IsGenericType)
            {
                Console.WriteLine("Skipping generation for generic type (for now).");
                return null;
            }

            CppType itemType = CppType.FromCSharp(this.Options, item.type);

            TypeDefinition definition = new TypeDefinition();
            definition.typeName = item.type.Name;
            definition.typeNamespace = itemType.GetFullyQualifiedNamespace();

            if (!item.type.IsStatic)
            {
                // Instances of this class may exist, so we need a field to hold the object handle and
                // a constructor to create this wrapper from a handle.
                definition.privateDeclarations.Add("::Oxidize::ObjectHandle _handle;");
                definition.declarations.Add($"explicit {definition.typeName}(::Oxidize::ObjectHandle&& handle) noexcept;");
                definition.definitions.Add(
                    $$"""
                    explicit {{definition.typeName}}::{{definition.typeName}}(::Oxidize::ObjectHandle&& handle) noexcept :
                      _handle(std::move(handle)) {}
                    """);

                // Also allow up- and down-casting this instance to related types.
                CppCasts.GenerateDowncasts(this.Options, item, definition);
                CppCasts.GenerateUpcasts(this.Options, item, definition);
            }
            else
            {
                // Instances of wrappers for static classes can never be constructed.
                definition.declarations.Add($"{definition.typeName}() = delete;");
            }

            CppProperties.GenerateProperties(this.Options, item, definition);
            CppMethods.GenerateMethods(this.Options, item, definition);

            return definition;
        }

        public void WriteType(TypeDefinition? definition)
        {
            if (definition == null)
                return;

            string header =
                $$"""
                #pragma once

                {{string.Join(Environment.NewLine, definition.headerIncludes.Select(include => "#include " + include))}}
                {{string.Join(Environment.NewLine, definition.forwardDeclarations)}}

                namespace {{definition.typeNamespace}} {

                class {{definition.typeName}} {
                public:
                  {{string.Join(Environment.NewLine + "  ", definition.declarations)}}

                  const ObjectHandle& GetHandle() const { return this->_handle; }

                private:
                  friend void initializeOxidize(void** functionPointers, int32_t count);
                  {{string.Join(Environment.NewLine + "  ", definition.privateDeclarations)}}
                };

                }
                """;

            Directory.CreateDirectory("generated/include");
            File.WriteAllText("generated/include/" + definition.typeName + ".h", header, Encoding.UTF8);

            string cpp =
                $$"""
                {{string.Join(Environment.NewLine, definition.cppIncludes.Select(include => "#include " + include))}}

                namespace {{definition.typeNamespace}} {

                {{string.Join(Environment.NewLine + Environment.NewLine, definition.definitions)}}

                }
                """;

            Directory.CreateDirectory("generated/src");
            File.WriteAllText("generated/src/" + definition.typeName + ".cpp", cpp, Encoding.UTF8);
        }
    }
}
