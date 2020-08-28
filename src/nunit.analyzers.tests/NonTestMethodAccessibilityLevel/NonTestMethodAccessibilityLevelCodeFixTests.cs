using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.NonTestMethodAccessibilityLevel;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.TestMethodAccessibilityLevel
{
    public class NonTestMethodAccessibilityLevelCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new NonTestMethodAccessibilityLevelAnalyzer();
        private static readonly CodeFixProvider fix = new NonTestMethodAccessibilityLevelCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.NonTestMethodIsPublic);

        private static readonly object[] NonPrivateModifiers =
        {
            new object[] { "public", "private" },
            new object[] { "internal", "private" },
            new object[] { "protected internal", "protected" },
            new object[] { "static public", "static private" },
            new object[] { "static internal", "static private" },
            new object[] { "static protected internal", "static protected" },
        };

        [TestCaseSource(nameof(NonPrivateModifiers))]
        public void FixesPublicAccessModifiers(string modifiers, string modifiersAfter)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [Test]
        public void TestMethod() {{ AssertMethod(); }}
        {modifiers} void ↓AssertMethod() {{ }}");

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [Test]
        public void TestMethod() {{ AssertMethod(); }}
        {modifiersAfter} void AssertMethod() {{ }}");

            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, testCode, fixedCode);
        }
    }
}
