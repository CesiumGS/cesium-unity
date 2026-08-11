using NUnit.Framework;

namespace Reinterop.Tests
{
    /// <summary>
    /// Recipe-level tests for Properties.cs: run the generator directly and assert on the resulting
    /// <see cref="CppInteropFunction"/> recipes in <see cref="GeneratedResult.InteropFunctions"/>,
    /// rather than on the printed C++ text.
    /// </summary>
    public class PropertiesGenerationTests
    {
        [Test]
        public void GetOnlyProperty_GeneratesSingleZeroParameterRecipe()
        {
            var results = GenerationTestHelper.GenerateResults(
                """
                using Reinterop;

                namespace TestNamespace
                {
                    public class Foo
                    {
                        public int Value => 42;
                    }

                    [Reinterop]
                    internal class ConfigureReinterop
                    {
                        public void ExposeToCPP()
                        {
                            Foo foo = new Foo();
                            int v = foo.Value;
                        }
                    }
                }
                """);

            List<CppInteropFunction> recipes = results["Foo"].InteropFunctions.Where(function => function.Name == "Value").ToList();

            Assert.That(recipes, Has.Count.EqualTo(1));
            Assert.That(recipes[0].Parameters(), Is.Empty);
            Assert.That(recipes[0].ReturnType().Name, Is.EqualTo("int32_t"));
        }

        [Test]
        public void GetSetProperty_GeneratesGetterAndSetterRecipes()
        {
            var results = GenerationTestHelper.GenerateResults(
                """
                using Reinterop;

                namespace TestNamespace
                {
                    public class Foo
                    {
                        public int Value { get; set; }
                    }

                    [Reinterop]
                    internal class ConfigureReinterop
                    {
                        public void ExposeToCPP()
                        {
                            Foo foo = new Foo();
                            foo.Value = 1;
                            int v = foo.Value;
                        }
                    }
                }
                """);

            CppInteropFunction getter = results["Foo"].Find("Value", 0);
            CppInteropFunction setter = results["Foo"].Find("Value", 1);

            Assert.That(getter.ReturnType().Name, Is.EqualTo("int32_t"));
            Assert.That(setter.Parameters()[0].Type.Name, Is.EqualTo("int32_t"));
            Assert.That(setter.ReturnType().Name, Is.EqualTo("void"));
        }

        [Test]
        public void StaticProperty_AccessorsAreStatic()
        {
            var results = GenerationTestHelper.GenerateResults(
                """
                using Reinterop;

                namespace TestNamespace
                {
                    public class Foo
                    {
                        public static int Value { get; set; }
                    }

                    [Reinterop]
                    internal class ConfigureReinterop
                    {
                        public void ExposeToCPP()
                        {
                            Foo.Value = 1;
                            int v = Foo.Value;
                        }
                    }
                }
                """);

            CppInteropFunction getter = results["Foo"].Find("Value", 0);
            CppInteropFunction setter = results["Foo"].Find("Value", 1);

            Assert.That(getter.Static(), Is.True);
            Assert.That(setter.Static(), Is.True);
        }

        [Test]
        public void Indexer_GeneratesOnlyGetterAsOperatorSubscript()
        {
            // Properties.cs explicitly skips generating a setter for any indexer
            // ("TODO: support element setters (i.e. obj[0] = x)"), so only a getter is ever produced.
            var results = GenerationTestHelper.GenerateResults(
                """
                using Reinterop;

                namespace TestNamespace
                {
                    public class Foo
                    {
                        public int this[int index] => index;
                    }

                    [Reinterop]
                    internal class ConfigureReinterop
                    {
                        public void ExposeToCPP()
                        {
                            Foo foo = new Foo();
                            int v = foo[0];
                        }
                    }
                }
                """);

            List<CppInteropFunction> recipes = results["Foo"].InteropFunctions.Where(function => function.Name == "operator[]").ToList();

            Assert.That(recipes, Has.Count.EqualTo(1));
            Assert.That(recipes[0].Parameters(), Has.Count.EqualTo(1));
            Assert.That(recipes[0].Static(), Is.False);
        }
    }
}
