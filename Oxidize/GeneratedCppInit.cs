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

        public void Generate(CppGenerationContext options, Dictionary<string, CppSourceFile> sourceFiles)
        {
            string headerPath = options.OutputHeaderDirectory;
            if (options.BaseNamespace != null)
                headerPath = Path.Combine(headerPath, options.BaseNamespace);
            headerPath = Path.Combine(headerPath, "initializeOxidize.h");

            CppSourceFile? initializeHeader = null;
            if (!sourceFiles.TryGetValue(headerPath, out initializeHeader))
            {
                initializeHeader = new CppSourceFile();
                initializeHeader.IsHeaderFile = true;
                initializeHeader.Filename = headerPath;
                sourceFiles.Add(headerPath, initializeHeader);
            }

            initializeHeader.Includes.Add("<cstdint>");
            var headerNamespace = initializeHeader.GetNamespace("");
            headerNamespace.Members.Add(
                $$"""
                extern "C" {
                __declspec(dllexport) void initializeOxidize(void** functionPointers, std::int32_t count);
                }
                """);

            string sourcePath = Path.Combine(options.OutputSourceDirectory, "initializeOxidize.cpp");

            CppSourceFile? initializeSource = null;
            if (!sourceFiles.TryGetValue(sourcePath, out initializeSource))
            {
                initializeSource = new CppSourceFile();
                initializeSource.IsHeaderFile = false;
                initializeSource.Filename = sourcePath;
                sourceFiles.Add(sourcePath, initializeSource);
            }

            AddIncludes(initializeSource.Includes);
            AddForwardDeclarations(initializeSource.ForwardDeclarations);
            var sourceNamespace = initializeSource.GetNamespace("");

            sourceNamespace.Members.Add("void start();");
            sourceNamespace.Members.Add("void stop();");
            sourceNamespace.Members.Add(
                $$"""
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
                """);
        }

        public void AddIncludes(ISet<string> includes)
        {
            // These are required for the init boilerplate
            includes.Add("<cassert>");
            includes.Add("<cstdint>");

            foreach (GeneratedCppFieldInit field in Fields)
            {
                field.AddIncludesToSet(includes);
            }
        }

        private void AddForwardDeclarations(ISet<string> forwardDeclarations)
        {
            foreach (GeneratedCppFieldInit field in Fields)
            {
                field.AddForwardDeclarationsToSet(forwardDeclarations);
            }
        }

        public IEnumerable<string> GetFieldAssignments()
        {
            return Fields.Select(field => $"{field.Name} = reinterpret_cast<{field.TypeSignature}>(functionPointers[i++]);");
        }
    }
}
