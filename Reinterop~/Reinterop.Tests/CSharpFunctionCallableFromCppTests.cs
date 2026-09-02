using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Reinterop.Tests
{
    [TestFixture]
    public class CSharpFunctionCallableFromCppTests
    {
        private static CppGenerationContext CreateContext() => new CppGenerationContext(CSharpCompilation.Create("Test"));

        [Test]
        public void BoolsInteropAsBytes()
        {
            CppGenerationContext context = CreateContext();
            CSharpType owner = new CSharpType(context, InteropTypeKind.ClassWrapper, ["TestNamespace"], "TestClass", SpecialType.None);
            CSharpFunctionCallableFromCpp interop = new CSharpFunctionCallableFromCpp(context, owner)
                .Name("TestMethod")
                .ReturnType(CSharpType.FromSymbol(context, context.Compilation.GetSpecialType(SpecialType.System_Boolean)))
                .Parameters([new CSharpParameter(CSharpType.FromSymbol(context, context.Compilation.GetSpecialType(SpecialType.System_Boolean)), "myBool")])
                .Body(new CSharpFunctionCallableFromCpp.CSharpBodyInvokeMethod());
            GeneratedResult result = new GeneratedResult(CppType.FromCSharp(context, owner));
            interop.GenerateCode(context, result);

            GeneratedInitFunction? initFunction = result.Init.Functions.FirstOrDefault();
            Assert.That(initFunction, Is.Not.Null);
            if (initFunction == null)
                return;

            Assert.That(initFunction.CSharpContent, Is.EqualTo(
            $$"""
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private unsafe delegate System.Byte TestNamespace_TestClass_Reinterop_TestMethod_lfcalBi63SvXpt4W5a7zawType(System.IntPtr thiz, System.Byte myBool, System.IntPtr* reinteropException);
            private static unsafe readonly TestNamespace_TestClass_Reinterop_TestMethod_lfcalBi63SvXpt4W5a7zawType TestNamespace_TestClass_Reinterop_TestMethod_lfcalBi63SvXpt4W5a7zawDelegate = new TestNamespace_TestClass_Reinterop_TestMethod_lfcalBi63SvXpt4W5a7zawType(TestNamespace_TestClass_Reinterop_TestMethod_lfcalBi63SvXpt4W5a7zaw);
            [AOT.MonoPInvokeCallback(typeof(TestNamespace_TestClass_Reinterop_TestMethod_lfcalBi63SvXpt4W5a7zawType))]
            private static unsafe System.Byte TestNamespace_TestClass_Reinterop_TestMethod_lfcalBi63SvXpt4W5a7zaw(System.IntPtr thiz, System.Byte myBool, System.IntPtr* reinteropException)
            {
                try
                {
                    return (byte)((((TestNamespace.TestClass)(Reinterop.ObjectHandleUtility.GetObjectFromHandle(thiz))).TestMethod(myBool != 0)) ? (1) : (0));
                }
                catch (System.Exception reinteropManagedException)
                {
                    *reinteropException = (Reinterop.ObjectHandleUtility).CreateHandle(reinteropManagedException);
                    return new System.Byte();
                }
            }
            """));
        }

        [Test]
        public void StructReturnRewrite()
        {
            CppGenerationContext context = CreateContext();
            CSharpType owner = new CSharpType(context, InteropTypeKind.ClassWrapper, ["TestNamespace"], "TestClass", SpecialType.None);
            CSharpType blittableStruct = new CSharpType(context, InteropTypeKind.BlittableStruct, ["TestNamespace"], "MyStruct", SpecialType.None);
            CSharpFunctionCallableFromCpp interop = new CSharpFunctionCallableFromCpp(context, owner)
                .Name("TestMethod")
                .ReturnType(blittableStruct)
                .Parameters([new CSharpParameter(blittableStruct, "myStruct")])
                .Body(new CSharpFunctionCallableFromCpp.CSharpBodyInvokeMethod());
            GeneratedResult result = new GeneratedResult(CppType.FromCSharp(context, owner));
            interop.GenerateCode(context, result);

            GeneratedInitFunction? initFunction = result.Init.Functions.FirstOrDefault();
            Assert.That(initFunction, Is.Not.Null);
            if (initFunction == null)
                return;

            Assert.That(initFunction.CSharpContent, Is.EqualTo(
            $$"""
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private unsafe delegate void TestNamespace_TestClass_Reinterop_TestMethod_vRryr9W8bE8j46kr7PWzkwType(System.IntPtr thiz, TestNamespace.MyStruct* myStruct, TestNamespace.MyStruct* pReturnValue, System.IntPtr* reinteropException);
            private static unsafe readonly TestNamespace_TestClass_Reinterop_TestMethod_vRryr9W8bE8j46kr7PWzkwType TestNamespace_TestClass_Reinterop_TestMethod_vRryr9W8bE8j46kr7PWzkwDelegate = new TestNamespace_TestClass_Reinterop_TestMethod_vRryr9W8bE8j46kr7PWzkwType(TestNamespace_TestClass_Reinterop_TestMethod_vRryr9W8bE8j46kr7PWzkw);
            [AOT.MonoPInvokeCallback(typeof(TestNamespace_TestClass_Reinterop_TestMethod_vRryr9W8bE8j46kr7PWzkwType))]
            private static unsafe void TestNamespace_TestClass_Reinterop_TestMethod_vRryr9W8bE8j46kr7PWzkw(System.IntPtr thiz, TestNamespace.MyStruct* myStruct, TestNamespace.MyStruct* pReturnValue, System.IntPtr* reinteropException)
            {
                try
                {
                    TestNamespace.MyStruct returnValue_interop = ((TestNamespace.TestClass)(Reinterop.ObjectHandleUtility.GetObjectFromHandle(thiz))).TestMethod(*myStruct);
                    *pReturnValue = returnValue_interop;
                }
                catch (System.Exception reinteropManagedException)
                {
                    *reinteropException = (Reinterop.ObjectHandleUtility).CreateHandle(reinteropManagedException);
                    return;
                }
            }
            """));
        }
    }
}
