using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.DelegateRequired.DelegateRequiredAnalyzer>;

namespace NUnit.Analyzers.Tests.DelegateRequired
{
    public class DelegateRequiredAnalyzerTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.DelegateRequired);

        [Test]
        public async Task AnalyzeWhenLambdaProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Assert.That(() => throw new InvalidOperationException(), Throws.InstanceOf<InvalidOperationException>());

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenAsyncLambdaProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Assert.That(() => Task.Delay(-2), Throws.InstanceOf<ArgumentException>());

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenDelegateProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Assert.That(MyOperation, Throws.InstanceOf<InvalidOperationException>());

                        void MyOperation() => throw new InvalidOperationException();

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

#if NUNIT5
        [Test]
        public async Task AnalyzeWhenDelegateLocalProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Action action = MyOperation;
                        Assert.That(action, Throws.InvalidOperationException);

                        static void MyOperation() => throw new InvalidOperationException();

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }
#else
        [Test]
        public async Task AnalyzeWhenDelegateLocalProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        TestDelegate action = MyOperation;
                        Assert.That(action, Throws.InvalidOperationException);

                        static void MyOperation() => throw new InvalidOperationException();

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }
#endif

        [Test]
        public async Task AnalyzeWhenDelegateParameterProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        AssertSupported(MyOperation);

                        static void AssertSupported(Action test) => Assert.That(test, Throws.InvalidOperationException);
                        static void MyOperation() => throw new InvalidOperationException();

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenAsyncDelegateProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Assert.That(MyAsyncOperation, Throws.InstanceOf<InvalidOperationException>());

                        Task MyAsyncOperation() => throw new InvalidOperationException();

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenNoDelegateProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Assert.That({|#0:MyOperation()|}, Throws.InstanceOf<InvalidOperationException>());

                        int MyOperation() => throw new InvalidOperationException();

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenNoAsyncDelegateProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Assert.That({|#0:MyAsyncOperation()|}, Throws.InstanceOf<InvalidOperationException>());

                        Task MyAsyncOperation() => throw new InvalidOperationException();

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenDelayedConstraint()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        int i = 0;
                        int actualFunc() => i++;
                        Assert.That({|#0:actualFunc()|}, Is.EqualTo(5).After(1000, 10));

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenDelayedConstraintWithDimensionedDelayInterval()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        int i = 0;
                        int actualFunc() => i++;
                        Assert.That({|#0:actualFunc()|}, Is.EqualTo(5).After(1).Minutes);

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }
    }
}
