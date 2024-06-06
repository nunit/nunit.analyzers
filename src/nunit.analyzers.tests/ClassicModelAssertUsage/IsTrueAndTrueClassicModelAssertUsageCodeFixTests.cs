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
    public sealed class IsTrueAndTrueClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ClassicModelAssertUsageAnalyzer();
        private static readonly CodeFixProvider fix = new IsTrueAndTrueClassicModelAssertUsageCodeFix();
        private static readonly Dictionary<string, string> diagnosticIdsToAssertions = new()
        {
            { AnalyzerIdentifiers.TrueUsage, nameof(ClassicAssert.True) },
            { AnalyzerIdentifiers.IsTrueUsage, nameof(ClassicAssert.IsTrue) },
        };
        private static readonly string[] diagnosticIds = diagnosticIdsToAssertions.Keys.ToArray();

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new IsTrueAndTrueClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.IsTrueUsage, AnalyzerIdentifiers.TrueUsage }));
        }

        [TestCase("IsTrue", AnalyzerIdentifiers.IsTrueUsage)]
        [TestCase("True", AnalyzerIdentifiers.TrueUsage)]
        public void VerifyIsTrueAndTrueFixes(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            ↓ClassicAssert.{assertion}(true);
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(true, Is.True);
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsTrue", AnalyzerIdentifiers.IsTrueUsage)]
        [TestCase("True", AnalyzerIdentifiers.TrueUsage)]
        public void VerifyIsTrueAndTrueFixesWithMessage(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            ↓ClassicAssert.{assertion}(true, ""message"");
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(true, Is.True, ""message"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsTrue", AnalyzerIdentifiers.IsTrueUsage)]
        [TestCase("True", AnalyzerIdentifiers.TrueUsage)]
        public void VerifyIsTrueAndTrueFixesWithMessageAndOneArgumentForParams(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            ↓ClassicAssert.{assertion}(true, ""message-id: {{0}}"", Guid.NewGuid());
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(true, Is.True, $""message-id: {Guid.NewGuid()}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsTrue", AnalyzerIdentifiers.IsTrueUsage)]
        [TestCase("True", AnalyzerIdentifiers.TrueUsage)]
        public void VerifyIsTrueAndTrueFixesWithMessageAndTwoArgumentsForParams(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            ↓ClassicAssert.{assertion}(true, ""{{0}}, {{1}}"", ""first"", ""second"");
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(true, Is.True, $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsTrue", AnalyzerIdentifiers.IsTrueUsage)]
        [TestCase("True", AnalyzerIdentifiers.TrueUsage)]
        public void VerifyIsTrueAndTrueFixesWithMessageAndArrayParamsInNonstandardOrder(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            ↓ClassicAssert.{assertion}(args: new[] {{ ""first"", ""second"" }}, message: ""{{0}}, {{1}}"", condition: true);
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(true, Is.True, $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsTrue", AnalyzerIdentifiers.IsTrueUsage)]
        [TestCase("True", AnalyzerIdentifiers.TrueUsage)]
        public void VerifyIsTrueAndTrueWithImplicitTypeConversionFixes(string assertion, string diagnosticId)
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
            MyBool x = true;
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
            MyBool x = true;
            Assert.That((bool)x, Is.True);
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsTrue", AnalyzerIdentifiers.IsTrueUsage)]
        [TestCase("True", AnalyzerIdentifiers.TrueUsage)]
        public void VerifyIsTrueAndTrueWithImplicitTypeConversionFixesInNonstandardOrder(string assertion, string diagnosticId)
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
            MyBool x = true;
            ↓ClassicAssert.{assertion}(args: new[] {{ ""first"", ""second"" }}, message: ""{{0}}, {{1}}"", condition: x);
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
            MyBool x = true;
            Assert.That((bool)x, Is.True, $""{""first""}, {""second""}"");
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
                true{commaAndMessage});
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            Assert.That(
                true,
                Is.True{commaAndMessage});
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
                true{commaAndMessage}
            );
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            Assert.That(
                true,
                Is.True{commaAndMessage}
            );
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
