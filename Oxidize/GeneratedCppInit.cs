using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Oxidize
{
    internal class GeneratedCppFieldInit
    {
        public GeneratedCppFieldInit(string name, string typeSignature, IEnumerable<CppTypeReference> typesReferenced)
        {
            this.Name = name;
            this.TypeSignature = typeSignature;
            this.TypesReferenced = new List<CppTypeReference>(typesReferenced);
        }

        public string Name;
        public string TypeSignature;
        public List<CppTypeReference> TypesReferenced;
    }

    internal class GeneratedCppInit
    {
        public List<GeneratedCppFieldInit> Fields = new List<GeneratedCppFieldInit>();

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

        public IEnumerable<string> GetIncludes()
        {
            HashSet<string> result = new HashSet<string>();

            // These are required for the init boilerplate
            result.Add("<casset>");
            result.Add("<cstdint>");

            foreach (GeneratedCppFieldInit field in Fields)
            {
                foreach (CppTypeReference reference in field.TypesReferenced)
                {
                    if (reference.Type != null && reference.RequiresCompleteDefinition)
                        reference.Type.AddSourceIncludesToSet(result);
                }
            }

            return result.Select(include => $"#include {include}");
        }

        private IEnumerable<string> GetForwardDeclarations()
        {
            HashSet<string> result = new HashSet<string>();

            foreach (GeneratedCppFieldInit field in Fields)
            {
                foreach (CppTypeReference reference in field.TypesReferenced)
                {
                    if (reference.Type != null && !reference.RequiresCompleteDefinition)
                        reference.Type.AddForwardDeclarationsToSet(result);
                }
            }

            return result;
        }

        public IEnumerable<string> GetFieldAssignments()
        {
            return Fields.Select(field => $"{field.Name} = reinterpret_cast<{field.TypeSignature}>(functionPointers[i++]);");
        }
    }
}
