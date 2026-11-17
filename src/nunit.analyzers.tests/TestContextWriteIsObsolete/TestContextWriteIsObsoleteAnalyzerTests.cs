using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.TestContextWriteIsObsolete.TestContextWriteIsObsoleteAnalyzer>;

namespace NUnit.Analyzers.Tests.TestContextWriteIsObsolete
{
    public class TestContextWriteIsObsoleteAnalyzerTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.TestContextWriteIsObsolete);

        [TestCaseSource(typeof(TestContextWriteIsObsoleteTestCases), nameof(TestContextWriteIsObsoleteTestCases.WriteInvocations))]
        public async Task AnyDirectWriteMethod(string writeMethodAndParameters)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void Test()
                    {
                        {|#0:TestContext.{{writeMethodAndParameters}}|};
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [TestCaseSource(typeof(TestContextWriteIsObsoleteTestCases), nameof(TestContextWriteIsObsoleteTestCases.WriteInvocations))]
        public async Task AnyIndirectWriteMethod(string writeMethodAndParameters)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void Test()
                    {
                        TestContext.Out.{{writeMethodAndParameters}};
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
