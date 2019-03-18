using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.ConstActualValueUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ConstActualValueUsage
{
    public class ConstActualValueUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ConstActualValueUsageAnalyzer();
        private static readonly CodeFixProvider fix = new ConstActualValueUsageCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.ConstActualValueUsage);

        [Test]
        public void LiteralArgumentIsProvidedForAreEqualCodeFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    int expected = 5;
                    Assert.AreEqual(expected, ↓1);
                }");

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    int expected = 5;
                    Assert.AreEqual(1, expected);
                }");

            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode,
                fixTitle: CodeFixConstants.SwapArgumentsDescription);
        }


        [Test]
        public void LiteralNamedArgumentIsProvidedForAreEqualCodeFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    int expected = 5;
                    Assert.AreEqual(↓actual: 1, expected: expected);
                }");

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    int expected = 5;
                    Assert.AreEqual(actual: expected, expected: 1);
                }");

            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode,
                fixTitle: CodeFixConstants.SwapArgumentsDescription);
        }

        [Test]
        public void LiteralArgumentIsProvidedForAssertThatCodeFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    var expected = 2;
                    Assert.That(↓1, Is.EqualTo(expected));
                }");

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    var expected = 2;
                    Assert.That(expected, Is.EqualTo(1));
                }");

            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode,
                fixTitle: CodeFixConstants.SwapArgumentsDescription);
        }
    }
}
