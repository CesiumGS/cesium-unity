using System.Drawing;

namespace Oxidize
{
    internal class GeneratedCSharpDelegateInit
    {
        public GeneratedCSharpDelegateInit(string name, string content)
        {
            this.Name = name;
            this.Content = content;
        }

        public string Name;
        public string Content;
    }

    internal class GeneratedCSharpInit
    {
        public List<GeneratedCSharpDelegateInit> Delegates = new List<GeneratedCSharpDelegateInit>();

        public static GeneratedCSharpInit Merge(IEnumerable<GeneratedCSharpInit> inits)
        {
            List<GeneratedCSharpDelegateInit> delegates = new List<GeneratedCSharpDelegateInit>();
            foreach (GeneratedCSharpInit init in inits)
            {
                delegates.AddRange(init.Delegates);
            }

            return new GeneratedCSharpInit() { Delegates = delegates };
        }

        public string ToSourceFileString()
        {
            return
                $$"""
                using System;
                using System.Runtime.InteropServices;

                namespace Oxidize
                {
                    public class OxidizeInitializer
                    {
                        public static void InitializeOxidize()
                        {
                            unsafe
                            {
                                IntPtr memory = Marshal.AllocHGlobal(sizeof(IntPtr) * {{Delegates.Count}});
                                int i = 0;
                                {{GetFunctionPointerInitLines().JoinAndIndent("                ")}}
                                initializeOxidize(memory, {{Delegates.Count}});
                            }
                        }

                        [DllImport("CesiumForUnityNative.dll", CallingConvention=CallingConvention.Cdecl)]
                        private static extern void initializeOxidize(IntPtr functionPointers, int count);

                        {{GetContent().JoinAndIndent("        ")}}
                    }
                }
                """;
        }

        private IEnumerable<string> GetFunctionPointerInitLines()
        {
            return Delegates.Select(d => $"Marshal.WriteIntPtr(memory, (i++) * sizeof(IntPtr), Marshal.GetFunctionPointerForDelegate({d.Name}));");
        }

        private IEnumerable<string> GetContent()
        {
            return Delegates.Select(item => item.Content);
        }
    }
}