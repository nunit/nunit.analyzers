using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.UseAssertMultiple;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.UseAssertMultiple.UseAssertMultipleAnalyzer, NUnit.Analyzers.UseAssertMultiple.UseAssertMultipleCodeFix>;

namespace NUnit.Analyzers.Tests.UseAssertMultiple
{
    [TestFixture]
    public sealed class UseAssertMultipleCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.UseAssertMultiple);

        private static readonly string[] newlines = ["", Environment.NewLine];
        private static readonly string[] postComments = ["", "// Same line Comment", $"{Environment.NewLine}            // Final Comment on next line"];

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new UseAssertMultipleCodeFix();
            var ids = new UseAssertMultipleCodeFix().FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.UseAssertMultiple }));
        }

        [Test]
        public async Task VerifyIndependent()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:Assert.That(true, Is.True)|};
                        Assert.That(false, Is.False);
                        Console.WriteLine("Next Statement");
                    }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.Multiple(() =>
                        {
                            Assert.That(true, Is.True);
                            Assert.That(false, Is.False);
                        });
                        Console.WriteLine("Next Statement");
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, UseAssertMultipleCodeFix.WrapWithAssertMultiple);

#if NUNIT4
            fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        using (Assert.EnterMultipleScope())
                        {
                            Assert.That(true, Is.True);
                            Assert.That(false, Is.False);
                        }
                        Console.WriteLine("Next Statement");
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, UseAssertMultipleCodeFix.WrapWithAssertEnterMultipleScope);
#endif
        }

        [Test]
        public async Task VerifyPartlyIndependent()
        {
            const string configurationClass = """
                    private sealed class Configuration
                    {
                        public int Value1 { get; set; }
                        public double Value2 { get; set; }
                        public string Value11 { get; set; } = string.Empty;
                    }
            """;

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        var configuration = new Configuration();
                        Assert.That(configuration, Is.Not.Null);
                        {|#0:Assert.That(configuration.Value1, Is.EqualTo(0))|};
                        Assert.That(configuration.Value2, Is.EqualTo(0.0));
                        Assert.That(configuration.Value11, Is.EqualTo(string.Empty));
                        configuration = null;
                    }
            """ + "\n" + configurationClass);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
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
            """ + "\n" + configurationClass);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, UseAssertMultipleCodeFix.WrapWithAssertMultiple);

#if NUNIT4
            fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
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
                    }
            """ + "\n" + configurationClass);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, UseAssertMultipleCodeFix.WrapWithAssertEnterMultipleScope);
#endif
        }

        [Test]
        public async Task AddsAsyncWhenAwaitIsUsed()
        {
            const string configurationClass = """
                    private sealed class Configuration
                    {
                        public int Value1 { get; set; }
                        public double Value2 { get; set; }
                        public string Value11 { get; set; } = string.Empty;
                        public Task<string> AsStringAsync() => Task.FromResult(Value11);
                    }
            """;

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public async Task Test()
                    {
                        var configuration = new Configuration();
                        Assert.That(configuration, Is.Not.Null);
                        {|#0:Assert.That(configuration.Value1, Is.EqualTo(0))|};
                        Assert.That(configuration.Value2, Is.EqualTo(0.0));
                        Assert.That(await configuration.AsStringAsync(), Is.EqualTo(string.Empty));
                        configuration = null;
                    }
            """ + "\n" + configurationClass);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
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
                    }
            """ + "\n" + configurationClass);

            // The test method itself no longer awaits, so CS1998 is generated.
            // Fixing this is outside the scope of this analyzer and there could be other non-touched statements that are waited.
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, UseAssertMultipleCodeFix.WrapWithAssertMultiple, test => test.CompilerDiagnostics = CompilerDiagnostics.None);

#if NUNIT4
            fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
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
                    }
            """ + "\n" + configurationClass);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, UseAssertMultipleCodeFix.WrapWithAssertEnterMultipleScope);
#endif
        }

        [Test]
        public async Task VerifyKeepsTrivia(
            [ValueSource(nameof(newlines))] string newline,
            [Values("", "// ")] string preComment,
            [ValueSource(nameof(postComments))] string postComment)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        const bool True = true;
                        const bool False = false;

                        // Verify that our bool constants are correct
                        {|#0:Assert.That(True, Is.True)|};
                        Assert.That(False, Is.False);{{newline}}
                        {{preComment}}Console.WriteLine("Next Statement");{{postComment}}
                    }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        const bool True = true;
                        const bool False = false;

                        Assert.Multiple(() =>
                        {
                            // Verify that our bool constants are correct
                            Assert.That(True, Is.True);
                            Assert.That(False, Is.False);
                        });{{newline}}
                        {{preComment}}Console.WriteLine("Next Statement");{{postComment}}
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, UseAssertMultipleCodeFix.WrapWithAssertMultiple);

#if NUNIT4
            fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        const bool True = true;
                        const bool False = false;

                        using (Assert.EnterMultipleScope())
                        {
                            // Verify that our bool constants are correct
                            Assert.That(True, Is.True);
                            Assert.That(False, Is.False);
                        }{{newline}}
                        {{preComment}}Console.WriteLine("Next Statement");{{postComment}}
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, UseAssertMultipleCodeFix.WrapWithAssertEnterMultipleScope);
#endif
        }

        [Test]
        public async Task VerifyKeepsTrivia()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        // Verify that boolean work as expected
                        {|#0:Assert.That(true, Is.True)|};
                        Assert.That(false, Is.False);
                    }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.Multiple(() =>
                        {
                            // Verify that boolean work as expected
                            Assert.That(true, Is.True);
                            Assert.That(false, Is.False);
                        });
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, UseAssertMultipleCodeFix.WrapWithAssertMultiple);

#if NUNIT4
            fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        using (Assert.EnterMultipleScope())
                        {
                            // Verify that boolean work as expected
                            Assert.That(true, Is.True);
                            Assert.That(false, Is.False);
                        }
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, UseAssertMultipleCodeFix.WrapWithAssertEnterMultipleScope);
#endif
        }
    }
}
