using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.DelegateUnnecessary;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.DelegateUnnecessary
{
    [TestFixture]
    public sealed class DelegateUnnecessaryCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new DelegateUnnecessaryAnalyzer();
        private static readonly CodeFixProvider fix = new DelegateUnnecessaryCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.DelegateUnnecessary);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new DelegateUnnecessaryCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.DelegateUnnecessary }));
        }

        [Test]
        public void VerifyUnnecessaryDelegateFix()
        {
            var code = TestUtility.WrapInTestMethod(@"
                Assert.That(↓() => MyOperation(1), Is.Not.EqualTo(42));

                int MyOperation(int n) => 42 + n;
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@"
                Assert.That(MyOperation(1), Is.Not.EqualTo(42));

                int MyOperation(int n) => 42 + n;
            ");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: DelegateUnnecessaryCodeFix.RemoveAnonymousLambdaDescription);
        }

        [Test]
        public void VerifyUnnecessaryDelegateFixWithMessage()
        {
            var code = TestUtility.WrapInTestMethod(@"
                Assert.That(↓() => MyOperation(1), Is.Not.EqualTo(42), ""message"");

                int MyOperation(int n) => 42 + n;
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@"
                Assert.That(MyOperation(1), Is.Not.EqualTo(42), ""message"");

                int MyOperation(int n) => 42 + n;
            ");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: DelegateUnnecessaryCodeFix.RemoveAnonymousLambdaDescription);
        }

        [Test]
        public void VerifyUnnecessaryDelegateFixWithFormattableMessage()
        {
            var code = TestUtility.WrapInTestMethod(@"
                Assert.That(↓() => MyOperation() + MyOtherOperation(), Is.EqualTo(42), $""message-id: {Guid.NewGuid()}"");

                int MyOperation() => 35;
                int MyOtherOperation() => 7;
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@"
                Assert.That(MyOperation() + MyOtherOperation(), Is.EqualTo(42), $""message-id: {Guid.NewGuid()}"");

                int MyOperation() => 35;
                int MyOtherOperation() => 7;
            ");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: DelegateUnnecessaryCodeFix.RemoveAnonymousLambdaDescription);
        }

        [Test]
        public void VerifyUnnecessaryMethodGroupFix()
        {
            var code = TestUtility.WrapInTestMethod(@"
                Assert.That(↓MyOperation, Is.EqualTo(42));

                int MyOperation() => 42;
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@"
                Assert.That(MyOperation(), Is.EqualTo(42));

                int MyOperation() => 42;
            ");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: DelegateUnnecessaryCodeFix.InvokedMethodExplicitly);
        }

        [Test]
        public void VerifyUnnecessaryAsyncDelegateFix()
        {
            var code = TestUtility.WrapInTestMethod(@"
                Assert.That(↓() => MyOperation(1), Is.Not.EqualTo(42));

                static Task<int> MyOperation(int n) => Task.FromResult(42 + n);
            ");
            var fixedCode = TestUtility.WrapInAsyncTestMethod(@"
                Assert.That(await MyOperation(1), Is.Not.EqualTo(42));

                static Task<int> MyOperation(int n) => Task.FromResult(42 + n);
            ");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: DelegateUnnecessaryCodeFix.RemoveAnonymousLambdaDescription);
        }
    }
}
