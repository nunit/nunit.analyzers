using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.ConstraintUsage.StringConstraintUsageAnalyzer, NUnit.Analyzers.ConstraintUsage.StringConstraintUsageCodeFix>;

namespace NUnit.Analyzers.Tests.ConstraintsUsage
{
    public class StringConstraintUsageCodeFixTests
    {
        private static readonly Dictionary<string, string[]> PositiveAssertDictionary = new()
        {
            { nameof(string.Contains), new[] { nameof(string.Contains), AnalyzerIdentifiers.StringContainsConstraintUsage, "Does.Contain" } },
            { nameof(string.StartsWith), new[] { nameof(string.StartsWith), AnalyzerIdentifiers.StringStartsWithConstraintUsage, "Does.StartWith" } },
            { nameof(string.EndsWith), new[] { nameof(string.EndsWith), AnalyzerIdentifiers.StringEndsWithConstraintUsage, "Does.EndWith" } },
        };
        private static readonly object[] PositiveAssertData = PositiveAssertDictionary.Values.ToArray();

        private static readonly Dictionary<string, string[]> NegativeAssertDictionary = new()
        {
            { nameof(string.Contains), new[] { nameof(string.Contains), AnalyzerIdentifiers.StringContainsConstraintUsage, "Does.Not.Contain" } },
            { nameof(string.StartsWith), new[] { nameof(string.StartsWith), AnalyzerIdentifiers.StringStartsWithConstraintUsage, "Does.Not.StartWith" } },
            { nameof(string.EndsWith), new[] { nameof(string.EndsWith), AnalyzerIdentifiers.StringEndsWithConstraintUsage, "Does.Not.EndWith" } },
        };
        private static readonly object[] NegativeAssertData = NegativeAssertDictionary.Values.ToArray();
        private static readonly string[] StringConstraints = PositiveAssertDictionary.Keys.ToArray();

        [TestCaseSource(nameof(PositiveAssertData))]
        public async Task AnalyzeStringBooleanMethodAssertTrue(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        ClassicAssert.True({|#0:"abc".{{method}}("ab")|});
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That("abc", {{suggestedConstraint}}("ab"));
            """);

            await Verify.VerifyCodeFixAsync(code, Verify.Diagnostic(analyzerId).WithLocation(0).WithArguments(suggestedConstraint), fixedCode);
        }

        [TestCaseSource(nameof(PositiveAssertData))]
        public async Task AnalyzeStringBooleanMethodAssertIsTrue(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        ClassicAssert.IsTrue({|#0:"abc".{{method}}("ab")|});
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That("abc", {{suggestedConstraint}}("ab"));
            """);

            await Verify.VerifyCodeFixAsync(code, Verify.Diagnostic(analyzerId).WithLocation(0).WithArguments(suggestedConstraint), fixedCode);
        }

        [TestCaseSource(nameof(PositiveAssertData))]
        public async Task AnalyzeStringBooleanMethodAssertThat(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        Assert.That({|#0:"abc".{{method}}("ab")|});
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That("abc", {{suggestedConstraint}}("ab"));
            """);

            await Verify.VerifyCodeFixAsync(code, Verify.Diagnostic(analyzerId).WithLocation(0).WithArguments(suggestedConstraint), fixedCode);
        }

        [TestCaseSource(nameof(PositiveAssertData))]
        public async Task AnalyzeStringBooleanMethodAssertThatIsTrue(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        Assert.That({|#0:"abc".{{method}}("ab")|}, Is.True);
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That("abc", {{suggestedConstraint}}("ab"));
            """);

            await Verify.VerifyCodeFixAsync(code, Verify.Diagnostic(analyzerId).WithLocation(0).WithArguments(suggestedConstraint), fixedCode);
        }

        [TestCaseSource(nameof(NegativeAssertData))]
        public async Task AnalyzeStringBooleanMethodAssertThatNegated(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        Assert.That({|#0:!"abc".{{method}}("ab")|});
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That("abc", {{suggestedConstraint}}("ab"));
            """);

            await Verify.VerifyCodeFixAsync(code, Verify.Diagnostic(analyzerId).WithLocation(0).WithArguments(suggestedConstraint), fixedCode);
        }

        [TestCaseSource(nameof(NegativeAssertData))]
        public async Task AnalyzeStringBooleanMethodAssertFalse(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        ClassicAssert.False({|#0:"abc".{{method}}("ab")|});
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That("abc", {{suggestedConstraint}}("ab"));
            """);

            await Verify.VerifyCodeFixAsync(code, Verify.Diagnostic(analyzerId).WithLocation(0).WithArguments(suggestedConstraint), fixedCode);
        }

        [TestCaseSource(nameof(NegativeAssertData))]
        public async Task AnalyzeStringBooleanMethodAssertIsFalse(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        ClassicAssert.IsFalse({|#0:"abc".{{method}}("ab")|});
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That("abc", {{suggestedConstraint}}("ab"));
            """);

            await Verify.VerifyCodeFixAsync(code, Verify.Diagnostic(analyzerId).WithLocation(0).WithArguments(suggestedConstraint), fixedCode);
        }

        [TestCaseSource(nameof(NegativeAssertData))]
        public async Task AnalyzeStringBooleanMethodAssertThatIsFalse(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        Assert.That({|#0:"abc".{{method}}("ab")|}, Is.False);
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That("abc", {{suggestedConstraint}}("ab"));
            """);

            await Verify.VerifyCodeFixAsync(code, Verify.Diagnostic(analyzerId).WithLocation(0).WithArguments(suggestedConstraint), fixedCode);
        }

        [TestCaseSource(nameof(NegativeAssertData))]
        public async Task CodeFixMaintainsReasonableTriviaWithEndOfLineClosingParen(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        ClassicAssert.IsFalse(
                            {|#0:"abc".{{method}}("ab")|});
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That(
                            "abc",
                            {{suggestedConstraint}}("ab"));
            """);

            await Verify.VerifyCodeFixAsync(code, Verify.Diagnostic(analyzerId).WithLocation(0).WithArguments(suggestedConstraint), fixedCode);
        }

        [TestCaseSource(nameof(NegativeAssertData))]
        public async Task CodeFixMaintainsReasonableTriviaWithNewLineClosingParen(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        Assert.That(
                            {|#0:"abc".{{method}}("ab")|},
                            Is.False
                        );
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That(
                            "abc",
                            {{suggestedConstraint}}("ab")
                        );
            """);

            await Verify.VerifyCodeFixAsync(code, Verify.Diagnostic(analyzerId).WithLocation(0).WithArguments(suggestedConstraint), fixedCode);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWithAllArgumentsOnSameLine(
            [ValueSource(nameof(StringConstraints))] string method,
            [Values] bool newlineBeforeClosingParen)
        {
            var analyzerId = NegativeAssertDictionary[method][1];
            var suggestedConstraint = NegativeAssertDictionary[method][2];
            var optionalNewline = newlineBeforeClosingParen ? $"{Environment.NewLine}            " : string.Empty;
            var code = TestUtility.WrapInTestMethod($$"""
                        Assert.That(
                            {|#0:"abc".{{method}}("ab")|}, Is.False{{optionalNewline}});
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That(
                            "abc", {{suggestedConstraint}}("ab"){{optionalNewline}});
            """);

            await Verify.VerifyCodeFixAsync(code, Verify.Diagnostic(analyzerId).WithLocation(0).WithArguments(suggestedConstraint), fixedCode);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWhenActualArgumentIsMultipleLines()
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        Assert.That({|#0:SomeReallyLongMethodName()
                                    .Contains("Lorem ipsum dolor sit amet, consectetur adipiscing elit,")|},
                                    Is.True);

                        string SomeReallyLongMethodName() => "Long string";
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That(SomeReallyLongMethodName(),
                                    Does.Contain("Lorem ipsum dolor sit amet, consectetur adipiscing elit,"));

                        string SomeReallyLongMethodName() => "Long string";
            """);

            await Verify.VerifyCodeFixAsync(code, Verify.Diagnostic(AnalyzerIdentifiers.StringContainsConstraintUsage).WithLocation(0).WithArguments("Does.Contain"), fixedCode);
        }
    }
}
