using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TestCaseUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.TestCaseUsage
{
    [TestFixture]
    public sealed class TestMethodUsageAnalyzerTests
    {
        private DiagnosticAnalyzer analyzer = new TestMethodUsageAnalyzer();

        [Test]
        public void VerifySupportedDiagnostics()
        {
            var analyzer = new TestMethodUsageAnalyzer();
            var diagnostics = analyzer.SupportedDiagnostics;

            var expectedIdentifiers = new List<string>
            {
                AnalyzerIdentifiers.TestMethodExpectedResultTypeMismatchUsage,
                AnalyzerIdentifiers.TestMethodSpecifiedExpectedResultForVoidUsage
            };
            CollectionAssert.AreEquivalent(expectedIdentifiers, diagnostics.Select(d => d.Id));

            foreach (var diagnostic in diagnostics)
            {
                Assert.That(diagnostic.Title.ToString(), Is.EqualTo(TestMethodUsageAnalyzerConstants.Title),
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Title)}");
                Assert.That(diagnostic.Category, Is.EqualTo(Categories.Usage),
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Category)}");
                Assert.That(diagnostic.DefaultSeverity, Is.EqualTo(DiagnosticSeverity.Error),
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.DefaultSeverity)}");
            }

            var diagnosticMessage = diagnostics.Select(_ => _.MessageFormat.ToString()).ToImmutableArray();

            Assert.That(diagnosticMessage, Contains.Item(TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage),
                $"{TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage} is missing.");
            Assert.That(diagnosticMessage, Contains.Item(TestMethodUsageAnalyzerConstants.SpecifiedExpectedResultForVoidMethodMessage),
                $"{TestMethodUsageAnalyzerConstants.SpecifiedExpectedResultForVoidMethodMessage} is missing.");
        }

        [Test]
        public void AnalyzeWhenExpectedResultIsProvidedCorrectly()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenExpectedResultIsProvidedCorrectly
    {
        [TestCase(2, ExpectedResult = 3)]
        public int Test(int a) { return 3; }
    }");
            AnalyzerAssert.Valid<TestMethodUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenExpectedResultIsProvidedAndReturnTypeIsVoid()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestMethodSpecifiedExpectedResultForVoidUsage,
                TestMethodUsageAnalyzerConstants.SpecifiedExpectedResultForVoidMethodMessage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenExpectedResultIsProvidedAndReturnTypeIsVoid
    {
        [TestCase(2, ↓ExpectedResult = '3')]
        public void Test(int a) { }
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenExpectedResultIsProvidedAndTypeIsIncorrect()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestMethodExpectedResultTypeMismatchUsage,
                String.Format(TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage, typeof(int).Name));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenExpectedResultIsProvidedAndTypeIsIncorrect
    {
        [TestCase(2, ↓ExpectedResult = '3')]
        public int Test(int a) { return 3; }
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenExpectedResultIsProvidedAndPassesNullToValueType()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestMethodExpectedResultTypeMismatchUsage,
                String.Format(TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage, typeof(int).Name));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenExpectedResultIsProvidedAndPassesNullToValueType
    {
        [TestCase(2, ↓ExpectedResult = null)]
        public int Test(int a) { return 3; }
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenExpectedResultIsProvidedAndPassesNullToNullableType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenExpectedResultIsProvidedAndPassesNullToNullableType
    {
        [TestCase(2, ExpectedResult = null)]
        public int? Test(int a) { return null; }
    }");
            AnalyzerAssert.Valid<TestMethodUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenExpectedResultIsProvidedAndPassesValueToNullableType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenExpectedResultIsProvidedAndPassesValueToNullableType
    {
        [TestCase(2, ExpectedResult = 2)]
        public int? Test(int a) { return 2; }
    }");
            AnalyzerAssert.Valid<TestMethodUsageAnalyzer>(testCode);
        }
    }
}
