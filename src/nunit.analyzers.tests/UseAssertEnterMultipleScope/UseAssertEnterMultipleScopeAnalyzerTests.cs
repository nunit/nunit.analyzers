#if NUNIT4
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;

using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.UseAssertEnterMultipleScope.UseAssertEnterMultipleScopeAnalyzer>;

namespace NUnit.Analyzers.Tests.UseAssertEnterMultipleScope
{
    [TestFixture]
    public sealed class UseAssertEnterMultipleScopeAnalyzerTests
    {
        private readonly DiagnosticResult diagnostic = Verify.Diagnostic(AnalyzerIdentifiers.UseAssertEnterMultipleScope);

        [Test]
        public async Task AnalyzeWhenMultipleIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        {|#0:Assert.Multiple(() =>
                        {
                            Assert.That(true, Is.True);
                            Assert.That(false, Is.False);
                        })|};
                    }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenMultipleIsUsedWithAnAsyncDelegate()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        {|#0:Assert.Multiple(async () =>
                        {
                            Assert.That(true, Is.True);
                            await Task.Yield();
                            Assert.That(false, Is.False);
                        })|};
                    }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenMultipleAsyncIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public async Task Test()
                    {
                        await {|#0:Assert.MultipleAsync(async () =>
                        {
                            Assert.That(await Get1(), Is.Not.Null);
                            Assert.That(await Get2(), Is.Not.Null);
                        })|};

                        static Task<string?> Get1() => Task.FromResult(default(string));
                        static Task<string?> Get2() => Task.FromResult(default(string));
                    }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenMultipleAsyncAndValueTaskIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public async ValueTask Test()
                    {
                        await {|#0:Assert.MultipleAsync(async () =>
                        {
                            Assert.That(await Get1(), Is.Not.Null);
                            Assert.That(await Get2(), Is.Not.Null);
                        })|};

                        static Task<string?> Get1() => Task.FromResult(default(string));
                        static Task<string?> Get2() => Task.FromResult(default(string));
                    }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenMultipleAsyncAndGenericTaskIsUsed()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class TestClass
                {
                    [Test(ExpectedResult = 4)]
                    public async Task<int> Test()
                    {
                        await {|#0:Assert.MultipleAsync(async () =>
                        {
                            Assert.That(await Get1(), Is.Not.Null);
                            Assert.That(await Get2(), Is.Not.Null);
                        })|};

                        return 4;

                        static Task<string?> Get1() => Task.FromResult(default(string));
                        static Task<string?> Get2() => Task.FromResult(default(string));
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenMultipleIsNot()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        Assert.That(true, Is.True);
                        Assert.That(false, Is.False);
                    }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
#endif
