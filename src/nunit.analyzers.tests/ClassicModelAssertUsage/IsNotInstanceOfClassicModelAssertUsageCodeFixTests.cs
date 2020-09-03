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
    public sealed class IsNotInstanceOfClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ClassicModelAssertUsageAnalyzer();
        private static readonly CodeFixProvider fix = new IsNotInstanceOfClassicModelAssertUsageCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.IsNotInstanceOfUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new IsNotInstanceOfClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.IsNotInstanceOfUsage }));
        }

        [Test]
        public void VerifyIsNotInstanceOfFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var expected = typeof(int);
            var actual = 42;

            ↓Assert.IsNotInstanceOf(expected, actual);
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            Assert.That(actual, Is.Not.InstanceOf(expected));
        }");
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotInstanceOfFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var expected = typeof(int);
            var actual = 42;

            ↓Assert.IsNotInstanceOf(expected, actual, ""message"");
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            Assert.That(actual, Is.Not.InstanceOf(expected), ""message"");
        }");
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotInstanceOfFixWithMessageAndParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var expected = typeof(int);
            var actual = 42;

            ↓Assert.IsNotInstanceOf(expected, actual, ""message"", Guid.NewGuid());
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            Assert.That(actual, Is.Not.InstanceOf(expected), ""message"", Guid.NewGuid());
        }");
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotInstanceOfGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
            public void TestMethod()
            {{
                var actual = 42;

                ↓Assert.IsNotInstanceOf<int>(actual);
            }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
            public void TestMethod()
            {
                var actual = 42;

                Assert.That(actual, Is.Not.InstanceOf<int>());
            }");
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotInstanceOfSingleNestedGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
            public void TestMethod()
            {
                var wrapped = Create(42);
                ↓Assert.IsNotInstanceOf<Wrapped<int>>(wrapped);
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
                Assert.That(wrapped, Is.Not.InstanceOf<Wrapped<int>>());
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
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotInstanceOfDoubleNestedGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
            public void TestMethod()
            {
                var wrapped = Create(42);
                var nested = Create(wrapped);
                ↓Assert.IsNotInstanceOf<Wrapped<Wrapped<int>>>(wrapped);
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
                Assert.That(wrapped, Is.Not.InstanceOf<Wrapped<Wrapped<int>>>());
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
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotInstanceOfGenericFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
            public void TestMethod()
            {{
                var actual = 42;

                ↓Assert.IsNotInstanceOf<int>(actual, ""message"");
            }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
            public void TestMethod()
            {
                var actual = 42;

                Assert.That(actual, Is.Not.InstanceOf<int>(), ""message"");
            }");
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotInstanceOfGenericFixWithMessageAndParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
            public void TestMethod()
            {{
                var actual = 42;

                ↓Assert.IsNotInstanceOf<int>(actual, ""message"", Guid.NewGuid());
            }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
            public void TestMethod()
            {
                var actual = 42;

                Assert.That(actual, Is.Not.InstanceOf<int>(), ""message"", Guid.NewGuid());
            }");
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }
    }
}
