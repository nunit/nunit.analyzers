using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.ConstraintUsage;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.ConstraintUsage.ComparisonConstraintUsageAnalyzer, NUnit.Analyzers.ConstraintUsage.ComparisonConstraintUsageCodeFix>;

namespace NUnit.Analyzers.Tests.ConstraintsUsage
{
    public class ComparisonConstraintUsageCodeFixTests
    {
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
        public async Task FixesComparisonOperator([ValueSource(nameof(operatorTokens))] string operatorToken)
        {
            var constraint = operatorTokensToConstraints[operatorToken];
            var code = TestUtility.WrapInTestMethod($$"""
                        int actual = 5;
                        Assert.That({|#0:actual {{operatorToken}} 9|});
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        int actual = 5;
                        Assert.That(actual, {{constraint}}(9));
            """);

            var diagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ComparisonConstraintUsage).WithMessage(string.Format(CultureInfo.InvariantCulture, ComparisonConstraintUsageConstants.Message, constraint));

            await Verify.VerifyCodeFixAsync(code, diagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task FixesComparisonOperatorWithIsTrue([ValueSource(nameof(operatorTokens))] string operatorToken)
        {
            var constraint = operatorTokensToConstraints[operatorToken];
            var code = TestUtility.WrapInTestMethod($$"""
                        int actual = 5;
                        Assert.That({|#0:actual {{operatorToken}} 9|}, Is.True);
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        int actual = 5;
                        Assert.That(actual, {{constraint}}(9));
            """);

            var diagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ComparisonConstraintUsage).WithMessage(string.Format(CultureInfo.InvariantCulture, ComparisonConstraintUsageConstants.Message, constraint));

            await Verify.VerifyCodeFixAsync(code, diagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task FixesWhenComparisonOperatorUsedWithIsFalse([ValueSource(nameof(operatorTokens))] string operatorToken)
        {
            var constraint = operatorTokensToConstraintsReversed[operatorToken];
            var code = TestUtility.WrapInTestMethod($$"""
                        int actual = 5;
                        Assert.That({|#0:actual {{operatorToken}} 9|}, Is.False);
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        int actual = 5;
                        Assert.That(actual, {{constraint}}(9));
            """);

            var diagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ComparisonConstraintUsage).WithMessage(string.Format(CultureInfo.InvariantCulture, ComparisonConstraintUsageConstants.Message, constraint));

            await Verify.VerifyCodeFixAsync(code, diagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task FixesWhenComparisonOperatorUseConstantOnLeftHandSide([ValueSource(nameof(operatorTokens))] string operatorToken)
        {
            var constraint = operatorTokensToConstraintsReversed[operatorToken];
            var code = TestUtility.WrapInTestMethod($$"""
                        int actual = 5;
                        Assert.That({|#0:9 {{operatorToken}} actual|}, Is.True);
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        int actual = 5;
                        Assert.That(actual, {{constraint}}(9));
            """);

            var diagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ComparisonConstraintUsage).WithMessage(string.Format(CultureInfo.InvariantCulture, ComparisonConstraintUsageConstants.Message, constraint));

            await Verify.VerifyCodeFixAsync(code, diagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWithEndOfLineClosingParen([ValueSource(nameof(operatorTokens))] string operatorToken)
        {
            var constraint = operatorTokensToConstraintsReversed[operatorToken];
            var code = TestUtility.WrapInTestMethod($$"""
                        int actual = 5;
                        Assert.That(
                            {|#0:actual {{operatorToken}} 9|},
                            Is.False);
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        int actual = 5;
                        Assert.That(
                            actual,
                            {{constraint}}(9));
            """);

            var diagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ComparisonConstraintUsage).WithMessage(string.Format(CultureInfo.InvariantCulture, ComparisonConstraintUsageConstants.Message, constraint));

            await Verify.VerifyCodeFixAsync(code, diagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWithNewLineClosingParen([ValueSource(nameof(operatorTokens))] string operatorToken)
        {
            var constraint = operatorTokensToConstraints[operatorToken];
            var code = TestUtility.WrapInTestMethod($$"""
                        int actual = 5;
                        Assert.That(
                            {|#0:actual {{operatorToken}} 9|}
                        );
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        int actual = 5;
                        Assert.That(
                            actual,
                            {{constraint}}(9)
                        );
            """);

            var diagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ComparisonConstraintUsage).WithMessage(string.Format(CultureInfo.InvariantCulture, ComparisonConstraintUsageConstants.Message, constraint));

            await Verify.VerifyCodeFixAsync(code, diagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWithAllArgumentsOnSameLine(
            [ValueSource(nameof(operatorTokens))] string operatorToken,
            [Values] bool newlineBeforeClosingParen)
        {
            var optionalNewline = newlineBeforeClosingParen ? $"{Environment.NewLine}            " : string.Empty;
            var constraint = operatorTokensToConstraints[operatorToken];
            var code = TestUtility.WrapInTestMethod($$"""
                        int actual = 5;
                        Assert.That(
                            {|#0:actual {{operatorToken}} 9|}, "message"{{optionalNewline}});
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        int actual = 5;
                        Assert.That(
                            actual, {{constraint}}(9), "message"{{optionalNewline}});
            """);

            var diagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ComparisonConstraintUsage).WithMessage(string.Format(CultureInfo.InvariantCulture, ComparisonConstraintUsageConstants.Message, constraint));

            await Verify.VerifyCodeFixAsync(code, diagnostic.WithLocation(0), fixedCode);
        }
    }
}
