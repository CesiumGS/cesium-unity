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

            HashSet<string> includes = new HashSet<string>();
            includes.Add("<cassert>");
            includes.Add("<cstdint>");

            foreach (InteropFunction interop in interopFunctions)
            {
                interop.CppType.AddSourceIncludesToSet(includes);
            }

            string initialize = $$"""
                {{string.Join(Environment.NewLine, includes.Select(include => "#include " + include))}}

                extern "C" {

                __declspec(dllexport) void initializeOxidize(void** functionPointers, std::int32_t count) {
                  // If this assertion fails, the C# and C++ layers are out of sync.
                  assert(count == {{interopFunctions.Count()}});
                
                  std::int32_t i = 0;
                  {{string.Join(Environment.NewLine + "  ", assignments)}}
                }

                }
                """;

            Directory.CreateDirectory(Options.OutputSourceDirectory);
            File.WriteAllText(Path.Combine(Options.OutputSourceDirectory, "initializeOxidize.cpp"), initialize, Encoding.UTF8);
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

            // No need to generate code for primitives.
            if (itemType.Kind == CppTypeKind.Primitive)
                return null;

            TypeDefinition definition = new TypeDefinition();
            definition.Type = itemType;

            itemType.AddSourceIncludesToSet(definition.cppIncludes);

            if (!item.type.IsStatic)
            {
                // Instances of this class may exist, so we need a field to hold the object handle and
                // a constructor to create this wrapper from a handle.
                CppType objectHandleType = CppObjectHandle.GetCppType(this.Options);
                objectHandleType.AddForwardDeclarationsToSet(definition.forwardDeclarations);
                objectHandleType.AddSourceIncludesToSet(definition.cppIncludes);

                // We need a full definition for ObjectHandle even in the header in order to declare the field.
                objectHandleType.AddSourceIncludesToSet(definition.headerIncludes);
                definition.privateDeclarations.Add($"{objectHandleType.GetFullyQualifiedName()} _handle;");

                definition.cppIncludes.Add("<utility>"); // for std::move
                definition.declarations.Add($"explicit {itemType.Name}({objectHandleType.GetFullyQualifiedName()}&& handle) noexcept;");
                definition.definitions.Add(
                    $$"""
                    {{itemType.Name}}::{{itemType.Name}}({{objectHandleType.GetFullyQualifiedName()}}&& handle) noexcept :
                      _handle(std::move(handle)) {}
                    """);

                definition.declarations.Add($"const {objectHandleType.GetFullyQualifiedName()}& GetHandle() const;");
                definition.definitions.Add(
                    $$"""
                    const {{objectHandleType.GetFullyQualifiedName()}}& {{itemType.Name}}::GetHandle() const {
                        return this->_handle;
                    }
                    """);

                // Also allow up- and down-casting this instance to related types.
                CppCasts.GenerateDowncasts(this.Options, item, definition);
                CppCasts.GenerateUpcasts(this.Options, item, definition);
            }
            else
            {
                // Instances of wrappers for static classes can never be constructed.
                definition.declarations.Add($"{itemType.Name}() = delete;");
            }

            definition.headerIncludes.Add("<cstdint>");
            definition.privateDeclarations.Add("friend void ::initializeOxidize(void** functionPointers, std::int32_t count);");

            CppProperties.GenerateProperties(this.Options, item, definition);
            CppMethods.GenerateMethods(this.Options, item, definition);

            return definition;
        }

        public void WriteType(TypeDefinition? definition)
        {
            if (definition == null)
                return;

            CppType? type = definition.Type;
            if (type == null)
                return;

            string header =
                $$"""
                #pragma once

                {{string.Join(Environment.NewLine, definition.headerIncludes.Select(include => "#include " + include))}}
                {{string.Join(Environment.NewLine, definition.forwardDeclarations)}}

                extern "C" {
                __declspec(dllexport) void initializeOxidize(void** functionPointers, std::int32_t count);
                }
                
                namespace {{type.GetFullyQualifiedNamespace(false)}} {

                class {{type.Name}} {
                public:
                  {{string.Join(Environment.NewLine + "  ", definition.declarations)}}

                private:
                  {{string.Join(Environment.NewLine + "  ", definition.privateDeclarations)}}
                };

                }
                """;

            string headerPath = Path.Combine(new string[] { Options.OutputHeaderDirectory }.Concat(type.Namespaces).ToArray());
            Directory.CreateDirectory(headerPath);
            File.WriteAllText(Path.Combine(headerPath, type.Name + ".h"), header, Encoding.UTF8);

            string cpp =
                $$"""
                {{string.Join(Environment.NewLine, definition.cppIncludes.Select(include => "#include " + include))}}

                namespace {{type.GetFullyQualifiedNamespace(false)}} {

                {{string.Join(Environment.NewLine + Environment.NewLine, definition.definitions)}}

                }
                """;

            Directory.CreateDirectory(Options.OutputSourceDirectory);
            File.WriteAllText(Path.Combine(Options.OutputSourceDirectory, type.Name + ".cpp"), cpp, Encoding.UTF8);
        }
    }
}
