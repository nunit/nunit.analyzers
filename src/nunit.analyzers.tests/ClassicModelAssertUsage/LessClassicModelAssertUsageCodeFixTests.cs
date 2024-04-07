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
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.Less(2d, 3d);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(2d, Is.LessThan(3d));
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyLessFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.Less(2d, 3d, ""message"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(2d, Is.LessThan(3d), ""message"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyLessFixWithMessageAndParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.Less(2d, 3d, ""message-id: {0}"", Guid.NewGuid());
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(2d, Is.LessThan(3d), $""message-id: {Guid.NewGuid()}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyLessFixWithMessageAndParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            ↓ClassicAssert.Less(args: new[] { ""first"", ""second"" }, arg2: 3d, message: ""{0}, {1}"", arg1: 2d);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(actual: 2d, Is.LessThan(expected: 3d), $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixPreservesLineBreakBeforeMessage()
        {
            var code = TestUtility.WrapInTestMethod(@"
            ClassicAssert.Less(2d, 3d,
                ""message"");");

            var fixedCode = TestUtility.WrapInTestMethod(@"
            Assert.That(2d, Is.LessThan(3d),
                ""message"");");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
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
            ↓ClassicAssert.Less(x, y);
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
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
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
            ↓ClassicAssert.Less(x, y);
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
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyLessWithImplicitConversionFixInNonstandardOrder()
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
            ↓ClassicAssert.Less(args: new[] { ""first"", ""second"" }, message: ""{0}, {1}"", arg2: y, arg1: x);
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
            Assert.That((float)x, Is.LessThan((float)y), $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
