using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.MisusedConstraints;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace NUnit.Analyzers.Tests.MisusedConstraints
{
    public class MisusedConstraintsCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new MisusedConstraintsAnalyzer();
        private static readonly CodeFixProvider fix = new MisusedConstraintsCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.MisusedConstraints);

        [Test]
        public void AnalyzeIsNotNullOrEmpty()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                string s = string.Empty;
                Assert.That(s, ↓Is.Not.Null.Or.Empty);
            ");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                string s = string.Empty;
                Assert.That(s, Is.Not.Null.And.Not.Empty);
            ");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, testCode, fixedCode);
        }
    }
}
