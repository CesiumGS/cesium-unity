using NUnit.Framework;

namespace Reinterop.Tests
{
    public class CustomArrayGenerationTests
    {
        [Test]
        public void ArrayConstructionAndItemAssignment_GenerateExpectedCSharp()
        {
            Dictionary<string, GeneratedResult> results = GenerationTestHelper.GenerateResults(
                """
                using Reinterop;

                namespace TestNamespace
                {
                    [Reinterop]
                    internal class ConfigureReinterop
                    {
                        public void ExposeToCPP()
                        {
                            int[] values = new int[1];
                            values[0] = 42;
                        }
                    }
                }
                """,
                [new CustomArrayGenerator()]);

            GeneratedResult arrayResult = results.Values.Single(result => result.InteropFunctions2.Any(function => function.Name() == "Construct_Size"));
            GeneratedInitFunction constructor = arrayResult.Init.Functions.Single(function => function.CSharpName.Contains("_Construct_Size_"));
            GeneratedInitFunction setItem = arrayResult.Init.Functions.Single(function => function.CSharpName.Contains("_Item_"));

            Assert.That(constructor.CSharpContent, Does.Contain("return Reinterop.ObjectHandleUtility.CreateHandle(new System.Int32[size]);"));
            Assert.That(setItem.CSharpContent, Does.Contain("((System.Int32[])Reinterop.ObjectHandleUtility.GetObjectFromHandle(thiz))[index] = value;"));
        }
    }
}