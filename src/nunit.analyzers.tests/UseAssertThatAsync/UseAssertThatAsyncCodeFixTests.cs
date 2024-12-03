using System.Threading.Tasks;
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

#if NUNIT4
    [Test]
    public void VerifyWithoutConfigureAwait()
    {
        var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [Test]
        public async Task Test1()
        {
            Assert.That(await GetResultAsync(), Is.EqualTo(42));
        }

        private static Task<int> GetResultAsync() => Task.FromResult(42);",
            isNUnit4Only: true);
        var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [Test]
        public async Task Test1()
        {
            await Assert.ThatAsync(() => GetResultAsync(), Is.EqualTo(42));
        }

        private static Task<int> GetResultAsync() => Task.FromResult(42);",
            isNUnit4Only: true);
        RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
    }

    [Test]
    public void VerifyWithConfigureAwait()
    {
        Assert.ThatAsync(() => Task.FromResult(1), Is.EqualTo(1));
        var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task Test()
        {
            Assert.That(await GetResultAsync().ConfigureAwait(false), Is.EqualTo(42));
        }

        private static Task<int> GetResultAsync() => Task.FromResult(42);",
            isNUnit4Only: true);
        var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task Test()
        {
            await Assert.ThatAsync(() => GetResultAsync(), Is.EqualTo(42));
        }

        private static Task<int> GetResultAsync() => Task.FromResult(42);",
            isNUnit4Only: true);
        RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
    }

    [Test]
    public void AnalyzeWhenAwaitIsUsedWithoutConfigureAwaitAsSecondArgument()
    {
        var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task Test()
        {
            ↓Assert.That(expression: Is.EqualTo(42), actual: await GetResultAsync());
        }

        private static Task<int> GetResultAsync() => Task.FromResult(42);",
            isNUnit4Only: true);
        var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task Test()
        {
            await Assert.ThatAsync(constraint: Is.EqualTo(42), code: () => GetResultAsync());
        }

        private static Task<int> GetResultAsync() => Task.FromResult(42);",
            isNUnit4Only: true);
        RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
    }

    /*
    [Test]
    public void AddsAsyncWhenAwaitIsUsed()
    {
        var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void Test()
        {
            var configuration = new Configuration();
            Assert.That(configuration, Is.Not.Null);
            ↓Assert.That(configuration.Value1, Is.EqualTo(0));
            Assert.That(configuration.Value2, Is.EqualTo(0.0));
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
    */
#endif
}
