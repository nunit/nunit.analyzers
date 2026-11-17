using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.TestMethodAccessibilityLevel.TestMethodAccessibilityLevelAnalyzer, NUnit.Analyzers.TestMethodAccessibilityLevel.TestMethodAccessibilityLevelCodeFix>;

namespace NUnit.Analyzers.Tests.TestMethodAccessibilityLevel
{
    public class TestMethodAccessibilityLevelCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.TestMethodIsNotPublic);

        private static readonly object[] NonPublicModifiers =
        [
            new object[] { "private", "public" },
            new object[] { "protected", "public" },
            new object[] { "internal", "public" },
            new object[] { "protected internal", "public" },
            new object[] { "private protected", "public" },

            new object[] { "static private", "static public" },
            new object[] { "static protected", "static public" },
            new object[] { "static internal", "static public" },
            new object[] { "static protected internal", "static public" },
            new object[] { "static private protected", "static public" },

            new object[] { "private static", "public static" },
            new object[] { "protected static", "public static" },
            new object[] { "internal static", "public static" },
            new object[] { "protected internal static", "public static" },
            new object[] { "private protected static", "public static" },

            new object[] { "protected static internal", "public static" },
            new object[] { "private static protected", "public static" },
        ];

        [TestCaseSource(nameof(NonPublicModifiers))]
        public async Task FixesNonPublicAccessModifiers(string modifiers, string modifiersAfter)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase(1)]
                    {{modifiers}} void {|#0:TestMethod|}(int i) { }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase(1)]
                    {{modifiersAfter}} void TestMethod(int i) { }
            """);

            await Verify.VerifyCodeFixAsync(testCode, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task FixesDefaultImplicitAccessModifier()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase(1)]
                    void {|#0:TestMethod|}(int i) { }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase(1)]
                    public void TestMethod(int i) { }
            """);

            await Verify.VerifyCodeFixAsync(testCode, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task FixesDefaultImplicitAccessModifierWronglyIndented()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase(1)]
                    void {|#0:TestMethod|}(int i) { }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase(1)]
                    public void TestMethod(int i) { }
            """);

            await Verify.VerifyCodeFixAsync(testCode, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task FixesDefaultImplicitAccessModifierAsyncVariant()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase(1)]
                    async Task {|#0:TestMethod|}(int i) { await Task.CompletedTask; }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase(1)]
                    public async Task TestMethod(int i) { await Task.CompletedTask; }
            """);

            await Verify.VerifyCodeFixAsync(testCode, expectedDiagnostic.WithLocation(0), fixedCode);
        }
    }
}
