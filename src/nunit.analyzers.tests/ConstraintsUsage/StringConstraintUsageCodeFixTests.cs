using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.ConstraintUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ConstraintsUsage
{
    public class StringConstraintUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new StringConstraintUsageAnalyzer();
        private static readonly CodeFixProvider fix = new StringConstraintUsageCodeFix();

        private static readonly object[] PositiveAssertData = new[] {
            new [] { nameof(string.Contains), AnalyzerIdentifiers.StringContainsConstraintUsage, "Does.Contain" },
            new [] { nameof(string.StartsWith), AnalyzerIdentifiers.StringStartsWithConstraintUsage, "Does.StartWith" },
            new [] { nameof(string.EndsWith), AnalyzerIdentifiers.StringEndsWithConstraintUsage, "Does.EndWith" }
        };

        private static readonly object[] NegativeAssertData = new[] {
            new [] { nameof(string.Contains), AnalyzerIdentifiers.StringContainsConstraintUsage, "Does.Not.Contain" },
            new [] { nameof(string.StartsWith), AnalyzerIdentifiers.StringStartsWithConstraintUsage, "Does.Not.StartWith" },
            new [] { nameof(string.EndsWith), AnalyzerIdentifiers.StringEndsWithConstraintUsage, "Does.Not.EndWith" }
        };

        [TestCaseSource(nameof(PositiveAssertData))]
        public void AnalyzeStringBooleanMethod_AssertTrue(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($@"Assert.True(↓""abc"".{method}(""ab""));");

            var fixedCode = TestUtility.WrapInTestMethod($@"Assert.That(""abc"", {suggestedConstraint}(""ab""));");

            AnalyzerAssert.CodeFix(analyzer, fix, ExpectedDiagnostic.Create(analyzerId), code, fixedCode);
        }

        [TestCaseSource(nameof(PositiveAssertData))]
        public void AnalyzeStringBooleanMethod_AssertIsTrue(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($@"Assert.IsTrue(↓""abc"".{method}(""ab""));");

            var fixedCode = TestUtility.WrapInTestMethod($@"Assert.That(""abc"", {suggestedConstraint}(""ab""));");

            AnalyzerAssert.CodeFix(analyzer, fix, ExpectedDiagnostic.Create(analyzerId), code, fixedCode);
        }

        [TestCaseSource(nameof(PositiveAssertData))]
        public void AnalyzeStringBooleanMethod_AssertThat(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($@"Assert.That(↓""abc"".{method}(""ab""));");

            var fixedCode = TestUtility.WrapInTestMethod($@"Assert.That(""abc"", {suggestedConstraint}(""ab""));");

            AnalyzerAssert.CodeFix(analyzer, fix, ExpectedDiagnostic.Create(analyzerId), code, fixedCode);
        }

        [TestCaseSource(nameof(PositiveAssertData))]
        public void AnalyzeStringBooleanMethod_AssertThat_IsTrue(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($@"Assert.That(↓""abc"".{method}(""ab""), Is.True);");

            var fixedCode = TestUtility.WrapInTestMethod($@"Assert.That(""abc"", {suggestedConstraint}(""ab""));");

            AnalyzerAssert.CodeFix(analyzer, fix, ExpectedDiagnostic.Create(analyzerId), code, fixedCode);
        }

        [TestCaseSource(nameof(NegativeAssertData))]
        public void AnalyzeStringBooleanMethod_AssertThat_Negated(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($@"Assert.That(↓!""abc"".{method}(""ab""));");

            var fixedCode = TestUtility.WrapInTestMethod($@"Assert.That(""abc"", {suggestedConstraint}(""ab""));");

            AnalyzerAssert.CodeFix(analyzer, fix, ExpectedDiagnostic.Create(analyzerId), code, fixedCode);
        }

        [TestCaseSource(nameof(NegativeAssertData))]
        public void AnalyzeStringBooleanMethod_AssertFalse(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($@"Assert.False(↓""abc"".{method}(""ab""));");

            var fixedCode = TestUtility.WrapInTestMethod($@"Assert.That(""abc"", {suggestedConstraint}(""ab""));");

            AnalyzerAssert.CodeFix(analyzer, fix, ExpectedDiagnostic.Create(analyzerId), code, fixedCode);
        }

        [TestCaseSource(nameof(NegativeAssertData))]
        public void AnalyzeStringBooleanMethod_AssertIsFalse(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($@"Assert.IsFalse(↓""abc"".{method}(""ab""));");

            var fixedCode = TestUtility.WrapInTestMethod($@"Assert.That(""abc"", {suggestedConstraint}(""ab""));");

            AnalyzerAssert.CodeFix(analyzer, fix, ExpectedDiagnostic.Create(analyzerId), code, fixedCode);
        }

        [TestCaseSource(nameof(NegativeAssertData))]
        public void AnalyzeStringBooleanMethod_AssertThat_IsFalse(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($@"Assert.That(↓""abc"".{method}(""ab""), Is.False);");

            var fixedCode = TestUtility.WrapInTestMethod($@"Assert.That(""abc"", {suggestedConstraint}(""ab""));");

            AnalyzerAssert.CodeFix(analyzer, fix, ExpectedDiagnostic.Create(analyzerId), code, fixedCode);
        }
    }
}
