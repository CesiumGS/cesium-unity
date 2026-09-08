using NUnit.Framework;

namespace Reinterop.Tests
{
    /// <summary>Tests C# expression and statement rendering, including conservative precedence handling.</summary>
    public class CSharpSyntaxPrintingTests
    {
        private static string Lines(params string[] lines) => string.Join(Environment.NewLine, lines);

        [Test]
        public void Expressions_PrintAllNodeKinds()
        {
            Assert.That(CSharpPrinter.Print(new CSharpIdentifier("value")), Is.EqualTo("value"));
            Assert.That(CSharpPrinter.Print(new CSharpLiteral("null")), Is.EqualTo("null"));
            Assert.That(CSharpPrinter.Print(new CSharpCall(new CSharpIdentifier("f"), [new CSharpIdentifier("a"), new CSharpLiteral("1")])), Is.EqualTo("f(a, 1)"));
            Assert.That(CSharpPrinter.Print(new CSharpBinary("!=", new CSharpIdentifier("a"), new CSharpLiteral("0"))), Is.EqualTo("a != 0"));
            Assert.That(CSharpPrinter.Print(new CSharpTernary(new CSharpIdentifier("ok"), new CSharpIdentifier("a"), new CSharpIdentifier("b"))), Is.EqualTo("ok ? a : b"));
            Assert.That(CSharpPrinter.Print(new CSharpIs(new CSharpIdentifier("value"), "string", "text")), Is.EqualTo("value is string text"));
            Assert.That(CSharpPrinter.Print(new CSharpUnary("&", new CSharpIdentifier("value"))), Is.EqualTo("&value"));
            Assert.That(CSharpPrinter.Print(new CSharpMemberAccess(new CSharpIdentifier("value"), "Length")), Is.EqualTo("value.Length"));
            Assert.That(CSharpPrinter.Print(new CSharpElementAccess(new CSharpIdentifier("values"), [new CSharpIdentifier("index")])), Is.EqualTo("values[index]"));
            Assert.That(CSharpPrinter.Print(new CSharpNew("Widget", [new CSharpIdentifier("value")])), Is.EqualTo("new Widget(value)"));
            Assert.That(CSharpPrinter.Print(new CSharpArrayNew("int", [new CSharpLiteral("3")])), Is.EqualTo("new int[3]"));
            Assert.That(CSharpPrinter.Print(new CSharpCast("string", new CSharpIdentifier("value"))), Is.EqualTo("(string)value"));
        }

        [Test]
        public void Printer_AddsParenthesesForNestedOperators()
        {
            Assert.That(CSharpPrinter.Print(new CSharpCall(new CSharpUnary("*", new CSharpIdentifier("function")), [])), Is.EqualTo("(*function)()"));
            Assert.That(CSharpPrinter.Print(new CSharpBinary("+", new CSharpBinary("*", new CSharpIdentifier("a"), new CSharpIdentifier("b")), new CSharpIdentifier("c"))), Is.EqualTo("(a * b) + c"));
            Assert.That(CSharpPrinter.Print(new CSharpUnary("!", new CSharpUnary("!", new CSharpIdentifier("value")))), Is.EqualTo("!(!value)"));
            Assert.That(CSharpPrinter.Print(new CSharpMemberAccess(new CSharpBinary("+", new CSharpIdentifier("a"), new CSharpIdentifier("b")), "Length")), Is.EqualTo("(a + b).Length"));
        }

        [Test]
        public void Statements_PrintAllNodeKinds()
        {
            Assert.That(CSharpPrinter.Print(new CSharpVariableDeclaration("var", "value", new CSharpLiteral("null"))), Is.EqualTo("var value = null;"));
            Assert.That(CSharpPrinter.Print(new CSharpVariableDeclaration("int", "value")), Is.EqualTo("int value;"));
            Assert.That(CSharpPrinter.Print(new CSharpExpressionStatement(new CSharpCall(new CSharpIdentifier("Use"), [new CSharpIdentifier("value")]))), Is.EqualTo("Use(value);"));
            Assert.That(CSharpPrinter.Print(new CSharpThrow(new CSharpNew("Exception", []))), Is.EqualTo("throw new Exception();"));
            Assert.That(CSharpPrinter.Print(new CSharpReturn()), Is.EqualTo("return;"));
            Assert.That(CSharpPrinter.Print(new CSharpReturn(new CSharpIdentifier("value"))), Is.EqualTo("return value;"));
        }

        [Test]
        public void IfAndTryCatch_PrintStructuredBodies()
        {
            Assert.That(CSharpPrinter.Print(new CSharpIf(
                new CSharpIdentifier("condition"),
                [new CSharpReturn(new CSharpIdentifier("value"))],
                [new CSharpReturn()])), Is.EqualTo(Lines(
                    "if (condition)",
                    "    return value;",
                    "else",
                    "    return;")));

            Assert.That(CSharpPrinter.Print(new CSharpTryCatch(
                [new CSharpExpressionStatement(new CSharpCall(new CSharpIdentifier("DoThing"), []))],
                [new CSharpThrow(new CSharpIdentifier("exception"))])), Is.EqualTo(Lines(
                    "try",
                    "{",
                    "    DoThing();",
                    "}",
                    "catch (System.Exception reinteropManagedException)",
                    "{",
                    "    throw exception;",
                    "}")));
        }
    }
}
