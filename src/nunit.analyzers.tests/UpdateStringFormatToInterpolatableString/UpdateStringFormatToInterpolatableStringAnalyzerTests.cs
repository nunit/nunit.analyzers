using System;
using System.Collections.Generic;
using System.Linq;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
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
#if !NUNIT4
        private readonly ExpectedDiagnostic diagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.UpdateStringFormatToInterpolatableString);
#endif

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

        [Test]
        public void AnalyzeAssertBoolWhenNoArgumentsAreUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                Assert.That(true);
            ");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeAssertBoolWhenOnlyMessageArgumentIsUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                Assert.That(false, ""Message"");
            ");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

#if !NUNIT4
        [Test]
        public void AnalyzeAssertBoolWhenFormatAndArgumentsAreUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                ↓Assert.That(false, ""Method: {{0}}"", false.ToString());
            ");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }
#endif

        [Test]
        public void AnalyzeAssertBoolWhenFormattableStringIsUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                Assert.That(false, $""Method: {{false}}"");
            ");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeAssertThatWhenNoArgumentsAreUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                double pi = 3.1415;
                Assert.That(pi, Is.EqualTo(3.1415).Within(0.0001));
            ");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeAssertThatWhenOnlyMessageArgumentIsUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                double pi = 3.1415;
                Assert.That(pi, Is.EqualTo(3.1415).Within(0.0001), ""Message"");
            ");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

#if !NUNIT4
        [Test]
        public void AnalyzeAssertThatWhenFormatAndArgumentsAreUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                double pi = 3.1415;
                ↓Assert.That(pi, Is.EqualTo(3.1415).Within(0.0001), ""Method: {{0}}"", pi);
            ");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }
#endif

        [Test]
        public void AnalyzeAssertThatWhenFormatStringIsUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                double pi = 3.1415;
                Assert.That(pi, Is.EqualTo(3.1415).Within(0.0001), $""Method: {{pi}}"");
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
