#if NUNIT4
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
    private static readonly string[] configureAwaitValues =
    {
        "",
        ".ConfigureAwait(true)",
        ".ConfigureAwait(false)",
    };

    [Test]
    public void AnalyzeWhenIntResultIsUsed()
    {
        var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void Test()
        {
            Assert.That(GetIntAsync().Result, Is.EqualTo(42));
        }

        private static Task<int> GetIntAsync() => Task.FromResult(42);");
        RoslynAssert.Valid(analyzer, testCode);
    }

    [Test]
    public void AnalyzeWhenBoolResultIsUsed()
    {
        var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void Test()
        {
            Assert.That(GetBoolAsync().Result);
        }

        private static Task<bool> GetBoolAsync() => Task.FromResult(true);");
        RoslynAssert.Valid(analyzer, testCode);
    }

    [Test]
    public void AnalyzeWhenAwaitIsNotUsedInLineForInt()
    {
        var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task Test()
        {
            var fourtyTwo = await GetIntAsync();
            Assert.That(fourtyTwo, Is.EqualTo(42));
        }

        private static Task<int> GetIntAsync() => Task.FromResult(42);");
        RoslynAssert.Valid(analyzer, testCode);
    }

    // do not touch because there is no ThatAsync equivalent
    [Test]
    public void AnalyzeWhenExceptionMessageIsFuncString()
    {
        var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task Test()
        {
            Assert.That(await GetBoolAsync(), () => ""message"");
        }

        private static Task<bool> GetBoolAsync() => Task.FromResult(true);");
        RoslynAssert.Valid(analyzer, testCode);
    }

    [Test]
    public void AnalyzeWhenAwaitIsNotUsedInLineForBool()
    {
        var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task Test()
        {
            var myBool = await GetBoolAsync();
            Assert.That(myBool, Is.True);
        }

        private static Task<bool> GetBoolAsync() => Task.FromResult(true);");
        RoslynAssert.Valid(analyzer, testCode);
    }

    [Test]
    public void AnalyzeWhenAwaitIsUsedInLineForInt([ValueSource(nameof(configureAwaitValues))] string configureAwait, [Values] bool hasMessage)
    {
        var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public async Task Test()
        {{
            Assert.That(await GetIntAsync(){configureAwait}, Is.EqualTo(42){(hasMessage ? @", ""message""" : "")});
        }}

        private static Task<int> GetIntAsync() => Task.FromResult(42);");
        RoslynAssert.Diagnostics(analyzer, diagnostic, testCode);
    }

    [Test]
    public void AnalyzeWhenAwaitIsUsedInLineForBool([ValueSource(nameof(configureAwaitValues))] string configureAwait, [Values] bool hasConstraint, [Values] bool hasMessage)
    {
        var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public async Task Test()
        {{
            Assert.That(await GetBoolAsync(){configureAwait}{(hasConstraint ? ", Is.True" : "")}{(hasMessage ? @", ""message""" : "")});
        }}

        private static Task<bool> GetBoolAsync() => Task.FromResult(true);");
        RoslynAssert.Diagnostics(analyzer, diagnostic, testCode);
    }

    [Test]
    public void AnalyzeWhenAwaitIsUsedAsSecondArgument([ValueSource(nameof(configureAwaitValues))] string configureAwait, [Values] bool hasMessage)
    {
        var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public async Task Test()
        {{
            ↓Assert.That(expression: Is.EqualTo(42), actual: await GetIntAsync(){configureAwait}{(hasMessage ? @", message: ""message""" : "")});
        }}

        private static Task<int> GetIntAsync() => Task.FromResult(42);");
        RoslynAssert.Diagnostics(analyzer, diagnostic, testCode);
    }
}
#endif
