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
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var expected = typeof(int);
            var actual = 42;

            ↓Assert.IsInstanceOf(expected, actual);
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            Assert.That(actual, Is.InstanceOf(expected));
        }");
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsInstanceOfFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var expected = typeof(int);
            var actual = 42;

            ↓Assert.IsInstanceOf(expected, actual, ""message"");
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            Assert.That(actual, Is.InstanceOf(expected), ""message"");
        }");
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsInstanceOfFixWithMessageAndParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var expected = typeof(int);
            var actual = 42;

            ↓Assert.IsInstanceOf(expected, actual, ""message"", Guid.NewGuid());
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            Assert.That(actual, Is.InstanceOf(expected), ""message"", Guid.NewGuid());
        }");
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsInstanceOfGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
            public void TestMethod()
            {{
                var actual = 42;

                ↓Assert.IsInstanceOf<int>(actual);
            }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
            public void TestMethod()
            {
                var actual = 42;

                Assert.That(actual, Is.InstanceOf<int>());
            }");
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsInstanceOfSingleNestedGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
            public void TestMethod()
            {
                var wrapped = Create(42);
                ↓Assert.IsInstanceOf<Wrapped<int>>(wrapped);
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
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsInstanceOfDoubleNestedGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
            public void TestMethod()
            {
                var wrapped = Create(42);
                var nested = Create(wrapped);
                ↓Assert.IsInstanceOf<Wrapped<Wrapped<int>>>(wrapped);
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
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsInstanceOfGenericFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
            public void TestMethod()
            {{
                var actual = 42;

                ↓Assert.IsInstanceOf<int>(actual, ""message"");
            }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
            public void TestMethod()
            {
                var actual = 42;

                Assert.That(actual, Is.InstanceOf<int>(), ""message"");
            }");
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsInstanceOfGenericFixWithMessageAndParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
            public void TestMethod()
            {{
                var actual = 42;

                ↓Assert.IsInstanceOf<int>(actual, ""message"", Guid.NewGuid());
            }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
            public void TestMethod()
            {
                var actual = 42;

                Assert.That(actual, Is.InstanceOf<int>(), ""message"", Guid.NewGuid());
            }");
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.TransformToConstraintModelDescription);
        }
    }
}
