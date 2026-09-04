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

            CSharpFunctionCallableFromCpp recipe = results["Foo"].Find("Add", 1);
            Assert.That(recipe.NeedsStructReturnRewrite, Is.False);

            var functions = recipe.CreatePairedInteropFunctions();
            
            CppFunction cpp = functions.cpp;
            Assert.That(cpp.Static(), Is.False);
            Assert.That(cpp.Private(), Is.False);
            Assert.That(cpp.Parameters()[0].Name, Is.EqualTo("x"));
            Assert.That(cpp.Parameters()[0].Type.Name, Is.EqualTo("int32_t"));
            Assert.That(cpp.ReturnType().Name, Is.EqualTo("int32_t"));

            CSharpFunction csharp = functions.csharp;
            Assert.That(csharp.Static(), Is.True);
            Assert.That(csharp.Private(), Is.True);
            Assert.That(csharp.Parameters()[0].Name, Is.EqualTo("thiz"));
            Assert.That(csharp.Parameters()[0].Type.Name, Is.EqualTo("IntPtr"));
            Assert.That(csharp.Parameters()[1].Name, Is.EqualTo("x"));
            Assert.That(csharp.Parameters()[1].Type.Name, Is.EqualTo("Int32"));
            Assert.That(csharp.ReturnType().Name, Is.EqualTo("Int32"));
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

            CSharpFunctionCallableFromCpp recipe = results["Foo"].Find("DoubleIt", 1);

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

            List<CSharpFunctionCallableFromCpp> identityRecipes = results["Foo"].InteropFunctions2
                .Where(function => function.Name() == "Identity")
                .ToList();

            // One specialization per instantiation, each specializing the same shared template declaration
            // (added once to InteropFunctions3, cached by GenerateTypeState.MethodCache despite two calls).
            Assert.That(identityRecipes, Has.Count.EqualTo(2));
            Assert.That(identityRecipes, Has.All.Matches<CSharpFunctionCallableFromCpp>(function => function.Specializes() != null));

            CppFunction template = identityRecipes[0].Specializes()!;
            Assert.That(identityRecipes, Has.All.Matches<CSharpFunctionCallableFromCpp>(function => function.Specializes() == template));
            Assert.That(results["Foo"].InteropFunctions3, Has.Member(template));

            Assert.That(
                identityRecipes.Select(function => function.TypeArguments().Single().Name),
                Is.EquivalentTo(new[] { "Int32", "Boolean" }));
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

            CSharpFunctionCallableFromCpp interopRecipe = results["Foo"].Find("op_Equality", 2);
            CppFunction operatorRecipe = results["Foo"].InteropFunctions3.Single(function => function.Name == "operator==");

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

            CSharpFunctionCallableFromCpp recipe = results["Foo"].Find("GetPosition", 0);
            var functions = recipe.CreatePairedInteropFunctions();

            Assert.That(recipe.NeedsStructReturnRewrite, Is.True);
            Assert.That(functions.csharp.ReturnType().Name, Is.EqualTo("Void"));
            Assert.That(functions.cpp.ReturnType().Name, Is.EqualTo("Vector2"));
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

            CSharpFunctionCallableFromCpp recipe = results["Foo"].Find("MaybeGetValue", 0);
            var functions = recipe.CreatePairedInteropFunctions();

            Assert.That(recipe.NeedsStructReturnRewrite, Is.True);
            Assert.That(functions.csharp.ReturnType().Name, Is.EqualTo("Byte"));
            Assert.That(functions.cpp.ReturnType().GetFullyQualifiedName(), Is.EqualTo("::std::optional<::std::int32_t>"));
        }

        [Test]
        public void MethodReturningNullableNonBlittableStruct_DoesNotRewriteReturn()
        {
            var results = GenerationTestHelper.GenerateResults(
                """
                using Reinterop;

                namespace TestNamespace
                {
                    public struct NonBlittable
                    {
                        public string Value;
                    }

                    public class Foo
                    {
                        public NonBlittable? MaybeGetValue() { return null; }
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

            CSharpFunctionCallableFromCpp recipe = results["Foo"].Find("MaybeGetValue", 0);

            Assert.That(recipe.NeedsStructReturnRewrite, Is.False);
        }
    }
}
