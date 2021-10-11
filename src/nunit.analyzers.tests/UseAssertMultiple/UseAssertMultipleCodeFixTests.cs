using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.UseAssertMultiple;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.UseAssertMultiple
{
    [TestFixture]
    public sealed class UseAssertMultipleCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new UseAssertMultipleAnalyzer();
        private static readonly CodeFixProvider fix = new UseAssertMultipleCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.UseAssertMultiple);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new UseAssertMultipleCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.UseAssertMultiple }));
        }

        [Test]
        public void VerifyIndependent()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓Assert.That(true, Is.True);
            Assert.That(false, Is.False);
            Console.WriteLine(""Next Statement"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.Multiple(() =>
            {
                Assert.That(true, Is.True);
                Assert.That(false, Is.False);
            });
            Console.WriteLine(""Next Statement"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void VerifyPartlyIndependent()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void Test()
        {
            var configuration = new Configuration();
            Assert.That(configuration, Is.Not.Null);
            ↓Assert.That(configuration.Value1, Is.EqualTo(0));
            ↓Assert.That(configuration.Value2, Is.EqualTo(0.0));
            Assert.That(configuration.Value11, Is.EqualTo(string.Empty));
            configuration = null;
        }

        private sealed class Configuration
        {
            public int Value1 { get; set; }
            public double Value2 { get; set; }
            public string Value11 { get; set; } = string.Empty;
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void Test()
        {
            var configuration = new Configuration();
            Assert.That(configuration, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(configuration.Value1, Is.EqualTo(0));
                Assert.That(configuration.Value2, Is.EqualTo(0.0));
                Assert.That(configuration.Value11, Is.EqualTo(string.Empty));
            });
            configuration = null;
        }

        private sealed class Configuration
        {
            public int Value1 { get; set; }
            public double Value2 { get; set; }
            public string Value11 { get; set; } = string.Empty;
        }");
            RoslynAssert.FixAll(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }
    }
}
