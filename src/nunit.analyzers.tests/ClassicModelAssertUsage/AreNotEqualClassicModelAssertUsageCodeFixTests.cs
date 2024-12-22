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
    public sealed class AreNotEqualClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ClassicModelAssertUsageAnalyzer();
        private static readonly CodeFixProvider fix = new AreNotEqualClassicModelAssertUsageCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.AreNotEqualUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new AreNotEqualClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.AreNotEqualUsage }));
        }

        [Test]
        public void VerifyAreNotEqualFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.AreNotEqual(2d, 3d);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(3d, Is.Not.EqualTo(2d));
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyAreNotEqualFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.AreNotEqual(2d, 3d, ""message"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(3d, Is.Not.EqualTo(2d), ""message"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyAreNotEqualFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.AreNotEqual(2d, 3d, ""message-id: {0}"", Guid.NewGuid());
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(3d, Is.Not.EqualTo(2d), $""message-id: {Guid.NewGuid()}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyAreNotEqualFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.AreNotEqual(2d, 3d, ""{0}, {1}"", ""first"", ""second"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(3d, Is.Not.EqualTo(2d), $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyAreNotEqualFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.AreNotEqual(args: new[] { ""first"", ""second"" }, actual: ""actual"", message: ""{0}, {1}"", expected: ""expected"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(""actual"", Is.Not.EqualTo(""expected""), $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixMaintainsReasonableTriviaWithEndOfLineClosingParen([Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage
                ? ",\r\n                \"message\""
                : string.Empty;
            var code = TestUtility.WrapInTestMethod($@"
            ↓ClassicAssert.AreNotEqual(
                2d,
                3d{commaAndMessage});");

            var fixedCode = TestUtility.WrapInTestMethod($@"
            Assert.That(
                3d,
                Is.Not.EqualTo(2d){commaAndMessage});");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixMaintainsReasonableTriviaWithNewLineClosingParen([Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage
                ? ",\r\n                \"message\""
                : string.Empty;
            var code = TestUtility.WrapInTestMethod($@"
            ↓ClassicAssert.AreNotEqual(
                2d,
                3d{commaAndMessage}
            );");

            var fixedCode = TestUtility.WrapInTestMethod($@"
            Assert.That(
                3d,
                Is.Not.EqualTo(2d){commaAndMessage}
            );");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixMaintainsReasonableTriviaWithAllArgumentsOnSameLine([Values] bool newlineBeforeClosingParen)
        {
            var optionalNewline = newlineBeforeClosingParen ? "\r\n            " : string.Empty;
            var code = TestUtility.WrapInTestMethod($@"
            ↓ClassicAssert.AreNotEqual(
                2d, 3d{optionalNewline});");

            var fixedCode = TestUtility.WrapInTestMethod($@"
            Assert.That(
                3d, Is.Not.EqualTo(2d){optionalNewline});");

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
            ↓ClassicAssert.AreNotEqual({expected}, value);",
            additionalUsings);

            var fixedCode = TestUtility.WrapInTestMethod($@"
            string value = ""Value"";
            Assert.That(value, Is.Not.Empty);",
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
            const string UsingSystemCollectionsImmutable = "using System.Collections.Immutable;";

            var code = TestUtility.WrapInTestMethod($@"
            string value = ""Value"";
            ↓ClassicAssert.AreNotEqual({expected}, value);",
            UsingSystemCollectionsImmutable);

            var fixedCode = TestUtility.WrapInTestMethod($@"
            string value = ""Value"";
            Assert.That(value, Is.Not.Empty);",
            UsingSystemCollectionsImmutable);

            IEnumerable<MetadataReference> existingReferences = Settings.Default.MetadataReferences ?? Enumerable.Empty<MetadataReference>();

            Settings settings = Settings.Default
                                        .WithMetadataReferences(existingReferences.Concat(MetadataReferences.Transitive(typeof(ImmutableArray<>))));

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode,
                fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription,
                settings: settings);
        }
    }
}
