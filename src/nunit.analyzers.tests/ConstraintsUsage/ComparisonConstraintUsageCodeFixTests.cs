using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        private static readonly Dictionary<string, string> operatorTokensToConstraints = new()
        {
            { ">=", "Is.GreaterThanOrEqualTo" },
            { ">", "Is.GreaterThan" },
            { "<=", "Is.LessThanOrEqualTo" },
            { "<", "Is.LessThan" },
        };
        private static readonly string[] operatorTokens = operatorTokensToConstraints.Keys.ToArray();
        private static readonly Dictionary<string, string> operatorTokensToConstraintsReversed = new()
        {
            { ">=", "Is.LessThan" },
            { ">", "Is.LessThanOrEqualTo" },
            { "<=", "Is.GreaterThan" },
            { "<", "Is.GreaterThanOrEqualTo" },
        };

        [Test]
        public void FixesComparisonOperator([ValueSource(nameof(operatorTokens))] string operatorToken)
        {
            var constraint = operatorTokensToConstraints[operatorToken];
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

        [Test]
        public void FixesComparisonOperatorWithIsTrue([ValueSource(nameof(operatorTokens))] string operatorToken)
        {
            var constraint = operatorTokensToConstraints[operatorToken];
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

        [Test]
        public void FixesWhenComparisonOperatorUsedWithIsFalse([ValueSource(nameof(operatorTokens))] string operatorToken)
        {
            var constraint = operatorTokensToConstraintsReversed[operatorToken];
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

        [Test]
        public void FixesWhenComparisonOperatorUseConstantOnLeftHandSide([ValueSource(nameof(operatorTokens))] string operatorToken)
        {
            var constraint = operatorTokensToConstraintsReversed[operatorToken];
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

        [Test]
        public void CodeFixMaintainsReasonableTriviaWithEndOfLineClosingParen([ValueSource(nameof(operatorTokens))] string operatorToken)
        {
            var constraint = operatorTokensToConstraintsReversed[operatorToken];
            var code = TestUtility.WrapInTestMethod(@$"
            int actual = 5;
            Assert.That(
                ↓actual {operatorToken} 9,
                Is.False);");

            var fixedCode = TestUtility.WrapInTestMethod(@$"
            int actual = 5;
            Assert.That(
                actual,
                {constraint}(9));");

            var diagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.ComparisonConstraintUsage,
                string.Format(CultureInfo.InvariantCulture, ComparisonConstraintUsageConstants.Message, constraint));

            RoslynAssert.CodeFix(analyzer, fix, diagnostic, code, fixedCode);
        }

        [Test]
        public void CodeFixMaintainsReasonableTriviaWithNewLineClosingParen([ValueSource(nameof(operatorTokens))] string operatorToken)
        {
            var constraint = operatorTokensToConstraints[operatorToken];
            var code = TestUtility.WrapInTestMethod(@$"
            int actual = 5;
            Assert.That(
                ↓actual {operatorToken} 9
            );");

            var fixedCode = TestUtility.WrapInTestMethod(@$"
            int actual = 5;
            Assert.That(
                actual,
                {constraint}(9)
            );");

            var diagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.ComparisonConstraintUsage,
                string.Format(CultureInfo.InvariantCulture, ComparisonConstraintUsageConstants.Message, constraint));

            RoslynAssert.CodeFix(analyzer, fix, diagnostic, code, fixedCode);
        }

        [Test]
        public void CodeFixMaintainsReasonableTriviaWithAllArgumentsOnSameLine(
            [ValueSource(nameof(operatorTokens))] string operatorToken,
            [Values] bool newlineBeforeClosingParen)
        {
            var optionalNewline = newlineBeforeClosingParen ? "\r\n            " : string.Empty;
            var constraint = operatorTokensToConstraints[operatorToken];
            var code = TestUtility.WrapInTestMethod(@$"
            int actual = 5;
            Assert.That(
                ↓actual {operatorToken} 9, ""message""{optionalNewline});");

            var fixedCode = TestUtility.WrapInTestMethod(@$"
            int actual = 5;
            Assert.That(
                actual, {constraint}(9), ""message""{optionalNewline});");

            var diagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.ComparisonConstraintUsage,
                string.Format(CultureInfo.InvariantCulture, ComparisonConstraintUsageConstants.Message, constraint));

            RoslynAssert.CodeFix(analyzer, fix, diagnostic, code, fixedCode);
        }
    }
}
