#if !NUNIT4

using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.UpdateStringFormatToInterpolatableString;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.UpdateStringFormatToInterpolatableString
{
    [TestFixture]
    public sealed class UpdateStringFormatToInterpolatableStringCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new UpdateStringFormatToInterpolatableStringAnalyzer();
        private static readonly CodeFixProvider fix = new UpdateStringFormatToInterpolatableStringCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.UpdateStringFormatToInterpolatableString);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.UpdateStringFormatToInterpolatableString }));
        }

        [TestCase(NUnitFrameworkConstants.NameOfAssertIgnore)]
        public void ConvertWhenNoMinimumParameters(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
                ↓Assert.{method}(""Method: {{0}}"", ""{method}"");
            ");
            var fixedCode = TestUtility.WrapInTestMethod($@"
                Assert.{method}($""Method: {{""{method}""}}"");
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void ConvertWhenMinimumParametersIsOne()
        {
            var code = TestUtility.WrapInTestMethod(@"
                const bool actual = false;
                ↓Assert.That(actual, ""Expected actual value to be true, but was: {0}"", actual);
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@"
                const bool actual = false;
                Assert.That(actual, $""Expected actual value to be true, but was: {actual}"");
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void ConvertWhenMinimumParametersIsTwo()
        {
            var code = TestUtility.WrapInTestMethod(@"
                const int actual = 42;
                ↓Assert.That(actual, Is.EqualTo(42), ""Expected actual value to be 42, but was: {0}"", actual);
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@"
                const int actual = 42;
                Assert.That(actual, Is.EqualTo(42), $""Expected actual value to be 42, but was: {actual}"");
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        // We need to double the backslashes as the text is formatted and we want the code to contain \n and not LF
        [TestCase("Passed: {0}")]
        [TestCase("Passed: {0} {{")]
        [TestCase("Passed: {{{0}}}")]
        [TestCase("{{{0}}}")]
        [TestCase("Passed: \\\"{0}\\\"")]
        [TestCase("Passed:\\n{0}")]
        [TestCase("Passed:\\t\\\"{0}\\\"")]
        public void TestConvertWithEmbeddedSpecialCharacters(string text)
        {
            var code = TestUtility.WrapInTestMethod($"↓Assert.Pass(\"{text}\", 42);");

            string interpolatableText = text.Replace("{0}", "{42}");
            var fixedCode = TestUtility.WrapInTestMethod($"Assert.Pass($\"{interpolatableText}\");");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }
    }
}
#endif
