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
    public sealed class IsFalseAndFalseClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ClassicModelAssertUsageAnalyzer();
        private static readonly CodeFixProvider fix = new IsFalseAndFalseClassicModelAssertUsageCodeFix();
        private static readonly Dictionary<string, string> diagnosticIdsToAssertions = new()
        {
            { AnalyzerIdentifiers.FalseUsage, nameof(ClassicAssert.False) },
            { AnalyzerIdentifiers.IsFalseUsage, nameof(ClassicAssert.IsFalse) },
        };
        private static readonly string[] diagnosticIds = diagnosticIdsToAssertions.Keys.ToArray();

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new IsFalseAndFalseClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.IsFalseUsage, AnalyzerIdentifiers.FalseUsage }));
        }

        [TestCase("IsFalse", AnalyzerIdentifiers.IsFalseUsage)]
        [TestCase("False", AnalyzerIdentifiers.FalseUsage)]
        public void VerifyIsFalseAndFalseFixes(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            ↓ClassicAssert.{assertion}(false);
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(false, Is.False);
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsFalse", AnalyzerIdentifiers.IsFalseUsage)]
        [TestCase("False", AnalyzerIdentifiers.FalseUsage)]
        public void VerifyIsFalseAndFalseFixesWithMessage(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            ↓ClassicAssert.{assertion}(false, ""message"");
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(false, Is.False, ""message"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsFalse", AnalyzerIdentifiers.IsFalseUsage)]
        [TestCase("False", AnalyzerIdentifiers.FalseUsage)]
        public void VerifyIsFalseAndFalseFixesWithMessageAndOneArgumentForParams(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            ↓ClassicAssert.{assertion}(false, ""message-id: {{0}}"", Guid.NewGuid());
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(false, Is.False, $""message-id: {Guid.NewGuid()}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsFalse", AnalyzerIdentifiers.IsFalseUsage)]
        [TestCase("False", AnalyzerIdentifiers.FalseUsage)]
        public void VerifyIsFalseAndFalseFixesWithMessageAndTwoArgumentsForParams(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            ↓ClassicAssert.{assertion}(false, ""{{0}}, {{1}}"", ""first"", ""second"");
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(false, Is.False, $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsFalse", AnalyzerIdentifiers.IsFalseUsage)]
        [TestCase("False", AnalyzerIdentifiers.FalseUsage)]
        public void VerifyIsFalseAndFalseFixesWithMessageAndArrayParamsInNonstandardOrder(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            ↓ClassicAssert.{assertion}(args: new[] {{ ""first"", ""second"" }}, message: ""{{0}}, {{1}}"", condition: false);
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(false, Is.False, $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsFalse", AnalyzerIdentifiers.IsFalseUsage)]
        [TestCase("False", AnalyzerIdentifiers.FalseUsage)]
        public void VerifyIsFalseAndFalseWithImplicitTypeConversionFixes(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private struct MyBool
        {{
            private readonly bool _value;

            public MyBool(bool value) => _value = value;

            public static implicit operator bool(MyBool value) => value._value;
            public static implicit operator MyBool(bool value) => new MyBool(value);
        }}
        public void TestMethod()
        {{
            MyBool x = false;
            ↓ClassicAssert.{assertion}(x);
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        private struct MyBool
        {
            private readonly bool _value;

            public MyBool(bool value) => _value = value;

            public static implicit operator bool(MyBool value) => value._value;
            public static implicit operator MyBool(bool value) => new MyBool(value);
        }
        public void TestMethod()
        {
            MyBool x = false;
            Assert.That((bool)x, Is.False);
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
                ? @",
                ""message"""
                : string.Empty;
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            ↓ClassicAssert.{assertion}(
                false{commaAndMessage});
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            Assert.That(
                false,
                Is.False{commaAndMessage});
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
                ? @",
                ""message"""
                : string.Empty;
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            ↓ClassicAssert.{assertion}(
                false{commaAndMessage}
            );
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            Assert.That(
                false,
                Is.False{commaAndMessage}
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
            ↓ClassicAssert.{assertion}(
                false, ""message""{optionalNewline});
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            Assert.That(
                false, Is.False, ""message""{optionalNewline});
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
