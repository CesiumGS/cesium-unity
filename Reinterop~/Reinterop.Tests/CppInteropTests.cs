using NUnit.Framework;

namespace Reinterop.Tests
{
    /// <summary>
    /// Characterization tests proving that CppInterop's structured recipes render exactly the same
    /// text that the hand-written string interpolation in Methods.cs/Properties.cs/etc. used to
    /// produce for the "call a managed function, check for an exception, optionally return a
    /// converted result" pattern.
    /// </summary>
    public class CppInteropTests
    {
        [Test]
        public void CallManagedFunction_VoidReturn_MatchesOriginalTemplate()
        {
            IReadOnlyList<CppStatement> body = CppInterop.CallManagedFunction(
                new CppIdentifier("CallFoo_1234"),
                new[] { CppArgument.Value("a"), CppArgument.Value("b") });

            string expected = string.Join(Environment.NewLine, new[]
            {
                "void* reinteropException = nullptr;",
                "CallFoo_1234(a, b, &reinteropException);",
                "if (reinteropException != nullptr)",
                "    throw Reinterop::ReinteropNativeException(::DotNet::System::Exception(::DotNet::Reinterop::ObjectHandle(reinteropException)));"
            });

            Assert.That(CppPrinter.Print(body), Is.EqualTo(expected));
        }

        [Test]
        public void CallManagedFunction_ValueReturn_MatchesOriginalTemplate()
        {
            IReadOnlyList<CppStatement> body = CppInterop.CallManagedFunction(
                new CppIdentifier("CallFoo_1234"),
                new[] { CppArgument.Value("a"), CppArgument.Value("b") },
                resultTypeName: "auto",
                returnExpression: new CppRaw("::DotNet::Foo(result)"));

            string expected = string.Join(Environment.NewLine, new[]
            {
                "void* reinteropException = nullptr;",
                "auto result = CallFoo_1234(a, b, &reinteropException);",
                "if (reinteropException != nullptr)",
                "    throw Reinterop::ReinteropNativeException(::DotNet::System::Exception(::DotNet::Reinterop::ObjectHandle(reinteropException)));",
                "return ::DotNet::Foo(result);"
            });

            Assert.That(CppPrinter.Print(body), Is.EqualTo(expected));
        }

        [Test]
        public void CallManagedFunction_OutParameter_MatchesOriginalTemplate()
        {
            // Mirrors the struct-return-rewrite shape used by Properties.cs/Constructors.cs/Fields.cs:
            // the result is produced via an out-parameter, so it's declared (with no initializer)
            // before the call, and its address is passed as the call's own argument.
            IReadOnlyList<CppStatement> body = CppInterop.CallManagedFunction(
                new CppIdentifier("Construct_1234"),
                new[] { CppArgument.Value("a"), CppArgument.Value("b"), CppArgument.OutParameter("MyStruct", "result") });

            string expected = string.Join(Environment.NewLine, new[]
            {
                "void* reinteropException = nullptr;",
                "MyStruct result;",
                "Construct_1234(a, b, &result, &reinteropException);",
                "if (reinteropException != nullptr)",
                "    throw Reinterop::ReinteropNativeException(::DotNet::System::Exception(::DotNet::Reinterop::ObjectHandle(reinteropException)));"
            });

            Assert.That(CppPrinter.Print(body), Is.EqualTo(expected));
        }

        [Test]
        public void CallManagedFunction_CustomResultVariableName_MatchesOriginalTemplate()
        {
            // Mirrors the non-blittable-struct constructor shape in Constructors.cs, which captures the
            // call's result into a variable named "handle" rather than "result".
            IReadOnlyList<CppStatement> body = CppInterop.CallManagedFunction(
                new CppIdentifier("Construct_1234"),
                new[] { CppArgument.Value("a"), CppArgument.Value("b") },
                resultTypeName: "void*",
                returnExpression: new CppRaw("handle"),
                resultVariableName: "handle");

            string expected = string.Join(Environment.NewLine, new[]
            {
                "void* reinteropException = nullptr;",
                "void* handle = Construct_1234(a, b, &reinteropException);",
                "if (reinteropException != nullptr)",
                "    throw Reinterop::ReinteropNativeException(::DotNet::System::Exception(::DotNet::Reinterop::ObjectHandle(reinteropException)));",
                "return handle;"
            });

            Assert.That(CppPrinter.Print(body), Is.EqualTo(expected));
        }
    }
}
