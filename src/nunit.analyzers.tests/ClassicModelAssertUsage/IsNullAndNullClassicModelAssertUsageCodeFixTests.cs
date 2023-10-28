using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class IsNullAndNullClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ClassicModelAssertUsageAnalyzer();
        private static readonly CodeFixProvider fix = new IsNullAndNullClassicModelAssertUsageCodeFix();

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new IsNullAndNullClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.IsNullUsage, AnalyzerIdentifiers.NullUsage }));
        }

        [TestCase("IsNull", AnalyzerIdentifiers.IsNullUsage)]
        [TestCase("Null", AnalyzerIdentifiers.NullUsage)]
        public void VerifyIsNullAndNullFixes(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            object? obj = null;
            ↓ClassicAssert.{assertion}(obj);
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            object? obj = null;
            Assert.That(obj, Is.Null);
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsNull", AnalyzerIdentifiers.IsNullUsage)]
        [TestCase("Null", AnalyzerIdentifiers.NullUsage)]
        public void VerifyIsNullAndNullFixesWithMessage(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            object? obj = null;
            ↓ClassicAssert.{assertion}(obj, ""message"");
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            object? obj = null;
            Assert.That(obj, Is.Null, ""message"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsNull", AnalyzerIdentifiers.IsNullUsage)]
        [TestCase("Null", AnalyzerIdentifiers.NullUsage)]
        public void VerifyIsNullAndNullFixesWithMessageAndParams(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            object? obj = null;
            ↓ClassicAssert.{assertion}(obj, ""message-id: {{0}}"", Guid.NewGuid());
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            object? obj = null;
            Assert.That(obj, Is.Null, $""message-id: {Guid.NewGuid()}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
