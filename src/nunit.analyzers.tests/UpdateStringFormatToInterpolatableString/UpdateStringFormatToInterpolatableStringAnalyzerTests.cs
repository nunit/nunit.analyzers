using System.Collections.Generic;
using System.Linq;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.UpdateStringFormatToInterpolatableString;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.UpdateStringFormatToInterpolatableString
{
    [TestFixture]
    public sealed class UpdateStringFormatToInterpolatableStringAnalyzerTests
    {
        private readonly DiagnosticAnalyzer analyzer = new UpdateStringFormatToInterpolatableStringAnalyzer();
        private readonly ExpectedDiagnostic diagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.UpdateStringFormatToInterpolatableString);

        private static readonly string[] AssertAndAssume = new[]
        {
            NUnitFrameworkConstants.NameOfAssert,
            NUnitFrameworkConstants.NameOfAssume,
        };

        private static IEnumerable<string> NoArgumentsAsserts { get; } = new[]
        {
            NUnitFrameworkConstants.NameOfAssertPass,
            NUnitFrameworkConstants.NameOfAssertFail,
            NUnitFrameworkConstants.NameOfAssertIgnore,
            NUnitFrameworkConstants.NameOfAssertInconclusive,
        };

        private static IEnumerable<string> OneArgumentsAsserts { get; } = NoArgumentsAsserts.Concat(new[] { NUnitFrameworkConstants.NameOfAssertWarn });

        [TestCaseSource(nameof(NoArgumentsAsserts))]
        public void AnalyzeWhenNoArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                Assert.{method}();
            ");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [TestCaseSource(nameof(OneArgumentsAsserts))]
        public void AnalyzeWhenOnlyMessageArgumentIsUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                Assert.{method}(""Message"");
            ");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

#if !NUNIT4
        [TestCaseSource(nameof(OneArgumentsAsserts))]
        public void AnalyzeWhenFormatAndArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                ↓Assert.{method}(""Method: {{0}}"", ""{method}"");
            ");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [TestCaseSource(nameof(OneArgumentsAsserts))]
        public void AnalyzeWhenFormatAndArrayArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                var values = new object[] {{ ""{method}"" }};
                ↓Assert.{method}(""Method: {{0}}"", values);
            ");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }
#endif

        [TestCaseSource(nameof(OneArgumentsAsserts))]
        public void AnalyzeWhenFormattableStringIsUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                Assert.{method}($""Method: {{""method""}}"");
            ");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public void AnalyzeAssertBoolWhenNoArgumentsAreUsed(string assertOrAssume)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                {assertOrAssume}.That(true);
            ");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public void AnalyzeAssertBoolWhenOnlyMessageArgumentIsUsed(string assertOrAssume)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                {assertOrAssume}.That(false, ""Message"");
            ");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public void AnalyzeAssertBoolWhenFormatAndArgumentsAreUsed(string assertOrAssume)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                ↓{assertOrAssume}.That(false, ""Method: {{0}}"", false.ToString());
            ");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public void AnalyzeAssertBoolWhenFormattableStringIsUsed(string assertOrAssume)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                {assertOrAssume}.That(false, $""Method: {{false}}"");
            ");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public void AnalyzeAssertThatWhenNoArgumentsAreUsed(string assertOrAssume)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                double pi = 3.1415;
                {assertOrAssume}.That(pi, Is.EqualTo(3.1415).Within(0.0001));
            ");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public void AnalyzeAssertThatWhenOnlyMessageArgumentIsUsed(string assertOrAssume)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                double pi = 3.1415;
                {assertOrAssume}.That(pi, Is.EqualTo(3.1415).Within(0.0001), ""Message"");
            ");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public void AnalyzeAssertThatWhenFormatAndStringArgumentsAreUsed(string assertOrAssume)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
        [TestCase(""NUnit 4.0"", ""NUnit 3.14"")]
        public void AssertSomething(string actual, string expected)
        {{
            ↓{assertOrAssume}.That(actual, Is.EqualTo(expected).IgnoreCase, ""Expected '{{0}}', but got: {{1}}"", expected, actual);
        }}");

            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

#if !NUNIT4
        [TestCaseSource(nameof(AssertAndAssume))]
        public void AnalyzeAssertThatWhenFormatAndArgumentsAreUsed(string assertOrAssume)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                double pi = 3.1415;
                ↓{assertOrAssume}.That(pi, Is.EqualTo(3.1415).Within(0.0001), ""Method: {{0}}"", pi);
            ");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }
#endif

        [TestCaseSource(nameof(AssertAndAssume))]
        public void AnalyzeAssertThatWhenFormatStringIsUsed(string assertOrAssume)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                double pi = 3.1415;
                {assertOrAssume}.That(pi, Is.EqualTo(3.1415).Within(0.0001), $""Method: {{pi}}"");
            ");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeAssertDoesNotThrowWhenFormatAndArgumentsAreUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                Assert.DoesNotThrow(() => SomeOperation(), ""Expected no exception to be thrown by {{0}}"", nameof(SomeOperation));

                static void SomeOperation()
                {{
                }}
            ");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeAssertThrowsWhenFormatAndArgumentsAreUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                Assert.Throws<InvalidOperationException>(() => SomeOperation(),
                    ""Expected an InvalidOperationException exception to be thrown by {{0}}"", nameof(SomeOperation));

                static void SomeOperation()
                {{
                }}
            ");
            RoslynAssert.Valid(this.analyzer, testCode);
        }
    }
}
