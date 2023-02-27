using System.Text;

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
            
            ulong validationHash = ComputeValidationHash();

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
                std::uint8_t initializeReinterop(std::uint64_t validationHashValue, void** functionPointers, std::int32_t count);
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
                std::uint8_t initializeReinterop(std::uint64_t validationHashValue, void** functionPointers, std::int32_t count) {
                  // Make sure the C++ and C# layers are in sync.
                  if (count != {{Functions.Count}})
                    return 0;
                  if (validationHashValue != {{validationHash}}ULL)
                    return 0;
                
                  {{GetFieldAssignments().JoinAndIndent("  ")}}

                  // Invoke user startup code.
                  start();

                  return 1;
                }

                }
                """);
        }

        public string ToCSharpSourceFileString(CppGenerationContext cppContext)
        {
            Sort();

            ulong validationHash = ComputeValidationHash();

            return
                $$"""
                using System;
                using System.Runtime.InteropServices;

                namespace Reinterop
                {
                    internal class ReinteropInitializer
                    {
                        public static void Initialize()
                        {
                            // This does nothing, but ensures the static constructor is
                            // called exactly once.
                        }

                        // This function must be separate and occur before the static constructor.
                        // See https://github.com/CesiumGS/cesium-unity/issues/227
                        private static void AddFunctionPointers(IntPtr memory)
                        {
                            unsafe
                            {
                                {{GetFunctionPointerInitLines().JoinAndIndent("                ")}}
                            }
                        }

                        static ReinteropInitializer()
                        {
                            unsafe
                            {
                                IntPtr memory = Marshal.AllocHGlobal(sizeof(IntPtr) * {{Functions.Count}});
                                AddFunctionPointers(memory);
                                byte success = initializeReinterop({{validationHash}}UL, memory, {{Functions.Count}});
                                if (success == 0)
                                    throw new NotImplementedException("The native library is out of sync with the managed one.");
                            }
                        }

                        [DllImport("{{cppContext.NativeLibraryName}}", CallingConvention=CallingConvention.Cdecl)]
                        private static extern byte initializeReinterop(ulong validationHash, IntPtr functionPointers, int count);

                        // Roslyn raises CS0252 spuriously for MulticastDelegate operator==, so disable the warning
                        // See https://github.com/dotnet/roslyn/issues/17212
                        //     https://github.com/dotnet/roslyn/issues/58996
                        #pragma warning disable 0252

                        {{GetContent().JoinAndIndent("        ")}}

                        #pragma warning restore 0252
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
            return Functions.Select((f, i) => $"{f.CppName} = reinterpret_cast<{f.CppTypeSignature}>(functionPointers[{i}]);");
        }

        private IEnumerable<string> GetFunctionPointerInitLines()
        {
            return Functions.Select((d, i) => $"Marshal.WriteIntPtr(memory, {i} * sizeof(IntPtr), Marshal.GetFunctionPointerForDelegate({d.CSharpName}));");
        }

        private IEnumerable<string> GetContent()
        {
            return Functions.Select(item => item.CSharpContent);
        }

        private void Sort()
        {
            Functions.Sort((a, b) => string.Compare(a.CppName, b.CppName));
        }

        /// <summary>
        /// Compute a value representing the state of the interop, so that C# can pass it and C++ can verify it
        /// in order to ensure that the two are in sync.
        /// </summary>
        /// <returns></returns>
        private ulong ComputeValidationHash()
        {
            StringBuilder s = new StringBuilder();
            foreach (GeneratedInitFunction func in Functions)
            {
                s.Append(func.CppName);
                s.Append(',');
                s.AppendLine(func.CppTypeSignature);
            }

            return Interop.InsecureHash64bits(s.ToString());
        }
    }
}
