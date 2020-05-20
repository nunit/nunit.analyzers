using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TestCaseSourceUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.TestCaseSourceUsage
{
    public sealed class TestCaseSourceExistsTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new TestCaseSourceUsesStringAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.TestCaseSourceIsMissing);

        [Test]
        public void ErrorsWhenSourceDoesNotExists()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class Tests
    {
        [TestCaseSource(↓""Missing"")]
        public void Test()
        {
        }
    }");
            var message = "The TestCaseSource argument 'Missing' does not specify an existing member.";
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic.WithMessage(message), testCode);
        }
    }
}
