using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TypeOfConstraint;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.TypeOfConstraint
{
    public class TypeOfAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new TypeOfAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.TypeOf);

        [TestCase("0.GetType()", "typeof(int)")]
        [TestCase("0.GetType()", "1.GetType()")]
        public void AnalyzeWhenGetTypeInvocation(string actualValue, string expectedValue)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                Assert.That(↓{actualValue}, Is.EqualTo({expectedValue}));");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }
    }
}
