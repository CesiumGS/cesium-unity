using NUnit.Framework;

namespace Reinterop.Tests
{
    /// <summary>
    /// Recipe-level tests for Methods.cs: run the generator directly and assert on the resulting
    /// <see cref="CppInteropFunction"/> recipes in <see cref="GeneratedResult.InteropFunctions"/>,
    /// rather than on the printed C++ text.
    /// </summary>
    public class MethodsGenerationTests
    {
        [Test]
        public void PlainInstanceMethod()
        {
            var results = GenerationTestHelper.GenerateResults(
                """
                using Reinterop;

                namespace TestNamespace
                {
                    public class Foo
                    {
                        public int Add(int x) { return x + 1; }
                    }

                    [Reinterop]
                    internal class ConfigureReinterop
                    {
                        public void ExposeToCPP()
                        {
                            Foo foo = new Foo();
                            foo.Add(1);
                        }
                    }
                }
                """);

            CppInteropFunction recipe = results["Foo"].Find("Add", 1);

            Assert.That(recipe.Static(), Is.False);
            Assert.That(recipe.Private(), Is.False);
            Assert.That(recipe.Parameters()[0].Name, Is.EqualTo("x"));
            Assert.That(recipe.Parameters()[0].Type.Name, Is.EqualTo("int32_t"));
            Assert.That(recipe.ReturnType().Name, Is.EqualTo("int32_t"));
            Assert.That(recipe.NeedsStructReturnRewrite, Is.False);
        }

        [Test]
        public void StaticMethod()
        {
            var results = GenerationTestHelper.GenerateResults(
                """
                using Reinterop;

                namespace TestNamespace
                {
                    public class Foo
                    {
                        public static int DoubleIt(int x) { return x * 2; }
                    }

                    [Reinterop]
                    internal class ConfigureReinterop
                    {
                        public void ExposeToCPP()
                        {
                            Foo.DoubleIt(1);
                        }
                    }
                }
                """);

            CppInteropFunction recipe = results["Foo"].Find("DoubleIt", 1);

            Assert.That(recipe.Static(), Is.True);
        }

        [Test]
        public void GenericMethod_TwoInstantiations_ShareOneTemplateRecipe()
        {
            var results = GenerationTestHelper.GenerateResults(
                """
                using Reinterop;

                namespace TestNamespace
                {
                    public class Foo
                    {
                        public T Identity<T>(T value) { return value; }
                    }

                    [Reinterop]
                    internal class ConfigureReinterop
                    {
                        public void ExposeToCPP()
                        {
                            Foo foo = new Foo();
                            foo.Identity(1);
                            foo.Identity(true);
                        }
                    }
                }
                """);

            List<CppInteropFunction> identityRecipes = results["Foo"].InteropFunctions
                .Where(function => function.Name == "Identity")
                .ToList();

            // One unspecialized template recipe (added once, cached by GenerateTypeState.MethodCache
            // despite two calls) plus one specialization per instantiation.
            Assert.That(identityRecipes, Has.Count.EqualTo(3));

            CppInteropFunction template = identityRecipes.Single(function => function.Specializes() == null);
            List<CppInteropFunction> specializations = identityRecipes.Where(function => function.Specializes() != null).ToList();

            Assert.That(specializations, Has.Count.EqualTo(2));
            Assert.That(specializations, Has.All.Matches<CppInteropFunction>(function => function.Specializes() == template));
            Assert.That(
                specializations.Select(function => function.TypeArguments().Single().Name),
                Is.EquivalentTo(new[] { "int32_t", "bool" }));
        }

        [Test]
        public void EqualityOperator_GeneratesPrivateInteropRecipeAndPublicOperator()
        {
            var results = GenerationTestHelper.GenerateResults(
                """
                using Reinterop;

                namespace TestNamespace
                {
                    public class Foo
                    {
                        public static bool operator==(Foo a, Foo b) { return true; }
                        public static bool operator!=(Foo a, Foo b) { return false; }
                    }

                    [Reinterop]
                    internal class ConfigureReinterop
                    {
                        public void ExposeToCPP()
                        {
                            Foo foo = new Foo();
                        }
                    }
                }
                """);

            CppInteropFunction interopRecipe = results["Foo"].Find("op_Equality", 2);
            CppInteropFunction operatorRecipe = results["Foo"].InteropFunctions.Single(function => function.Name == "operator==");

            Assert.That(interopRecipe.Private(), Is.True);
            Assert.That(operatorRecipe.Private(), Is.False);
            Assert.That(operatorRecipe.Parameters(), Has.Count.EqualTo(1));
        }

        [Test]
        public void MethodReturningBlittableStruct_NeedsVoidReturnRewrite()
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
                    }

                    public class Foo
                    {
                        public Vector2 GetPosition() { return new Vector2(); }
                    }

                    [Reinterop]
                    internal class ConfigureReinterop
                    {
                        public void ExposeToCPP()
                        {
                            Foo foo = new Foo();
                            foo.GetPosition();
                        }
                    }
                }
                """);

            CppInteropFunction recipe = results["Foo"].Find("GetPosition", 0);

            Assert.That(recipe.NeedsStructReturnRewrite, Is.True);
            Assert.That(recipe.InteropReturnType.Name, Is.EqualTo("void"));
        }

        [Test]
        public void MethodReturningNullablePrimitive_NeedsBoolFlagReturnRewrite()
        {
            var results = GenerationTestHelper.GenerateResults(
                """
                using Reinterop;

                namespace TestNamespace
                {
                    public class Foo
                    {
                        public int? MaybeGetValue() { return null; }
                    }

                    [Reinterop]
                    internal class ConfigureReinterop
                    {
                        public void ExposeToCPP()
                        {
                            Foo foo = new Foo();
                            foo.MaybeGetValue();
                        }
                    }
                }
                """);

            CppInteropFunction recipe = results["Foo"].Find("MaybeGetValue", 0);

            Assert.That(recipe.NeedsStructReturnRewrite, Is.True);
            Assert.That(recipe.InteropReturnType.Name, Is.EqualTo("uint8_t"));
        }
    }
}
