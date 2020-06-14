using System.Collections.Generic;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TestMethodAccessibilityLevel;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.TestMethodAccessibilityLevel
{
    public class TestMethodAccessibilityLevelAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new TestMethodAccessibilityLevelAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.TestMethodIsNotPublic);

        [Test]
        public void AnalyzeWhenSimpleTestMethodIsPublic()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [Test]
        public void TestMethod() {{ }}");
            AnalyzerAssert.Valid<TestMethodAccessibilityLevelAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenTestMethodIsPublic()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [TestCase(1)]
        public void TestMethod(int i) {{ }}");
            AnalyzerAssert.Valid<TestMethodAccessibilityLevelAnalyzer>(testCode);
        }

        [TestCaseSource(nameof(NonPublicModifiers))]
        public void AnalyzeWhenSimpleTestMethodIsNotPublic(string modifiers)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [Test]
        {modifiers} void ↓TestMethod() {{ }}");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [TestCaseSource(nameof(NonPublicModifiers))]
        public void AnalyzeWhenTestMethodIsNotPublic(string modifiers)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [TestCase(1)]
        {modifiers} void ↓TestMethod(int i) {{ }}");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [TestCaseSource(nameof(NonPublicModifiers))]
        public void AnalyzeWhenAsyncSimpleTestMethodIsNotPublic(string modifiers)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [Test]
        {modifiers} async Task ↓TestMethod() {{ }}");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [TestCaseSource(nameof(NonPublicModifiers))]
        public void AnalyzeWhenAsyncTestMethodIsNotPublic(string modifiers)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [TestCase(1)]
        {modifiers} async Task ↓TestMethod(int i) {{ }}");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        static IEnumerable<string> NonPublicModifiers => new string[]
        {
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
        };
    }
}
