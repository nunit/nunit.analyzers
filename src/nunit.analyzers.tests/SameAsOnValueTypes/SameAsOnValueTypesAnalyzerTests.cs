using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.SameAsOnValueTypes;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.SameAsOnValueTypes
{
    public class SameAsOnValueTypesAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new SameAsOnValueTypesAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.SameAsOnValueTypes);

        [Test]
        public void AnalyzeWhenLiteralTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "Assert.That(1, Is.SameAs(↓1));");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenStructTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var expected = Guid.Empty;
                var actual = expected;

                Assert.That(actual, Is.SameAs(↓expected));");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenStructDelegateProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var expected = Guid.Empty;

                Assert.That(() => expected, Is.SameAs(↓expected));");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenStructTaskProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var expected = Guid.Empty;

                Assert.That(Task.FromResult(expected), Is.SameAs(↓expected));");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenActualIsReferenceTypeAndExpectedIsValueType()
        {
            var testCode = TestUtility.WrapInTestMethod(
                @"Assert.That(""3"", Is.Not.SameAs(↓3));");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenActualIsValueTypeAndExpectedIsReferenceType()
        {
            var testCode = TestUtility.WrapInTestMethod(
                @"Assert.That(↓3, Is.Not.SameAs(""3""));");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenBothArgumentsAreReferenceType()
        {
            var testCode = TestUtility.WrapInTestMethod(
                @"Assert.That(""3"", Is.SameAs(""3""));");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenCollectionsWithValueTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new List<int> { 1 };
                var expected = new [] { 1 };
                Assert.That(actual, Is.SameAs(expected));",
                additionalUsings: "using System.Collections.Generic;");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeClassicWhenLiteralTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "Assert.AreSame(↓1, 1);");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeClassicWhenStructTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var expected = Guid.Empty;
                var actual = expected;

                Assert.AreSame(↓expected, actual);");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeClassicWhenActualIsReferenceTypeAndExpectedIsValueType()
        {
            var testCode = TestUtility.WrapInTestMethod(
                @"Assert.AreNotSame(↓3, ""3"");");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeClassicWhenActualIsValueTypeAndExpectedIsReferenceType()
        {
            var testCode = TestUtility.WrapInTestMethod(
                @"Assert.AreNotSame(""3"", ↓3);");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeClassicWhenBothArgumentsAreReferenceType()
        {
            var testCode = TestUtility.WrapInTestMethod(
                @"Assert.AreSame(""3"", ""3"");");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenMoreThanOneConstraintExpressionIsUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(
                @"Assert.That(""1"", Is.Not.SameAs(""1"") & Is.Not.SameAs(↓2);");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }
    }
}
