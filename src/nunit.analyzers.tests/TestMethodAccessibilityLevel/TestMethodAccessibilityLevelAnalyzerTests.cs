using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.TestMethodAccessibilityLevel.TestMethodAccessibilityLevelAnalyzer>;

namespace NUnit.Analyzers.Tests.TestMethodAccessibilityLevel
{
    public class TestMethodAccessibilityLevelAnalyzerTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.TestMethodIsNotPublic);

        private static IEnumerable<string> SetUpTearDownMethodRelatedAttributes =>
        [
            "OneTimeSetUp",
            "OneTimeTearDown",
            "SetUp",
            "TearDown"
        ];

        private static IEnumerable<string> NonPublicModifiers =>
        [
            "",
            "private",
            "protected",
            "internal",
            "protected internal",
            "private protected",

            "static private",
            "static protected",
            "static internal",
            "static protected internal",
            "static private protected",

            "private static",
            "protected static",
            "internal static",
            "protected internal static",
            "private protected static ",

            "protected static internal",
            "private static protected",
        ];

        private static IEnumerable<string> NonPublicOrFamilyModifiers =>
        [
            "",
            "private",
            "internal",
            "protected internal",
            "private protected",

            "static private",
            "static internal",
            "static protected internal",
            "static private protected",

            "private static",
            "internal static",
            "protected internal static",
            "private protected static ",

            "protected static internal",
            "private static protected",
        ];

        [TestCaseSource(nameof(SetUpTearDownMethodRelatedAttributes))]
        public async Task AnalyzeWhenSetUpTearDownMethodIsPublic(string attribute)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [{{attribute}}]
                    public void SetUpTearDownMethod() { }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCaseSource(nameof(SetUpTearDownMethodRelatedAttributes))]
        public async Task AnalyzeWhenSetUpTearDownMethodIsProtected(string attribute)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [{{attribute}}]
                    protected void SetUpTearDownMethod() { }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenSimpleTestMethodIsPublic()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void TestMethod() { }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenTestMethodIsPublic()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase(1)]
                    public void TestMethod(int i) { }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenSetUpTearDownMethodIsNotPublic([ValueSource(nameof(SetUpTearDownMethodRelatedAttributes))] string attribute,
                                                              [ValueSource(nameof(NonPublicOrFamilyModifiers))] string modifiers)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [{{attribute}}]
                    {{modifiers}} void {|#0:SetUpTearDownMethod|}() { }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(NonPublicModifiers))]
        public async Task AnalyzeWhenSimpleTestMethodIsNotPublic(string modifiers)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    {{modifiers}} void {|#0:TestMethod|}() { }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(NonPublicModifiers))]
        public async Task AnalyzeWhenTestMethodIsNotPublic(string modifiers)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase(1)]
                    {{modifiers}} void {|#0:TestMethod|}(int i) { }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(NonPublicModifiers))]
        public async Task AnalyzeWhenAsyncSimpleTestMethodIsNotPublic(string modifiers)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    {{modifiers}} async Task {|#0:TestMethod|}() { await Task.CompletedTask; }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(NonPublicModifiers))]
        public async Task AnalyzeWhenAsyncTestMethodIsNotPublic(string modifiers)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase(1)]
                    {{modifiers}} async Task {|#0:TestMethod|}(int i) { await Task.CompletedTask; }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }
    }
}
