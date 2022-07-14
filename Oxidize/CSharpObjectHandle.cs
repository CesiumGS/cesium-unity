using Microsoft.CodeAnalysis;

namespace Oxidize
{
    /// <summary>
    /// Inserts the "ObjectHandle" class into an assembly as it is compiled.
    /// This is intended to be used from a RegisterPostInitializationOutput
    /// callback on an IIncrementalGenerator.
    /// </summary>
    internal class CSharpObjectHandle
    {
        public static void Generate(IncrementalGeneratorPostInitializationContext context)
        {
            context.AddSource("ObjectHandle", source);
        }

        private const string source =
            """
            using System.Runtime.InteropServices;

            namespace Oxidize
            {
                [Oxidize]
                public static class ObjectHandle
                {
                    public static IntPtr CreateHandle(object o)
                    {
                        if (o == null)
                            return IntPtr.Zero;
                        return GCHandle.ToIntPtr(GCHandle.Alloc(o));
                    }

                    public static IntPtr CopyHandle(IntPtr handle)
                    {
                        if (handle == IntPtr.Zero)
                            return handle;

                        // Allocate a new GCHandle pointing to the same object.
                        GCHandle gcHandle = GCHandle.FromIntPtr(handle);
                        return GCHandle.ToIntPtr(GCHandle.Alloc(gcHandle.Target));
                    }

                    public static void FreeHandle(IntPtr handle)
                    {
                        if (handle == IntPtr.Zero)
                            return;

                        GCHandle.FromIntPtr(handle).Free();
                    }

                    public static void ExposeToCPP()
                    {
                        IntPtr p = CreateHandle(new object());
                        IntPtr copy = CopyHandle(p);
                        FreeHandle(p);
                        FreeHandle(copy);
                    }
                }
            }
            """;
    }
}
