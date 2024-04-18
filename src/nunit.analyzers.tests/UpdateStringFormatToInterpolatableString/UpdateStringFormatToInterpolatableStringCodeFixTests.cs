using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.UpdateStringFormatToInterpolatableString;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.UpdateStringFormatToInterpolatableString
{
    [TestFixture]
    public sealed class UpdateStringFormatToInterpolatableStringCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new UpdateStringFormatToInterpolatableStringAnalyzer();
        private static readonly CodeFixProvider fix = new UpdateStringFormatToInterpolatableStringCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.UpdateStringFormatToInterpolatableString);

        private static readonly string[] AssertAndAssume = new[]
        {
            NUnitFrameworkConstants.NameOfAssert,
            NUnitFrameworkConstants.NameOfAssume,
        };

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.UpdateStringFormatToInterpolatableString }));
        }

#if NUNIT4
        [TestCaseSource(nameof(AssertAndAssume))]
        public void AccidentallyUseFormatSpecification(string assertOrAssume)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
        [TestCase(""NUnit 4.0"", ""NUnit 3.14"")]
        public void {assertOrAssume}Something(string actual, string expected)
        {{
            ↓{assertOrAssume}.That(actual, Is.EqualTo(expected).IgnoreCase, ""Expected '{{0}}', but got: {{1}}"", expected, actual);
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
        [TestCase(""NUnit 4.0"", ""NUnit 3.14"")]
        public void {assertOrAssume}Something(string actual, string expected)
        {{
            {assertOrAssume}.That(actual, Is.EqualTo(expected).IgnoreCase, $""Expected '{{expected}}', but got: {{actual}}"");
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public void AccidentallyUseFormatSpecificationWithVariable(string assertOrAssume)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
        [TestCase(""NUnit 4.0"", ""NUnit 3.14"")]
        public void {assertOrAssume}Something(string actual, string expected)
        {{
            const string formatSpecification = ""Expected '{{0}}', but got: {{1}}"";
            ↓{assertOrAssume}.That(actual, Is.EqualTo(expected).IgnoreCase, formatSpecification, expected, actual);
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
        [TestCase(""NUnit 4.0"", ""NUnit 3.14"")]
        public void {assertOrAssume}Something(string actual, string expected)
        {{
            const string formatSpecification = ""Expected '{{0}}', but got: {{1}}"";
            {assertOrAssume}.That(actual, Is.EqualTo(expected).IgnoreCase, () => string.Format(formatSpecification, expected, actual));
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }
#else
        [TestCase(NUnitFrameworkConstants.NameOfAssertIgnore)]
        public void ConvertWhenNoMinimumParameters(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
                ↓Assert.{method}(""Method: {{0}}"", ""{method}"");
            ");
            var fixedCode = TestUtility.WrapInTestMethod($@"
                Assert.{method}($""Method: {{""{method}""}}"");
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCase(NUnitFrameworkConstants.NameOfAssertIgnore)]
        public void ConvertWhenNoMinimumParametersAndArgsArray(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
                object[] args = {{ ""{method}"" }};
                ↓Assert.{method}(""Method: {{0}}"", args);
            ");
            var fixedCode = TestUtility.WrapInTestMethod($@"
                object[] args = {{ ""{method}"" }};
                Assert.{method}(string.Format(""Method: {{0}}"", args));
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCase(NUnitFrameworkConstants.NameOfAssertIgnore)]
        public void ConvertWhenNoMinimumParametersVariableFormatSpecification(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
                const string formatSpecification = ""Method: {{0}}"";
                ↓Assert.{method}(formatSpecification, ""{method}"");
            ");
            var fixedCode = TestUtility.WrapInTestMethod($@"
                const string formatSpecification = ""Method: {{0}}"";
                Assert.{method}(string.Format(formatSpecification, ""{method}""));
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public void ConvertWhenMinimumParametersIsOne(string assertOrAssume)
        {
            var code = TestUtility.WrapInTestMethod(@$"
                const bool actual = false;
                ↓{assertOrAssume}.That(actual, ""Expected actual value to be true, but was: {{0}}"", actual);
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
                const bool actual = false;
                {assertOrAssume}.That(actual, $""Expected actual value to be true, but was: {{actual}}"");
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public void ConvertWhenMinimumParametersIsOneAndArgsArray(string assertOrAssume)
        {
            var code = TestUtility.WrapInTestMethod(@$"
                const bool actual = false;
                object[] args = {{ actual }};
                ↓{assertOrAssume}.That(actual, ""Expected actual value to be true, but was: {{0}}"", args);
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
                const bool actual = false;
                object[] args = {{ actual }};
                {assertOrAssume}.That(actual, () => string.Format(""Expected actual value to be true, but was: {{0}}"", args));
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public void ConvertWhenMinimumParametersIsOneAndVariableFormatSpecification(string assertOrAssume)
        {
            var code = TestUtility.WrapInTestMethod(@$"
                const bool actual = false;
                const string formatSpecification = ""Expected actual value to be true, but was: {{0}}"";
                ↓{assertOrAssume}.That(actual, formatSpecification, actual);
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
                const bool actual = false;
                const string formatSpecification = ""Expected actual value to be true, but was: {{0}}"";
                {assertOrAssume}.That(actual, () => string.Format(formatSpecification, actual));
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public void ConvertWhenMinimumParametersIsTwo(string assertOrAssume)
        {
            var code = TestUtility.WrapInTestMethod(@$"
                const int actual = 42;
                ↓{assertOrAssume}.That(actual, Is.EqualTo(42), ""Expected actual value to be 42, but was: {{0}} at time {{1:HH:mm:ss}}"", actual, DateTime.Now);
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
                const int actual = 42;
                {assertOrAssume}.That(actual, Is.EqualTo(42), $""Expected actual value to be 42, but was: {{actual}} at time {{DateTime.Now:HH:mm:ss}}"");
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public void ConvertWhenMinimumParametersIsTwoAndArgsArray(string assertOrAssume)
        {
            var code = TestUtility.WrapInTestMethod(@$"
                const int actual = 42;
                object[] args = {{ actual, DateTime.Now }};
                ↓{assertOrAssume}.That(actual, Is.EqualTo(42), ""Expected actual value to be 42, but was: {{0}} at time {{1:HH:mm:ss}}"", args);
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
                const int actual = 42;
                object[] args = {{ actual, DateTime.Now }};
                {assertOrAssume}.That(actual, Is.EqualTo(42), () => string.Format(""Expected actual value to be 42, but was: {{0}} at time {{1:HH:mm:ss}}"", args));
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public void ConvertWhenMinimumParametersIsTwoAndVariableFormatSpecification(string assertOrAssume)
        {
            var code = TestUtility.WrapInTestMethod(@$"
                const int actual = 42;
                const string formatSpecification = ""Expected actual value to be 42, but was: {{0}} at time {{1:HH:mm:ss}}"";
                ↓{assertOrAssume}.That(actual, Is.EqualTo(42), formatSpecification, actual, DateTime.Now);
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
                const int actual = 42;
                const string formatSpecification = ""Expected actual value to be 42, but was: {{0}} at time {{1:HH:mm:ss}}"";
                {assertOrAssume}.That(actual, Is.EqualTo(42), () => string.Format(formatSpecification, actual, DateTime.Now));
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        // We need to double the backslashes as the text is formatted and we want the code to contain \n and not LF
        [TestCase("Passed: {0}")]
        [TestCase("Passed: {0} {{")]
        [TestCase("Passed: {{{0}}}")]
        [TestCase("{{{0}}}")]
        [TestCase("Passed: \\\"{0}\\\"")]
        [TestCase("Passed:\\n{0}")]
        [TestCase("Passed:\\t\\\"{0}\\\"")]
        public void TestConvertWithEmbeddedSpecialCharacters(string text)
        {
            var code = TestUtility.WrapInTestMethod($"↓Assert.Pass(\"{text}\", 42);");

            string interpolatableText = text.Replace("{0}", "{42}");
            var fixedCode = TestUtility.WrapInTestMethod($"Assert.Pass($\"{interpolatableText}\");");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCase("")]
        [TestCase(":N1")]
        [TestCase(",5")]
        [TestCase(",-5")]
        [TestCase(",5:N1")]
        public void TestFormatModifiers(string modifier)
        {
            string argument = "1.23";
            var code = TestUtility.WrapInTestMethod($"↓Assert.Pass(\"{{0{modifier}}}\", {argument});");

            var fixedCode = TestUtility.WrapInTestMethod($"Assert.Pass($\"{{{argument}{modifier}}}\");");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void TestEscapedBraces()
        {
            var code = TestUtility.WrapInTestMethod("↓Assert.Pass(\"{{{0}}}\", 42);");

            var fixedCode = TestUtility.WrapInTestMethod("Assert.Pass($\"{{{42}}}\");");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void TestConditionalExpressionArgument()
        {
            var code = TestUtility.WrapInTestMethod(@"
                string? someValue = ""Value"";
                ↓Assert.Pass(""Value is {0}"", someValue is null ? ""null"" : someValue);
            ");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                string? someValue = ""Value"";
                Assert.Pass($""Value is {(someValue is null ? ""null"" : someValue)}"");
            ");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }
#endif

    }
}
