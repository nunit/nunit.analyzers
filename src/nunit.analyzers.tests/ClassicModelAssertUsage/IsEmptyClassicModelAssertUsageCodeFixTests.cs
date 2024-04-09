using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class IsEmptyClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ClassicModelAssertUsageAnalyzer();
        private static readonly CodeFixProvider fix = new IsEmptyClassicModelAssertUsageCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.IsEmptyUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new IsEmptyClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.IsEmptyUsage }));
        }

        [Test]
        public void VerifyIsEmptyFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var collection = Array.Empty<object>();

            ↓ClassicAssert.IsEmpty(collection);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var collection = Array.Empty<object>();

            Assert.That(collection, Is.Empty);
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsEmptyFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var collection = Array.Empty<object>();

            ↓ClassicAssert.IsEmpty(collection, ""message"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var collection = Array.Empty<object>();

            Assert.That(collection, Is.Empty, ""message"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsEmptyFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var collection = Array.Empty<object>();

            ↓ClassicAssert.IsEmpty(collection, ""message-id: {0}"", Guid.NewGuid());
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var collection = Array.Empty<object>();

            Assert.That(collection, Is.Empty, $""message-id: {Guid.NewGuid()}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsEmptyFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var collection = Array.Empty<object>();

            ↓ClassicAssert.IsEmpty(collection, ""{0}, {1}"", ""first"", ""second"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var collection = Array.Empty<object>();

            Assert.That(collection, Is.Empty, $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsEmptyFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var collection = Array.Empty<object>();

            ↓ClassicAssert.IsEmpty(args: new[] { ""first"", ""second"" }, message: ""{0}, {1}"", collection: collection);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var collection = Array.Empty<object>();

            Assert.That(collection, Is.Empty, $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsEmptyWithImplicitTypeConversionFix()
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
            MyString s = string.Empty;
            ↓ClassicAssert.IsEmpty(s);
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
            MyString s = string.Empty;
            Assert.That((string)s, Is.Empty);
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
