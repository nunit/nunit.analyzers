using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TestContextWriteIsObsolete;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.TestContextWriteIsObsolete.TestContextWriteIsObsoleteAnalyzer, NUnit.Analyzers.TestContextWriteIsObsolete.TestContextWriteIsObsoleteCodeFix>;

namespace NUnit.Analyzers.Tests.TestContextWriteIsObsolete
{
    public class TestContextWriteIsObsoleteCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.TestContextWriteIsObsolete);

        [TestCaseSource(typeof(TestContextWriteIsObsoleteTestCases), nameof(TestContextWriteIsObsoleteTestCases.WriteInvocations))]
        public async Task AnyWriteMethod(string writeMethodAndParameters)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void Test()
                    {
                        {|#0:TestContext.{{writeMethodAndParameters}}|};
                    }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void Test()
                    {
                        TestContext.Out.{{writeMethodAndParameters}};
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, TestContextWriteIsObsoleteCodeFix.InsertOutDescription);
        }
    }
}
