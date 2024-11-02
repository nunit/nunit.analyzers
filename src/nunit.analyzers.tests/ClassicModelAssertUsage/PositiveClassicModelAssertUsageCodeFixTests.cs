using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class PositiveClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ClassicModelAssertUsageAnalyzer();
        private static readonly CodeFixProvider fix = new PositiveClassicModelAssertUsageCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.PositiveUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new PositiveClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.PositiveUsage }));
        }

        [Test]
        public void VerifyPositiveFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expr = default(int);

            ↓ClassicAssert.Positive(expr);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expr = default(int);

            Assert.That(expr, Is.Positive);
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyPositiveFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expr = default(int);

            ↓ClassicAssert.Positive(expr, ""message"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expr = default(int);

            Assert.That(expr, Is.Positive, ""message"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyPositiveFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expr = default(int);

            ↓ClassicAssert.Positive(expr, ""message-id: {0}"", Guid.NewGuid());
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expr = default(int);

            Assert.That(expr, Is.Positive, $""message-id: {Guid.NewGuid()}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyPositiveFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expr = default(int);

            ↓ClassicAssert.Positive(expr, ""{0}, {1}"", ""first"", ""second"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expr = default(int);

            Assert.That(expr, Is.Positive, $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyPositiveFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expr = default(int);

            ↓ClassicAssert.Positive(args: new[] { ""first"", ""second"" }, message: ""{0}, {1}"", actual: expr);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expr = default(int);

            Assert.That(expr, Is.Positive, $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixMaintainsReasonableTriviaWithEndOfLineClosingParen([Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage
                ? ",\r\n                \"message\""
                : string.Empty;
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var expr = default(int);

            ↓ClassicAssert.Positive(
                expr{commaAndMessage});
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var expr = default(int);

            Assert.That(
                expr,
                Is.Positive{commaAndMessage});
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixMaintainsReasonableTriviaWithNewLineClosingParen([Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage
                ? ",\r\n                \"message\""
                : string.Empty;
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var expr = default(int);

            ↓ClassicAssert.Positive(
                expr{commaAndMessage}
            );
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var expr = default(int);

            Assert.That(
                expr,
                Is.Positive{commaAndMessage}
            );
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixMaintainsReasonableTriviaWithAllArgumentsOnSameLine([Values] bool newlineBeforeClosingParen)
        {
            var optionalNewline = newlineBeforeClosingParen ? "\r\n            " : string.Empty;

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var expr = default(int);

            ↓ClassicAssert.Positive(
                expr, ""message""{optionalNewline});
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var expr = default(int);

            Assert.That(
                expr, Is.Positive, ""message""{optionalNewline});
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
