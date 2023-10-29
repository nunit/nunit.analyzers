using System.Globalization;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.ConstraintUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ConstraintsUsage
{
    public class ComparisonConstraintUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ComparisonConstraintUsageAnalyzer();
        private static readonly CodeFixProvider fix = new ComparisonConstraintUsageCodeFix();

        [TestCase(">=", "Is.GreaterThanOrEqualTo")]
        [TestCase(">", "Is.GreaterThan")]
        [TestCase("<=", "Is.LessThanOrEqualTo")]
        [TestCase("<", "Is.LessThan")]
        public void FixesComparisonOperator(string operatorToken, string constraint)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            int actual = 5;
            Assert.That(↓actual {operatorToken} 9);");

            var fixedCode = TestUtility.WrapInTestMethod(@$"
            int actual = 5;
            Assert.That(actual, {constraint}(9));");

            var diagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.ComparisonConstraintUsage,
                string.Format(CultureInfo.InvariantCulture, ComparisonConstraintUsageConstants.Message, constraint));

            RoslynAssert.CodeFix(analyzer, fix, diagnostic, code, fixedCode);
        }

        [TestCase(">=", "Is.GreaterThanOrEqualTo")]
        [TestCase(">", "Is.GreaterThan")]
        [TestCase("<=", "Is.LessThanOrEqualTo")]
        [TestCase("<", "Is.LessThan")]
        public void FixesComparisonOperatorWithIsTrue(string operatorToken, string constraint)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            int actual = 5;
            Assert.That(↓actual {operatorToken} 9, Is.True);");

            var fixedCode = TestUtility.WrapInTestMethod(@$"
            int actual = 5;
            Assert.That(actual, {constraint}(9));");

            var diagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.ComparisonConstraintUsage,
                string.Format(CultureInfo.InvariantCulture, ComparisonConstraintUsageConstants.Message, constraint));

            RoslynAssert.CodeFix(analyzer, fix, diagnostic, code, fixedCode);
        }

        [TestCase(">=", "Is.LessThan")]
        [TestCase(">", "Is.LessThanOrEqualTo")]
        [TestCase("<=", "Is.GreaterThan")]
        [TestCase("<", "Is.GreaterThanOrEqualTo")]
        public void FixesWhenComparisonOperatorUsedWithIsFalse(string operatorToken, string constraint)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            int actual = 5;
            Assert.That(↓actual {operatorToken} 9, Is.False);");

            var fixedCode = TestUtility.WrapInTestMethod(@$"
            int actual = 5;
            Assert.That(actual, {constraint}(9));");

            var diagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.ComparisonConstraintUsage,
                string.Format(CultureInfo.InvariantCulture, ComparisonConstraintUsageConstants.Message, constraint));

            RoslynAssert.CodeFix(analyzer, fix, diagnostic, code, fixedCode);
        }

        [TestCase(">=", "Is.LessThan")]
        [TestCase(">", "Is.LessThanOrEqualTo")]
        [TestCase("<=", "Is.GreaterThan")]
        [TestCase("<", "Is.GreaterThanOrEqualTo")]
        public void FixesWhenComparisonOperatorUseConstantOnLeftHandSide(string operatorToken, string constraint)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            int actual = 5;
            Assert.That(↓9 {operatorToken} actual, Is.True);");

            var fixedCode = TestUtility.WrapInTestMethod(@$"
            int actual = 5;
            Assert.That(actual, {constraint}(9));");

            var diagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.ComparisonConstraintUsage,
                string.Format(CultureInfo.InvariantCulture, ComparisonConstraintUsageConstants.Message, constraint));

            RoslynAssert.CodeFix(analyzer, fix, diagnostic, code, fixedCode);
        }
    }
}
