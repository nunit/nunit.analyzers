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

        [Test]
        public void AddsAsyncWhenAwaitIsUsed()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void Test()
        {
            var configuration = new Configuration();
            Assert.That(configuration, Is.Not.Null);
            ↓Assert.That(configuration.Value1, Is.EqualTo(0));
            ↓Assert.That(configuration.Value2, Is.EqualTo(0.0));
            Assert.That(await configuration.AsStringAsync(), Is.EqualTo(string.Empty));
            configuration = null;
        }

        private sealed class Configuration
        {
            public int Value1 { get; set; }
            public double Value2 { get; set; }
            public string Value11 { get; set; } = string.Empty;
            public Task<string> AsStringAsync() => Task.FromResult(Value11);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void Test()
        {
            var configuration = new Configuration();
            Assert.That(configuration, Is.Not.Null);
            Assert.Multiple(async () =>
            {
                Assert.That(configuration.Value1, Is.EqualTo(0));
                Assert.That(configuration.Value2, Is.EqualTo(0.0));
                Assert.That(await configuration.AsStringAsync(), Is.EqualTo(string.Empty));
            });
            configuration = null;
        }

        private sealed class Configuration
        {
            public int Value1 { get; set; }
            public double Value2 { get; set; }
            public string Value11 { get; set; } = string.Empty;
            public Task<string> AsStringAsync() => Task.FromResult(Value11);
        }");
            RoslynAssert.FixAll(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCase("            // ", "")]
        [TestCase("\r\n            // ", "")]
        [TestCase("\r\n            ", "")]
        [TestCase("\r\n            ", " // Same line Comment")]
        [TestCase("\r\n            ", "\r\n            // Final Comment on next line")]
        public void VerifyKeepsTrivia(string separation, string comment)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
        public void TestMethod()
        {{
            const bool True = true;
            const bool False = false;

            // Verify that our bool constants are correct
            ↓Assert.That(True, Is.True);
            Assert.That(False, Is.False);
{separation}Console.WriteLine(""Next Statement"");{comment}
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
        public void TestMethod()
        {{
            const bool True = true;
            const bool False = false;

            Assert.Multiple(() =>
            {{
                // Verify that our bool constants are correct
                Assert.That(True, Is.True);
                Assert.That(False, Is.False);
            }});
{separation}Console.WriteLine(""Next Statement"");{comment}
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void VerifyKeepsTrivia()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
        public void TestMethod()
        {{
                // Verify that boolean work as expected
            ↓Assert.That(true, Is.True);
            Assert.That(false, Is.False);
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
        public void TestMethod()
        {{
            Assert.Multiple(() =>
            {{
                // Verify that boolean work as expected
                Assert.That(true, Is.True);
                Assert.That(false, Is.False);
            }});
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }
    }
}
