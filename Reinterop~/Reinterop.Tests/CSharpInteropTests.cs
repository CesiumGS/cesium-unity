using NUnit.Framework;

namespace Reinterop.Tests
{
    /// <summary>
    /// Characterization tests proving that CSharpInterop's structured recipes render exactly the same
    /// text that the hand-written string interpolation in MethodsImplementedInCpp.cs/
    /// CustomDelegateGenerator.cs used to produce for the "call into native code, check for an
    /// exception, optionally return a converted result" pattern.
    /// </summary>
    public class CSharpInteropTests
    {
        [Test]
        public void CallNativeFunction_VoidReturn_MatchesOriginalTemplate()
        {
            IReadOnlyList<CSharpStatement> body = CSharpInterop.CallNativeFunction(
                new CSharpIdentifier("CallFoo_1234"),
                new CSharpExpression[] { new CSharpRaw("a"), new CSharpRaw("b") });

            string expected = string.Join(Environment.NewLine, new[]
            {
                "System.IntPtr reinteropException = System.IntPtr.Zero;",
                "CallFoo_1234(a, b, &reinteropException);",
                "if (reinteropException != System.IntPtr.Zero)",
                "    throw (System.Exception)Reinterop.ObjectHandleUtility.GetObjectAndFreeHandle(reinteropException);"
            });

            Assert.That(CSharpPrinter.Print(body), Is.EqualTo(expected));
        }

        [Test]
        public void CallNativeFunction_ValueReturn_MatchesOriginalTemplate()
        {
            IReadOnlyList<CSharpStatement> body = CSharpInterop.CallNativeFunction(
                new CSharpIdentifier("CallFoo_1234"),
                new CSharpExpression[] { new CSharpRaw("a"), new CSharpRaw("b") },
                resultTypeName: "var",
                returnExpression: new CSharpRaw("result != 0"));

            string expected = string.Join(Environment.NewLine, new[]
            {
                "System.IntPtr reinteropException = System.IntPtr.Zero;",
                "var result = CallFoo_1234(a, b, &reinteropException);",
                "if (reinteropException != System.IntPtr.Zero)",
                "    throw (System.Exception)Reinterop.ObjectHandleUtility.GetObjectAndFreeHandle(reinteropException);",
                "return result != 0;"
            });

            Assert.That(CSharpPrinter.Print(body), Is.EqualTo(expected));
        }
    }
}
