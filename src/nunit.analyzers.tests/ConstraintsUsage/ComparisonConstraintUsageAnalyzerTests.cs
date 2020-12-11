using System.Globalization;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.ConstraintUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ConstraintsUsage
{
    public class ComparisonConstraintUsageAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ComparisonConstraintUsageAnalyzer();

        [TestCase(">=", "Is.GreaterThanOrEqualTo")]
        [TestCase(">", "Is.GreaterThan")]
        [TestCase("<=", "Is.LessThanOrEqualTo")]
        [TestCase("<", "Is.LessThan")]
        public void AnalyzeWhenComparisonOperatorUsed(string operatorToken, string message)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                int actual = 5;
                Assert.That(↓actual {operatorToken} 9);");

            var diagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.ComparisonConstraintUsage,
                string.Format(CultureInfo.InvariantCulture, ComparisonConstraintUsageConstants.Message, message));
            RoslynAssert.Diagnostics(analyzer, diagnostic, testCode);
        }

        [TestCase(">=", "Is.GreaterThanOrEqualTo")]
        [TestCase(">", "Is.GreaterThan")]
        [TestCase("<=", "Is.LessThanOrEqualTo")]
        [TestCase("<", "Is.LessThan")]
        public void AnalyzeWhenComparisonOperatorUsedWithIsTrue(string operatorToken, string message)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                int actual = 5;
                Assert.That(↓actual {operatorToken} 9, Is.True);");

            var diagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.ComparisonConstraintUsage,
                string.Format(CultureInfo.InvariantCulture, ComparisonConstraintUsageConstants.Message, message));
            RoslynAssert.Diagnostics(analyzer, diagnostic, testCode);
        }

        [TestCase(">=", "Is.LessThan")]
        [TestCase(">", "Is.LessThanOrEqualTo")]
        [TestCase("<=", "Is.GreaterThan")]
        [TestCase("<", "Is.GreaterThanOrEqualTo")]
        public void AnalyzeWhenComparisonOperatorUsedWithIsFalse(string operatorToken, string message)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                int actual = 5;
                Assert.That(↓actual {operatorToken} 9, Is.False);");

            var diagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.ComparisonConstraintUsage,
                string.Format(CultureInfo.InvariantCulture, ComparisonConstraintUsageConstants.Message, message));
            RoslynAssert.Diagnostics(analyzer, diagnostic, testCode);
        }

        [TestCase("Is.GreaterThanOrEqualTo")]
        [TestCase("Is.GreaterThan")]
        [TestCase("Is.LessThanOrEqualTo")]
        [TestCase("Is.LessThan")]
        public void ValidWhenConstraintUsed(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                int actual = 5;
                Assert.That(actual, {constraint}(9));");

            RoslynAssert.Valid(analyzer, testCode);
        }
    }
}
