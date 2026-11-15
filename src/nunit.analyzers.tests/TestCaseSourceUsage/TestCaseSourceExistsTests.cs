using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.TestCaseSourceUsage.TestCaseSourceUsesStringAnalyzer>;

namespace NUnit.Analyzers.Tests.TestCaseSourceUsage
{
    public sealed class TestCaseSourceExistsTests
    {
        private static readonly DiagnosticResult expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseSourceIsMissing);

        [Test]
        public async Task ErrorsWhenSourceDoesNotExists()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class Tests
                {
                    [TestCaseSource({|#0:"Missing"|})]
                    public void Test()
                    {
                    }
                }
            """);
            var message = "The TestCaseSource argument 'Missing' does not specify an existing member";
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithMessage(message).WithLocation(0));
        }

        [Test]
        public async Task NoErrorsWhenSourceExists()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                internal class Base
                {
                    protected static IEnumerable<string> TargetFrameworks { get; } = new[] { "net48", "netcoreapp3.1" };
                }

                internal class Tests : Base
                {
                    [TestCaseSource(nameof(TargetFrameworks))]
                    public void Test(string targetFramework)
                    {
                        ClassicAssert.IsNotNull(targetFramework);
                    }
                }
            """, "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
