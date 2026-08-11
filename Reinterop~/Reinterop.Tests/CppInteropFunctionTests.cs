using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Reinterop.Tests
{
    /// <summary>
    /// Tests for <see cref="CppInteropFunction"/> itself: build recipes directly from known
    /// <see cref="CppType"/> values (no Roslyn compilation/symbols needed) and assert that
    /// <see cref="CppInteropFunction.AddToGeneration"/> produces the exact expected declaration,
    /// definition, and init-function text. Expected text is hard-coded (rather than reconstructed via
    /// interpolation/helper calls) so a reader can see at a glance what's expected, even though it
    /// makes these tests more likely to need updating if the generated format changes.
    /// </summary>
    public class CppInteropFunctionTests
    {
        private static CppGenerationContext CreateContext() => new CppGenerationContext(CSharpCompilation.Create("Test"));

        [Test]
        public void InstanceMethod_GeneratesDeclarationDefinitionAndInit()
        {
            CppGenerationContext context = CreateContext();
            CppType owner = new CppType(InteropTypeKind.ClassWrapper, new[] { "MyNamespace" }, "Foo", null, 0);
            GeneratedResult result = new GeneratedResult(owner);

            CppInteropFunction recipe = new CppInteropFunction(context, owner, "Add")
                .Parameters(new[] { new CppInteropParameter("x", CppType.Int32.AsParameterType()) })
                .ReturnType(CppType.Int32.AsReturnType())
                .CSharp("CallAdd", "csharpContent");

            recipe.AddToGeneration(result);

            // The only piece of the expected text that can't be hard-coded: a hash of the parameters.
            string functionPointerName = recipe.FunctionPointerName;

            Assert.That(result.CppDeclaration.Elements, Has.Count.EqualTo(2));

            GeneratedCppDeclarationElement pointerFieldDeclaration = result.CppDeclaration.Elements[0];
            Assert.That(pointerFieldDeclaration.Content, Is.EqualTo(
                $"static ::std::int32_t (*{functionPointerName})(void* thiz, ::std::int32_t x, void** reinteropException);"));
            Assert.That(pointerFieldDeclaration.IsPrivate, Is.True);

            GeneratedCppDeclarationElement methodDeclaration = result.CppDeclaration.Elements[1];
            Assert.That(methodDeclaration.Content, Is.EqualTo("::std::int32_t Add(::std::int32_t x) const;"));
            Assert.That(methodDeclaration.IsPrivate, Is.False);

            Assert.That(result.CppDefinition.Elements, Has.Count.EqualTo(2));

            GeneratedCppDefinitionElement pointerFieldDefinition = result.CppDefinition.Elements[0];
            Assert.That(pointerFieldDefinition.Content, Is.EqualTo(
                $"::std::int32_t (*MyNamespace::Foo::{functionPointerName})(void* thiz, ::std::int32_t x, void** reinteropException) = nullptr;"));

            GeneratedCppDefinitionElement methodDefinition = result.CppDefinition.Elements[1];
            string expectedDefinition = string.Join(Environment.NewLine, new[]
            {
                "::std::int32_t Foo::Add(::std::int32_t x) const {",
                "    void* reinteropException = nullptr;",
                $"    auto result = {functionPointerName}((*this).GetHandle().GetRaw(), x, &reinteropException);",
                "    if (reinteropException != nullptr)",
                "        throw Reinterop::ReinteropNativeException(::DotNet::System::Exception(::DotNet::Reinterop::ObjectHandle(reinteropException)));",
                "    return result;",
                "}"
            });
            Assert.That(methodDefinition.Content, Is.EqualTo(expectedDefinition));

            Assert.That(result.Init.Functions, Has.Count.EqualTo(1));
            GeneratedInitFunction initFunction = result.Init.Functions[0];
            Assert.That(initFunction.CppName, Is.EqualTo($"::MyNamespace::Foo::{functionPointerName}"));
            Assert.That(initFunction.CppTypeSignature, Is.EqualTo("::std::int32_t (*)(void*, ::std::int32_t, void**)"));
            Assert.That(initFunction.CSharpName, Is.EqualTo("CallAdd"));
            Assert.That(initFunction.CSharpContent, Is.EqualTo("csharpContent"));
        }

        [Test]
        public void StaticMethod_OmitsThisParameterAndConstQualifier()
        {
            CppGenerationContext context = CreateContext();
            CppType owner = new CppType(InteropTypeKind.ClassWrapper, new[] { "MyNamespace" }, "Foo", null, 0);
            GeneratedResult result = new GeneratedResult(owner);

            CppInteropFunction recipe = new CppInteropFunction(context, owner, "DoubleIt")
                .Parameters(new[] { new CppInteropParameter("x", CppType.Int32.AsParameterType()) })
                .ReturnType(CppType.Int32.AsReturnType())
                .Static(true)
                .CSharp("CallDoubleIt", "csharpContent");

            recipe.AddToGeneration(result);

            string functionPointerName = recipe.FunctionPointerName;

            GeneratedCppDeclarationElement pointerFieldDeclaration = result.CppDeclaration.Elements[0];
            Assert.That(pointerFieldDeclaration.Content, Is.EqualTo(
                $"static ::std::int32_t (*{functionPointerName})(::std::int32_t x, void** reinteropException);"));

            GeneratedCppDeclarationElement methodDeclaration = result.CppDeclaration.Elements[1];
            Assert.That(methodDeclaration.Content, Is.EqualTo("static ::std::int32_t DoubleIt(::std::int32_t x);"));

            GeneratedCppDefinitionElement methodDefinition = result.CppDefinition.Elements[1];
            string expectedDefinition = string.Join(Environment.NewLine, new[]
            {
                "::std::int32_t Foo::DoubleIt(::std::int32_t x) {",
                "    void* reinteropException = nullptr;",
                $"    auto result = {functionPointerName}(x, &reinteropException);",
                "    if (reinteropException != nullptr)",
                "        throw Reinterop::ReinteropNativeException(::DotNet::System::Exception(::DotNet::Reinterop::ObjectHandle(reinteropException)));",
                "    return result;",
                "}"
            });
            Assert.That(methodDefinition.Content, Is.EqualTo(expectedDefinition));
        }

        [Test]
        public void PrivateMethod_OwnDeclarationIsPrivate()
        {
            CppGenerationContext context = CreateContext();
            CppType owner = new CppType(InteropTypeKind.ClassWrapper, new[] { "MyNamespace" }, "Foo", null, 0);
            GeneratedResult result = new GeneratedResult(owner);

            CppInteropFunction recipe = new CppInteropFunction(context, owner, "op_Equality")
                .Parameters(new[] { new CppInteropParameter("other", owner.AsParameterType()) })
                .ReturnType(CppType.Boolean.AsReturnType())
                .Private(true)
                .CSharp("CallOpEquality", "csharpContent");

            recipe.AddToGeneration(result);

            // The interop function pointer field is always private, regardless of Private().
            Assert.That(result.CppDeclaration.Elements[0].IsPrivate, Is.True);
            Assert.That(result.CppDeclaration.Elements[1].IsPrivate, Is.True);
        }

        [Test]
        public void Constructor_OmitsReturnTypeAndUsesMemberInitializers()
        {
            CppGenerationContext context = CreateContext();
            CppType owner = new CppType(InteropTypeKind.ClassWrapper, new[] { "MyNamespace" }, "Foo", null, 0);
            GeneratedResult result = new GeneratedResult(owner);

            IReadOnlyList<CppStatement> constructorBody = new CppStatement[] { new CppRawStatement("DoSomething();") };

            // Name == Owner.Name makes this recipe a constructor (IsConstructor).
            CppInteropFunction recipe = new CppInteropFunction(context, owner, "Foo")
                .Parameters(new[] { new CppInteropParameter("value", CppType.Int32.AsParameterType()) })
                .Static(true)
                .MemberInitializers(new List<CppMemberInitializer> { new CppMemberInitializer("_value", new CppIdentifier("value")) })
                .DefinitionBody(constructorBody);

            // No CSharpDelegateInit() call - constructors build their own interop function pointer
            // separately (via AddInteropFunctionPointer), so AddToGeneration should add neither a
            // pointer field nor an init registration here.
            recipe.AddToGeneration(result);

            Assert.That(recipe.IsConstructor, Is.True);
            Assert.That(result.CppDeclaration.Elements, Has.Count.EqualTo(1));
            Assert.That(result.CppDeclaration.Elements[0].Content, Is.EqualTo("Foo(::std::int32_t value);"));

            Assert.That(result.CppDefinition.Elements, Has.Count.EqualTo(1));
            string expectedDefinition = string.Join(Environment.NewLine, new[]
            {
                "Foo::Foo(::std::int32_t value) : _value(value) {",
                "    DoSomething();",
                "}"
            });
            Assert.That(result.CppDefinition.Elements[0].Content, Is.EqualTo(expectedDefinition));

            Assert.That(result.Init.Functions, Is.Empty);
        }

        [Test]
        public void GenericSpecialization_DefinitionIsTemplateQualifiedWithConstReferenceParameters()
        {
            CppGenerationContext context = CreateContext();
            CppType owner = new CppType(InteropTypeKind.ClassWrapper, new[] { "MyNamespace" }, "Foo", null, 0);
            GeneratedResult result = new GeneratedResult(owner);

            CppType genericParameterType = new CppType(InteropTypeKind.GenericParameter, Array.Empty<string>(), "T", null, 0);

            CppInteropFunction template = new CppInteropFunction(context, owner, "Identity")
                .TypeParameters(new[] { new CppInteropParameter("T", genericParameterType) })
                .TypeArguments(new[] { genericParameterType })
                .Parameters(new[] { new CppInteropParameter("value", genericParameterType.AsParameterType()) })
                .ReturnType(genericParameterType);

            // An unspecialized generic only gets its own template declaration - no definition or
            // interop function pointer (it isn't itself callable).
            template.AddToGeneration(result);

            Assert.That(result.CppDeclaration.Elements, Has.Count.EqualTo(1));
            Assert.That(result.CppDeclaration.Elements[0].Content, Is.EqualTo("template <typename T>\nT Identity(const T& value) const;"));
            Assert.That(result.CppDefinition.Elements, Is.Empty);
            Assert.That(result.Init.Functions, Is.Empty);

            CppInteropFunction specialization = new CppInteropFunction(context, owner, "Identity")
                .TypeArguments(new[] { CppType.Int32 })
                .Parameters(new[] { new CppInteropParameter("value", CppType.Int32.AsParameterType()) })
                .ReturnType(CppType.Int32.AsReturnType())
                .Specializes(template)
                .CSharp("CallIdentity_Int32", "csharpContent");

            specialization.AddToGeneration(result);

            // A specialization reuses the template's declaration - it doesn't add its own (only its
            // interop function pointer field declaration is added here).
            Assert.That(result.CppDeclaration.Elements, Has.Count.EqualTo(2));

            GeneratedCppDefinitionElement specializationDefinition = result.CppDefinition.Elements.Single(
                element => element.Content.StartsWith("template <> "));

            // The parameter is passed as "const T&" here (matching the unspecialized template's generic
            // parameter), even though the specialization's own Parameters() declares it as a plain value.
            Assert.That(specializationDefinition.Content, Does.StartWith(
                "template <> ::std::int32_t Foo::Identity<::std::int32_t>(const ::std::int32_t& value) const {"));
        }
    }
}
