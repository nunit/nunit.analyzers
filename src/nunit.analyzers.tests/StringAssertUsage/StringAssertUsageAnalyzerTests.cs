using System.Collections.Generic;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.StringAssertUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.StringAssertUsage
{
    [TestFixture]
    internal class StringAssertUsageAnalyzerTests
    {
        private readonly DiagnosticAnalyzer analyzer = new StringAssertUsageAnalyzer();
        private readonly ExpectedDiagnostic diagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.StringAssertUsage);

        private static IEnumerable<string> StringAsserts => StringAssertUsageAnalyzer.StringAssertToConstraint.Keys;

        [TestCaseSource(nameof(StringAsserts))]
        public void AnalyzeWhenNoArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                ↓StringAssert.{method}(""expected"", ""actual"");
            ");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [TestCaseSource(nameof(StringAsserts))]
        public void AnalyzeWhenOnlyMessageArgumentIsUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                ↓StringAssert.{method}(""expected"", ""actual"", ""message"");
            ");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [TestCaseSource(nameof(StringAsserts))]
        public void AnalyzeWhenFormatAndArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                ↓StringAssert.{method}(""expected"", ""actual"", ""Because of {{0}}"", ""message"");
            ");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }
    }
}
