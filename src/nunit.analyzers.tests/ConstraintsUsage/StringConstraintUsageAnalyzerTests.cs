using System.Globalization;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.ConstraintUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ConstraintsUsage
{
    public class StringConstraintUsageAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new StringConstraintUsageAnalyzer();

        private static readonly object[] PositiveAssertData = new[]
        {
            new[] { nameof(string.Contains), AnalyzerIdentifiers.StringContainsConstraintUsage, "Does.Contain" },
            new[] { nameof(string.StartsWith), AnalyzerIdentifiers.StringStartsWithConstraintUsage, "Does.StartWith" },
            new[] { nameof(string.EndsWith), AnalyzerIdentifiers.StringEndsWithConstraintUsage, "Does.EndWith" }
        };

        private static readonly object[] NegativeAssertData = new[]
        {
            new[] { nameof(string.Contains), AnalyzerIdentifiers.StringContainsConstraintUsage, "Does.Not.Contain" },
            new[] { nameof(string.StartsWith), AnalyzerIdentifiers.StringStartsWithConstraintUsage, "Does.Not.StartWith" },
            new[] { nameof(string.EndsWith), AnalyzerIdentifiers.StringEndsWithConstraintUsage, "Does.Not.EndWith" }
        };

        [TestCaseSource(nameof(PositiveAssertData))]
        public void AnalyzeStringBooleanMethod_AssertTrue(string method, string analyzerId, string suggestedConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"Assert.True(↓""abc"".{method}(""ab""));");

            var diagnostic = ExpectedDiagnostic.Create(analyzerId,
                string.Format(CultureInfo.InvariantCulture, StringConstraintUsageConstants.Message, suggestedConstraint));
            AnalyzerAssert.Diagnostics(analyzer, diagnostic, testCode);
        }

        [TestCaseSource(nameof(PositiveAssertData))]
        public void AnalyzeStringBooleanMethod_AssertIsTrue(string method, string analyzerId, string suggestedConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"Assert.IsTrue(↓""abc"".{method}(""ab""));");

            var diagnostic = ExpectedDiagnostic.Create(analyzerId,
                string.Format(CultureInfo.InvariantCulture, StringConstraintUsageConstants.Message, suggestedConstraint));
            AnalyzerAssert.Diagnostics(analyzer, diagnostic, testCode);
        }

        [TestCaseSource(nameof(PositiveAssertData))]
        public void AnalyzeStringBooleanMethod_AssertThat(string method, string analyzerId, string suggestedConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"Assert.That(↓""abc"".{method}(""ab""));");

            var diagnostic = ExpectedDiagnostic.Create(analyzerId,
                string.Format(CultureInfo.InvariantCulture, StringConstraintUsageConstants.Message, suggestedConstraint));
            AnalyzerAssert.Diagnostics(analyzer, diagnostic, testCode);
        }

        [TestCaseSource(nameof(PositiveAssertData))]
        public void AnalyzeStringBooleanMethod_AssertThat_IsTrue(string method, string analyzerId, string suggestedConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"Assert.That(↓""abc"".{method}(""ab""), Is.True);");

            var diagnostic = ExpectedDiagnostic.Create(analyzerId,
                string.Format(CultureInfo.InvariantCulture, StringConstraintUsageConstants.Message, suggestedConstraint));
            AnalyzerAssert.Diagnostics(analyzer, diagnostic, testCode);
        }

        [TestCaseSource(nameof(NegativeAssertData))]
        public void AnalyzeStringBooleanMethod_AssertFalse(string method, string analyzerId, string suggestedConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"Assert.False(↓""abc"".{method}(""ab""));");

            var diagnostic = ExpectedDiagnostic.Create(analyzerId,
                string.Format(CultureInfo.InvariantCulture, StringConstraintUsageConstants.Message, suggestedConstraint));
            AnalyzerAssert.Diagnostics(analyzer, diagnostic, testCode);
        }

        [TestCaseSource(nameof(NegativeAssertData))]
        public void AnalyzeStringBooleanMethod_AssertIsFalse(string method, string analyzerId, string suggestedConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"Assert.IsFalse(↓""abc"".{method}(""ab""));");

            var diagnostic = ExpectedDiagnostic.Create(analyzerId,
                string.Format(CultureInfo.InvariantCulture, StringConstraintUsageConstants.Message, suggestedConstraint));
            AnalyzerAssert.Diagnostics(analyzer, diagnostic, testCode);
        }

        [TestCaseSource(nameof(NegativeAssertData))]
        public void AnalyzeStringBooleanMethod_AssertThat_IsFalse(string method, string analyzerId, string suggestedConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"Assert.That(↓""abc"".{method}(""ab""), Is.False);");

            var diagnostic = ExpectedDiagnostic.Create(analyzerId,
                string.Format(CultureInfo.InvariantCulture, StringConstraintUsageConstants.Message, suggestedConstraint));
            AnalyzerAssert.Diagnostics(analyzer, diagnostic, testCode);
        }

        [Test]
        public void ValidForUnsupportedStringMethods()
        {
            var testCode = TestUtility.WrapInTestMethod(@"Assert.That(""abc"".IsNormalized());");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidForNonStringMethods()
        {
            var testCode = TestUtility.WrapInTestMethod(
                @"Assert.That(new System.Collections.Generic.List<string> { ""a"",""ab""}.Contains(""ab""));");

            AnalyzerAssert.Valid(analyzer, testCode);
        }
    }
}
