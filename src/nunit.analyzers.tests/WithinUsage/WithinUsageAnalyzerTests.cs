using System;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.WithinUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.WithinUsage
{
    public class WithinUsageAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new WithinUsageAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.WithinIncompatibleTypes);

        [TestCase(NUnitFrameworkConstants.NameOfIsEqualTo)]
        [TestCase(NUnitFrameworkConstants.NameOfIsLessThan)]
        [TestCase(NUnitFrameworkConstants.NameOfIsLessThanOrEqualTo)]
        [TestCase(NUnitFrameworkConstants.NameOfIsGreaterThan)]
        [TestCase(NUnitFrameworkConstants.NameOfIsGreaterThanOrEqualTo)]
        public void AnalyzeWhenAppliedToEqualityConstraintForStrings(string constraintName)
        {
            var testCode = TestUtility.WrapInTestMethod(
                $@"Assert.That(""1"", Is.{constraintName}(""1"").↓Within(0.1));");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [TestCase(NUnitFrameworkConstants.NameOfIsEqualTo)]
        [TestCase(NUnitFrameworkConstants.NameOfIsLessThan)]
        [TestCase(NUnitFrameworkConstants.NameOfIsLessThanOrEqualTo)]
        [TestCase(NUnitFrameworkConstants.NameOfIsGreaterThan)]
        [TestCase(NUnitFrameworkConstants.NameOfIsGreaterThanOrEqualTo)]
        public void AnalyzeWhenAppliedToInvertedEqualityConstraintForStrings(string constraintName)
        {
            var testCode = TestUtility.WrapInTestMethod(
                $@"Assert.That(""1"", Is.Not.{constraintName}(""1"").↓Within(0.1));");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenAppliedToNotEqualConstraintForStrings()
        {
            var testCode = TestUtility.WrapInTestMethod(
                @"Assert.That(""1"", Is.Not.EqualTo(""1"").↓Within(0.1));");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenAppliedToEqualityConstraintForArrays()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var a = new[] {1, 2};
                var b = new[] {1.1, 2.1};
                Assert.That(a, Is.EqualTo(b).↓Within(0.1));");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenAppliedToEqualityConstraintForIncompatibleTuples()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var a = (""1"", ""1"");
                var b = (""1.1"", ""1"");
                Assert.That(a, Is.EqualTo(b).↓Within(0.1));");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenAppliedToEqualityConstraintForMixedCompatibleTuples()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var a = (1, ""1"");
                var b = (1.01, ""1"");
                Assert.That(a, Is.EqualTo(b).Within(0.1));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenAppliedToEqualityConstraintForNamedTuples()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var a = (value: 1, name: ""1"");
                var b = (value: 1.01, name: ""1"");
                Assert.That(a, Is.EqualTo(b).Within(0.1));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase("1", "1")]
        [TestCase("0.1234", "0.2134")]
        public void AnalyzeWhenAppliedToEqualityConstraintForNumericTypes(string a, string b)
        {
            var testCode = TestUtility.WrapInTestMethod(
                $"Assert.That({a}, Is.EqualTo({b}).Within(0.1));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenAppliedToEqualityConstraintForNumericTuples()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var a = (1, 1);
                var b = (1.1, 1);
                Assert.That(a, Is.EqualTo(b).Within(0.1));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenAppliedToEqualityConstraintForMixedValidTypeTuples()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var a = (1, DateTime.Now);
                var b = (1.1, DateTime.Now);
                Assert.That(a, Is.EqualTo(b).Within(0.1));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase(nameof(DateTimeOffset))]
        [TestCase(nameof(DateTime))]
        public void AnalyzeWhenAppliedToEqualityConstraintForDateTypes(string typeName)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var a = {typeName}.Now;
                var b = {typeName}.Now;
                Assert.That(a, Is.EqualTo(b).Within(TimeSpan.Zero));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenAppliedToEqualityConstraintForDateTypeKind()
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var a = DateTimeKind.Local;
                var b = DateTimeKind.Local;
                Assert.That(a, Is.EqualTo(b).↓Within(1));");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenAppliedToEqualityConstraintForTimeSpanTypes()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var a = TimeSpan.Zero;
                var b = TimeSpan.Zero;
                Assert.That(a, Is.EqualTo(b).Within(TimeSpan.Zero));");

            RoslynAssert.Valid(analyzer, testCode);
        }
    }
}
