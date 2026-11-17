using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.DelegateUnnecessary.DelegateUnnecessaryAnalyzer>;

namespace NUnit.Analyzers.Tests.DelegateUnnecessary
{
    public class DelegateUnnecessaryAnalyzerTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.DelegateUnnecessary);

        [Test]
        public async Task AnalyzeWhenNecessaryLambdaProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Assert.That(() => throw new InvalidOperationException(), Throws.InstanceOf<InvalidOperationException>());

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenNecessaryAsyncLambdaProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Assert.That(() => Task.Delay(-2), Throws.InstanceOf<ArgumentException>());

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenNecessaryDelegateProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Assert.That(MyOperation, Throws.InstanceOf<InvalidOperationException>());

                        void MyOperation() => throw new InvalidOperationException();

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("Throws.InstanceOf<InvalidOperationException>().Or.InstanceOf<ArgumentException>()")]
        [TestCase("Throws.InvalidOperationException.With.Message.EqualTo(\"Oops!\")")]
        [TestCase("Is.Null.Or.EqualTo(42).After(100).MilliSeconds")]
        public async Task AnalyzeWhenNecessaryDelegateProvidedMultipleConstraints(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That(MyOperation, {{constraint}});
                        int? MyOperation() => throw new InvalidOperationException("Oops!");

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

#if NUNIT5
        [Test]
        public async Task AnalyzeWhenNecessaryDelegateLocalProvided()
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
        public async Task AnalyzeWhenNecessaryDelegateLocalProvided()
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
        public async Task AnalyzeWhenNecessaryDelegateParameterProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        AssertSupported(MyOperation);

                        static void AssertSupported(Action test) => Assert.That(test, Throws.InvalidOperationException);
                        static void MyOperation() => throw new InvalidOperationException();

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenNecessaryAsyncDelegateProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Assert.That(MyAsyncOperation, Throws.InstanceOf<InvalidOperationException>());

                        Task MyAsyncOperation() => throw new InvalidOperationException();

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenUnnecessaryLambdaProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Assert.That({|#0:() => 42|}, Is.EqualTo(42));

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenUnnecessaryAsyncLambdaProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Assert.That({|#0:() => MyOperation(42)|}, Is.EqualTo(42));

                        static Task<int> MyOperation(int n) => Task.FromResult(n);

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenUnnecessaryDelegateProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Assert.That({|#0:MyOperation|}, Is.EqualTo(42));

                        static int MyOperation() => 42;

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenDelayedConstraint()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        int i = 0;
                        int actualFunc() => i++;
                        Assert.That(actualFunc, Is.EqualTo(5).After(1000, 10));

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenDelayedConstraintWithDimensionedDelayInterval()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        int i = 0;
                        int actualFunc() => i++;
                        Assert.That(actualFunc, Is.EqualTo(5).After(1).Minutes);

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
