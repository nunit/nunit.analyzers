using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.NonTestMethodAccessibilityLevel.NonTestMethodAccessibilityLevelAnalyzer, NUnit.Analyzers.NonTestMethodAccessibilityLevel.NonTestMethodAccessibilityLevelCodeFix>;

namespace NUnit.Analyzers.Tests.TestMethodAccessibilityLevel
{
    public class NonTestMethodAccessibilityLevelCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.NonTestMethodIsPublic);

        private static readonly object[] NonPrivateModifiers =
        [
            new object[] { "public", "private" },
            new object[] { "internal", "private" },
            new object[] { "protected internal", "protected" },
            new object[] { "static public", "static private" },
            new object[] { "static internal", "static private" },
            new object[] { "static protected internal", "static protected" },
        ];

        [TestCaseSource(nameof(NonPrivateModifiers))]
        public async Task FixesPublicAccessModifiers(string modifiers, string modifiersAfter)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void TestMethod() { AssertMethod(); }
                    {{modifiers}} void {|#0:AssertMethod|}() { }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void TestMethod() { AssertMethod(); }
                    {{modifiersAfter}} void AssertMethod() { }
            """);

            await Verify.VerifyCodeFixAsync(testCode, expectedDiagnostic.WithLocation(0), fixedCode);
        }
    }
}
