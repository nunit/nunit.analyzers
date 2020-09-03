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
    public sealed class ContainsClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ClassicModelAssertUsageAnalyzer();
        private static readonly CodeFixProvider fix = new ContainsClassicModelAssertUsageCodeFix();
        private static readonly ExpectedDiagnostic instanceDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.ContainsUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new ContainsClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.ContainsUsage }));
        }

        [Test]
        public void VerifyContainsFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var instance = new object();
            var collection = Array.Empty<object>();

            ↓Assert.Contains(instance, collection);
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var instance = new object();
            var collection = Array.Empty<object>();

            Assert.That(collection, Does.Contain(instance));
        }");
            AnalyzerAssert.CodeFix(analyzer, fix, instanceDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyContainsFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var instance = new object();
            var collection = Array.Empty<object>();

            ↓Assert.Contains(instance, collection, ""message"");
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var instance = new object();
            var collection = Array.Empty<object>();

            Assert.That(collection, Does.Contain(instance), ""message"");
        }");
            AnalyzerAssert.CodeFix(analyzer, fix, instanceDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyContainsFixWithMessageAndParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var instance = new object();
            var collection = Array.Empty<object>();

            ↓Assert.Contains(instance, collection, ""message"", Guid.NewGuid());
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var instance = new object();
            var collection = Array.Empty<object>();

            Assert.That(collection, Does.Contain(instance), ""message"", Guid.NewGuid());
        }");
            AnalyzerAssert.CodeFix(analyzer, fix, instanceDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }
    }
}
