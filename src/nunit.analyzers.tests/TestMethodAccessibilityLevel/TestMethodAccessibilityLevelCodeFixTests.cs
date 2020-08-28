using System.Collections.Generic;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TestMethodAccessibilityLevel;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.TestMethodAccessibilityLevel
{
    public class TestMethodAccessibilityLevelCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new TestMethodAccessibilityLevelAnalyzer();
        private static readonly CodeFixProvider fix = new TestMethodAccessibilityLevelCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.TestMethodIsNotPublic);

        private static readonly object[] NonPublicModifiers =
        {
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
        };

        [TestCaseSource(nameof(NonPublicModifiers))]
        public void FixesNonPublicAccessModifiers(string modifiers, string modifiersAfter)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [TestCase(1)]
        {modifiers} void ↓TestMethod(int i) {{ }}");

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [TestCase(1)]
        {modifiersAfter} void TestMethod(int i) {{ }}");

            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void FixesDefaultImplicitAccessModifier()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [TestCase(1)]
        void ↓TestMethod(int i) {{ }}");

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [TestCase(1)]
        public void TestMethod(int i) {{ }}");

            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void FixesDefaultImplicitAccessModifierWronglyIndented()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
    [TestCase(1)]
        void ↓TestMethod(int i) {{ }}");

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
    [TestCase(1)]
        public void TestMethod(int i) {{ }}");

            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void FixesDefaultImplicitAccessModifierAsyncVariant()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [TestCase(1)]
        async Task ↓TestMethod(int i) {{ }}");

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [TestCase(1)]
        public async Task TestMethod(int i) {{ }}");

            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, testCode, fixedCode);
        }
    }
}
