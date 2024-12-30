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

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, UseAssertMultipleCodeFix.WrapWithAssertMultiple);

#if NUNIT4
            fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(true, Is.True);
                Assert.That(false, Is.False);
            }
            Console.WriteLine(""Next Statement"");
        }");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, UseAssertMultipleCodeFix.WrapWithAssertEnterMultipleScope);
#endif
        }

        [Test]
        public void VerifyPartlyIndependent()
        {
            const string ConfigurationClass = @"
        private sealed class Configuration
        {
            public int Value1 { get; set; }
            public double Value2 { get; set; }
            public string Value11 { get; set; } = string.Empty;
        }";

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void Test()
        {
            var configuration = new Configuration();
            Assert.That(configuration, Is.Not.Null);
            ↓Assert.That(configuration.Value1, Is.EqualTo(0));
            Assert.That(configuration.Value2, Is.EqualTo(0.0));
            Assert.That(configuration.Value11, Is.EqualTo(string.Empty));
            configuration = null;
        }" + ConfigurationClass);

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
        }" + ConfigurationClass);

            RoslynAssert.FixAll(analyzer, fix, expectedDiagnostic, code, fixedCode, UseAssertMultipleCodeFix.WrapWithAssertMultiple);

#if NUNIT4
            fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void Test()
        {
            var configuration = new Configuration();
            Assert.That(configuration, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(configuration.Value1, Is.EqualTo(0));
                Assert.That(configuration.Value2, Is.EqualTo(0.0));
                Assert.That(configuration.Value11, Is.EqualTo(string.Empty));
            }
            configuration = null;
        }" + ConfigurationClass);

            RoslynAssert.FixAll(analyzer, fix, expectedDiagnostic, code, fixedCode, UseAssertMultipleCodeFix.WrapWithAssertEnterMultipleScope);
#endif
        }

        [Test]
        public void AddsAsyncWhenAwaitIsUsed()
        {
            const string ConfigurationClass = @"
        private sealed class Configuration
        {
            public int Value1 { get; set; }
            public double Value2 { get; set; }
            public string Value11 { get; set; } = string.Empty;
            public Task<string> AsStringAsync() => Task.FromResult(Value11);
        }";

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task Test()
        {
            var configuration = new Configuration();
            Assert.That(configuration, Is.Not.Null);
            ↓Assert.That(configuration.Value1, Is.EqualTo(0));
            Assert.That(configuration.Value2, Is.EqualTo(0.0));
            Assert.That(await configuration.AsStringAsync(), Is.EqualTo(string.Empty));
            configuration = null;
        }" + ConfigurationClass);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task Test()
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
        }" + ConfigurationClass);

            // The test method itself no longer awaits, so CS1998 is generated.
            // Fixing this is outside the scope of this analyzer and there could be other non-touched statements that are waited.
            RoslynAssert.FixAll(analyzer, fix, expectedDiagnostic, code, fixedCode, UseAssertMultipleCodeFix.WrapWithAssertMultiple,
                Settings.Default.WithAllowedCompilerDiagnostics(AllowedCompilerDiagnostics.WarningsAndErrors));

#if NUNIT4
            fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task Test()
        {
            var configuration = new Configuration();
            Assert.That(configuration, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(configuration.Value1, Is.EqualTo(0));
                Assert.That(configuration.Value2, Is.EqualTo(0.0));
                Assert.That(await configuration.AsStringAsync(), Is.EqualTo(string.Empty));
            }
            configuration = null;
        }" + ConfigurationClass);

            RoslynAssert.FixAll(analyzer, fix, expectedDiagnostic, code, fixedCode, UseAssertMultipleCodeFix.WrapWithAssertEnterMultipleScope);
#endif
        }

        [Test]
        public void VerifyKeepsTrivia(
            [Values("", "\r\n")] string newline,
            [Values("", "// ")] string preComment,
            [Values("", "// Same line Comment", "\r\n            // Final Comment on next line")] string postComment)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
        public void TestMethod()
        {{
            const bool True = true;
            const bool False = false;

            // Verify that our bool constants are correct
            ↓Assert.That(True, Is.True);
            Assert.That(False, Is.False);{newline}
            {preComment}Console.WriteLine(""Next Statement"");{postComment}
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
            }});{newline}
            {preComment}Console.WriteLine(""Next Statement"");{postComment}
        }}");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, UseAssertMultipleCodeFix.WrapWithAssertMultiple);

#if NUNIT4
            fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
        public void TestMethod()
        {{
            const bool True = true;
            const bool False = false;

            using (Assert.EnterMultipleScope())
            {{
                // Verify that our bool constants are correct
                Assert.That(True, Is.True);
                Assert.That(False, Is.False);
            }}{newline}
            {preComment}Console.WriteLine(""Next Statement"");{postComment}
        }}");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, UseAssertMultipleCodeFix.WrapWithAssertEnterMultipleScope);
#endif
        }

        [Test]
        public void VerifyKeepsTrivia()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            // Verify that boolean work as expected
            ↓Assert.That(true, Is.True);
            Assert.That(false, Is.False);
        }");

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.Multiple(() =>
            {
                // Verify that boolean work as expected
                Assert.That(true, Is.True);
                Assert.That(false, Is.False);
            });
        }");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, UseAssertMultipleCodeFix.WrapWithAssertMultiple);

#if NUNIT4
            fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            using (Assert.EnterMultipleScope())
            {
                // Verify that boolean work as expected
                Assert.That(true, Is.True);
                Assert.That(false, Is.False);
            }
        }");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, UseAssertMultipleCodeFix.WrapWithAssertEnterMultipleScope);
#endif
        }
    }
}
