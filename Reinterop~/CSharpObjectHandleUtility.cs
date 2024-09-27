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

                        try
                        {
                            GCHandle.FromIntPtr(handle).Free();
                        }
                        catch (ArgumentException e)
                        {
                            // The "GCHandle value belongs to a different domain" exception tends
                            // to happen on AppDomain reload, which is common in Unity.
                            // Catch the exception to prevent it propagating through our native
                            // code and blowing things up.
                            // See: https://github.com/CesiumGS/cesium-unity/issues/18
                            System.Console.WriteLine(e.ToString());
                        }
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
