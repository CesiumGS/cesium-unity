namespace Reinterop
{
    internal record GeneratedInitFunction(
        string CppName,
        string CppTypeSignature,
        string CSharpName,
        string CSharpContent,
        IEnumerable<CppType>? CppTypeDeclarationsReferenced = null,
        IEnumerable<CppType>? CppTypeDefinitionsReferenced = null,
        IEnumerable<string>? CppAdditionalIncludes = null) : GeneratedCppElement(CppTypeDeclarationsReferenced, CppTypeDefinitionsReferenced, CppAdditionalIncludes)
    {
    }

    internal class GeneratedInit
    {
        public List<GeneratedInitFunction> Functions = new List<GeneratedInitFunction>();

        public static GeneratedInit Merge(IEnumerable<GeneratedInit> inits)
        {
            List<GeneratedInitFunction> functions = new List<GeneratedInitFunction>();
            foreach (GeneratedInit init in inits)
            {
                functions.AddRange(init.Functions);
            }

            return new GeneratedInit() { Functions = functions };
        }

        public void GenerateCpp(CppGenerationContext options, Dictionary<string, CppSourceFile> sourceFiles)
        {
            Sort();

            string headerPath = "include";
            if (options.BaseNamespace != null)
                headerPath = Path.Combine(headerPath, options.BaseNamespace);
            headerPath = Path.Combine(headerPath, "initializeReinterop.h");

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
                #if defined(_WIN32)
                __declspec(dllexport)
                #endif
                void initializeReinterop(void** functionPointers, std::int32_t count);
                }
                """);

            string sourcePath = Path.Combine("src", "initializeReinterop.cpp");

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
                #if defined(_WIN32)
                __declspec(dllexport)
                #endif
                void initializeReinterop(void** functionPointers, std::int32_t count) {
                  // If this assertion fails, the C# and C++ layers are out of sync.
                  assert(count == {{Functions.Count}});
                
                  std::int32_t i = 0;
                  {{GetFieldAssignments().JoinAndIndent("  ")}}

                  // Invoke user startup code.
                  start();
                }

                }
                """);
        }

        public string ToCSharpSourceFileString(CppGenerationContext cppContext)
        {
            Sort();

            return
                $$"""
                using System;
                using System.Runtime.InteropServices;

                namespace Reinterop
                {
                    public class ReinteropInitializer
                    {
                        public static void Initialize()
                        {
                            // This does nothing, but ensures the static constructor is
                            // called exactly once.
                        }

                        static ReinteropInitializer()
                        {
                            unsafe
                            {
                                IntPtr memory = Marshal.AllocHGlobal(sizeof(IntPtr) * {{Functions.Count}});
                                int i = 0;
                                {{GetFunctionPointerInitLines().JoinAndIndent("                ")}}
                                initializeReinterop(memory, {{Functions.Count}});
                            }
                        }

                        [DllImport("{{cppContext.NativeLibraryName}}", CallingConvention=CallingConvention.Cdecl)]
                        private static extern void initializeReinterop(IntPtr functionPointers, int count);

                        {{GetContent().JoinAndIndent("        ")}}
                    }
                }
                """;
        }

        private void AddIncludes(ISet<string> includes)
        {
            // These are required for the init boilerplate
            includes.Add("<cassert>");
            includes.Add("<cstdint>");

            foreach (GeneratedInitFunction f in Functions)
            {
                f.AddIncludesToSet(includes);
            }
        }

        private void AddForwardDeclarations(ISet<string> forwardDeclarations)
        {
            foreach (GeneratedInitFunction f in Functions)
            {
                f.AddForwardDeclarationsToSet(forwardDeclarations);
            }
        }

        public IEnumerable<string> GetFieldAssignments()
        {
            return Functions.Select(f => $"{f.CppName} = reinterpret_cast<{f.CppTypeSignature}>(functionPointers[i++]);");
        }

        private IEnumerable<string> GetFunctionPointerInitLines()
        {
            return Functions.Select(d => $"Marshal.WriteIntPtr(memory, (i++) * sizeof(IntPtr), Marshal.GetFunctionPointerForDelegate({d.CSharpName}));");
        }

        private IEnumerable<string> GetContent()
        {
            return Functions.Select(item => item.CSharpContent);
        }

        private void Sort()
        {
            Functions.Sort((a, b) => string.Compare(a.CppName, b.CppName));
        }
    }
}
