using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.SomeItemsIncompatibleTypes;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.SomeItemsIncompatibleTypes
{
    [TestFixtureSource(nameof(ConstraintExpressions))]
    public class SomeItemsIncompatibleTypesAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new SomeItemsIncompatibleTypesAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.SomeItemsIncompatibleTypes);

        private static readonly string[] ConstraintExpressions = new[] { "Does.Contain", "Contains.Item" };

        private readonly string constraint;

        public SomeItemsIncompatibleTypesAnalyzerTests(string constraintExpession)
        {
            this.constraint = constraintExpession;
        }

        [Test]
        public void AnalyzeWhenNonCollectionActualArgumentProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(
                $"Assert.That(123, ↓{this.constraint}(1));");

            AnalyzerAssert.Diagnostics(analyzer,
                expectedDiagnostic.WithMessage("The SomeItemsConstraint cannot be used with 'int' actual and 'int' expected arguments."),
                testCode);
        }

        [Test]
        public void AnalyzeWhenInvalidCollectionActualArgumentProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(
                $"Assert.That(new[] {{\"1\", \"2\"}}, ↓{this.constraint}(1));");

            AnalyzerAssert.Diagnostics(analyzer,
                expectedDiagnostic.WithMessage("The SomeItemsConstraint cannot be used with 'string[]' actual and 'int' expected arguments."),
                testCode);
        }

        [Test]
        public void ValidWhenCollectionIsProvidedAsActualWithMatchingExpectedType()
        {
            var testCode = TestUtility.WrapInTestMethod(
                $"Assert.That(new [] {{1, 2, 3}}, {this.constraint}(2));");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenCollectionOfCollectionsIsProvidedAsActualAndCollectionAsExpected()
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var actual = new List<IEnumerable<int>>
                {{
                    new [] {{ 1, 2 }},
                    new List<int> {{ 2, 3 }},
                    new [] {{ 3, 4 }}
                }};
                Assert.That(actual, {this.constraint}(new[] {{ 2, 3 }}));",
                additionalUsings: "using System.Collections.Generic;");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenCollectionItemTypeAndExpectedTypeAreNumeric()
        {
            var testCode = TestUtility.WrapInTestMethod(
                $"Assert.That(new [] {{1.1, 2.0, 3.2}}, {this.constraint}(2));");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenActualIsNonGenericCollection()
        {
            var testCode = TestUtility.WrapInTestMethod(
                $"Assert.That(new ArrayList {{ 1, 2, 3 }}, {this.constraint}(2));",
                additionalUsings: "using System.Collections;");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenUsedWithAllOperator()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "Assert.That(new[] { new[] { 1 }, new[] { 1, 2 } }, Has.All.Contain(1));");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenActualIsCollectionTask()
        {
            var testCode = TestUtility.WrapInTestMethod(
                $"Assert.That(Task.FromResult(new[] {{1,2,3}}), {this.constraint}(1));");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenActualIsCollectionDelegate()
        {
            var testCode = TestUtility.WrapInTestMethod(
                $"Assert.That(() => new[] {{1,2,3}}, {this.constraint}(1));");

            AnalyzerAssert.Valid(analyzer, testCode);
        }
    }
}
