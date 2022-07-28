using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.PortableExecutable;
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

        public void WriteInitializeFunction(ImmutableArray<GeneratedResult?> results)
        {
            GeneratedCppInit init = GeneratedCppInit.Merge(results.Select(result => result == null ? new GeneratedCppInit() : result.CppInit));

            Directory.CreateDirectory(Options.OutputSourceDirectory);
            File.WriteAllText(Path.Combine(Options.OutputSourceDirectory, "initializeOxidize.cpp"), init.ToSourceFileString(), Encoding.UTF8);

            string headerPath = Options.OutputHeaderDirectory;
            if (this.Options.BaseNamespace != null)
                headerPath = Path.Combine(headerPath, this.Options.BaseNamespace);

            Directory.CreateDirectory(headerPath);
            File.WriteAllText(Path.Combine(headerPath, "initializeOxidize.h"), init.ToHeaderFileString(), Encoding.UTF8);
        }

        public GeneratedResult? GenerateType(GenerationItem item)
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

            GeneratedResult result = new GeneratedResult(itemType);

            Interop.GenerateForType(this.Options, item, result);
            CppHandleManagement.Generate(this.Options, item, result);
            CppConstructors.Generate(this.Options, item, result);
            CppCasts.Generate(this.Options, item, result);
            CppFields.Generate(this.Options, item, result);

            // Generate properties and methods throughout the whole inheritance hierarchy.
            GenerationItem? current = item;
            while (current != null)
            {
                CppProperties.Generate(this.Options, item, current, result);
                CppMethods.Generate(this.Options, item, current, result);
                current = current.baseClass;
            }

            // If this class has partial methods that are meant to be implemented in C++,
            // generate the necessary bindings.
            if (item.implClassName != null)
            {
                // TODO: parse out namespaces? Require user to specify them separately?
                CppType implementationType = new CppType(CppTypeKind.Unknown, Array.Empty<string>(), item.implClassName, null, 0, item.implHeaderName);
                result.CppImplementationInvoker = new GeneratedCppImplementationInvoker(implementationType);
                result.CSharpPartialMethodDefinitions = new GeneratedCSharpPartialMethodDefinitions(CSharpType.FromSymbol(this.Options.Compilation, item.type));
                
                MethodsImplementedInCpp.Generate(this.Options, item, result);
                Console.WriteLine(result.CSharpPartialMethodDefinitions.ToSourceFileString());
            }

            return result;
        }

        public void WriteCppCode(GeneratedResult? result)
        {
            if (result == null)
                return;

            CppType type = result.CppDefinition.Type;
            string headerPath = Path.Combine(new string[] { Options.OutputHeaderDirectory }.Concat(type.Namespaces).ToArray());
            Directory.CreateDirectory(headerPath);
            File.WriteAllText(Path.Combine(headerPath, type.Name + ".h"), result.CppDeclaration.ToHeaderFileString(), Encoding.UTF8);

            Directory.CreateDirectory(Options.OutputSourceDirectory);
            File.WriteAllText(Path.Combine(Options.OutputSourceDirectory, type.Name + ".cpp"), result.CppDefinition.ToSourceFileString(), Encoding.UTF8);
        }


        private void GenerateType(GenerationItem mainItem, GenerationItem currentItem, CppType mainType, TypeDefinition definition)
        {
            mainType.AddSourceIncludesToSet(definition.cppIncludes);

            if (mainItem == currentItem)
            {
                if (!mainItem.type.IsStatic)
                {
                    if (mainType.Kind == CppTypeKind.ClassWrapper)
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
                        definition.declarations.Add($"explicit {mainType.Name}({objectHandleType.GetFullyQualifiedName()}&& handle) noexcept;");
                        definition.definitions.Add(
                            $$"""
                            {{mainType.Name}}::{{mainType.Name}}({{objectHandleType.GetFullyQualifiedName()}}&& handle) noexcept :
                              _handle(std::move(handle)) {}
                            """);

                        // Add constructor for a null reference
                        definition.declarations.Add($"{mainType.Name}(std::nullptr_t) noexcept;");
                        definition.definitions.Add(
                            $$"""
                            {{mainType.Name}}::{{mainType.Name}}(std::nullptr_t) noexcept : _handle(nullptr) {
                            }
                            """);

                        // Add comparison to a null reference
                        definition.declarations.Add($"bool operator==(std::nullptr_t) const noexcept;");
                        definition.declarations.Add($"bool operator!=(std::nullptr_t) const noexcept;");
                        definition.definitions.Add(
                            $$"""
                            bool {{mainType.Name}}::operator==(std::nullptr_t) const noexcept {
                              return this->_handle.GetRaw() == nullptr;
                            }
                            """);
                        definition.definitions.Add(
                            $$"""
                            bool {{mainType.Name}}::operator!=(std::nullptr_t) const noexcept {
                              return this->_handle.GetRaw() != nullptr;
                            }
                            """);

                        definition.declarations.Add($"const {objectHandleType.GetFullyQualifiedName()}& GetHandle() const;");
                        definition.definitions.Add(
                            $$"""
                            const {{objectHandleType.GetFullyQualifiedName()}}& {{mainType.Name}}::GetHandle() const {
                                return this->_handle;
                            }
                            """);
                    }
                    else if (mainType.Kind == CppTypeKind.BlittableStruct)
                    {
                        CppFields.GenerateFields(this.Options, mainItem, definition);
                    }

                    // TODO: we're currently not generating this for value types, which isn't quite right.
                    if (mainType.Kind == CppTypeKind.ClassWrapper)
                    {
                        // Generate constructors
                        CppConstructors.GenerateConstructors(this.Options, mainItem, definition);

                        // Also allow up- and down-casting this instance to related types.
                        CppCasts.GenerateDowncasts(this.Options, mainItem, definition);
                        CppCasts.GenerateUpcasts(this.Options, mainItem, definition);
                    }
                }
                else
                {
                    // Instances of wrappers for static classes can never be constructed.
                    definition.declarations.Add($"{mainType.Name}() = delete;");
                }

                definition.headerIncludes.Add("<cstdint>");
                definition.privateDeclarations.Add("friend void ::initializeOxidize(void** functionPointers, std::int32_t count);");
            }

            CppProperties.GenerateProperties(this.Options, mainItem, currentItem, definition);
            CppMethods.GenerateMethods(this.Options, mainItem, currentItem, definition);
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

            if (definition.methodsImplementedInCpp.Count > 0)
            {
                string impl =
                    $$"""
                    foo
                    """;
            }
        }
    }
}
