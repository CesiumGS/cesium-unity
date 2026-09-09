using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Reinterop.Tests
{
    /// <summary>
    /// Tests for <see cref="CppFunction"/>: build recipes directly from known
    /// <see cref="CppType"/> values (no Roslyn compilation/symbols needed) and assert that
    /// <see cref="CppFunction.AddToGeneration"/> produces the expected declaration and definition text.
    /// </summary>
    public class CppFunctionTests
    {
        private static ReinteropGenerationContext CreateContext() => new ReinteropGenerationContext(CSharpCompilation.Create("Test"));

        [Test]
        public void InstanceMethod_GeneratesDeclarationAndDefinition()
        {
            ReinteropGenerationContext context = CreateContext();
            CppType owner = new CppType(InteropTypeKind.ClassWrapper, new[] { "MyNamespace" }, "Foo", null, 0);
            GeneratedResult result = new GeneratedResult(owner);

            CppFunction function = new CppFunction(context, owner, "Add")
                .Parameters(new[] { new CppParameter(CppType.Int32.AsParameterType(), "x") })
                .ReturnType(CppType.Int32.AsReturnType())
                .DefinitionBody(new CppStatement[]
                {
                    new CppReturn(new CppIdentifier("x"))
                });

            function.AddToGeneration(result);

            Assert.That(result.CppDeclaration.Elements, Has.Count.EqualTo(1));
            Assert.That(result.CppDeclaration.Elements[0].Content, Is.EqualTo("::std::int32_t Add(::std::int32_t x) const;"));

            Assert.That(result.CppDefinition.Elements, Has.Count.EqualTo(1));
            Assert.That(result.CppDefinition.Elements[0].Content, Is.EqualTo(string.Join(Environment.NewLine, new[]
            {
                "::std::int32_t Foo::Add(::std::int32_t x) const {",
                "    return x;",
                "}"
            })));
        }

        [Test]
        public void StaticMethod_OmitsConstQualifier()
        {
            ReinteropGenerationContext context = CreateContext();
            CppType owner = new CppType(InteropTypeKind.ClassWrapper, new[] { "MyNamespace" }, "Foo", null, 0);
            GeneratedResult result = new GeneratedResult(owner);

            CppFunction function = new CppFunction(context, owner, "DoubleIt")
                .Parameters(new[] { new CppParameter(CppType.Int32.AsParameterType(), "x") })
                .ReturnType(CppType.Int32.AsReturnType())
                .Static(true)
                .DefinitionBody(new CppStatement[]
                {
                    new CppReturn(new CppBinary("*", new CppIdentifier("x"), new CppLiteral("2")))
                });

            function.AddToGeneration(result);

            Assert.That(result.CppDeclaration.Elements[0].Content, Is.EqualTo("static ::std::int32_t DoubleIt(::std::int32_t x);"));
            Assert.That(result.CppDefinition.Elements[0].Content, Is.EqualTo(string.Join(Environment.NewLine, new[]
            {
                "::std::int32_t Foo::DoubleIt(::std::int32_t x) {",
                "    return x * 2;",
                "}"
            })));
        }

        [Test]
        public void PrivateMethod_OwnDeclarationIsPrivate()
        {
            ReinteropGenerationContext context = CreateContext();
            CppType owner = new CppType(InteropTypeKind.ClassWrapper, new[] { "MyNamespace" }, "Foo", null, 0);
            GeneratedResult result = new GeneratedResult(owner);

            CppFunction function = new CppFunction(context, owner, "op_Equality")
                .Parameters(new[] { new CppParameter(owner.AsParameterType(), "other") })
                .ReturnType(CppType.Boolean.AsReturnType())
                .Private(true);

            function.AddToGeneration(result);

            Assert.That(result.CppDeclaration.Elements, Has.Count.EqualTo(1));
            Assert.That(result.CppDeclaration.Elements[0].IsPrivate, Is.True);
        }

        [Test]
        public void Constructor_OmitsReturnTypeAndUsesMemberInitializers()
        {
            ReinteropGenerationContext context = CreateContext();
            CppType owner = new CppType(InteropTypeKind.ClassWrapper, new[] { "MyNamespace" }, "Foo", null, 0);
            GeneratedResult result = new GeneratedResult(owner);

            CppFunction function = new CppFunction(context, owner, "Foo")
                .Parameters(new[] { new CppParameter(CppType.Int32.AsParameterType(), "value") })
                .Static(true)
                .MemberInitializers(new[] { new CppMemberInitializer("_value", new CppIdentifier("value")) })
                .DefinitionBody(new CppStatement[] { new CppExpressionStatement(new CppCall(new CppIdentifier("DoSomething"), [])) });

            function.AddToGeneration(result);

            Assert.That(function.IsConstructor, Is.True);
            Assert.That(result.CppDeclaration.Elements[0].Content, Is.EqualTo("Foo(::std::int32_t value);"));
            Assert.That(result.CppDefinition.Elements[0].Content, Is.EqualTo(string.Join(Environment.NewLine, new[]
            {
                "Foo::Foo(::std::int32_t value) : _value(value) {",
                "    DoSomething();",
                "}"
            })));
        }

        [Test]
        public void GenericSpecialization_DefinitionIsTemplateQualifiedWithConstReferenceParameters()
        {
            ReinteropGenerationContext context = CreateContext();
            CppType owner = new CppType(InteropTypeKind.ClassWrapper, new[] { "MyNamespace" }, "Foo", null, 0);
            GeneratedResult result = new GeneratedResult(owner);
            CppType genericParameterType = new CppType(InteropTypeKind.GenericParameter, Array.Empty<string>(), "T", null, 0);

            CppFunction template = new CppFunction(context, owner, "Identity")
                .TypeParameters(new[] { new CppParameter(genericParameterType, "T") })
                .TypeArguments(new[] { genericParameterType })
                .Parameters(new[] { new CppParameter(genericParameterType.AsParameterType(), "value") })
                .ReturnType(genericParameterType);

            template.AddToGeneration(result);

            Assert.That(result.CppDeclaration.Elements, Has.Count.EqualTo(1));
            Assert.That(result.CppDeclaration.Elements[0].Content, Is.EqualTo("template <typename T>\nT Identity(const T& value) const;"));
            Assert.That(result.CppDefinition.Elements, Is.Empty);

            CppFunction specialization = new CppFunction(context, owner, "Identity")
                .TypeArguments(new[] { CppType.Int32 })
                .Parameters(new[] { new CppParameter(CppType.Int32.AsParameterType(), "value") })
                .ReturnType(CppType.Int32.AsReturnType())
                .Specializes(template)
                .DefinitionBody(new CppStatement[] { new CppReturn(new CppIdentifier("value")) });

            specialization.AddToGeneration(result);

            GeneratedCppDefinitionElement specializationDefinition = result.CppDefinition.Elements.Single(
                element => element.Content.StartsWith("template <> "));
            Assert.That(specializationDefinition.Content, Does.StartWith(
                "template <> ::std::int32_t Foo::Identity<::std::int32_t>(const ::std::int32_t& value) const {"));
        }
    }
}