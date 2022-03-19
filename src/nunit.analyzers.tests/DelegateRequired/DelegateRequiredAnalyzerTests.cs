using System;
using System.Threading.Tasks;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.DelegateRequired;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.DelegateRequired
{
    public class DelegateRequiredAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new DelegateRequiredAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.DelegateRequired);

        [Test]
        public void AnalyzeWhenLambdaProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.That(() => throw new InvalidOperationException(), Throws.InstanceOf<InvalidOperationException>());
            ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenAsyncLambdaProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.That(() => Task.Delay(-2), Throws.InstanceOf<ArgumentException>());
            ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenDelegateProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.That(MyOperation, Throws.InstanceOf<InvalidOperationException>());

                void MyOperation() => throw new InvalidOperationException();
            ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenDelegateLocalProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                TestDelegate action = MyOperation;
                Assert.That(action, Throws.InvalidOperationException);

                static void MyOperation() => throw new InvalidOperationException();
            ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenDelegateParameterProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                AssertSupported(MyOperation);

                static void AssertSupported(Action test) => Assert.That(test, Throws.InvalidOperationException);
                static void MyOperation() => throw new InvalidOperationException();
            ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenAsyncDelegateProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.That(MyAsyncOperation, Throws.InstanceOf<InvalidOperationException>());

                Task MyAsyncOperation() => throw new InvalidOperationException();
            ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenNoDelegateProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.That(↓MyOperation(), Throws.InstanceOf<InvalidOperationException>());

                int MyOperation() => throw new InvalidOperationException();
            ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenNoAsyncDelegateProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.That(↓MyAsyncOperation(), Throws.InstanceOf<InvalidOperationException>());

                Task MyAsyncOperation() => throw new InvalidOperationException();
            ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenDelayedConstraint()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                int i = 0;
                int actualFunc() => i++;
                Assert.That(↓actualFunc(), Is.EqualTo(5).After(1000, 10));
            ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenDelayedConstraintWithDimensionedDelayInterval()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                int i = 0;
                int actualFunc() => i++;
                Assert.That(↓actualFunc(), Is.EqualTo(5).After(1).Minutes);
            ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        // https://github.com/nunit/nunit.analyzers/issues/431
        [Test]
        public void AnalyzeWhenDelayedConstraintOnNonValueType()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var list = new List<int>();
                Task.Run(async () => {
                    await Task.Delay(100);
                    list.Add(1);
                });
                
                // The below are re-evaluated every time.
                Assert.That(list, Is.Not.Empty.After(200, 10));
                Assert.That(list, Has.Count.EqualTo(1).After(200, 10));
                Assert.That(list, Does.Contain(1).After(200, 10));
                Assert.That(list, Contains.Item(1).After(200, 10));

                // The below is a one off.
                Assert.That(↓list.Count, Is.EqualTo(1).After(200, 10));
            ", "using System.Collections.Generic;");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }
    }
}
