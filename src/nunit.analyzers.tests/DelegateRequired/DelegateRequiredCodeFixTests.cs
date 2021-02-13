using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.DelegateRequired;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.DelegateRequired
{
    [TestFixture]
    public sealed class DelegateRequiredCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new DelegateRequiredAnalyzer();
        private static readonly CodeFixProvider fix = new DelegateRequiredCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.DelegateRequired);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new DelegateRequiredCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.DelegateRequired }));
        }

        [Test]
        public void VerifyNonDelegateFix()
        {
            var code = TestUtility.WrapInTestMethod(@"
                Assert.That(↓MyOperation(1), Throws.InstanceOf<InvalidOperationException>());

                int MyOperation(int n) => throw new InvalidOperationException();
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@"
                Assert.That(() => MyOperation(1), Throws.InstanceOf<InvalidOperationException>());

                int MyOperation(int n) => throw new InvalidOperationException();
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: DelegateRequiredCodeFix.UseAnonymousLambdaDescription);
        }

        [Test]
        public void VerifyNonDelegateFixWithMessage()
        {
            var code = TestUtility.WrapInTestMethod(@"
                Assert.That(↓MyOperation(), Throws.InstanceOf<InvalidOperationException>(), ""message"");

                int MyOperation() => throw new InvalidOperationException();
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@"
                Assert.That(MyOperation, Throws.InstanceOf<InvalidOperationException>(), ""message"");

                int MyOperation() => throw new InvalidOperationException();
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: DelegateRequiredCodeFix.UseMethodGroupDescription);
        }

        [Test]
        public void VerifyNonDelegateFixWithMessageAndParams()
        {
            var code = TestUtility.WrapInTestMethod(@"
                Assert.That(↓MyOperation() + MyOtherOperation(), Throws.InstanceOf<InvalidOperationException>(), ""message"", Guid.NewGuid());

                int MyOperation() => throw new InvalidOperationException();
                int MyOtherOperation() => throw new InvalidOperationException();
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@"
                Assert.That(() => MyOperation() + MyOtherOperation(), Throws.InstanceOf<InvalidOperationException>(), ""message"", Guid.NewGuid());

                int MyOperation() => throw new InvalidOperationException();
                int MyOtherOperation() => throw new InvalidOperationException();
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: DelegateRequiredCodeFix.UseAnonymousLambdaDescription);
        }

        [Test]
        public void AnalyzeWhenDelayedConstraintWithDimensionedDelayInterval()
        {
            var code = TestUtility.WrapInTestMethod(@"
                int i = 0;
                int actualFunc() => i++;
                Assert.That(↓actualFunc(), Is.EqualTo(5).After(1).Minutes);
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@"
                int i = 0;
                int actualFunc() => i++;
                Assert.That(actualFunc, Is.EqualTo(5).After(1).Minutes);
            ");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: DelegateRequiredCodeFix.UseMethodGroupDescription);
        }
    }
}
