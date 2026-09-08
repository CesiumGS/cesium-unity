using NUnit.Framework;

namespace Reinterop.Tests
{
    /// <summary>
    /// Tests that each <see cref="CppExpression"/> and <see cref="CppStatement"/> node renders to the
    /// expected C++ source text, and reports the `#includes` it requires.
    /// </summary>
    public class CppSyntaxPrintingTests
    {
        private static string NL => Environment.NewLine;

        private static string Lines(params string[] lines) => string.Join(NL, lines);

        #region Expressions

        [Test]
        public void Identifier_PrintsName()
        {
            Assert.That(CppPrinter.Print(new CppIdentifier("x")), Is.EqualTo("x"));
            Assert.That(CppPrinter.Print(new CppIdentifier("::DotNet::System::String")), Is.EqualTo("::DotNet::System::String"));
        }

        [Test]
        public void Identifier_FromType_PrintsFullyQualifiedNameAndRequiresItsIncludes()
        {
            CppIdentifier identifier = new CppIdentifier(CppType.StlString);
            Assert.That(CppPrinter.Print(identifier), Is.EqualTo("::std::string"));
            Assert.That(CppPrinter.GetRequiredIncludes(identifier), Is.EquivalentTo(new[] { "<string>" }));
        }

        [Test]
        public void Literal_PrintsValueVerbatim()
        {
            Assert.That(CppPrinter.Print(new CppLiteral("42")), Is.EqualTo("42"));
            Assert.That(CppPrinter.Print(CppLiteral.Nullptr), Is.EqualTo("nullptr"));
            Assert.That(CppPrinter.Print(CppLiteral.True), Is.EqualTo("true"));
            Assert.That(CppPrinter.Print(CppLiteral.False), Is.EqualTo("false"));
        }

        [Test]
        public void Literal_String_QuotesAndEscapes()
        {
            Assert.That(CppPrinter.Print(CppLiteral.String("hello")), Is.EqualTo("\"hello\""));
            Assert.That(CppPrinter.Print(CppLiteral.String("say \"hi\"")), Is.EqualTo("\"say \\\"hi\\\"\""));
            Assert.That(CppPrinter.Print(CppLiteral.String("a\\b")), Is.EqualTo("\"a\\\\b\""));
            Assert.That(CppPrinter.Print(CppLiteral.String("a\r\nb\tc")), Is.EqualTo("\"a\\r\\nb\\tc\""));
        }

        [Test]
        public void This_PrintsThis()
        {
            Assert.That(CppPrinter.Print(CppIdentifier.This), Is.EqualTo("this"));
            Assert.That(CppPrinter.Print(new CppUnary("*", CppIdentifier.This)), Is.EqualTo("*this"));
        }

        [Test]
        public void Call_PrintsCommaSeparatedArguments()
        {
            Assert.That(
                CppPrinter.Print(new CppCall(new CppIdentifier("f"), [])),
                Is.EqualTo("f()"));
            Assert.That(
                CppPrinter.Print(new CppCall(new CppIdentifier("f"), [new CppIdentifier("a")])),
                Is.EqualTo("f(a)"));
            Assert.That(
                CppPrinter.Print(new CppCall(new CppIdentifier("f"), [new CppIdentifier("a"), new CppLiteral("1")])),
                Is.EqualTo("f(a, 1)"));
        }

        [Test]
        public void Call_ChainedThroughMemberAccess()
        {
            CppExpression expression = new CppCall(
                new CppMemberAccess(new CppCall(new CppIdentifier("Make"), []), "Release"),
                []);
            Assert.That(CppPrinter.Print(expression), Is.EqualTo("Make().Release()"));
        }

        [Test]
        public void Call_ParenthesizesDereferencedCallee()
        {
            CppExpression expression = new CppCall(
                new CppUnary("*", new CppIdentifier("pFunc")),
                []);

            Assert.That(CppPrinter.Print(expression), Is.EqualTo("(*pFunc)()"));
        }

        [Test]
        public void MemberAccess_UsesDot()
        {
            Assert.That(
                CppPrinter.Print(new CppMemberAccess(new CppIdentifier("s"), "data")),
                Is.EqualTo("s.data"));
        }

        [Test]
        public void PointerMemberAccess_UsesArrow()
        {
            Assert.That(
                CppPrinter.Print(new CppPointerMemberAccess(CppIdentifier.This, "_handle")),
                Is.EqualTo("this->_handle"));
        }

        [Test]
        public void Cast_PrintsCStyleCast()
        {
            Assert.That(
                CppPrinter.Print(CppCast.CStyle(CppType.StlString, new CppIdentifier("x"))),
                Is.EqualTo("(::std::string)(x)"));
        }

        [Test]
        public void NamedCasts_PrintCastOperatorAndTargetType()
        {
            Assert.That(
                CppPrinter.Print(CppCast.Static(CppType.Char.AsPointer(), new CppIdentifier("p"))),
                Is.EqualTo("static_cast<char*>(p)"));
            Assert.That(
                CppPrinter.Print(CppCast.Const(CppType.UInt8.AsPointer(), new CppIdentifier("p"))),
                Is.EqualTo("const_cast<::std::uint8_t*>(p)"));
            Assert.That(
                CppPrinter.Print(CppCast.Reinterpret(CppType.VoidPointer, new CppIdentifier("p"))),
                Is.EqualTo("reinterpret_cast<void*>(p)"));
        }

        [Test]
        public void NamedCasts_Nest()
        {
            CppExpression expression = CppCast.Const(
                CppType.UInt8.AsPointer(),
                CppCast.Reinterpret(CppType.UInt8.AsConstPointer(), new CppCall(new CppMemberAccess(new CppIdentifier("s"), "data"), [])));
            Assert.That(
                CppPrinter.Print(expression),
                Is.EqualTo("const_cast<::std::uint8_t*>(reinterpret_cast<const ::std::uint8_t*>(s.data()))"));
        }

        [Test]
        public void New_PrintsNewWithArguments()
        {
            CppType functionType = new CppType(
                InteropTypeKind.Unknown,
                [ "std" ], "function",
                [ new CppType(InteropTypeKind.Primitive, [], "void()", null, 0) ],
                0, "<functional>");
            Assert.That(
                CppPrinter.Print(new CppNew(functionType, [])),
                Is.EqualTo("new ::std::function<void()>()"));

            CppType fooType = new CppType(InteropTypeKind.ClassWrapper, [], "Foo", null, 0, null);
            Assert.That(
                CppPrinter.Print(new CppNew(fooType, [new CppIdentifier("a"), new CppIdentifier("b")])),
                Is.EqualTo("new ::Foo(a, b)"));
        }

        [Test]
        public void Move_PrintsStdMoveAndRequiresUtility()
        {
            CppExpression expression = new CppMove(new CppIdentifier("callback"));
            Assert.That(CppPrinter.Print(expression), Is.EqualTo("std::move(callback)"));
            Assert.That(CppPrinter.GetRequiredIncludes(expression), Does.Contain("<utility>"));
        }

        [Test]
        public void Binary_PrintsSpacedOperator()
        {
            Assert.That(
                CppPrinter.Print(new CppBinary("==", new CppIdentifier("a"), CppLiteral.Nullptr)),
                Is.EqualTo("a == nullptr"));
            Assert.That(
                CppPrinter.Print(new CppBinary("!=", new CppIdentifier("a"), new CppIdentifier("b"))),
                Is.EqualTo("a != b"));
        }

        [Test]
        public void Unary_PrintsPrefixOperatorWithNoSpace()
        {
            Assert.That(CppPrinter.Print(new CppUnary("&", new CppIdentifier("x"))), Is.EqualTo("&x"));
            Assert.That(
                CppPrinter.Print(new CppUnary("!", new CppUnary("!", new CppIdentifier("x")))),
                Is.EqualTo("!(!x)"));
        }

        [Test]
        public void Ternary_PrintsConditionalExpression()
        {
            Assert.That(
                CppPrinter.Print(new CppTernary(
                    new CppBinary("==", new CppIdentifier("p"), CppLiteral.Nullptr),
                    new CppIdentifier("std::nullopt"),
                    new CppCall(new CppIdentifier("std::make_optional"), [new CppUnary("*", new CppIdentifier("p"))]))),
                Is.EqualTo("(p == nullptr) ? std::nullopt : std::make_optional(*p)"));
        }

        [Test]
        public void GetRequiredIncludes_GathersFromNestedExpressions()
        {
            CppExpression expression = new CppCall(new CppIdentifier("f"), [
                CppCast.Static(CppType.Char.AsPointer(), new CppIdentifier("p") { RequiredIncludes = ["<a.h>"] }),
                new CppMove(new CppIdentifier("q")),
                new CppTernary(
                    new CppIdentifier("c") { RequiredIncludes = ["<b.h>"] },
                    new CppIdentifier("t") { RequiredIncludes = ["<c.h>"] },
                    new CppMemberAccess(new CppIdentifier("e") { RequiredIncludes = ["<d.h>"] }, "m"))
            ])
            {
                RequiredIncludes = ["<f.h>"]
            };

            Assert.That(
                CppPrinter.GetRequiredIncludes(expression),
                Is.EquivalentTo(new[] { "<a.h>", "<b.h>", "<c.h>", "<d.h>", "<f.h>", "<utility>" }));
        }

        #endregion

        #region Statements

        [Test]
        public void VariableDeclaration_WithoutInitializer()
        {
            CppType myStructType = new CppType(InteropTypeKind.BlittableStruct, [], "MyStruct", null, 0, null);
            Assert.That(
                CppPrinter.Print(new CppVariableDeclaration(myStructType, "result")),
                Is.EqualTo("::MyStruct result;"));
        }

        [Test]
        public void VariableDeclaration_WithInitializer()
        {
            Assert.That(
                CppPrinter.Print(new CppVariableDeclaration(CppType.VoidPointer, "p", new CppCall(new CppIdentifier("Get"), []))),
                Is.EqualTo("void* p = Get();"));
        }

        [Test]
        public void VariableDeclaration_CanUseBracedInitializationWithoutBracingInitializerExpression()
        {
            CppType wrapperType = new CppType(InteropTypeKind.ClassWrapper, ["Example"], "Wrapper", null, 0).AsConst();
            CppType objectHandleType = new CppType(InteropTypeKind.ClassWrapper, ["Example"], "ObjectHandle", null, 0);

            Assert.That(
                CppPrinter.Print(new CppVariableDeclaration(
                    wrapperType,
                    "wrapper",
                    new CppCall(new CppIdentifier(objectHandleType), [new CppIdentifier("handle")]),
                    UseBracedInitialization: true)),
                Is.EqualTo("const ::Example::Wrapper wrapper{::Example::ObjectHandle(handle)};"));
        }

        [Test]
        public void ExpressionStatement_AppendsSemicolon()
        {
            Assert.That(
                CppPrinter.Print(new CppExpressionStatement(new CppCall(new CppIdentifier("Free"), [new CppIdentifier("p")]))),
                Is.EqualTo("Free(p);"));
        }

        [Test]
        public void Assignment_PrintsTargetAndValue()
        {
            Assert.That(
                CppPrinter.Print(new CppAssignment(
                    new CppPointerMemberAccess(CppIdentifier.This, "_handle"),
                    new CppMove(new CppMemberAccess(new CppIdentifier("result"), "_handle")))),
                Is.EqualTo("this->_handle = std::move(result._handle);"));
        }

        [Test]
        public void If_SingleStatementBody_OmitsBraces()
        {
            Assert.That(
                CppPrinter.Print(new CppIf(
                    new CppBinary("==", new CppUnary("*", CppIdentifier.This), CppLiteral.Nullptr),
                    [new CppReturn(new CppCall(new CppIdentifier("std::string"), []))])),
                Is.EqualTo(Lines(
                    "if ((*this) == nullptr)",
                    "    return std::string();")));
        }

        [Test]
        public void If_MultiStatementBody_UsesBracesAndIndents()
        {
            Assert.That(
                CppPrinter.Print(new CppIf(
                    new CppIdentifier("ok"),
                    [
                        new CppExpressionStatement(new CppCall(new CppIdentifier("A"), [])),
                        new CppExpressionStatement(new CppCall(new CppIdentifier("B"), []))
                    ])),
                Is.EqualTo(Lines(
                    "if (ok) {",
                    "    A();",
                    "    B();",
                    "}")));
        }

        [Test]
        public void Throw_WithException()
        {
            Assert.That(
                CppPrinter.Print(new CppThrow(new CppCall(new CppIdentifier("std::runtime_error"), [CppLiteral.String("nope")]))),
                Is.EqualTo("throw std::runtime_error(\"nope\");"));
        }

        [Test]
        public void Throw_WithoutException_IsRethrow()
        {
            Assert.That(CppPrinter.Print(new CppThrow()), Is.EqualTo("throw;"));
        }

        [Test]
        public void Return_WithAndWithoutValue()
        {
            Assert.That(CppPrinter.Print(new CppReturn()), Is.EqualTo("return;"));
            Assert.That(CppPrinter.Print(new CppReturn(new CppIdentifier("result"))), Is.EqualTo("return result;"));
        }

        [Test]
        public void Try_WithTypedAndCatchAllClauses()
        {
            CppStatement statement = new CppTry(
                [
                    new CppVariableDeclaration(CppType.StlString, "result", CppCast.Static(CppType.Char.AsPointer(), new CppIdentifier("p"))),
                    new CppReturn(new CppIdentifier("result"))
                ],
                [
                    new CppCatch("std::exception", "e", [new CppReturn(new CppIdentifier("fallback"))]),
                    new CppCatch(null, null, [new CppThrow()])
                ]);

            Assert.That(CppPrinter.Print(statement), Is.EqualTo(Lines(
                "try {",
                "    ::std::string result = static_cast<char*>(p);",
                "    return result;",
                "} catch (std::exception& e) {",
                "    return fallback;",
                "} catch (...) {",
                "    throw;",
                "}")));
        }

        [Test]
        public void PrintStatementList_SeparatesWithNewlines()
        {
            IReadOnlyList<CppStatement> statements = [
                new CppVariableDeclaration(CppType.Int32, "x", new CppLiteral("1")),
                new CppExpressionStatement(new CppCall(new CppIdentifier("Use"), [new CppIdentifier("x")])),
                new CppReturn(new CppIdentifier("x"))
            ];

            Assert.That(CppPrinter.Print(statements), Is.EqualTo(Lines(
                "::std::int32_t x = 1;",
                "Use(x);",
                "return x;")));
        }

        [Test]
        public void GetRequiredIncludes_GathersFromAllStatementKinds()
        {
            IReadOnlyList<CppStatement> statements = [
                new CppVariableDeclaration(CppType.Int32, "a", new CppIdentifier("x") { RequiredIncludes = ["<decl.h>"] }),
                new CppExpressionStatement(new CppIdentifier("y") { RequiredIncludes = ["<expr.h>"] }),
                new CppAssignment(
                    new CppIdentifier("t") { RequiredIncludes = ["<target.h>"] },
                    new CppIdentifier("v") { RequiredIncludes = ["<value.h>"] }),
                new CppIf(
                    new CppIdentifier("c") { RequiredIncludes = ["<cond.h>"] },
                    [new CppReturn(new CppIdentifier("r") { RequiredIncludes = ["<then.h>"] })]),
                new CppThrow(new CppIdentifier("ex") { RequiredIncludes = ["<throw.h>"] }),
                new CppThrow(),
                new CppTry(
                    [new CppReturn(new CppIdentifier("b") { RequiredIncludes = ["<try.h>"] })],
                    [new CppCatch(null, null, [new CppReturn(new CppIdentifier("c2") { RequiredIncludes = ["<catch.h>"] })])])
            ];

            Assert.That(CppPrinter.GetRequiredIncludes(statements), Is.EquivalentTo(new[]
            {
                "<cstdint>", "<decl.h>", "<expr.h>", "<target.h>", "<value.h>", "<cond.h>", "<then.h>", "<throw.h>", "<try.h>", "<catch.h>"
            }));
        }

        #endregion
    }
}
