using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.UpdateStringFormatToInterpolatableString;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.UpdateStringFormatToInterpolatableString.UpdateStringFormatToInterpolatableStringAnalyzer, NUnit.Analyzers.UpdateStringFormatToInterpolatableString.UpdateStringFormatToInterpolatableStringCodeFix>;

namespace NUnit.Analyzers.Tests.UpdateStringFormatToInterpolatableString
{
    [TestFixture]
    public sealed class UpdateStringFormatToInterpolatableStringCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.UpdateStringFormatToInterpolatableString);

        private static readonly string[] AssertAndAssume =
        [
            NUnitFrameworkConstants.NameOfAssert,
            NUnitFrameworkConstants.NameOfAssume,
        ];

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var ids = new UpdateStringFormatToInterpolatableStringCodeFix().FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.UpdateStringFormatToInterpolatableString }));
        }

#if NUNIT4
        [TestCaseSource(nameof(AssertAndAssume))]
        public async Task AccidentallyUseFormatSpecification(string assertOrAssume)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase("NUnit 4.0", "NUnit 3.14")]
                    public void {{assertOrAssume}}Something(string actual, string expected)
                    {
                        {|#0:{{assertOrAssume}}.That(actual, Is.EqualTo(expected).IgnoreCase, "Expected '{0}', but got: {1}", expected, actual)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase("NUnit 4.0", "NUnit 3.14")]
                    public void {{assertOrAssume}}Something(string actual, string expected)
                    {
                        {{assertOrAssume}}.That(actual, Is.EqualTo(expected).IgnoreCase, $"Expected '{expected}', but got: {actual}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public async Task AccidentallyUseFormatSpecificationWithVariable(string assertOrAssume)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase("NUnit 4.0", "NUnit 3.14")]
                    public void {{assertOrAssume}}Something(string actual, string expected)
                    {
                        const string formatSpecification = "Expected '{0}', but got: {1}";
                        {|#0:{{assertOrAssume}}.That(actual, Is.EqualTo(expected).IgnoreCase, formatSpecification, expected, actual)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase("NUnit 4.0", "NUnit 3.14")]
                    public void {{assertOrAssume}}Something(string actual, string expected)
                    {
                        const string formatSpecification = "Expected '{0}', but got: {1}";
                        {{assertOrAssume}}.That(actual, Is.EqualTo(expected).IgnoreCase, () => string.Format(formatSpecification, expected, actual));
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }
#else
        [TestCase(NUnitFrameworkConstants.NameOfAssertIgnore)]
        public async Task ConvertWhenNoMinimumParameters(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        {|#0:Assert.{{method}}("Method: {0}", "{{method}}")|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.{{method}}($"Method: {"{{method}}"}");

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCase(NUnitFrameworkConstants.NameOfAssertIgnore)]
        public async Task ConvertWhenNoMinimumParametersAndArgsArray(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        object[] args = { "{{method}}" };
                        {|#0:Assert.{{method}}("Method: {0}", args)|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        object[] args = { "{{method}}" };
                        Assert.{{method}}(string.Format("Method: {0}", args));

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCase(NUnitFrameworkConstants.NameOfAssertIgnore)]
        public async Task ConvertWhenNoMinimumParametersVariableFormatSpecification(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        const string formatSpecification = "Method: {0}";
                        {|#0:Assert.{{method}}(formatSpecification, "{{method}}")|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        const string formatSpecification = "Method: {0}";
                        Assert.{{method}}(string.Format(formatSpecification, "{{method}}"));

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public async Task ConvertWhenMinimumParametersIsOne(string assertOrAssume)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        const bool actual = false;
                        {|#0:{{assertOrAssume}}.That(actual, "Expected actual value to be true, but was: {0}", actual)|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        const bool actual = false;
                        {{assertOrAssume}}.That(actual, $"Expected actual value to be true, but was: {actual}");

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public async Task ConvertWhenMinimumParametersIsOneAndArgsArray(string assertOrAssume)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        const bool actual = false;
                        object[] args = { actual };
                        {|#0:{{assertOrAssume}}.That(actual, "Expected actual value to be true, but was: {0}", args)|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        const bool actual = false;
                        object[] args = { actual };
                        {{assertOrAssume}}.That(actual, () => string.Format("Expected actual value to be true, but was: {0}", args));

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public async Task ConvertWhenMinimumParametersIsOneAndVariableFormatSpecification(string assertOrAssume)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        const bool actual = false;
                        const string formatSpecification = "Expected actual value to be true, but was: {0}";
                        {|#0:{{assertOrAssume}}.That(actual, formatSpecification, actual)|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        const bool actual = false;
                        const string formatSpecification = "Expected actual value to be true, but was: {0}";
                        {{assertOrAssume}}.That(actual, () => string.Format(formatSpecification, actual));

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public async Task ConvertWhenMinimumParametersIsTwo(string assertOrAssume)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        const int actual = 42;
                        {|#0:{{assertOrAssume}}.That(actual, Is.EqualTo(42), "Expected actual value to be 42, but was: {0} at time {1:HH:mm:ss}", actual, DateTime.Now)|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        const int actual = 42;
                        {{assertOrAssume}}.That(actual, Is.EqualTo(42), $"Expected actual value to be 42, but was: {actual} at time {DateTime.Now:HH:mm:ss}");

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public async Task ConvertWhenMinimumParametersIsTwoAndArgsArray(string assertOrAssume)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        const int actual = 42;
                        object[] args = { actual, DateTime.Now };
                        {|#0:{{assertOrAssume}}.That(actual, Is.EqualTo(42), "Expected actual value to be 42, but was: {0} at time {1:HH:mm:ss}", args)|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        const int actual = 42;
                        object[] args = { actual, DateTime.Now };
                        {{assertOrAssume}}.That(actual, Is.EqualTo(42), () => string.Format("Expected actual value to be 42, but was: {0} at time {1:HH:mm:ss}", args));

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public async Task ConvertWhenMinimumParametersIsTwoAndVariableFormatSpecification(string assertOrAssume)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        const int actual = 42;
                        const string formatSpecification = "Expected actual value to be 42, but was: {0} at time {1:HH:mm:ss}";
                        {|#0:{{assertOrAssume}}.That(actual, Is.EqualTo(42), formatSpecification, actual, DateTime.Now)|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        const int actual = 42;
                        const string formatSpecification = "Expected actual value to be 42, but was: {0} at time {1:HH:mm:ss}";
                        {{assertOrAssume}}.That(actual, Is.EqualTo(42), () => string.Format(formatSpecification, actual, DateTime.Now));

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        // We need to double the backslashes as the text is formatted and we want the code to contain \n and not LF
        [TestCase("Passed: {0}")]
        [TestCase("Passed: {0} {{")]
        [TestCase("Passed: {{{0}}}")]
        [TestCase("{{{0}}}")]
        [TestCase("Passed: \\\"{0}\\\"")]
        [TestCase("Passed:\\n{0}")]
        [TestCase("Passed:\\t\\\"{0}\\\"")]
        public async Task TestConvertWithEmbeddedSpecialCharacters(string text)
        {
            var code = TestUtility.WrapInTestMethod($"{{|#0:Assert.Pass(\"{text}\", 42)|}};");

            string interpolatableText = text.Replace("{0}", "{42}");
            var fixedCode = TestUtility.WrapInTestMethod($"Assert.Pass($\"{interpolatableText}\");");

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCase("")]
        [TestCase(":N1")]
        [TestCase(",5")]
        [TestCase(",-5")]
        [TestCase(",5:N1")]
        public async Task TestFormatModifiers(string modifier)
        {
            string argument = "1.23";
            var code = TestUtility.WrapInTestMethod($"{{|#0:Assert.Pass(\"{{0{modifier}}}\", {argument})|}};");

            var fixedCode = TestUtility.WrapInTestMethod($"Assert.Pass($\"{{{argument}{modifier}}}\");");

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task TestEscapedBraces()
        {
            var code = TestUtility.WrapInTestMethod("{|#0:Assert.Pass(\"{{{0}}}\", 42)|};");

            var fixedCode = TestUtility.WrapInTestMethod("Assert.Pass($\"{{{42}}}\");");

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task TestConditionalExpressionArgument()
        {
            var code = TestUtility.WrapInTestMethod("""
                        string? someValue = "Value";
                        {|#0:Assert.Pass("Value is {0}", someValue is null ? "null" : someValue)|};

            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        string? someValue = "Value";
                        Assert.Pass($"Value is {(someValue is null ? "null" : someValue)}");

            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }
#endif

    }
}
