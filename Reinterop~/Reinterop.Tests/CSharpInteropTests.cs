using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
        private static CSharpCompilation compilation = GenerationTestHelper.CreateCompilation("");
        private static ReinteropGenerationContext context = new ReinteropGenerationContext(compilation);

        [Test]
        public void CallNativeFunction_VoidReturn_MatchesOriginalTemplate()
        {
            IReadOnlyList<CSharpStatement> body = CSharpInterop.CallNativeFunction(
                context,
                new CSharpIdentifier("CallFoo_1234"),
                new CSharpExpression[] { new CSharpIdentifier("a"), new CSharpIdentifier("b") });

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
                context,
                new CSharpIdentifier("CallFoo_1234"),
                new CSharpExpression[] { new CSharpIdentifier("a"), new CSharpIdentifier("b") },
                resultType: CSharpType.FromSymbol(context, compilation.GetSpecialType(SpecialType.System_Int32)),
                returnExpression: new CSharpBinary("!=", new CSharpIdentifier("result"), new CSharpLiteral("0")));

            string expected = string.Join(Environment.NewLine, new[]
            {
                "System.IntPtr reinteropException = System.IntPtr.Zero;",
                "System.Int32 result = CallFoo_1234(a, b, &reinteropException);",
                "if (reinteropException != System.IntPtr.Zero)",
                "    throw (System.Exception)Reinterop.ObjectHandleUtility.GetObjectAndFreeHandle(reinteropException);",
                "return result != 0;"
            });

            Assert.That(CSharpPrinter.Print(body), Is.EqualTo(expected));
        }
    }
}
