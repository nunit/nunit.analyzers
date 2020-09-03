using System.Collections.Immutable;
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
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var collection = Array.Empty<object>();

            ↓Assert.IsNotEmpty(collection);
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var collection = Array.Empty<object>();

            Assert.That(collection, Is.Not.Empty);
        }");
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotEmptyFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var collection = Array.Empty<object>();

            ↓Assert.IsNotEmpty(collection, ""message"");
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var collection = Array.Empty<object>();

            Assert.That(collection, Is.Not.Empty, ""message"");
        }");
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotEmptyFixWithMessageAndParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var collection = Array.Empty<object>();

            ↓Assert.IsNotEmpty(collection, ""message"", Guid.NewGuid());
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var collection = Array.Empty<object>();

            Assert.That(collection, Is.Not.Empty, ""message"", Guid.NewGuid());
        }");
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
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
            ↓Assert.IsNotEmpty(s);
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
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }
    }
}
