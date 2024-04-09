using System.Collections.Generic;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.StringAssertUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.StringAssertUsage
{
    [TestFixture]
    internal sealed class StringAssertUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new StringAssertUsageAnalyzer();
        private static readonly CodeFixProvider fix = new StringAssertUsageCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.StringAssertUsage);

        private static IEnumerable<string> StringAsserts => StringAssertUsageAnalyzer.StringAssertToConstraint.Keys;

        [TestCaseSource(nameof(StringAsserts))]
        public void AnalyzeWhenNoArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            ↓StringAssert.{method}(""expected"", ""actual"");
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            Assert.That(""actual"", {GetAdjustedConstraint(method)});
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(StringAsserts))]
        public void AnalyzeWhenOnlyMessageArgumentIsUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            ↓StringAssert.{method}(""expected"", ""actual"", ""message"");
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            Assert.That(""actual"", {GetAdjustedConstraint(method)}, ""message"");
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(StringAsserts))]
        public void AnalyzeWhenFormatAndArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            ↓StringAssert.{method}(""expected"", ""actual"", ""Because of {{0}}"", ""message"");
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            Assert.That(""actual"", {GetAdjustedConstraint(method)}, $""Because of {{""message""}}"");
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(StringAsserts))]
        public void AnalyzeWhenFormatAndArgumentsAreUsedOutOfOrder(string method)
        {
            var firstParameterName = StringAssertUsageCodeFix.StringAssertToExpectedParameterName[method];
            var code = TestUtility.WrapInTestMethod(@$"
		    ↓StringAssert.{method}(args: new[] {{ ""first"", ""second"" }}, message: ""{{0}}, {{1}}"", actual: ""actual"", {firstParameterName}: ""expected"");");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            Assert.That(""actual"", {GetAdjustedConstraint(method)}, $""{{""first""}}, {{""second""}}"");");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        private static string GetAdjustedConstraint(string method) =>
            StringAssertUsageAnalyzer.StringAssertToConstraint[method]
                .Replace("expected", "\"expected\"");
    }
}
