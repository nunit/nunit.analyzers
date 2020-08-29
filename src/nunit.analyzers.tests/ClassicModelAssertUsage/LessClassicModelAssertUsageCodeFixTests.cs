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
    public sealed class LessClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ClassicModelAssertUsageAnalyzer();
        private static readonly CodeFixProvider fix = new LessClassicModelAssertUsageCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.LessUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new LessClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.LessUsage }));
        }

        [Test]
        public void VerifyLessFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            ↓Assert.Less(2d, 3d);
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(2d, Is.LessThan(3d));
        }");
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyLessFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            ↓Assert.Less(2d, 3d, ""message"");
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(2d, Is.LessThan(3d), ""message"");
        }");
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyLessFixWithMessageAndParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            ↓Assert.Less(2d, 3d, ""message"", Guid.NewGuid());
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(2d, Is.LessThan(3d), ""message"", Guid.NewGuid());
        }");
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixPreservesLineBreakBeforeMessage()
        {
            var code = TestUtility.WrapInTestMethod($@"
            Assert.Less(2d, 3d,
                ""message"",
                Guid.NewGuid());");

            var fixedCode = TestUtility.WrapInTestMethod(@"
            Assert.That(2d, Is.LessThan(3d),
                ""message"",
                Guid.NewGuid());");

            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyLessWithActualTypeImplicitConversionFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        private struct MyFloat
        {
            private readonly float _value;

            public MyFloat(float value) => _value = value;

            public static implicit operator float(MyFloat value) => value._value;
            public static implicit operator MyFloat(float value) => new MyFloat(value);
        }
        public void TestMethod()
        {
            MyFloat x = 1;
            float y = 2;
            ↓Assert.Less(x, y);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        private struct MyFloat
        {
            private readonly float _value;

            public MyFloat(float value) => _value = value;

            public static implicit operator float(MyFloat value) => value._value;
            public static implicit operator MyFloat(float value) => new MyFloat(value);
        }
        public void TestMethod()
        {
            MyFloat x = 1;
            float y = 2;
            Assert.That((float)x, Is.LessThan(y));
        }");
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyLessWithExpectedTypeImplicitConversionFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        private struct MyFloat
        {
            private readonly float _value;

            public MyFloat(float value) => _value = value;

            public static implicit operator float(MyFloat value) => value._value;
            public static implicit operator MyFloat(float value) => new MyFloat(value);
        }
        public void TestMethod()
        {
            float x = 1;
            MyFloat y = 2;
            ↓Assert.Less(x, y);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        private struct MyFloat
        {
            private readonly float _value;

            public MyFloat(float value) => _value = value;

            public static implicit operator float(MyFloat value) => value._value;
            public static implicit operator MyFloat(float value) => new MyFloat(value);
        }
        public void TestMethod()
        {
            float x = 1;
            MyFloat y = 2;
            Assert.That(x, Is.LessThan((float)y));
        }");
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }
    }
}
