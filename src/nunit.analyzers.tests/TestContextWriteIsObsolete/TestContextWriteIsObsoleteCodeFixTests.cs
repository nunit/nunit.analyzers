using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TestContextWriteIsObsolete;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.TestContextWriteIsObsolete
{
    public class TestContextWriteIsObsoleteCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new TestContextWriteIsObsoleteAnalyzer();
        private static readonly CodeFixProvider fix = new TestContextWriteIsObsoleteCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.TestContextWriteIsObsolete);

        [TestCaseSource(typeof(TestContextWriteIsObsoleteTestCases), nameof(TestContextWriteIsObsoleteTestCases.WriteInvocations))]
        public void AnyWriteMethod(string writeMethodAndParameters)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
                public void Test()
                {{
                    ↓TestContext.{writeMethodAndParameters};
                }}");

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
                public void Test()
                {{
                    TestContext.Out.{writeMethodAndParameters};
                }}");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode,
                fixTitle: TestContextWriteIsObsoleteCodeFix.InsertOutDescription);
        }
    }
}
