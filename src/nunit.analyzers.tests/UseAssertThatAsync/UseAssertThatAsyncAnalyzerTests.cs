using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.UseAssertThatAsync;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.UseAssertThatAsync;

[TestFixture]
public sealed class UseAssertThatAsyncAnalyzerTests
{
    private static readonly DiagnosticAnalyzer analyzer = new UseAssertThatAsyncAnalyzer();
    private static readonly ExpectedDiagnostic diagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.UseAssertThatAsync);

    [Test]
    public void AnalyzeWhenResultIsUsed()
    {
        var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void Test()
        {
            Assert.That(GetResultAsync().Result, Is.EqualTo(42));
        }

        private static Task<int> GetResultAsync() => Task.FromResult(42);");
        RoslynAssert.Valid(analyzer, testCode);
    }

    [Test]
    public void AnalyzeWhenAwaitIsNotUsedInLine()
    {
        var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task Test()
        {
            var fourtyTwo = await GetResultAsync();
            Assert.That(fourtyTwo, Is.EqualTo(42));
        }

        private static Task<int> GetResultAsync() => Task.FromResult(42);");
        RoslynAssert.Valid(analyzer, testCode);
    }

    /* TODO:
    #if NUNIT4
        [Test]
        public void AnalyzeWhenMultipleAsyncIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
            public async Task Test()
            {
                await Assert.MultipleAsync(async () =>
                {
                    Assert.That(await Get1(), Is.Not.Null);
                    Assert.That(await Get2(), Is.Not.Null);
                });

                static Task<string?> Get1() => Task.FromResult(default(string));
                static Task<string?> Get2() => Task.FromResult(default(string));
            }");
            RoslynAssert.Valid(analyzer, testCode);
        }
    #endif
    */

    [Test]
    public void AnalyzeWhenAwaitItsUsedWithConfigureAwait()
    {
        var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task Test()
        {
            Assert.That(await GetResultAsync().ConfigureAwait(false), Is.EqualTo(42));
        }

        private static Task<int> GetResultAsync() => Task.FromResult(42);");
        RoslynAssert.Diagnostics(analyzer, diagnostic, testCode);
    }

    [Test]
    public void AnalyzeWhenAwaitIsUsedWithoutConfigureAwait()
    {
        var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task Test()
        {
            ↓Assert.That(await GetResultAsync(), Is.EqualTo(42));
        }

        private static Task<int> GetResultAsync() => Task.FromResult(42);");
        RoslynAssert.Diagnostics(analyzer, diagnostic, testCode);
    }

    [Test]
    public void AnalyzeWhenAwaitIsUsedWithoutConfigureAwaitAsSecondArgument()
    {
        var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task Test()
        {
            ↓Assert.That(expression: Is.EqualTo(42), actual: await GetResultAsync());
        }

        private static Task<int> GetResultAsync() => Task.FromResult(42);");
        RoslynAssert.Diagnostics(analyzer, diagnostic, testCode);
    }
}
