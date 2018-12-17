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
    public sealed class TestCaseUsageAnalyzerTests
    {
        private DiagnosticAnalyzer analyzer = new TestCaseUsageAnalyzer();

        [Test]
        public void VerifySupportedDiagnostics()
        {
            var analyzer = new TestCaseUsageAnalyzer();
            var diagnostics = analyzer.SupportedDiagnostics;

            var expectedIdentifiers = new List<string>
            {
                AnalyzerIdentifiers.TestCaseNotEnoughArgumentsUsage,
                AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage,
                AnalyzerIdentifiers.TestCaseTooManyArgumentsUsage
            };
            CollectionAssert.AreEquivalent(expectedIdentifiers, diagnostics.Select(d => d.Id));

            foreach (var diagnostic in diagnostics)
            {
                Assert.That(diagnostic.Title.ToString(), Is.EqualTo(TestCaseUsageAnalyzerConstants.Title),
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Title)}");
                Assert.That(diagnostic.Category, Is.EqualTo(Categories.Usage),
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Category)}");
                Assert.That(diagnostic.DefaultSeverity, Is.EqualTo(DiagnosticSeverity.Error),
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.DefaultSeverity)}");
            }

            var diagnosticMessage = diagnostics.Select(_ => _.MessageFormat.ToString()).ToImmutableArray();

            Assert.That(diagnosticMessage, Contains.Item(TestCaseUsageAnalyzerConstants.NotEnoughArgumentsMessage),
                $"{TestCaseUsageAnalyzerConstants.NotEnoughArgumentsMessage} is missing.");
            Assert.That(diagnosticMessage, Contains.Item(TestCaseUsageAnalyzerConstants.TooManyArgumentsMessage),
                $"{TestCaseUsageAnalyzerConstants.TooManyArgumentsMessage} is missing.");
            Assert.That(diagnosticMessage, Contains.Item(TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage),
                $"{TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage} is missing.");
        }

        [Test]
        public void AnalyzeWhenAttributeIsNotInNUnit()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenAttributeIsNotInNUnit
    {
        [TestCase]
        public void ATest() { }

        private sealed class TestCaseAttribute : Attribute
        { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenAttributeIsTestAttribute()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenAttributeIsTestAttribute
    {
        [Test]
        public void ATest() { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenAttributeHasNoArguments()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenAttributeHasNoArguments
    {
        [TestCase]
        public void ATest() { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenArgumentIsCorrect()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenArgumentIsCorrect
    {
        [TestCase(2)]
        public void Test(int a) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenArgumentIsACast()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class AnalyzeWhenArgumentIsACast
    {
        [TestCase((byte)2)]
        public void Test(byte a) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenArgumentIsAPrefixedValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class AnalyzeWhenArgumentIsAPrefixedValue
    {
        [TestCase(-2)]
        public void Test(int a) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenArgumentIsAReferenceToConstant()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class AnalyzeWhenArgumentIsAReferenceToConstant
    {
        const int value = 42;

        [TestCase(value)]
        public void Test(int a) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenArgumentTypeIsIncorrect()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage,
                String.Format(TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage, 0, "a"));

        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenArgumentTypeIsIncorrect
    {
        [TestCase(↓2)]
        public void Test(char a) { }
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenArgumentPassesNullToValueType()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage,
                String.Format(TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage, 0, "a"));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenArgumentPassesNullToValueType
    {
        [TestCase(↓null)]
        public void Test(char a) { }
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenArgumentPassesNullToNullableType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenArgumentPassesNullToNullableType
    {
        [TestCase(null)]
        public void Test(int? a) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenArgumentPassesValueToNullableType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenArgumentPassesValueToNullableType
    {
        [TestCase(2)]
        public void Test(int? a) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenNotEnoughRequiredArgumentsAreProvided()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestCaseNotEnoughArgumentsUsage,
                TestCaseUsageAnalyzerConstants.NotEnoughArgumentsMessage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenNotEnoughRequiredArgumentsAreProvided
    {
        [↓TestCase(2)]
        public void Test(int a, char b) { }
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenTooManyRequiredArgumentsAreProvided()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestCaseTooManyArgumentsUsage,
                TestCaseUsageAnalyzerConstants.TooManyArgumentsMessage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenTooManyRequiredArgumentsAreProvided
    {
        [↓TestCase(2, 'b')]
        public void Test(int a) { }
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenTooManyRequiredAndOptionalArgumentsAreProvided()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestCaseTooManyArgumentsUsage,
                TestCaseUsageAnalyzerConstants.TooManyArgumentsMessage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenTooManyRequiredAndOptionalArgumentsAreProvided
    {
        [↓TestCase(2, 'b', 2d)]
        public void Test(int a, char b = 'c') { }
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenMethodHasRequiredAndParamsAndMoreArgumentsThanParametersAreProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenMethodHasRequiredAndParamsAndMoreArgumentsThanParametersAreProvided
    {
        [TestCase(1, 2, 3, 4)]
        public void Test(int a, int b, params int[] c) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenMethodHasOnlyParamsAndNoArgumentsAreProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenMethodHasOnlyParamsAndNoArgumentsAreProvided
    {
        [TestCase]
        public void Test(params object[] a) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenMethodHasOnlyParamsAndArgumentTypeIsCorrect()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenMethodHasOnlyParamsAndArgumentTypeIsCorrect
    {
        [TestCase(""a"")]
        public void Test(params string[] a) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenMethodHasOnlyParamsAndArgumentTypeIsIncorrect()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage,
                String.Format(TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage, 0, "a"));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenMethodHasOnlyParamsAndArgumentTypeIsIncorrect
    {
        [TestCase(↓2)]
        public void Test(params string[] a) { }
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenMethodHasOnlyParamsAndArgumentPassesNullToValueType()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage,
                String.Format(TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage, 0, "a"));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenMethodHasOnlyParamsAndArgumentPassesNullToValueType
    {
        [TestCase(↓null)]
        public void Test(params int[] a) { }
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }
    }
}
