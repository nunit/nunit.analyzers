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
    public class EqualConstraintUsageAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new EqualConstraintUsageAnalyzer();
        private static readonly ExpectedDiagnostic isEqualToDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.EqualConstraintUsage,
                string.Format(CultureInfo.InvariantCulture, EqualConstraintUsageConstants.Message, "Is.EqualTo"));
        private static readonly ExpectedDiagnostic isNotEqualToDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.EqualConstraintUsage,
                string.Format(CultureInfo.InvariantCulture, EqualConstraintUsageConstants.Message, "Is.Not.EqualTo"));

        [Test]
        public void AnalyzeWhenEqualsOperatorUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(↓actual == ""abc"");");

            RoslynAssert.Diagnostics(analyzer, isEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenNotEqualsOperatorUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(↓actual != ""bcd"");");

            RoslynAssert.Diagnostics(analyzer, isNotEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIsOperatorUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(↓actual is ""abc"");");

            RoslynAssert.Diagnostics(analyzer, isEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIsNotOperatorUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(↓actual is not ""bcd"");");

            RoslynAssert.Diagnostics(analyzer, isNotEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenEqualsInstanceMethodUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(↓actual.Equals(""abc""));");

            RoslynAssert.Diagnostics(analyzer, isEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenNegatedEqualsInstanceMethodUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(↓!actual.Equals(""bcd""));");

            RoslynAssert.Diagnostics(analyzer, isNotEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenEqualsStaticMethodUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(↓Equals(actual,""abc""));");

            RoslynAssert.Diagnostics(analyzer, isEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenNegatedEqualsStaticMethodUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(↓!Equals(actual,""abc""));");

            RoslynAssert.Diagnostics(analyzer, isNotEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenEqualsMethodUsedWithIsTrue()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(↓actual.Equals(""abc""), Is.True);");

            RoslynAssert.Diagnostics(analyzer, isEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenEqualsMethodUsedWithIsFalse()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(↓actual.Equals(""abc""), Is.False);");

            RoslynAssert.Diagnostics(analyzer, isNotEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenEqualsMethodUsedWithAssertTrue()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                ClassicAssert.True(↓actual.Equals(""abc""));");

            RoslynAssert.Diagnostics(analyzer, isEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenEqualsMethodUsedWithAssertFalse()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                ClassicAssert.False(↓actual.Equals(""bcd""));");

            RoslynAssert.Diagnostics(analyzer, isNotEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenEqualsMethodUsedWithAssertIsTrue()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                ClassicAssert.IsTrue(↓actual.Equals(""abc""));");

            RoslynAssert.Diagnostics(analyzer, isEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenEqualsMethodUsedWithAssertIsFalse()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                ClassicAssert.IsFalse(↓actual.Equals(""bcd""));");

            RoslynAssert.Diagnostics(analyzer, isNotEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeDoubleNegationAsPositive()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                ClassicAssert.IsFalse(↓!actual.Equals(""bcd""));");

            RoslynAssert.Diagnostics(analyzer, isEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenAssertThatIsUsedWithMessage()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(↓actual == ""abc"", ""Assertion message"");");

            RoslynAssert.Diagnostics(analyzer, isEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenEqualsMethodUsedWithIsTrueWithMessage()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(↓actual.Equals(""abc""), Is.True, ""Assertion message"");");

            RoslynAssert.Diagnostics(analyzer, isEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenEqualsMethodUsedWithAssertTrueWithMessage()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                ClassicAssert.True(↓actual.Equals(""abc""), ""Assertion message"");");

            RoslynAssert.Diagnostics(analyzer, isEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenEqualsMethodUsedWithAssertFalseWithMessage()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                ClassicAssert.False(↓actual.Equals(""bcd""), ""Assertion message"");");

            RoslynAssert.Diagnostics(analyzer, isNotEqualToDiagnostic, testCode);
        }

        [Test]
        public void ValidWhenIsEqualToConstraintUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual, Is.EqualTo(""bcd""));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenIsNotEqualToConstraintUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual, Is.Not.EqualTo(""bcd""));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidOtherMethodUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual.Contains(""bc""));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase("==")]
        [TestCase("!=")]
        public void UseEqualsOperatorOnRefStruct(string operatorToken)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                var span1 = new[] {{ 1 }}.AsSpan();
                var span2 = new[] {{ 1 }}.AsSpan();
                Assert.That(span1 {operatorToken} span2, Is.True);");

            IEnumerable<MetadataReference> spanMetadata = MetadataReferences.Transitive(typeof(Span<>));
            IEnumerable<MetadataReference> metadataReferences = (Settings.Default.MetadataReferences ?? Enumerable.Empty<MetadataReference>()).Concat(spanMetadata);

            RoslynAssert.Valid(analyzer, testCode, Settings.Default.WithMetadataReferences(metadataReferences));
        }

        [Test]
        public void UseEqualsMethodWithRefStructArgument()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    [TestFixture]
    public class TestClass
    {
        [Test]
        public void TestMethod()
        {
            var span1 = new[] { 1 }.AsSpan();
            var span2 = new[] { 1 }.AsSpan();
            Assert.That(span1.Equals(span2), Is.True, ""Span comparison"");
        }
    }

    internal static class SpanExtension
    {
        public static bool Equals<T>(this Span<T> left, Span<T> right) => left == right;
    }");

            IEnumerable<MetadataReference> spanMetadata = MetadataReferences.Transitive(typeof(Span<>));
            IEnumerable<MetadataReference> metadataReferences = (Settings.Default.MetadataReferences ?? Enumerable.Empty<MetadataReference>()).Concat(spanMetadata);

            RoslynAssert.Valid(analyzer, testCode, Settings.Default.WithMetadataReferences(metadataReferences));
        }

        [Test]
        public void UseEqualsMethodWithRefStructInstance()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    [TestFixture]
    public class TestClass
    {
        [Test]
        public void TestMethod()
        {
            ReadOnlySpan<char> actual = GetPart();
            Assert.That(actual.Equals(""Text""), Is.True);
        }

        ReadOnlySpan<char> GetPart() => ""Some Text"".AsSpan(4);
    }");

            IEnumerable<MetadataReference> spanMetadata = MetadataReferences.Transitive(typeof(ReadOnlySpan<>));
            IEnumerable<MetadataReference> metadataReferences = (Settings.Default.MetadataReferences ?? Enumerable.Empty<MetadataReference>()).Concat(spanMetadata);

            RoslynAssert.Valid(analyzer, testCode, Settings.Default.WithMetadataReferences(metadataReferences));
        }
    }
}
