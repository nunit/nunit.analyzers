using System.Collections.Generic;
using System.Linq;
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
        public void AnalyzeStringBooleanMethodAssertTrue(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($@"
            ClassicAssert.True(↓""abc"".{method}(""ab""));");

            var fixedCode = TestUtility.WrapInTestMethod($@"
            Assert.That(""abc"", {suggestedConstraint}(""ab""));");

            RoslynAssert.CodeFix(analyzer, fix, ExpectedDiagnostic.Create(analyzerId), code, fixedCode);
        }

        [TestCaseSource(nameof(PositiveAssertData))]
        public void AnalyzeStringBooleanMethodAssertIsTrue(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($@"
            ClassicAssert.IsTrue(↓""abc"".{method}(""ab""));");

            var fixedCode = TestUtility.WrapInTestMethod($@"
            Assert.That(""abc"", {suggestedConstraint}(""ab""));");

            RoslynAssert.CodeFix(analyzer, fix, ExpectedDiagnostic.Create(analyzerId), code, fixedCode);
        }

        [TestCaseSource(nameof(PositiveAssertData))]
        public void AnalyzeStringBooleanMethodAssertThat(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($@"
            Assert.That(↓""abc"".{method}(""ab""));");

            var fixedCode = TestUtility.WrapInTestMethod($@"
            Assert.That(""abc"", {suggestedConstraint}(""ab""));");

            RoslynAssert.CodeFix(analyzer, fix, ExpectedDiagnostic.Create(analyzerId), code, fixedCode);
        }

        [TestCaseSource(nameof(PositiveAssertData))]
        public void AnalyzeStringBooleanMethodAssertThatIsTrue(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($@"
            Assert.That(↓""abc"".{method}(""ab""), Is.True);");

            var fixedCode = TestUtility.WrapInTestMethod($@"
            Assert.That(""abc"", {suggestedConstraint}(""ab""));");

            RoslynAssert.CodeFix(analyzer, fix, ExpectedDiagnostic.Create(analyzerId), code, fixedCode);
        }

        [TestCaseSource(nameof(NegativeAssertData))]
        public void AnalyzeStringBooleanMethodAssertThatNegated(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($@"
            Assert.That(↓!""abc"".{method}(""ab""));");

            var fixedCode = TestUtility.WrapInTestMethod($@"
            Assert.That(""abc"", {suggestedConstraint}(""ab""));");

            RoslynAssert.CodeFix(analyzer, fix, ExpectedDiagnostic.Create(analyzerId), code, fixedCode);
        }

        [TestCaseSource(nameof(NegativeAssertData))]
        public void AnalyzeStringBooleanMethodAssertFalse(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($@"
            ClassicAssert.False(↓""abc"".{method}(""ab""));");

            var fixedCode = TestUtility.WrapInTestMethod($@"
            Assert.That(""abc"", {suggestedConstraint}(""ab""));");

            RoslynAssert.CodeFix(analyzer, fix, ExpectedDiagnostic.Create(analyzerId), code, fixedCode);
        }

        [TestCaseSource(nameof(NegativeAssertData))]
        public void AnalyzeStringBooleanMethodAssertIsFalse(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($@"
            ClassicAssert.IsFalse(↓""abc"".{method}(""ab""));");

            var fixedCode = TestUtility.WrapInTestMethod($@"
            Assert.That(""abc"", {suggestedConstraint}(""ab""));");

            RoslynAssert.CodeFix(analyzer, fix, ExpectedDiagnostic.Create(analyzerId), code, fixedCode);
        }

        [TestCaseSource(nameof(NegativeAssertData))]
        public void AnalyzeStringBooleanMethodAssertThatIsFalse(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($@"
            Assert.That(↓""abc"".{method}(""ab""), Is.False);");

            var fixedCode = TestUtility.WrapInTestMethod($@"
            Assert.That(""abc"", {suggestedConstraint}(""ab""));");

            RoslynAssert.CodeFix(analyzer, fix, ExpectedDiagnostic.Create(analyzerId), code, fixedCode);
        }

        [TestCaseSource(nameof(NegativeAssertData))]
        public void CodeFixMaintainsReasonableTriviaWithEndOfLineClosingParen(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($@"
            ClassicAssert.IsFalse(
                ↓""abc"".{method}(""ab""));");

            var fixedCode = TestUtility.WrapInTestMethod($@"
            Assert.That(
                ""abc"",
                {suggestedConstraint}(""ab""));");

            RoslynAssert.CodeFix(analyzer, fix, ExpectedDiagnostic.Create(analyzerId), code, fixedCode);
        }

        [TestCaseSource(nameof(NegativeAssertData))]
        public void CodeFixMaintainsReasonableTriviaWithNewLineClosingParen(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($@"
            Assert.That(
                ↓""abc"".{method}(""ab""),
                Is.False
            );");

            var fixedCode = TestUtility.WrapInTestMethod($@"
            Assert.That(
                ""abc"",
                {suggestedConstraint}(""ab"")
            );");

            RoslynAssert.CodeFix(analyzer, fix, ExpectedDiagnostic.Create(analyzerId), code, fixedCode);
        }

        [Test]
        public void CodeFixMaintainsReasonableTriviaWithAllArgumentsOnSameLine(
            [ValueSource(nameof(StringConstraints))] string method,
            [Values] bool newlineBeforeClosingParen)
        {
            var analyzerId = NegativeAssertDictionary[method][1];
            var suggestedConstraint = NegativeAssertDictionary[method][2];
            var optionalNewline = newlineBeforeClosingParen ? "\r\n            " : string.Empty;
            var code = TestUtility.WrapInTestMethod($@"
            Assert.That(
                ↓""abc"".{method}(""ab""), Is.False{optionalNewline});");

            var fixedCode = TestUtility.WrapInTestMethod($@"
            Assert.That(
                ""abc"", {suggestedConstraint}(""ab""){optionalNewline});");

            RoslynAssert.CodeFix(analyzer, fix, ExpectedDiagnostic.Create(analyzerId), code, fixedCode);
        }
    }
}
