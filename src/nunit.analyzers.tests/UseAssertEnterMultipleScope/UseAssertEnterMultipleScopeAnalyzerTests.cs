#if NUNIT4
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.UseAssertEnterMultipleScope;

using NUnit.Framework;

namespace NUnit.Analyzers.Tests.UseAssertEnterMultipleScope
{
    [TestFixture]
    public sealed class UseAssertEnterMultipleScopeAnalyzerTests
    {
        private readonly DiagnosticAnalyzer analyzer = new UseAssertEnterMultipleScopeAnalyzer();
        private readonly ExpectedDiagnostic diagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.UseAssertEnterMultipleScope);

        [Test]
        public void AnalyzeWhenMultipleIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void Test()
        {
            ↓Assert.Multiple(() =>
            {
                Assert.That(true, Is.True);
                Assert.That(false, Is.False);
            });
        }");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenMultipleIsUsedWithAnAsyncDelegate()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void Test()
        {
            ↓Assert.Multiple(async () =>
            {
                Assert.That(true, Is.True);
                await Task.Yield();
                Assert.That(false, Is.False);
            });
        }");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenMultipleAsyncIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task Test()
        {
            await ↓Assert.MultipleAsync(async () =>
            {
                Assert.That(await Get1(), Is.Not.Null);
                Assert.That(await Get2(), Is.Not.Null);
            });

            static Task<string?> Get1() => Task.FromResult(default(string));
            static Task<string?> Get2() => Task.FromResult(default(string));
        }");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenMultipleAsyncAndValueTaskIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async ValueTask Test()
        {
            await ↓Assert.MultipleAsync(async () =>
            {
                Assert.That(await Get1(), Is.Not.Null);
                Assert.That(await Get2(), Is.Not.Null);
            });

            static Task<string?> Get1() => Task.FromResult(default(string));
            static Task<string?> Get2() => Task.FromResult(default(string));
        }");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenMultipleAsyncAndGenericTaskIsUsed()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class TestClass
    {
        [Test(ExpectedResult = 4)]
        public async Task<int> Test()
        {
            await ↓Assert.MultipleAsync(async () =>
            {
                Assert.That(await Get1(), Is.Not.Null);
                Assert.That(await Get2(), Is.Not.Null);
            });

            return 4;

            static Task<string?> Get1() => Task.FromResult(default(string));
            static Task<string?> Get2() => Task.FromResult(default(string));
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenMultipleIsNot()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void Test()
        {
            Assert.That(true, Is.True);
            Assert.That(false, Is.False);
        }");
            RoslynAssert.Valid(this.analyzer, testCode);
        }
    }
}
#endif
