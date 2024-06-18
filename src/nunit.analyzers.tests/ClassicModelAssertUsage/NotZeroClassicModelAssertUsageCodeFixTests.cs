using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class NotZeroClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ClassicModelAssertUsageAnalyzer();
        private static readonly CodeFixProvider fix = new NotZeroClassicModelAssertUsageCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.NotZeroUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new NotZeroClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.NotZeroUsage }));
        }

        [Test]
        public void VerifyNotZeroFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expr = default(int);

            ↓ClassicAssert.NotZero(expr);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expr = default(int);

            Assert.That(expr, Is.Not.Zero);
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyNotZeroFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expr = default(int);

            ↓ClassicAssert.NotZero(expr, ""message"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expr = default(int);

            Assert.That(expr, Is.Not.Zero, ""message"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyNotZeroFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expr = default(int);

            ↓ClassicAssert.NotZero(expr, ""message-id: {0}"", Guid.NewGuid());
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expr = default(int);

            Assert.That(expr, Is.Not.Zero, $""message-id: {Guid.NewGuid()}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyNotZeroFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expr = default(int);

            ↓ClassicAssert.NotZero(expr, ""{0}, {1}"", ""first"", ""second"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expr = default(int);

            Assert.That(expr, Is.Not.Zero, $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyNotZeroFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expr = default(int);

            ↓ClassicAssert.NotZero(args: new[] { ""first"", ""second"" }, message: ""{0}, {1}"", actual: expr);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expr = default(int);

            Assert.That(expr, Is.Not.Zero, $""{""first""}, {""second""}"");
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

            ↓ClassicAssert.NotZero(
                expr{commaAndMessage});
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var expr = default(int);

            Assert.That(
                expr,
                Is.Not.Zero{commaAndMessage});
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

            ↓ClassicAssert.NotZero(
                expr{commaAndMessage}
            );
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var expr = default(int);

            Assert.That(
                expr,
                Is.Not.Zero{commaAndMessage}
            );
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixMaintainsReasonableTriviaWithAllArgumentsOnSameLine([Values] bool newlineBeforeClosingParen)
        {
            var optionalNewline = newlineBeforeClosingParen ? @"
            " : string.Empty;

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var expr = default(int);

            ↓ClassicAssert.NotZero(
                expr, ""message""{optionalNewline});
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var expr = default(int);

            Assert.That(
                expr, Is.Not.Zero, ""message""{optionalNewline});
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
