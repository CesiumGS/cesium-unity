using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Oxidize
{
    internal record GeneratedCppFieldInit(
        string Name,
        string TypeSignature,
        IEnumerable<CppType>? TypeDeclarationsReferenced = null,
        IEnumerable<CppType>? TypeDefinitionsReferenced = null,
        IEnumerable<string>? AdditionalIncludes = null)
        : GeneratedCppElement(TypeDeclarationsReferenced, TypeDefinitionsReferenced, AdditionalIncludes)
    {
    }

    internal class GeneratedCppInit
    {
        public List<GeneratedCppFieldInit> Fields = new List<GeneratedCppFieldInit>();

        public static GeneratedCppInit Merge(IEnumerable<GeneratedCppInit> inits)
        {
            List<GeneratedCppFieldInit> fields = new List<GeneratedCppFieldInit>();
            foreach (GeneratedCppInit init in inits)
            {
                fields.AddRange(init.Fields);
            }

            return new GeneratedCppInit() { Fields = fields };
        }

        public string ToSourceFileString()
        {
            return $$"""
                {{GetIncludes().JoinAndIndent("")}}

                {{GetForwardDeclarations().JoinAndIndent("")}}

                void start();
                void stop();

                extern "C" {

                __declspec(dllexport) void initializeOxidize(void** functionPointers, std::int32_t count) {
                  // If this assertion fails, the C# and C++ layers are out of sync.
                  assert(count == {{Fields.Count}});
                
                  std::int32_t i = 0;
                  {{GetFieldAssignments().JoinAndIndent("  ")}}

                  // Invoke user startup code.
                  start();
                }

                }
                """;
        }

        public string ToHeaderFileString()
        {
            return
                $$"""
                #pragma once

                #include <cstdint>

                extern "C" {
                __declspec(dllexport) void initializeOxidize(void** functionPointers, std::int32_t count);
                }
                """;
        }

        public IEnumerable<string> GetIncludes()
        {
            HashSet<string> result = new HashSet<string>();

            // These are required for the init boilerplate
            result.Add("<cassert>");
            result.Add("<cstdint>");

            foreach (GeneratedCppFieldInit field in Fields)
            {
                field.AddIncludesToSet(result);
            }

            return result.Select(include => $"#include {include}");
        }

        private IEnumerable<string> GetForwardDeclarations()
        {
            HashSet<string> result = new HashSet<string>();

            foreach (GeneratedCppFieldInit field in Fields)
            {
                field.AddForwardDeclarationsToSet(result);
            }

            return result;
        }

        public IEnumerable<string> GetFieldAssignments()
        {
            return Fields.Select(field => $"{field.Name} = reinterpret_cast<{field.TypeSignature}>(functionPointers[i++]);");
        }
    }
}
