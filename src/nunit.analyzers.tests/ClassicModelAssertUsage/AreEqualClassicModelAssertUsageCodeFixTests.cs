using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class AreEqualClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ClassicModelAssertUsageAnalyzer();
        private static readonly CodeFixProvider fix = new AreEqualClassicModelAssertUsageCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.AreEqualUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new AreEqualClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.AreEqualUsage }));
        }

        [Test]
        public void VerifyAreEqualFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.AreEqual(2d, 3d);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(3d, Is.EqualTo(2d));
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyAreEqualFixWhenToleranceExistsWithNamedArgumentButInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.AreEqual(delta: 0.0000001d, actual: 3d, expected: 2d);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(3d, Is.EqualTo(2d).Within(0.0000001d));
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyAreEqualFixWithNamedParametersInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.AreEqual(actual: 3d, expected: 2d);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(3d, Is.EqualTo(2d));
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyAreEqualFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.AreEqual(2d, 3d, ""message"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(3d, Is.EqualTo(2d), ""message"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyAreEqualFixWithNullMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.AreEqual(2d, 3d, null);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(3d, Is.EqualTo(2d));
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyAreEqualFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.AreEqual(2d, 3d, ""message-id: {0}"", Guid.NewGuid());
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(3d, Is.EqualTo(2d), $""message-id: {Guid.NewGuid()}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyAreEqualFixWithMessageAnd2ParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var actual = 3d;
            ↓ClassicAssert.AreEqual(delta: 0.0000001d, expected: 2d, actual: actual, args: new[] { ""Guid.NewGuid()"", Guid.NewGuid().ToString() }, message: ""message-id: {0}, {1}"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var actual = 3d;
            Assert.That(actual, Is.EqualTo(2d).Within(0.0000001d), $""message-id: {""Guid.NewGuid()""}, {Guid.NewGuid().ToString()}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyAreEqualFixWithMessageAnd2ParamsInStandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var actual = 3d;
            ↓ClassicAssert.AreEqual(expected: 2d, actual: actual, delta: 0.0000001d, ""message-id: {0}, {1}"", ""Guid.NewGuid()"", Guid.NewGuid());
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var actual = 3d;
            Assert.That(actual, Is.EqualTo(2d).Within(0.0000001d), $""message-id: {""Guid.NewGuid()""}, {Guid.NewGuid()}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyAreEqualFixWhenToleranceExists()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.AreEqual(2d, 3d, 0.0000001d);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(3d, Is.EqualTo(2d).Within(0.0000001d));
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyAreEqualFixWhenToleranceExistsWithNamedArgument()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.AreEqual(expected: 2d, actual: 3d, delta: 0.0000001d);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(3d, Is.EqualTo(2d).Within(0.0000001d));
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyAreEqualFixWhenToleranceExistsWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.AreEqual(2d, 3d, 0.0000001d, ""message"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(3d, Is.EqualTo(2d).Within(0.0000001d), ""message"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyAreEqualFixWhenToleranceExistsWithNullMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.AreEqual(2d, 3d, 0.1, null);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(3d, Is.EqualTo(2d).Within(0.1));
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyAreEqualFixWhenToleranceExistsWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.AreEqual(2d, 3d, 0.0000001d, ""message-id: {0}"", Guid.NewGuid());
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(3d, Is.EqualTo(2d).Within(0.0000001d), $""message-id: {Guid.NewGuid()}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyAreEqualFixWhenToleranceExistsWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.AreEqual(2d, 3d, 0.0000001d, ""{0}, {1}"", ""first"", ""second"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(3d, Is.EqualTo(2d).Within(0.0000001d), $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyAreEqualFixWhenWithMessageAndArgsArray()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [TestCase(3.0, 3)]
        public void Test(object actual, object expected)
        {
            object[] args = { expected, actual };
            ↓ClassicAssert.AreEqual(expected, actual, ""Expected: {0} Got: {1}"", args);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [TestCase(3.0, 3)]
        public void Test(object actual, object expected)
        {
            object[] args = { expected, actual };
            Assert.That(actual, Is.EqualTo(expected), () => string.Format(""Expected: {0} Got: {1}"", args));
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyAreEqualFixWhenWithMessageVariable()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [TestCase(3.0, 3)]
        public void Test(object actual, object expected)
        {
            ↓ClassicAssert.AreEqual(expected, actual, GetLocalizedFormatSpecification(), expected, actual);
        }
        private static string GetLocalizedFormatSpecification() => ""Expected: {0} Got: {1}"";
        ");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [TestCase(3.0, 3)]
        public void Test(object actual, object expected)
        {
            Assert.That(actual, Is.EqualTo(expected), () => string.Format(GetLocalizedFormatSpecification(), expected, actual));
        }
        private static string GetLocalizedFormatSpecification() => ""Expected: {0} Got: {1}"";
        ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixMaintainsReasonableTriviaWithEndOfLineClosingParen([Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage
                ? ",\r\n                \"message\""
                : string.Empty;
            var code = TestUtility.WrapInTestMethod($@"
            ↓ClassicAssert.AreEqual(
                2d,
                3d,
                0.0000001d{commaAndMessage});");

            var fixedCode = TestUtility.WrapInTestMethod($@"
            Assert.That(
                3d,
                Is.EqualTo(2d).Within(0.0000001d){commaAndMessage});");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixMaintainsReasonableTriviaWithNewLineClosingParen([Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage
                ? ",\r\n                \"message\""
                : string.Empty;
            var code = TestUtility.WrapInTestMethod($@"
            ↓ClassicAssert.AreEqual(
                2d,
                3d,
                0.0000001d{commaAndMessage}
            );");

            var fixedCode = TestUtility.WrapInTestMethod($@"
            Assert.That(
                3d,
                Is.EqualTo(2d).Within(0.0000001d){commaAndMessage}
            );");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixMaintainsReasonableTriviaWithAllArgumentsOnSameLine([Values] bool newlineBeforeClosingParen)
        {
            var optionalNewline = newlineBeforeClosingParen ? "\r\n            " : string.Empty;
            var code = TestUtility.WrapInTestMethod($@"
            ↓ClassicAssert.AreEqual(
                2d, 3d, 0.0000001d{optionalNewline});");

            var fixedCode = TestUtility.WrapInTestMethod($@"
            Assert.That(
                3d, Is.EqualTo(2d).Within(0.0000001d){optionalNewline});");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("string.Empty")]
        [TestCase("String.Empty")]
        [TestCase("Guid.Empty")]
        [TestCase("\"\"")]
        [TestCase("Array.Empty<int>()")]
        [TestCase("Enumerable.Empty<int>()", "using System.Linq;")]
        public void CodeFixUsesIsEmpty(string expected, string? additionalUsings = null)
        {
            var code = TestUtility.WrapInTestMethod($@"
            string value = ""Value"";
            ↓ClassicAssert.AreEqual({expected}, value);",
            additionalUsings);

            var fixedCode = TestUtility.WrapInTestMethod($@"
            string value = ""Value"";
            Assert.That(value, Is.Empty);",
            additionalUsings);

            IEnumerable<MetadataReference> existingReferences = Settings.Default.MetadataReferences ?? Enumerable.Empty<MetadataReference>();

            Settings settings = Settings.Default
                                        .WithMetadataReferences(existingReferences.Concat(MetadataReferences.Transitive(typeof(ImmutableArray<>))));

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode,
                fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription,
                settings: settings);
        }

        [TestCase("ImmutableArray<int>.Empty")]
        [TestCase("ImmutableList<int>.Empty")]
        [TestCase("ImmutableHashSet<int>.Empty")]
        public void CodeFixUsesIsEmpty(string expected)
        {
            const string usingSystemCollectionsImmutable = "using System.Collections.Immutable;";

            var code = TestUtility.WrapInTestMethod($@"
            string value = ""Value"";
            ↓ClassicAssert.AreEqual({expected}, value);",
            usingSystemCollectionsImmutable);

            var fixedCode = TestUtility.WrapInTestMethod($@"
            string value = ""Value"";
            Assert.That(value, Is.Empty);",
            usingSystemCollectionsImmutable);

            IEnumerable<MetadataReference> existingReferences = Settings.Default.MetadataReferences ?? Enumerable.Empty<MetadataReference>();

            Settings settings = Settings.Default
                                        .WithMetadataReferences(existingReferences.Concat(MetadataReferences.Transitive(typeof(ImmutableArray<>))));

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode,
                fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription,
                settings: settings);
        }

        [Test]
        public void CodeFixUsesIsEmpty()
        {
            const string usingSystemCollectionsImmutable = "using System.Collections.Generic;";

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [Test]
        public void TestMethod()
        {{
            MyCollection value = new MyCollection();
            ↓ClassicAssert.AreEqual(MyCollection.Empty, value);
        }}

        public class MyCollection : List<int>
        {{
            public static readonly MyCollection Empty = new MyCollection();
        }}",
            usingSystemCollectionsImmutable);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [Test]
        public void TestMethod()
        {{
            MyCollection value = new MyCollection();
            Assert.That(value, Is.Empty);
        }}

        public class MyCollection : List<int>
        {{
            public static readonly MyCollection Empty = new MyCollection();
        }}",
            usingSystemCollectionsImmutable);

            IEnumerable<MetadataReference> existingReferences = Settings.Default.MetadataReferences ?? Enumerable.Empty<MetadataReference>();

            Settings settings = Settings.Default
                                        .WithMetadataReferences(existingReferences.Concat(MetadataReferences.Transitive(typeof(ImmutableArray<>))));

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode,
                fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription,
                settings: settings);
        }

        [Test]
        public void CodeFixDoesNotUseIsEmpty()
        {
            var code = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class TestClass
    {
        [Test]
        public void TestMethod()
        {
            var rangeValue = Range.Empty;
            ↓ClassicAssert.AreEqual(Range.Empty, rangeValue);
        }

        public enum Range
        {
            Empty,
            Full
        }
    }");

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class TestClass
    {
        [Test]
        public void TestMethod()
        {
            var rangeValue = Range.Empty;
            Assert.That(rangeValue, Is.EqualTo(Range.Empty));
        }

        public enum Range
        {
            Empty,
            Full
        }
    }");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode,
                fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
