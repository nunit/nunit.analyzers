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
    private static readonly ExpectedDiagnostic diagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.UseAssertThatAsync);
    private static readonly string[] configureAwaitValues =
    {
        "",
        ".ConfigureAwait(true)",
        ".ConfigureAwait(false)",
    };

    [Test]
    public void VerifyGetFixableDiagnosticIds()
    {
        var fix = new UseAssertThatAsyncCodeFix();
        var ids = fix.FixableDiagnosticIds;

        Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.UseAssertThatAsync }));
    }

    [Test]
    public void VerifyIntAndConstraint([ValueSource(nameof(configureAwaitValues))] string configureAwait, [Values] bool hasMessage)
    {
        var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
        public async Task Test()
        {{
            Assert.That(await GetIntAsync(){configureAwait}, Is.EqualTo(42){(hasMessage ? @", ""message""" : "")});
        }}

        private static Task<int> GetIntAsync() => Task.FromResult(42);");
        var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public async Task Test()
        {{
            await Assert.ThatAsync(() => GetIntAsync(), Is.EqualTo(42){(hasMessage ? @", ""message""" : "")});
        }}

        private static Task<int> GetIntAsync() => Task.FromResult(42);");
        RoslynAssert.CodeFix(analyzer, fix, diagnostic, code, fixedCode);
    }

    [Test]
    public void VerifyTaskIntReturningInstanceMethodAndConstraint([ValueSource(nameof(configureAwaitValues))] string configureAwait, [Values] bool hasMessage)
    {
        var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
        public async Task Test()
        {{
            Assert.That(await this.GetIntAsync(){configureAwait}, Is.EqualTo(42){(hasMessage ? @", ""message""" : "")});
        }}

        private Task<int> GetIntAsync() => Task.FromResult(42);");
        var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public async Task Test()
        {{
            await Assert.ThatAsync(() => this.GetIntAsync(), Is.EqualTo(42){(hasMessage ? @", ""message""" : "")});
        }}

        private Task<int> GetIntAsync() => Task.FromResult(42);");
        RoslynAssert.CodeFix(analyzer, fix, diagnostic, code, fixedCode);
    }

    [Test]
    public void VerifyBoolAndConstraint([ValueSource(nameof(configureAwaitValues))] string configureAwait, [Values] bool hasMessage)
    {
        var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
        public async Task Test()
        {{
            Assert.That(await GetBoolAsync(){configureAwait}, Is.EqualTo(true){(hasMessage ? @", ""message""" : "")});
        }}

        private static Task<bool> GetBoolAsync() => Task.FromResult(true);");
        var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public async Task Test()
        {{
            await Assert.ThatAsync(() => GetBoolAsync(), Is.EqualTo(true){(hasMessage ? @", ""message""" : "")});
        }}

        private static Task<bool> GetBoolAsync() => Task.FromResult(true);");
        RoslynAssert.CodeFix(analyzer, fix, diagnostic, code, fixedCode);
    }

    // Assert.That(bool) is supported, but there is no overload of Assert.ThatAsync that only takes a single bool.
    [Test]
    public void VerifyBoolOnly([ValueSource(nameof(configureAwaitValues))] string configureAwait, [Values] bool hasMessage)
    {
        var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
        public async Task Test()
        {{
            Assert.That(await GetBoolAsync(){configureAwait}{(hasMessage ? @", ""message""" : "")});
        }}

        private static Task<bool> GetBoolAsync() => Task.FromResult(true);");
        var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public async Task Test()
        {{
            await Assert.ThatAsync(() => GetBoolAsync(), Is.True{(hasMessage ? @", ""message""" : "")});
        }}

        private static Task<bool> GetBoolAsync() => Task.FromResult(true);");
        RoslynAssert.CodeFix(analyzer, fix, diagnostic, code, fixedCode);
    }

    [Test]
    public void VerifyIntAsSecondArgumentAndConstraint([ValueSource(nameof(configureAwaitValues))] string configureAwait)
    {
        var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
        public async Task Test()
        {{
            ↓Assert.That(expression: Is.EqualTo(42), actual: await GetIntAsync(){configureAwait});
        }}

        private static Task<int> GetIntAsync() => Task.FromResult(42);");
        var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task Test()
        {
            await Assert.ThatAsync(() => GetIntAsync(), Is.EqualTo(42));
        }

        private static Task<int> GetIntAsync() => Task.FromResult(42);");
        RoslynAssert.CodeFix(analyzer, fix, diagnostic, code, fixedCode);
    }

    [Test]
    public void VerifyBoolAsSecondArgumentAndConstraint([ValueSource(nameof(configureAwaitValues))] string configureAwait)
    {
        var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
        public async Task Test()
        {{
            ↓Assert.That(message: ""message"", condition: await GetBoolAsync(){configureAwait});
        }}

        private static Task<bool> GetBoolAsync() => Task.FromResult(true);");
        var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task Test()
        {
            await Assert.ThatAsync(() => GetBoolAsync(), Is.True, ""message"");
        }

        private static Task<bool> GetBoolAsync() => Task.FromResult(true);");
        RoslynAssert.CodeFix(analyzer, fix, diagnostic, code, fixedCode);
    }
}
#endif
