using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.ContainsConstraintWrongActualType;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ContainsConstraintWrongActualType
{
    public class ContainsConstraintWrongActualTypeAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ContainsConstraintWrongActualTypeAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.ContainsConstraintWrongActualType);

        [Test]
        public void AnalyzeWhenNonStringAndNonCollectionActualArgumentProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "Assert.That(123, ↓Does.Contain(\"1\"));");

            RoslynAssert.Diagnostics(analyzer,
                expectedDiagnostic.WithMessage("The ContainsConstraint cannot be used with an actual value of type 'int'"),
                testCode);
        }

        [Test]
        public void AnalyzeWhenNonStringCollectionActualArgumentProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new[] {1, 2, 3};
                Assert.That(actual, ↓Does.Contain(""1""));");

            RoslynAssert.Diagnostics(analyzer,
                expectedDiagnostic.WithMessage("The ContainsConstraint cannot be used with an actual value of type 'int[]'"),
                testCode);
        }

        [Test]
        public void AnalyzeWhenActualIsStringTask()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "Assert.That(Task.FromResult(\"1234\"), ↓Does.Contain(\"1\"));");

            RoslynAssert.Diagnostics(analyzer,
                expectedDiagnostic.WithMessage("The ContainsConstraint cannot be used with an actual value of type 'Task<string>'"),
                testCode);
        }

        [Test]
        public void ValidWhenStringProvidedAsActual()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "Assert.That(\"123\", Does.Contain(\"1\"));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenStringArrayProvidedAsActual()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new[] {""1"", ""2"", ""3""};
                Assert.That(actual, Does.Contain(""1""));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenUsedWithAllOperator()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "Assert.That(new [] {\"Aa\", \"Ba\", \"Ca\"}, Has.All.Contains(\"a\"));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenActualIsStringDelegate()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "Assert.That(() => \"1234\", Does.Contain(\"1\"));");

            RoslynAssert.Valid(analyzer, testCode);
        }
    }
}
