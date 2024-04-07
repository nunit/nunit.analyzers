using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class IsInstanceOfClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ClassicModelAssertUsageAnalyzer();
        private static readonly CodeFixProvider fix = new IsInstanceOfClassicModelAssertUsageCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.IsInstanceOfUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new IsInstanceOfClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.IsInstanceOfUsage }));
        }

        [Test]
        public void VerifyIsInstanceOfFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            ↓ClassicAssert.IsInstanceOf(expected, actual);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            Assert.That(actual, Is.InstanceOf(expected));
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsInstanceOfFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            ↓ClassicAssert.IsInstanceOf(expected, actual, ""message"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            Assert.That(actual, Is.InstanceOf(expected), ""message"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsInstanceOfFixWithMessageAndParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            ↓ClassicAssert.IsInstanceOf(expected, actual, ""message-id: {0}"", Guid.NewGuid());
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            Assert.That(actual, Is.InstanceOf(expected), $""message-id: {Guid.NewGuid()}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsInstanceOfFixWithMessageAndParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            ↓ClassicAssert.IsInstanceOf(args: new[] { ""first"", ""second"" },  message: ""{0}, {1}"", actual: actual, expected: expected);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            Assert.That(actual: actual, Is.InstanceOf(expectedType: expected), $""{""first""}, {""second"" }"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsInstanceOfGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var actual = 42;

            ↓ClassicAssert.IsInstanceOf<int>(actual);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var actual = 42;

            Assert.That(actual, Is.InstanceOf<int>());
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsInstanceOfSingleNestedGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var wrapped = Create(42);
            ↓ClassicAssert.IsInstanceOf<Wrapped<int>>(wrapped);
        }

        private Wrapped<T> Create<T>(T value) => new Wrapped<T>(value);

        private class Wrapped<T>
        {
            public Wrapped(T value)
            {
                Value = value;
            }

            public T Value { get; }
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var wrapped = Create(42);
            Assert.That(wrapped, Is.InstanceOf<Wrapped<int>>());
        }

        private Wrapped<T> Create<T>(T value) => new Wrapped<T>(value);

        private class Wrapped<T>
        {
            public Wrapped(T value)
            {
                Value = value;
            }

            public T Value { get; }
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsInstanceOfDoubleNestedGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var wrapped = Create(42);
            var nested = Create(wrapped);
            ↓ClassicAssert.IsInstanceOf<Wrapped<Wrapped<int>>>(wrapped);
        }

        private Wrapped<T> Create<T>(T value) => new Wrapped<T>(value);

        private class Wrapped<T>
        {
            public Wrapped(T value)
            {
                Value = value;
            }

            public T Value { get; }
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var wrapped = Create(42);
            var nested = Create(wrapped);
            Assert.That(wrapped, Is.InstanceOf<Wrapped<Wrapped<int>>>());
        }

        private Wrapped<T> Create<T>(T value) => new Wrapped<T>(value);

        private class Wrapped<T>
        {
            public Wrapped(T value)
            {
                Value = value;
            }

            public T Value { get; }
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsInstanceOfGenericFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var actual = 42;

            ↓ClassicAssert.IsInstanceOf<int>(actual, ""message"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var actual = 42;

            Assert.That(actual, Is.InstanceOf<int>(), ""message"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsInstanceOfGenericFixWithMessageAndParams()
        {
            var code = TestUtility.WrapInTestMethod(@"
            var actual = 42;

            ↓ClassicAssert.IsInstanceOf<int>(actual, ""message-id: {0}"", Guid.NewGuid());");
            var fixedCode = TestUtility.WrapInTestMethod(@"
            var actual = 42;

            Assert.That(actual, Is.InstanceOf<int>(), $""message-id: {Guid.NewGuid()}"");");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsInstanceOfGenericFixWithMessageAndParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapInTestMethod(@"
            var actual = 42;

            ↓ClassicAssert.IsInstanceOf<int>(args: new[] { ""first"", ""second"" },  message: ""{0}, {1}"", actual: actual);");
            var fixedCode = TestUtility.WrapInTestMethod(@"
            var actual = 42;

            Assert.That(actual: actual, Is.InstanceOf<int>(), $""{""first""}, {""second"" }"");");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
