using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Reinterop.Tests
{
    [TestFixture]
    public class CSharpFunctionCallableFromCppTests
    {
        private static CppGenerationContext CreateContext() => new CppGenerationContext(CSharpCompilation.Create("Test", references: GenerationTestHelper.References));

        [Test]
        public void BoolsInteropAsBytes()
        {
            CppGenerationContext context = CreateContext();
            CSharpType owner = new CSharpType(context, InteropTypeKind.ClassWrapper, ["TestNamespace"], "TestClass", SpecialType.None);
            CSharpFunctionCallableFromCpp interop = new CSharpFunctionCallableFromCpp(context, owner)
                .Name("TestMethod")
                .ReturnType(CSharpType.FromSymbol(context, context.Compilation.GetSpecialType(SpecialType.System_Boolean)))
                .Parameters([new CSharpParameter(CSharpType.FromSymbol(context, context.Compilation.GetSpecialType(SpecialType.System_Boolean)), "myBool")])
                .Body(new CSharpBodyInvokeMethod());
            GeneratedResult result = new GeneratedResult(CppType.FromCSharp(context, owner));
            interop.GenerateCode(context, result);

            GeneratedInitFunction? initFunction = result.Init.Functions.FirstOrDefault();
            Assert.That(initFunction, Is.Not.Null);
            if (initFunction == null)
                return;

            Assert.That(initFunction.CSharpContent, Is.EqualTo(
            $$"""
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private unsafe delegate System.Byte Reinterop_TestNamespace_TestClass_TestMethod_lfcalBi63SvXpt4W5a7zaw_Type(System.IntPtr thiz, System.Byte myBool, System.IntPtr* reinteropException);
            private static unsafe readonly Reinterop_TestNamespace_TestClass_TestMethod_lfcalBi63SvXpt4W5a7zaw_Type Reinterop_TestNamespace_TestClass_TestMethod_lfcalBi63SvXpt4W5a7zaw_Delegate = new Reinterop_TestNamespace_TestClass_TestMethod_lfcalBi63SvXpt4W5a7zaw_Type(Reinterop_TestNamespace_TestClass_TestMethod_lfcalBi63SvXpt4W5a7zaw);
            [AOT.MonoPInvokeCallback(typeof(Reinterop_TestNamespace_TestClass_TestMethod_lfcalBi63SvXpt4W5a7zaw_Type))]
            private static unsafe System.Byte Reinterop_TestNamespace_TestClass_TestMethod_lfcalBi63SvXpt4W5a7zaw(System.IntPtr thiz, System.Byte myBool, System.IntPtr* reinteropException)
            {
                try
                {
                    return (System.Byte)(((TestNamespace.TestClass)Reinterop.ObjectHandleUtility.GetObjectFromHandle(thiz)).TestMethod(myBool != 0) ? 1 : 0);
                }
                catch (System.Exception reinteropManagedException)
                {
                    (*reinteropException) = Reinterop.ObjectHandleUtility.CreateHandle(reinteropManagedException);
                    return new System.Byte();
                }
            }
            """));
            Assert.That(initFunction.CppName, Is.EqualTo("::DotNet::TestNamespace::TestClass::Reinterop_TestNamespace_TestClass_TestMethod_lfcalBi63SvXpt4W5a7zaw"));
            Assert.That(initFunction.CppTypeSignature, Is.EqualTo("::std::uint8_t (*)(void*, ::std::uint8_t, void**)"));
            Assert.That(initFunction.CppTypeDeclarationsReferenced, Contains.Item(CppType.UInt8));
            Assert.That(initFunction.CppTypeDefinitionsReferenced, Has.Some.Matches<CppType>(t => t.GetFullyQualifiedName() == "::DotNet::TestNamespace::TestClass"));
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
                .Body(new CSharpBodyInvokeMethod());
            GeneratedResult result = new GeneratedResult(CppType.FromCSharp(context, owner));
            interop.GenerateCode(context, result);

            GeneratedInitFunction? initFunction = result.Init.Functions.FirstOrDefault();
            Assert.That(initFunction, Is.Not.Null);
            if (initFunction == null)
                return;

            Assert.That(initFunction.CSharpContent, Is.EqualTo(
            $$"""
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private unsafe delegate void Reinterop_TestNamespace_TestClass_TestMethod_vRryr9W8bE8j46kr7PWzkw_Type(System.IntPtr thiz, TestNamespace.MyStruct* myStruct, TestNamespace.MyStruct* pReturnValue, System.IntPtr* reinteropException);
            private static unsafe readonly Reinterop_TestNamespace_TestClass_TestMethod_vRryr9W8bE8j46kr7PWzkw_Type Reinterop_TestNamespace_TestClass_TestMethod_vRryr9W8bE8j46kr7PWzkw_Delegate = new Reinterop_TestNamespace_TestClass_TestMethod_vRryr9W8bE8j46kr7PWzkw_Type(Reinterop_TestNamespace_TestClass_TestMethod_vRryr9W8bE8j46kr7PWzkw);
            [AOT.MonoPInvokeCallback(typeof(Reinterop_TestNamespace_TestClass_TestMethod_vRryr9W8bE8j46kr7PWzkw_Type))]
            private static unsafe void Reinterop_TestNamespace_TestClass_TestMethod_vRryr9W8bE8j46kr7PWzkw(System.IntPtr thiz, TestNamespace.MyStruct* myStruct, TestNamespace.MyStruct* pReturnValue, System.IntPtr* reinteropException)
            {
                try
                {
                    TestNamespace.MyStruct reinterop_returnValue = ((TestNamespace.TestClass)Reinterop.ObjectHandleUtility.GetObjectFromHandle(thiz)).TestMethod(*myStruct);
                    (*pReturnValue) = reinterop_returnValue;
                }
                catch (System.Exception reinteropManagedException)
                {
                    (*reinteropException) = Reinterop.ObjectHandleUtility.CreateHandle(reinteropManagedException);
                    return;
                }
            }
            """));
            Assert.That(initFunction.CppName, Is.EqualTo("::DotNet::TestNamespace::TestClass::Reinterop_TestNamespace_TestClass_TestMethod_vRryr9W8bE8j46kr7PWzkw"));
            Assert.That(initFunction.CppTypeSignature, Is.EqualTo("void (*)(void*, const ::DotNet::TestNamespace::MyStruct*, ::DotNet::TestNamespace::MyStruct*, void**)"));
            Assert.That(initFunction.CppTypeDeclarationsReferenced, Has.Some.Matches<CppType>(t => t.GetFullyQualifiedName() == "::DotNet::TestNamespace::MyStruct*"));
            Assert.That(initFunction.CppTypeDefinitionsReferenced, Has.Some.Matches<CppType>(t => t.GetFullyQualifiedName() == "::DotNet::TestNamespace::TestClass"));
        }

        [Test]
        public void GenericMethodInvocationIncludesTypeArguments()
        {
            CppGenerationContext context = CreateContext();
            CSharpType owner = new CSharpType(context, InteropTypeKind.ClassWrapper, ["TestNamespace"], "TestClass", SpecialType.None);
            CSharpType intType = CSharpType.FromSymbol(context, context.Compilation.GetSpecialType(SpecialType.System_Int32));
            CSharpFunctionCallableFromCpp interop = new CSharpFunctionCallableFromCpp(context, owner)
                .Name("TestMethod")
                .TypeArguments([intType])
                .ReturnType(CSharpType.FromSymbol(context, context.Compilation.GetSpecialType(SpecialType.System_Void)))
                .Body(new CSharpBodyInvokeMethod());
            GeneratedResult result = new GeneratedResult(CppType.FromCSharp(context, owner));
            interop.GenerateCode(context, result);

            GeneratedInitFunction? initFunction = result.Init.Functions.FirstOrDefault();
            Assert.That(initFunction, Is.Not.Null);
            if (initFunction == null)
                return;

            Assert.That(initFunction.CSharpContent, Does.Contain(".TestMethod<System.Int32>()"));
        }

        [Test]
        public void CloneCopiesConfiguration()
        {
            CppGenerationContext context = CreateContext();
            CSharpType owner = new CSharpType(context, InteropTypeKind.ClassWrapper, ["TestNamespace"], "TestClass", SpecialType.None);
            CSharpType intType = CSharpType.FromSymbol(context, context.Compilation.GetSpecialType(SpecialType.System_Int32));
            CSharpBodyInvokeMethod body = new CSharpBodyInvokeMethod();
            CSharpFunctionCallableFromCpp original = new CSharpFunctionCallableFromCpp(context, owner)
                .Name("TestMethod")
                .ReturnType(intType)
                .Parameters([new CSharpParameter(intType, "value")])
                .TypeArguments([intType])
                .Private(true)
                .Static(true)
                .Body(body);

            CSharpFunctionCallableFromCpp clone = original.Clone();

            Assert.That(clone, Is.Not.SameAs(original));
            Assert.That(clone.Context(), Is.SameAs(original.Context()));
            Assert.That(clone.Owner(), Is.EqualTo(original.Owner()));
            Assert.That(clone.Name(), Is.EqualTo(original.Name()));
            Assert.That(clone.ReturnType(), Is.EqualTo(original.ReturnType()));
            Assert.That(clone.Parameters(), Is.EqualTo(original.Parameters()));
            Assert.That(clone.TypeArguments(), Is.EqualTo(original.TypeArguments()));
            Assert.That(clone.Private(), Is.EqualTo(original.Private()));
            Assert.That(clone.Static(), Is.EqualTo(original.Static()));
            Assert.That(clone.Body(), Is.SameAs(original.Body()));
        }
    }
}
