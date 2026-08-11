using NUnit.Framework;

namespace Reinterop.Tests
{
    /// <summary>
    /// Recipe-level tests for Constructors.cs: run the generator directly and assert on the resulting
    /// <see cref="CppInteropFunction"/> recipes in <see cref="GeneratedResult.InteropFunctions"/>. The
    /// non-blittable constructor's hand-wrapped lambda body isn't expressible via recipe properties
    /// alone, so that one test also asserts on the printed definition text.
    /// </summary>
    public class ConstructorsGenerationTests
    {
        [Test]
        public void NonBlittableConstructor_IsStaticWithHandleMemberInitializerAndLambdaBody()
        {
            var results = GenerationTestHelper.GenerateResults(
                """
                using Reinterop;

                namespace TestNamespace
                {
                    public class Foo
                    {
                        public Foo(int value) { Value = value; }
                        public int Value;
                    }

                    [Reinterop]
                    internal class ConfigureReinterop
                    {
                        public void ExposeToCPP()
                        {
                            Foo foo = new Foo(1);
                        }
                    }
                }
                """);

            CppInteropFunction recipe = results["Foo"].InteropFunctions.Single(function => function.IsConstructor);

            Assert.That(recipe.Name, Is.EqualTo("Foo"));
            Assert.That(recipe.Static(), Is.True);
            Assert.That(recipe.MemberInitializers(), Has.Count.EqualTo(1));
            Assert.That(recipe.MemberInitializers()![0].MemberName, Is.EqualTo("_handle"));

            // The lambda-wrapped call+return body is hand-built in Constructors.cs, not derived from
            // CppInteropFunction.DefaultDefinitionBody(), so it can only be verified in printed text.
            // (CppHandleManagement.cs also emits other "Foo::Foo(...)" constructors - e.g. from an
            // object handle, from nullptr - so match on the lambda wrapper itself, not the name.)
            GeneratedCppDefinitionElement definitionElement = results["Foo"].CppDefinition.Elements
                .Single(element => element.Content.Contains("[&]() mutable {"));
            Assert.That(definitionElement.Content, Does.Contain(recipe.FunctionPointerName + "("));
        }

        [Test]
        public void BlittableStructConstructor_GeneratesStaticConstructFunction()
        {
            var results = GenerationTestHelper.GenerateResults(
                """
                using Reinterop;

                namespace TestNamespace
                {
                    public struct Vector2
                    {
                        public float X;
                        public float Y;

                        public Vector2(float x, float y) { X = x; Y = y; }
                    }

                    [Reinterop]
                    internal class ConfigureReinterop
                    {
                        public void ExposeToCPP()
                        {
                            Vector2 v = new Vector2(1, 2);
                        }
                    }
                }
                """);

            CppInteropFunction recipe = results["Vector2"].Find("Construct", 2);

            // Blittable-struct "constructors" are static factory functions, not C++ constructors -
            // IsConstructor (Name == Owner.Name) is therefore false for this recipe, by design.
            Assert.That(recipe.IsConstructor, Is.False);
            Assert.That(recipe.Static(), Is.True);
        }

        [Test]
        public void StaticClass_DeletesDefaultConstructor()
        {
            var results = GenerationTestHelper.GenerateResults(
                """
                using Reinterop;

                namespace TestNamespace
                {
                    public static class MathHelpers
                    {
                        public static int Add(int a, int b) { return a + b; }
                    }

                    [Reinterop]
                    internal class ConfigureReinterop
                    {
                        public void ExposeToCPP()
                        {
                            MathHelpers.Add(1, 2);
                        }
                    }
                }
                """);

            GeneratedCppDeclarationElement deletedConstructor = results["MathHelpers"].CppDeclaration.Elements
                .Single(element => element.Content.Contains("= delete"));

            Assert.That(deletedConstructor.Content, Is.EqualTo("MathHelpers() = delete;"));
            Assert.That(deletedConstructor.IsPrivate, Is.False);
        }
    }
}
