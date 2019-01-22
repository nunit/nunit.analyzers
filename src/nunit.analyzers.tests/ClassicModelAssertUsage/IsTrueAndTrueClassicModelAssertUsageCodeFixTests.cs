using System.Collections.Immutable;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class IsTrueAndTrueClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ClassicModelAssertUsageAnalyzer();
        private static readonly CodeFixProvider fix = new IsTrueAndTrueClassicModelAssertUsageCodeFix();

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new IsTrueAndTrueClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds.ToImmutableArray();

            Assert.That(ids.Length, Is.EqualTo(2), nameof(ids.Length));
            Assert.That(ids, Contains.Item(NUnit3.Id), nameof(NUnit3.Id));
            Assert.That(ids, Contains.Item(AnalyzerIdentifiers.TrueUsage), nameof(AnalyzerIdentifiers.TrueUsage));
        }

        [TestCase("IsTrue", NUnit3.Id)]
        [TestCase("True", AnalyzerIdentifiers.TrueUsage)]
        public void VerifyIsTrueAndTrueFixes(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            ↓Assert.{assertion}(true);
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(true, Is.True);
        }");
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [TestCase("IsTrue", NUnit3.Id)]
        [TestCase("True", AnalyzerIdentifiers.TrueUsage)]
        public void VerifyIsTrueAndTrueFixesWithMessage(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            ↓Assert.{assertion}(true, ""message"");
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(true, Is.True, ""message"");
        }");
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [TestCase("IsTrue", NUnit3.Id)]
        [TestCase("True", AnalyzerIdentifiers.TrueUsage)]
        public void VerifyIsTrueAndTrueFixesWithMessageAndParams(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            ↓Assert.{assertion}(true, ""message"", Guid.NewGuid());
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(true, Is.True, ""message"", Guid.NewGuid());
        }");
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }
    }
}
