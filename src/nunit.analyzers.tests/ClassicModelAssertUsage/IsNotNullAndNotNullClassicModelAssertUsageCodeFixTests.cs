using System.Collections.Generic;
using System.Linq;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
#if NUNIT4
using NUnit.Framework.Legacy;
#else
using ClassicAssert = NUnit.Framework.Assert;
#endif

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class IsNotNullAndNotNullClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ClassicModelAssertUsageAnalyzer();
        private static readonly CodeFixProvider fix = new IsNotNullAndNotNullClassicModelAssertUsageCodeFix();
        private static readonly Dictionary<string, string> diagnosticIdsToAssertions = new()
        {
            { AnalyzerIdentifiers.NotNullUsage, nameof(ClassicAssert.NotNull) },
            { AnalyzerIdentifiers.IsNotNullUsage, nameof(ClassicAssert.IsNotNull) },
        };
        private static readonly string[] diagnosticIds = diagnosticIdsToAssertions.Keys.ToArray();

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new IsNotNullAndNotNullClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.IsNotNullUsage, AnalyzerIdentifiers.NotNullUsage }));
        }

        [TestCase("IsNotNull", AnalyzerIdentifiers.IsNotNullUsage)]
        [TestCase("NotNull", AnalyzerIdentifiers.NotNullUsage)]
        public void VerifyIsNotNullAndNotNullFixes(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            object? obj = null;
            ↓ClassicAssert.{assertion}(obj);
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            object? obj = null;
            Assert.That(obj, Is.Not.Null);
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsNotNull", AnalyzerIdentifiers.IsNotNullUsage)]
        [TestCase("NotNull", AnalyzerIdentifiers.NotNullUsage)]
        public void VerifyIsNotNullAndNotNullFixesWithMessage(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            object? obj = null;
            ↓ClassicAssert.{assertion}(obj, ""message"");
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            object? obj = null;
            Assert.That(obj, Is.Not.Null, ""message"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsNotNull", AnalyzerIdentifiers.IsNotNullUsage)]
        [TestCase("NotNull", AnalyzerIdentifiers.NotNullUsage)]
        public void VerifyIsNotNullAndNotNullFixesWithMessageAndOneArgumentForParams(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            object? obj = null;
            ↓ClassicAssert.{assertion}(obj, ""message-id: {{0}}"", Guid.NewGuid());
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            object? obj = null;
            Assert.That(obj, Is.Not.Null, $""message-id: {Guid.NewGuid()}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsNotNull", AnalyzerIdentifiers.IsNotNullUsage)]
        [TestCase("NotNull", AnalyzerIdentifiers.NotNullUsage)]
        public void VerifyIsNotNullAndNotNullFixesWithMessageAndTwoArgumentsForParams(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            object? obj = null;
            ↓ClassicAssert.{assertion}(obj, ""{{0}}, {{1}}"", ""first"", ""second"");
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            object? obj = null;
            Assert.That(obj, Is.Not.Null, $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsNotNull", AnalyzerIdentifiers.IsNotNullUsage)]
        [TestCase("NotNull", AnalyzerIdentifiers.NotNullUsage)]
        public void VerifyIsNotNullAndNotNullFixesWithMessageAndArrayParamsInNonstandardOrder(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            object? obj = null;
            ↓ClassicAssert.{assertion}(args: new[] {{ ""first"", ""second"" }}, message: ""{{0}}, {{1}}"", anObject: obj);
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            object? obj = null;
            Assert.That(obj, Is.Not.Null, $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixMaintainsReasonableTriviaWithEndOfLineClosingParen(
            [ValueSource(nameof(diagnosticIds))] string diagnosticId,
            [Values] bool hasMessage)
        {
            var assertion = diagnosticIdsToAssertions[diagnosticId];
            var commaAndMessage = hasMessage
                ? ",\r\n                \"message\""
                : string.Empty;
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            object? obj = null;
            ↓ClassicAssert.{assertion}(
                obj{commaAndMessage});
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            object? obj = null;
            Assert.That(
                obj,
                Is.Not.Null{commaAndMessage});
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixMaintainsReasonableTriviaWithNewLineClosingParen(
            [ValueSource(nameof(diagnosticIds))] string diagnosticId,
            [Values] bool hasMessage)
        {
            var assertion = diagnosticIdsToAssertions[diagnosticId];
            var commaAndMessage = hasMessage
                ? ",\r\n                \"message\""
                : string.Empty;
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            object? obj = null;
            ↓ClassicAssert.{assertion}(
                obj{commaAndMessage}
            );
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            object? obj = null;
            Assert.That(
                obj,
                Is.Not.Null{commaAndMessage}
            );
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixMaintainsReasonableTriviaWithAllArgumentsOnSameLine(
            [ValueSource(nameof(diagnosticIds))] string diagnosticId,
            [Values] bool newlineBeforeClosingParen)
        {
            var optionalNewline = newlineBeforeClosingParen ? @"
            " : string.Empty;
            var assertion = diagnosticIdsToAssertions[diagnosticId];
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            object? obj = null;
            ↓ClassicAssert.{assertion}(
                obj, ""message""{optionalNewline});
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            object? obj = null;
            Assert.That(
                obj, Is.Not.Null, ""message""{optionalNewline});
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
