using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Reinterop.Tests
{
    /// <summary>
    /// Tier 0 tests: run the actual generator (via <see cref="CSharpGeneratorDriver"/>) against
    /// representative input snippets and assert that the resulting compilation has no errors. This
    /// is a broad, cheap safety net proving the generated C# actually compiles, without asserting on
    /// its exact text.
    /// </summary>
    public class RoslynSourceGeneratorTests
    {
        [Test]
        public void PlainMethod()
        {
            AssertGeneratesWithoutErrors(
                """
                using Reinterop;
                using System;

                namespace TestNamespace
                {
                    [Reinterop]
                    internal class ConfigureReinterop
                    {
                        public void ExposeToCPP()
                        {
                            Console.WriteLine("hi");
                        }
                    }
                }
                """);
        }

        [Test]
        public void ConstructorFieldAndProperty()
        {
            AssertGeneratesWithoutErrors(
                """
                using Reinterop;

                namespace TestNamespace
                {
                    public class Foo
                    {
                        public Foo(int initialValue)
                        {
                            Value = initialValue;
                        }

                        public int Value;

                        public int DoubledValue
                        {
                            get { return Value * 2; }
                        }
                    }

                    [Reinterop]
                    internal class ConfigureReinterop
                    {
                        public void ExposeToCPP()
                        {
                            Foo foo = new Foo(1);
                            foo.Value = 2;
                            int doubled = foo.DoubledValue;
                        }
                    }
                }
                """);
        }

        [Test]
        public void Enum()
        {
            AssertGeneratesWithoutErrors(
                """
                using Reinterop;

                namespace TestNamespace
                {
                    public enum Color
                    {
                        Red,
                        Green,
                        Blue
                    }

                    [Reinterop]
                    internal class ConfigureReinterop
                    {
                        public void ExposeToCPP()
                        {
                            Color color = Color.Green;
                        }
                    }
                }
                """);
        }

        [Test]
        public void Inheritance()
        {
            AssertGeneratesWithoutErrors(
                """
                using Reinterop;

                namespace TestNamespace
                {
                    public class Base
                    {
                        public virtual int GetValue() { return 1; }
                    }

                    public class Derived : Base
                    {
                        public override int GetValue() { return 2; }
                    }

                    [Reinterop]
                    internal class ConfigureReinterop
                    {
                        public void ExposeToCPP()
                        {
                            Derived derived = new Derived();
                            Base baseInstance = derived;
                            int value = baseInstance.GetValue();
                        }
                    }
                }
                """);
        }

        private static void AssertGeneratesWithoutErrors(string source)
        {
            // Reinterop writes generated C++ files as a side effect of running - redirect them to a
            // throwaway temp directory instead of wherever the test process happens to be running
            // from, and clean it up afterward regardless of outcome.
            string cppOutputPath = Path.Combine(Path.GetTempPath(), "ReinteropTests", Guid.NewGuid().ToString());
            try
            {
                CSharpCompilation compilation = GenerationTestHelper.CreateCompilation(source);

                GeneratorDriver driver = CSharpGeneratorDriver.Create(
                    new ISourceGenerator[] { new RoslynSourceGenerator() },
                    parseOptions: GenerationTestHelper.ParseOptions,
                    optionsProvider: new TestAnalyzerConfigOptionsProvider(cppOutputPath));
                driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation updatedCompilation, out ImmutableArray<Diagnostic> generatorDiagnostics);

                Diagnostic[] errors = generatorDiagnostics
                    .Concat(updatedCompilation.GetDiagnostics())
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .ToArray();

                Assert.That(errors, Is.Empty, string.Join(Environment.NewLine, errors.Select(e => e.ToString())));
            }
            finally
            {
                if (Directory.Exists(cppOutputPath))
                    Directory.Delete(cppOutputPath, recursive: true);
            }
        }

        private class TestAnalyzerConfigOptionsProvider : Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptionsProvider
        {
            public TestAnalyzerConfigOptionsProvider(string cppOutputPath)
            {
                GlobalOptions = new TestAnalyzerConfigOptions(cppOutputPath);
            }

            public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GlobalOptions { get; }

            public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GetOptions(SyntaxTree tree) => GlobalOptions;

            public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GetOptions(AdditionalText textFile) => GlobalOptions;
        }

        private class TestAnalyzerConfigOptions : Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions
        {
            private readonly string _cppOutputPath;

            public TestAnalyzerConfigOptions(string cppOutputPath)
            {
                _cppOutputPath = cppOutputPath;
            }

            public override bool TryGetValue(string key, out string value)
            {
                if (key == "cpp_output_path")
                {
                    value = _cppOutputPath;
                    return true;
                }

                value = null!;
                return false;
            }
        }
    }
}
