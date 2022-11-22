using Microsoft.CodeAnalysis;

namespace Reinterop
{
    /// <summary>
    /// Inserts the "ObjectHandle" class into an assembly as it is compiled.
    /// This is intended to be used from a RegisterPostInitializationOutput
    /// callback on an IIncrementalGenerator.
    /// </summary>
    internal class CSharpObjectHandleUtility
    {
        public static void Generate(GeneratorExecutionContext context)
        {
            context.AddSource("ObjectHandleUtility", Source);
        }

        public const string Source =
            """
            using System;
            using System.Runtime.InteropServices;

            namespace Reinterop
            {
                [Reinterop]
                internal static class ObjectHandleUtility
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

                    public static object GetObjectFromHandle(IntPtr handle)
                    {
                        if (handle == IntPtr.Zero)
                            return null;

                        return GCHandle.FromIntPtr(handle).Target;
                    }

                    public static object GetObjectAndFreeHandle(IntPtr handle)
                    {
                        if (handle == IntPtr.Zero)
                            return null;

                        GCHandle gcHandle = GCHandle.FromIntPtr(handle);
                        object result = gcHandle.Target;
                        gcHandle.Free();
                        return result;
                    }

                    public static void ResetHandleObject(IntPtr handle, object newValue)
                    {
                        if (handle == IntPtr.Zero)
                            throw new ArgumentException("handle must not be IntPtr.Zero");
                        GCHandle gcHandle = GCHandle.FromIntPtr(handle);
                        gcHandle.Target = newValue;
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
