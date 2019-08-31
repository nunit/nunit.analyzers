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

        [TestCase(nameof(string.Contains), AnalyzerIdentifiers.StringContainsConstraintUsage, "Does.Contain")]
        [TestCase(nameof(string.StartsWith), AnalyzerIdentifiers.StringStartsWithConstraintUsage, "Does.StartWith")]
        [TestCase(nameof(string.EndsWith), AnalyzerIdentifiers.StringEndsWithConstraintUsage, "Does.EndWith")]
        public void AnalyzeStringBooleanMethodPositiveAssert(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($@"Assert.True(""abc"".{method}(""ab""));");

            var fixedCode = TestUtility.WrapInTestMethod($@"Assert.That(""abc"", {suggestedConstraint}(""ab""));");

            AnalyzerAssert.CodeFix(analyzer, fix, ExpectedDiagnostic.Create(analyzerId), code, fixedCode);
        }

        [TestCase(nameof(string.Contains), AnalyzerIdentifiers.StringContainsConstraintUsage, "Does.Not.Contain")]
        [TestCase(nameof(string.StartsWith), AnalyzerIdentifiers.StringStartsWithConstraintUsage, "Does.Not.StartWith")]
        [TestCase(nameof(string.EndsWith), AnalyzerIdentifiers.StringEndsWithConstraintUsage, "Does.Not.EndWith")]
        public void AnalyzeStringBooleanMethodNegativeAssert(string method, string analyzerId, string suggestedConstraint)
        {
            var code = TestUtility.WrapInTestMethod($@"Assert.False(↓""abc"".{method}(""ab""));");

            var fixedCode = TestUtility.WrapInTestMethod($@"Assert.That(""abc"", {suggestedConstraint}(""ab""));");

            AnalyzerAssert.CodeFix(analyzer, fix, ExpectedDiagnostic.Create(analyzerId), code, fixedCode);
        }
    }
}
