#if NUNIT4
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.UseAssertThatAsync;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.UseAssertThatAsync;

[TestFixture]
public sealed class UseAssertThatAsyncCodeFixTests
{
    private static readonly DiagnosticAnalyzer analyzer = new UseAssertThatAsyncAnalyzer();
    private static readonly CodeFixProvider fix = new UseAssertThatAsyncCodeFix();
    private static readonly ExpectedDiagnostic expectedDiagnostic =
        ExpectedDiagnostic.Create(AnalyzerIdentifiers.UseAssertThatAsync);

    [Test]
    public void VerifyGetFixableDiagnosticIds()
    {
        var fix = new UseAssertThatAsyncCodeFix();
        var ids = fix.FixableDiagnosticIds;

        Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.UseAssertThatAsync }));
    }

    [Test]
    public void VerifyIntAndConstraint([Values] bool? configureAwaitValue)
    {
        var configurAwait = configureAwaitValue switch
        {
            null => string.Empty,
            true => ".ConfigureAwait(true)",
            false => ".ConfigureAwait(false)",
        };
        var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
        public async Task Test()
        {{
            Assert.That(await GetIntAsync(){configurAwait}, Is.EqualTo(42));
        }}

        private static Task<int> GetIntAsync() => Task.FromResult(42);");
        var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task Test()
        {
            await Assert.ThatAsync(() => GetIntAsync(), Is.EqualTo(42));
        }

        private static Task<int> GetIntAsync() => Task.FromResult(42);");
        RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
    }

    [Test]
    public void VerifyTaskIntReturningInstanceMethodAndConstraint([Values] bool? configureAwaitValue)
    {
        var configurAwait = configureAwaitValue switch
        {
            null => string.Empty,
            true => ".ConfigureAwait(true)",
            false => ".ConfigureAwait(false)",
        };
        var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
        public async Task Test()
        {{
            Assert.That(await this.GetIntAsync(){configurAwait}, Is.EqualTo(42));
        }}

        private Task<int> GetIntAsync() => Task.FromResult(42);");
        var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task Test()
        {
            await Assert.ThatAsync(() => this.GetIntAsync(), Is.EqualTo(42));
        }

        private Task<int> GetIntAsync() => Task.FromResult(42);");
        RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
    }

    [Test]
    public void VerifyBoolAndConstraint([Values] bool? configureAwaitValue)
    {
        var configurAwait = configureAwaitValue switch
        {
            null => string.Empty,
            true => ".ConfigureAwait(true)",
            false => ".ConfigureAwait(false)",
        };
        var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
        public async Task Test()
        {{
            Assert.That(await GetBoolAsync(){configurAwait}, Is.EqualTo(true));
        }}

        private static Task<bool> GetBoolAsync() => Task.FromResult(true);");
        var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task Test()
        {
            await Assert.ThatAsync(() => GetBoolAsync(), Is.EqualTo(true));
        }

        private static Task<bool> GetBoolAsync() => Task.FromResult(true);");
        RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
    }

    // Assert.That(bool) is supported, but there is no overload of Assert.ThatAsync that only takes a single bool.
    [Test]
    public void VerifyBoolOnly([Values] bool? configureAwaitValue)
    {
        var configurAwait = configureAwaitValue switch
        {
            null => string.Empty,
            true => ".ConfigureAwait(true)",
            false => ".ConfigureAwait(false)",
        };
        var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
        public async Task Test()
        {{
            Assert.That(await GetBoolAsync(){configurAwait});
        }}

        private static Task<bool> GetBoolAsync() => Task.FromResult(true);");
        var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task Test()
        {
            await Assert.ThatAsync(() => GetBoolAsync(), Is.True);
        }

        private static Task<bool> GetBoolAsync() => Task.FromResult(true);");
        RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
    }

    [Test]
    public void VerifyBoolAsSecondArgumentAndConstraint([Values] bool? configureAwaitValue)
    {
        var configurAwait = configureAwaitValue switch
        {
            null => string.Empty,
            true => ".ConfigureAwait(true)",
            false => ".ConfigureAwait(false)",
        };
        var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
        public async Task Test()
        {{
            ↓Assert.That(expression: Is.EqualTo(42), actual: await GetIntAsync(){configurAwait});
        }}

        private static Task<int> GetIntAsync() => Task.FromResult(42);");
        var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task Test()
        {
            await Assert.ThatAsync(() => GetIntAsync(), Is.EqualTo(42));
        }

        private static Task<int> GetIntAsync() => Task.FromResult(42);");
        RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
    }
}
#endif
