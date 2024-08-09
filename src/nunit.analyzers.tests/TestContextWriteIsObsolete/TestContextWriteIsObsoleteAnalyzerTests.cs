using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TestContextWriteIsObsolete;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.TestContextWriteIsObsolete
{
    public class TestContextWriteIsObsoleteAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new TestContextWriteIsObsoleteAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.TestContextWriteIsObsolete);

        [TestCaseSource(typeof(TestContextWriteIsObsoleteTestCases), nameof(TestContextWriteIsObsoleteTestCases.WriteInvocations))]
        public void AnyDirectWriteMethod(string writeMethodAndParameters)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
                public void Test()
                {{
                    ↓TestContext.{writeMethodAndParameters};
                }}");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [TestCaseSource(typeof(TestContextWriteIsObsoleteTestCases), nameof(TestContextWriteIsObsoleteTestCases.WriteInvocations))]
        public void AnyIndirectWriteMethod(string writeMethodAndParameters)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
                public void Test()
                {{
                    TestContext.Out.{writeMethodAndParameters};
                }}");

            RoslynAssert.Valid(analyzer, testCode);
        }
    }
}
