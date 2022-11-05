using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.ConstraintUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ConstraintsUsage
{
    public sealed class ComparisonConstraintUsageAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ComparisonConstraintUsageAnalyzer();

        [TestCase(">=", "Is.GreaterThanOrEqualTo")]
        [TestCase(">", "Is.GreaterThan")]
        [TestCase("<=", "Is.LessThanOrEqualTo")]
        [TestCase("<", "Is.LessThan")]
        public void AnalyzeWhenComparisonOperatorUsed(string operatorToken, string message)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                int actual = 5;
                Assert.That(↓actual {operatorToken} 9);");

            var diagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.ComparisonConstraintUsage,
                string.Format(CultureInfo.InvariantCulture, ComparisonConstraintUsageConstants.Message, message));
            RoslynAssert.Diagnostics(analyzer, diagnostic, testCode);
        }

        [TestCase(">=", "Is.GreaterThanOrEqualTo")]
        [TestCase(">", "Is.GreaterThan")]
        [TestCase("<=", "Is.LessThanOrEqualTo")]
        [TestCase("<", "Is.LessThan")]
        public void AnalyzeWhenComparisonOperatorUsedWithIsTrue(string operatorToken, string message)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                int actual = 5;
                Assert.That(↓actual {operatorToken} 9, Is.True);");

            var diagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.ComparisonConstraintUsage,
                string.Format(CultureInfo.InvariantCulture, ComparisonConstraintUsageConstants.Message, message));
            RoslynAssert.Diagnostics(analyzer, diagnostic, testCode);
        }

        [TestCase(">=", "Is.LessThan")]
        [TestCase(">", "Is.LessThanOrEqualTo")]
        [TestCase("<=", "Is.GreaterThan")]
        [TestCase("<", "Is.GreaterThanOrEqualTo")]
        public void AnalyzeWhenComparisonOperatorUsedWithIsFalse(string operatorToken, string message)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                int actual = 5;
                Assert.That(↓actual {operatorToken} 9, Is.False);");

            var diagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.ComparisonConstraintUsage,
                string.Format(CultureInfo.InvariantCulture, ComparisonConstraintUsageConstants.Message, message));
            RoslynAssert.Diagnostics(analyzer, diagnostic, testCode);
        }

        [TestCase("Is.GreaterThanOrEqualTo")]
        [TestCase("Is.GreaterThan")]
        [TestCase("Is.LessThanOrEqualTo")]
        [TestCase("Is.LessThan")]
        public void ValidWhenConstraintUsed(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                int actual = 5;
                Assert.That(actual, {constraint}(9));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase(">=")]
        [TestCase(">")]
        [TestCase("<=")]
        [TestCase("<")]
        public void ValidOnRefStruct(string operatorToken)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@$"
    [TestFixture]
    public class TestClass
    {{
        [Test]
        public void TestMethod()
        {{
            ComparableSpan<char> span1 = ""Hello"".AsSpan();
            ComparableSpan<char> span2 = ""World"".AsSpan();
            Assert.That(span1 {operatorToken} span2);
        }}

        private ref struct ComparableSpan<T>
            where T : IComparable
        {{
            private readonly ReadOnlySpan<T> span;

            public ComparableSpan(ReadOnlySpan<T> span) => this.span = span;

            public static implicit operator ReadOnlySpan<T>(ComparableSpan<T> c) => c.span;
            public static implicit operator ComparableSpan<T>(ReadOnlySpan<T> c) => new(c);

            public static bool operator <(ComparableSpan<T> left, ComparableSpan<T> right) => false;
            public static bool operator <=(ComparableSpan<T> left, ComparableSpan<T> right) => false;

            public static bool operator >(ComparableSpan<T> left, ComparableSpan<T> right) => true;
            public static bool operator >=(ComparableSpan<T> left, ComparableSpan<T> right) => true;
        }}
    }}");

            IEnumerable<MetadataReference> spanMetadata = MetadataReferences.Transitive(typeof(Span<>));
            IEnumerable<MetadataReference> metadataReferences = (Settings.Default.MetadataReferences ?? Enumerable.Empty<MetadataReference>()).Concat(spanMetadata);

            RoslynAssert.Valid(analyzer, testCode, Settings.Default.WithMetadataReferences(metadataReferences));
        }
    }
}
