using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.MisusedConstraints.MisusedConstraintsAnalyzer>;

namespace NUnit.Analyzers.Tests.MisusedConstraints
{
    public class MisusedConstraintsAnalyzerTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.MisusedConstraints);

        [Test]
        public async Task AnalyzeIsNotNullOrEmpty()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        string s = string.Empty;
                        Assert.That(s, {|#0:Is.Not.Null.Or.Empty|});

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("This is equivalent to Is.Not.Null, did you mean Is.Not.Null.And.Not.Empty?"));
        }

        [Test]
        public async Task AnalyzeIsNotNullOrEmptyOverMultipleLines()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        string s = string.Empty;
                        Assert.That(s, {|#0:Is.Not.Null
                                          .Or.Empty|});

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("This is equivalent to Is.Not.Null, did you mean Is.Not.Null.And.Not.Empty?"));
        }
    }
}
