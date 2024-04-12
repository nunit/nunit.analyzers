using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class LessOrEqualClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ClassicModelAssertUsageAnalyzer();
        private static readonly CodeFixProvider fix = new LessOrEqualClassicModelAssertUsageCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.LessOrEqualUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new LessOrEqualClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.LessOrEqualUsage }));
        }

        [Test]
        public void VerifyLessOrEqualFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.LessOrEqual(2d, 3d);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(2d, Is.LessThanOrEqualTo(3d));
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyLessOrEqualFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.LessOrEqual(2d, 3d, ""message"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(2d, Is.LessThanOrEqualTo(3d), ""message"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyLessOrEqualFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.LessOrEqual(2d, 3d, ""message-id: {0}"", Guid.NewGuid());
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(2d, Is.LessThanOrEqualTo(3d), $""message-id: {Guid.NewGuid()}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyLessOrEqualFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.LessOrEqual(2d, 3d, ""{0}, {1}"", ""first"", ""second"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(2d, Is.LessThanOrEqualTo(3d), $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyLessOrEqualFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.LessOrEqual(args: new[] { ""first"", ""second"" }, arg2: 3d, message: ""{0}, {1}"", arg1: 2d);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(2d, Is.LessThanOrEqualTo(3d), $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixPreservesLineBreakBeforeMessage()
        {
            var code = TestUtility.WrapInTestMethod(@"
            ClassicAssert.LessOrEqual(2d, 3d,
                ""message"");");

            var fixedCode = TestUtility.WrapInTestMethod(@"
            Assert.That(2d, Is.LessThanOrEqualTo(3d),
                ""message"");");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyLessOrEqualWithActualTypeImplicitConversionFix()
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
            ↓ClassicAssert.LessOrEqual(x, y);
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
            Assert.That((float)x, Is.LessThanOrEqualTo(y));
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyLessOrEqualWithExpectedTypeImplicitConversionFix()
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
            ↓ClassicAssert.LessOrEqual(x, y);
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
            Assert.That(x, Is.LessThanOrEqualTo((float)y));
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyLessOrEqualWithImplicitConversionFixInNonstandardOrder()
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
            MyFloat y = 2;
            ↓ClassicAssert.LessOrEqual(args: new[] { ""first"", ""second"" }, message: ""{0}, {1}"", arg2: y, arg1: x);
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
            MyFloat y = 2;
            Assert.That((float)x, Is.LessThanOrEqualTo((float)y), $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
