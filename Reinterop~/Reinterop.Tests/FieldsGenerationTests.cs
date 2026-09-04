using NUnit.Framework;

namespace Reinterop.Tests
{
    /// <summary>
    /// Recipe-level tests for Fields.cs: run the generator directly and assert on the resulting
    /// <see cref="CSharpFunctionCallableFromCpp"/> recipes.
    /// </summary>
    public class FieldsGenerationTests
    {
        [Test]
        public void InstanceField_GeneratesGetterAndSetterRecipes()
        {
            var results = GenerationTestHelper.GenerateResults(
                """
                using Reinterop;

                namespace TestNamespace
                {
                    public class Foo
                    {
                        public int Value;
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

            CSharpFunctionCallableFromCpp getter = results["Foo"].Find("Value", 0);
            CSharpFunctionCallableFromCpp setter = results["Foo"].Find("Value", 1);

            Assert.That(getter.ReturnType().Name, Is.EqualTo("Int32"));
            Assert.That(getter.NeedsStructReturnRewrite, Is.False);
            Assert.That(setter.Parameters()[0].Name, Is.EqualTo("value"));
            Assert.That(setter.Parameters()[0].Type.Name, Is.EqualTo("Int32"));
            Assert.That(getter.Body(), Is.InstanceOf<CSharpFunctionCallableFromCpp.CSharpBodyInvokeFieldAccessor>());

            // Fields.cs never marks a field accessor recipe as Private() - unlike CSharpTypeUtility's
            // notion of field privacy (only used for the raw blittable-struct field declaration), an
            // accessor pair is always public regardless of the underlying C# field's accessibility.
            Assert.That(getter.Private(), Is.False);
            Assert.That(setter.Private(), Is.False);
        }

        [Test]
        public void BlittableStructField_GetterNeedsVoidReturnRewrite()
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
                        public Vector2 Position;
                    }

                    [Reinterop]
                    internal class ConfigureReinterop
                    {
                        public void ExposeToCPP()
                        {
                            Foo foo = new Foo();
                            foo.Position = new Vector2();
                            Vector2 p = foo.Position;
                        }
                    }
                }
                """);

            CSharpFunctionCallableFromCpp getter = results["Foo"].Find("Position", 0);
            var functions = getter.CreatePairedInteropFunctions();

            Assert.That(getter.NeedsStructReturnRewrite, Is.True);
            Assert.That(functions.functionPointer.ReturnType().Name, Is.EqualTo("void"));
        }

        [Test]
        public void NonBlittableStructField_AccessorsReboxTheReceiver()
        {
            var results = GenerationTestHelper.GenerateResults(
                """
                using Reinterop;

                namespace TestNamespace
                {
                    public struct Foo
                    {
                        public string Value;
                    }

                    [Reinterop]
                    internal class ConfigureReinterop
                    {
                        public void ExposeToCPP()
                        {
                            Foo foo = new Foo();
                            foo.Value = "value";
                            string value = foo.Value;
                        }
                    }
                }
                """);

            List<GeneratedInitFunction> accessors = results["Foo"].Init.Functions.Where(function => function.CSharpName.Contains("_Value_")).ToList();

            Assert.That(accessors, Has.Count.EqualTo(2));
            Assert.That(accessors, Has.All.Matches<GeneratedInitFunction>(function => function.CSharpContent.Contains("thizUnboxed")));
            Assert.That(accessors, Has.All.Matches<GeneratedInitFunction>(function => function.CSharpContent.Contains("ResetHandleObject(thiz, thizUnboxed)")));
        }
    }
}
