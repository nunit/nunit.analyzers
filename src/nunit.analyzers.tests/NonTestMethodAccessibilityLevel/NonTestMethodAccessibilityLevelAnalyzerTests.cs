using System.Collections.Generic;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.NonTestMethodAccessibilityLevel;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.NonTestMethodAccessibilityLevel
{
    public class NonTestMethodAccessibilityLevelAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new NonTestMethodAccessibilityLevelAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.NonTestMethodIsPublic);

        private static IEnumerable<string> TestMethodRelatedAttributes => new string[]
        {
            "Test",
            "OneTimeSetUp",
            "OneTimeTearDown",
            "SetUp",
            "TearDown"
        };

        private static IEnumerable<string> NonPrivateModifiers => new string[]
        {
            "public",
            "internal",
            "protected internal",

            "static public",
            "static internal",
            "static protected internal",
        };

        [Test]
        public void AnalyzeWhenNonTestClassHasPublicMethod()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void AssertMethod() {{ }}");
            AnalyzerAssert.Valid<NonTestMethodAccessibilityLevelAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenNonTestMethodIsNotPublic()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [Test]
        public void TestMethod() {{ AssertMethod(); }}
        private void AssertMethod() {{ }}");
            AnalyzerAssert.Valid<NonTestMethodAccessibilityLevelAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenNonTestMethodIsProtected()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [Test]
        public void TestMethod() {{ AssertMethod(); }}
        protected virtual void AssertMethod() {{ }}");
            AnalyzerAssert.Valid<NonTestMethodAccessibilityLevelAnalyzer>(testCode);
        }

        [TestCaseSource(nameof(TestMethodRelatedAttributes))]
        public void AnalyzeWhenAuxiliaryTestMethodIsPublic(string attribute)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [Test]
        public void TestMethod() {{ }}
        [{attribute}]
        public void AuxiliaryMethod() {{ }}");
            AnalyzerAssert.Valid<NonTestMethodAccessibilityLevelAnalyzer>(testCode);
        }

        [TestCaseSource(nameof(NonPrivateModifiers))]
        public void AnalyzeWhenNonTestMethodIsPublicOrInternal(string modifiers)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [TestCase(1)]
        public void TestMethod(int i) {{ AssertMethod(); }}
        {modifiers} void ↓AssertMethod() {{ }}");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }
    }
}
