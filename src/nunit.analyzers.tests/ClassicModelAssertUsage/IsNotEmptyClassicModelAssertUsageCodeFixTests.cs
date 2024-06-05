using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class IsNotEmptyClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ClassicModelAssertUsageAnalyzer();
        private static readonly CodeFixProvider fix = new IsNotEmptyClassicModelAssertUsageCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.IsNotEmptyUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new IsNotEmptyClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.IsNotEmptyUsage }));
        }

        [Test]
        public void VerifyIsNotEmptyFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var collection = Array.Empty<object>();

            ↓ClassicAssert.IsNotEmpty(collection);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var collection = Array.Empty<object>();

            Assert.That(collection, Is.Not.Empty);
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotEmptyFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var collection = Array.Empty<object>();

            ↓ClassicAssert.IsNotEmpty(collection, ""message"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var collection = Array.Empty<object>();

            Assert.That(collection, Is.Not.Empty, ""message"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotEmptyFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var collection = Array.Empty<object>();

            ↓ClassicAssert.IsNotEmpty(collection, ""message-id: {0}"", Guid.NewGuid());
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var collection = Array.Empty<object>();

            Assert.That(collection, Is.Not.Empty, $""message-id: {Guid.NewGuid()}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotEmptyFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var collection = Array.Empty<object>();

            ↓ClassicAssert.IsNotEmpty(collection, ""{0}, {1}"", ""first"", ""second"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var collection = Array.Empty<object>();

            Assert.That(collection, Is.Not.Empty, $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotEmptyFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var collection = Array.Empty<object>();

            ↓ClassicAssert.IsNotEmpty(args: new[] { ""first"", ""second"" }, collection: collection, message: ""{0}, {1}"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var collection = Array.Empty<object>();

            Assert.That(collection, Is.Not.Empty, $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotEmptyWithImplicitTypeConversionFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        private struct MyString
        {
            private readonly string _value;

            public MyString(string value) => _value = value;

            public static implicit operator string(MyString value) => value._value;
            public static implicit operator MyString(string value) => new MyString(value);
        }
        public void TestMethod()
        {
            MyString s = ""Hello NUnit"";
            ↓ClassicAssert.IsNotEmpty(s);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        private struct MyString
        {
            private readonly string _value;

            public MyString(string value) => _value = value;

            public static implicit operator string(MyString value) => value._value;
            public static implicit operator MyString(string value) => new MyString(value);
        }
        public void TestMethod()
        {
            MyString s = ""Hello NUnit"";
            Assert.That((string)s, Is.Not.Empty);
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotEmptyWithImplicitTypeConversionFixInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        private struct MyString
        {
            private readonly string _value;

            public MyString(string value) => _value = value;

            public static implicit operator string(MyString value) => value._value;
            public static implicit operator MyString(string value) => new MyString(value);
        }
        public void TestMethod()
        {
            MyString s = ""Hello NUnit"";
            ↓ClassicAssert.IsNotEmpty(args: new[] { ""first"", ""second"" }, aString: s, message: ""{0}, {1}"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        private struct MyString
        {
            private readonly string _value;

            public MyString(string value) => _value = value;

            public static implicit operator string(MyString value) => value._value;
            public static implicit operator MyString(string value) => new MyString(value);
        }
        public void TestMethod()
        {
            MyString s = ""Hello NUnit"";
            Assert.That((string)s, Is.Not.Empty, $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixMaintainsReasonableTriviaWithEndOfLineClosingParen([Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage
                ? @",
                ""message"""
                : string.Empty;
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var collection = Array.Empty<object>();

            ↓ClassicAssert.IsNotEmpty(
                collection{commaAndMessage});
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var collection = Array.Empty<object>();

            Assert.That(
                collection,
                Is.Not.Empty{commaAndMessage});
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixMaintainsReasonableTriviaWithNewLineClosingParen([Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage
                ? @",
                ""message"""
                : string.Empty;
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var collection = Array.Empty<object>();

            ↓ClassicAssert.IsNotEmpty(
                collection{commaAndMessage}
            );
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var collection = Array.Empty<object>();

            Assert.That(
                collection,
                Is.Not.Empty{commaAndMessage}
            );
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
