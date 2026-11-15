using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.UseAssertMultiple.UseAssertMultipleAnalyzer>;

namespace NUnit.Analyzers.Tests.UseAssertMultiple
{
    [TestFixture]
    public sealed class UseAssertMultipleAnalyzerTests
    {
        private readonly DiagnosticResult diagnostic = Verify.Diagnostic(AnalyzerIdentifiers.UseAssertMultiple);

        [Test]
        public async Task AnalyzeWhenMultipleIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        Assert.Multiple(() =>
                        {
                            Assert.That(true, Is.True);
                            Assert.That(false, Is.False);
                        });
                    }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

#if NUNIT4
        [Test]
        public async Task AnalyzeWhenMultipleAsyncIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public async Task Test()
                    {
                        await Assert.MultipleAsync(async () =>
                        {
                            Assert.That(await Get1(), Is.Not.Null);
                            Assert.That(await Get2(), Is.Not.Null);
                        });

                        static Task<string?> Get1() => Task.FromResult(default(string));
                        static Task<string?> Get2() => Task.FromResult(default(string));
                    }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenMultipleScopeDeclarationIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        using IDisposable disposable = Assert.EnterMultipleScope();

                        Assert.That(true, Is.True);
                        Assert.That(false, Is.False);
                    }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenMultipleScopeDeclarationIsUsedAfterPossibleLocation()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        {|#0:Assert.That(true, Is.True)|};
                        Assert.That(false, Is.False);

                        using IDisposable disposable = Assert.EnterMultipleScope();

                        Assert.That(true, Is.True);
                        Assert.That(false, Is.False);
                    }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, Verify.Diagnostic().WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenMultipleScopeStatementIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        using (Assert.EnterMultipleScope())
                        {
                            Assert.That(true, Is.True);
                            Assert.That(false, Is.False);
                        }
                    }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }
#endif

        [Test]
        public async Task AnalyzeWhenDependent()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        byte[] instance = new byte[] { 0xBB };
                        Assert.That(instance, Is.Not.Null);
                        Assert.That(instance, Has.Length.EqualTo(1));
                        Assert.That(instance.Length, Is.EqualTo(1));
                        Assert.That(instance[0], Is.EqualTo(0xBB));
                    }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenPartlyDependent()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        var configuration = new Configuration();
                        Assert.That(configuration, Is.Not.Null);
                        {|#0:Assert.That(configuration.Value1, Is.EqualTo(0))|};
                        Assert.That(configuration.Value2, Is.EqualTo(0.0));
                        Assert.That(configuration.Value11, Is.EqualTo(string.Empty));
                    }

                    private sealed class Configuration
                    {
                        public int Value1 { get; set; }
                        public double Value2 { get; set; }
                        public string Value11 { get; set; } = string.Empty;
                    }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenIndependent()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        {|#0:Assert.That(true, Is.True)|};
                        Assert.That(false, Is.False);
                    }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenNoParameterIsUsedInFirstCall()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        Assert.Fail();
                        Assert.That(string.Empty, Has.Count.EqualTo(1));
                    }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenNoParameterIsUsedInSecondCall()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        Assert.That(string.Empty, Has.Count.EqualTo(1));
                        Assert.Fail();
                    }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenUsingAnonymousLambda()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        object? actualDeserialized = null;

                        Assert.That(() => actualDeserialized = Calculate(), Throws.Nothing);
                        Assert.That(actualDeserialized, Is.Not.Null);

                        static object? Calculate() => new object();
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenUsingTestDelegate()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        object? actualDeserialized = null;

                        Assert.That(Calculate, Throws.Nothing);
                        Assert.That(actualDeserialized, Is.Not.Null);

                        void Calculate() => actualDeserialized = new object();
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
