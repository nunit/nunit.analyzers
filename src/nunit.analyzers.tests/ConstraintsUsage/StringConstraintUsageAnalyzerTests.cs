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

        [TestCase(nameof(string.Contains), AnalyzerIdentifiers.StringContainsConstraintUsage, "Does.Contain")]
        [TestCase(nameof(string.StartsWith), AnalyzerIdentifiers.StringStartsWithConstraintUsage, "Does.StartWith")]
        [TestCase(nameof(string.EndsWith), AnalyzerIdentifiers.StringEndsWithConstraintUsage, "Does.EndWith")]
        public void AnalyzeStringBooleanMethodPositiveAssert(string method, string analyzerId, string suggestedConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"Assert.True(↓""abc"".{method}(""ab""));");

            var diagnostic = ExpectedDiagnostic.Create(analyzerId,
                string.Format(StringConstraintUsageConstants.Message, suggestedConstraint));
            AnalyzerAssert.Diagnostics(analyzer, diagnostic, testCode);
        }

        [TestCase(nameof(string.Contains), AnalyzerIdentifiers.StringContainsConstraintUsage, "Does.Not.Contain")]
        [TestCase(nameof(string.StartsWith), AnalyzerIdentifiers.StringStartsWithConstraintUsage, "Does.Not.StartWith")]
        [TestCase(nameof(string.EndsWith), AnalyzerIdentifiers.StringEndsWithConstraintUsage, "Does.Not.EndWith")]
        public void AnalyzeStringBooleanMethodNegativeAssert(string method, string analyzerId, string suggestedConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"Assert.False(↓""abc"".{method}(""ab""));");

            var diagnostic = ExpectedDiagnostic.Create(analyzerId,
                string.Format(StringConstraintUsageConstants.Message, suggestedConstraint));
            AnalyzerAssert.Diagnostics(analyzer, diagnostic, testCode);
        }

        [Test]
        public void ValidForUnsupportedStringMethods()
        {
            var testCode = TestUtility.WrapInTestMethod(@"Assert.That(↓""abc"".IsNormalized());");

            AnalyzerAssert.NoAnalyzerDiagnostics(analyzer, testCode);
        }

        [Test]
        public void ValidForNonStringMethods()
        {
            var testCode = TestUtility.WrapInTestMethod(
                @"Assert.That(new List<string> { ""a"",""ab""}.Contains(""ab""));");

            AnalyzerAssert.NoAnalyzerDiagnostics(analyzer, testCode);
        }
    }
}
