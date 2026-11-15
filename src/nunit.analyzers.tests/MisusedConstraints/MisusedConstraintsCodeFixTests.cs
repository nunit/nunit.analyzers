using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.MisusedConstraints.MisusedConstraintsAnalyzer, NUnit.Analyzers.MisusedConstraints.MisusedConstraintsCodeFix>;

namespace NUnit.Analyzers.Tests.MisusedConstraints
{
    public class MisusedConstraintsCodeFixTests
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

            var fixedCode = TestUtility.WrapInTestMethod("""
                        string s = string.Empty;
                        Assert.That(s, Is.Not.Null.And.Not.Empty);

            """);

            await Verify.VerifyCodeFixAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("This is equivalent to Is.Not.Null, did you mean Is.Not.Null.And.Not.Empty?"), fixedCode);
        }
    }
}
