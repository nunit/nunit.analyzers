using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.DelegateUnnecessary;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.DelegateUnnecessary.DelegateUnnecessaryAnalyzer, NUnit.Analyzers.DelegateUnnecessary.DelegateUnnecessaryCodeFix>;

namespace NUnit.Analyzers.Tests.DelegateUnnecessary
{
    [TestFixture]
    public sealed class DelegateUnnecessaryCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.DelegateUnnecessary);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new DelegateUnnecessaryCodeFix();
            var ids = new DelegateUnnecessaryCodeFix().FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.DelegateUnnecessary }));
        }

        [Test]
        public async Task VerifyUnnecessaryDelegateFix()
        {
            var code = TestUtility.WrapInTestMethod("""
                        Assert.That({|#0:() => MyOperation(1)|}, Is.Not.EqualTo(42));

                        int MyOperation(int n) => 42 + n;

            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        Assert.That(MyOperation(1), Is.Not.EqualTo(42));

                        int MyOperation(int n) => 42 + n;

            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, DelegateUnnecessaryCodeFix.RemoveAnonymousLambdaDescription);
        }

        [Test]
        public async Task VerifyUnnecessaryDelegateFixWithMessage()
        {
            var code = TestUtility.WrapInTestMethod("""
                        Assert.That({|#0:() => MyOperation(1)|}, Is.Not.EqualTo(42), "message");

                        int MyOperation(int n) => 42 + n;

            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        Assert.That(MyOperation(1), Is.Not.EqualTo(42), "message");

                        int MyOperation(int n) => 42 + n;

            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, DelegateUnnecessaryCodeFix.RemoveAnonymousLambdaDescription);
        }

        [Test]
        public async Task VerifyUnnecessaryDelegateFixWithFormattableMessage()
        {
            var code = TestUtility.WrapInTestMethod("""
                        Assert.That({|#0:() => MyOperation() + MyOtherOperation()|}, Is.EqualTo(42), $"message-id: {Guid.NewGuid()}");

                        int MyOperation() => 35;
                        int MyOtherOperation() => 7;

            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        Assert.That(MyOperation() + MyOtherOperation(), Is.EqualTo(42), $"message-id: {Guid.NewGuid()}");

                        int MyOperation() => 35;
                        int MyOtherOperation() => 7;

            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, DelegateUnnecessaryCodeFix.RemoveAnonymousLambdaDescription);
        }

        [Test]
        public async Task VerifyUnnecessaryMethodGroupFix()
        {
            var code = TestUtility.WrapInTestMethod("""
                        Assert.That({|#0:MyOperation|}, Is.EqualTo(42));

                        int MyOperation() => 42;

            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        Assert.That(MyOperation(), Is.EqualTo(42));

                        int MyOperation() => 42;

            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, DelegateUnnecessaryCodeFix.InvokedMethodExplicitly);
        }

        [Test]
        public async Task VerifyUnnecessaryAsyncDelegateFix()
        {
            var code = TestUtility.WrapInTestMethod("""
                        Assert.That({|#0:() => MyOperation(1)|}, Is.Not.EqualTo(42));

                        static Task<int> MyOperation(int n) => Task.FromResult(42 + n);

            """);
            var fixedCode = TestUtility.WrapInAsyncTestMethod("""
                        Assert.That(await MyOperation(1), Is.Not.EqualTo(42));

                        static Task<int> MyOperation(int n) => Task.FromResult(42 + n);

            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, DelegateUnnecessaryCodeFix.RemoveAnonymousLambdaDescription);
        }
    }
}
