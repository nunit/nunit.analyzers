using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.DelegateUnnecessary;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.DelegateUnnecessary
{
    public class DelegateUnnecessaryAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new DelegateUnnecessaryAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.DelegateUnnecessary);

        [Test]
        public void AnalyzeWhenNecessaryLambdaProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.That(() => throw new InvalidOperationException(), Throws.InstanceOf<InvalidOperationException>());
            ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenNecessaryAsyncLambdaProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.That(() => Task.Delay(-2), Throws.InstanceOf<ArgumentException>());
            ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenNecessaryDelegateProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.That(MyOperation, Throws.InstanceOf<InvalidOperationException>());

                void MyOperation() => throw new InvalidOperationException();
            ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase("Throws.InstanceOf<InvalidOperationException>().Or.InstanceOf<ArgumentException>()")]
        [TestCase("Throws.InvalidOperationException.With.Message.EqualTo(\"Oops!\")")]
        [TestCase("Is.Null.Or.EqualTo(42).After(100).MilliSeconds")]
        public void AnalyzeWhenNecessaryDelegateProvidedMultipleConstraints(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                Assert.That(MyOperation, {constraint});
                int? MyOperation() => throw new InvalidOperationException(""Oops!"");
            ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenNecessaryDelegateLocalProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                TestDelegate action = MyOperation;
                Assert.That(action, Throws.InvalidOperationException);

                static void MyOperation() => throw new InvalidOperationException();
            ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenNecessaryDelegateParameterProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                AssertSupported(MyOperation);

                static void AssertSupported(Action test) => Assert.That(test, Throws.InvalidOperationException);
                static void MyOperation() => throw new InvalidOperationException();
            ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenNecessaryAsyncDelegateProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.That(MyAsyncOperation, Throws.InstanceOf<InvalidOperationException>());

                Task MyAsyncOperation() => throw new InvalidOperationException();
            ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenUnnecessaryLambdaProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.That(↓() => 42, Is.EqualTo(42));
            ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenUnnecessaryAsyncLambdaProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.That(↓() => MyOperation(42), Is.EqualTo(42));

                static Task<int> MyOperation(int n) => Task.FromResult(n);
            ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenUnnecessaryDelegateProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.That(↓MyOperation, Is.EqualTo(42));

                static int MyOperation() => 42;
            ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenDelayedConstraint()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                int i = 0;
                int actualFunc() => i++;
                Assert.That(actualFunc, Is.EqualTo(5).After(1000, 10));
            ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenDelayedConstraintWithDimensionedDelayInterval()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                int i = 0;
                int actualFunc() => i++;
                Assert.That(actualFunc, Is.EqualTo(5).After(1).Minutes);
            ");

            RoslynAssert.Valid(analyzer, testCode);
        }
    }
}
