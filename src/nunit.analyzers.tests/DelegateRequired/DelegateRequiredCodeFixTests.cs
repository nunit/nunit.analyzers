using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.DelegateRequired;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.DelegateRequired.DelegateRequiredAnalyzer, NUnit.Analyzers.DelegateRequired.DelegateRequiredCodeFix>;

namespace NUnit.Analyzers.Tests.DelegateRequired
{
    [TestFixture]
    public sealed class DelegateRequiredCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.DelegateRequired);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new DelegateRequiredCodeFix();
            var ids = new DelegateRequiredCodeFix().FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.DelegateRequired }));
        }

        [Test]
        public async Task VerifyNonDelegateFix()
        {
            var code = TestUtility.WrapInTestMethod("""
                        Assert.That({|#0:MyOperation(1)|}, Throws.InstanceOf<InvalidOperationException>());

                        int MyOperation(int n) => throw new InvalidOperationException();

            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        Assert.That(() => MyOperation(1), Throws.InstanceOf<InvalidOperationException>());

                        int MyOperation(int n) => throw new InvalidOperationException();

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, DelegateRequiredCodeFix.UseAnonymousLambdaDescription);
        }

        [Test]
        public async Task VerifyNonDelegateFixWithMessage()
        {
            var code = TestUtility.WrapInTestMethod("""
                        Assert.That({|#0:MyOperation()|}, Throws.InstanceOf<InvalidOperationException>(), "message");

                        int MyOperation() => throw new InvalidOperationException();

            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        Assert.That(MyOperation, Throws.InstanceOf<InvalidOperationException>(), "message");

                        int MyOperation() => throw new InvalidOperationException();

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, DelegateRequiredCodeFix.UseMethodGroupDescription);
        }

        [Test]
        public async Task VerifyNonDelegateFixWithFormattableMessage()
        {
            var code = TestUtility.WrapInTestMethod("""
                        Assert.That({|#0:MyOperation() + MyOtherOperation()|}, Throws.InstanceOf<InvalidOperationException>(), $"message-id: {Guid.NewGuid()}");

                        int MyOperation() => throw new InvalidOperationException();
                        int MyOtherOperation() => throw new InvalidOperationException();

            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        Assert.That(() => MyOperation() + MyOtherOperation(), Throws.InstanceOf<InvalidOperationException>(), $"message-id: {Guid.NewGuid()}");

                        int MyOperation() => throw new InvalidOperationException();
                        int MyOtherOperation() => throw new InvalidOperationException();

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, DelegateRequiredCodeFix.UseAnonymousLambdaDescription);
        }

        [Test]
        public async Task AnalyzeWhenDelayedConstraintWithDimensionedDelayInterval()
        {
            var code = TestUtility.WrapInTestMethod("""
                        int i = 0;
                        int actualFunc() => i++;
                        Assert.That({|#0:actualFunc()|}, Is.EqualTo(5).After(1).Minutes);

            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        int i = 0;
                        int actualFunc() => i++;
                        Assert.That(actualFunc, Is.EqualTo(5).After(1).Minutes);

            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, DelegateRequiredCodeFix.UseMethodGroupDescription);
        }
    }
}
