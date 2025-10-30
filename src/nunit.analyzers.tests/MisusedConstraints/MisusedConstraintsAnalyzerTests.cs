using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.MisusedConstraints;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.MisusedConstraints
{
    public class MisusedConstraintsAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new MisusedConstraintsAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.MisusedConstraints);

        [Test]
        public void AnalyzeIsNotNullOrEmpty()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                string s = string.Empty;
                Assert.That(s, ↓Is.Not.Null.Or.Empty);
            ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeIsNotNullOrEmptyOverMultipleLines()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                string s = string.Empty;
                Assert.That(s, ↓Is.Not.Null
                                  .Or.Empty);
            ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }
    }
}
