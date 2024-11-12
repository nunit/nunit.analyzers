using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class IsAssignableFromClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ClassicModelAssertUsageAnalyzer();
        private static readonly CodeFixProvider fix = new IsAssignableFromClassicModelAssertUsageCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.IsAssignableFromUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new IsAssignableFromClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.IsAssignableFromUsage }));
        }

        [Test]
        public void VerifyIsAssignableFromFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            ↓ClassicAssert.IsAssignableFrom(expected, actual);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            Assert.That(actual, Is.AssignableFrom(expected));
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsAssignableFromFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            ↓ClassicAssert.IsAssignableFrom(expected, actual, ""message"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            Assert.That(actual, Is.AssignableFrom(expected), ""message"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsAssignableFromFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            ↓ClassicAssert.IsAssignableFrom(expected, actual, ""message-id: {0}"", Guid.NewGuid());
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            Assert.That(actual, Is.AssignableFrom(expected), $""message-id: {Guid.NewGuid()}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsAssignableFromFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            ↓ClassicAssert.IsAssignableFrom(expected, actual, ""{0}, {1}"", ""first"", ""second"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            Assert.That(actual, Is.AssignableFrom(expected), $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsAssignableFromFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            ↓ClassicAssert.IsAssignableFrom(args: new[] { ""first"", ""second"" },  message: ""{0}, {1}"", actual: actual, expected: expected);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            Assert.That(actual, Is.AssignableFrom(expected), $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsAssignableFromGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var actual = 42;

            ↓ClassicAssert.IsAssignableFrom<int>(actual);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var actual = 42;

            Assert.That(actual, Is.AssignableFrom<int>());
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsAssignableFromSingleNestedGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var wrapped = Create(42);
            ↓ClassicAssert.IsAssignableFrom<Wrapped<int>>(wrapped);
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
            Assert.That(wrapped, Is.AssignableFrom<Wrapped<int>>());
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
        public void VerifyIsAssignableFromDoubleNestedGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var wrapped = Create(42);
            var nested = Create(wrapped);
            ↓ClassicAssert.IsAssignableFrom<Wrapped<Wrapped<int>>>(wrapped);
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
            Assert.That(wrapped, Is.AssignableFrom<Wrapped<Wrapped<int>>>());
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
        public void VerifyIsAssignableFromGenericFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var actual = 42;

            ↓ClassicAssert.IsAssignableFrom<int>(actual, ""message"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var actual = 42;

            Assert.That(actual, Is.AssignableFrom<int>(), ""message"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsAssignableFromGenericFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapInTestMethod(@"
            var actual = 42;

            ↓ClassicAssert.IsAssignableFrom<int>(actual, ""message-id: {0}"", Guid.NewGuid());");
            var fixedCode = TestUtility.WrapInTestMethod(@"
            var actual = 42;

            Assert.That(actual, Is.AssignableFrom<int>(), $""message-id: {Guid.NewGuid()}"");");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsAssignableFromGenericFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapInTestMethod(@"
            var actual = 42;

            ↓ClassicAssert.IsAssignableFrom<int>(actual, ""{0}, {1}"", ""first"", ""second"");");
            var fixedCode = TestUtility.WrapInTestMethod(@"
            var actual = 42;

            Assert.That(actual, Is.AssignableFrom<int>(), $""{""first""}, {""second""}"");");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsAssignableFromGenericFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapInTestMethod(@"
            var actual = 42;

            ↓ClassicAssert.IsAssignableFrom<int>(args: new[] { ""first"", ""second"" },  message: ""{0}, {1}"", actual: actual);");
            var fixedCode = TestUtility.WrapInTestMethod(@"
            var actual = 42;

            Assert.That(actual, Is.AssignableFrom<int>(), $""{""first""}, {""second""}"");");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixMaintainsReasonableTriviaWithEndOfLineClosingParen([Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage
                ? ",\r\n                \"message\""
                : string.Empty;
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var expected = typeof(int);
            var actual = 42;

            ↓ClassicAssert.IsAssignableFrom(
                expected,
                actual{commaAndMessage});
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var expected = typeof(int);
            var actual = 42;

            Assert.That(
                actual,
                Is.AssignableFrom(expected){commaAndMessage});
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixMaintainsReasonableTriviaWithNewLineClosingParen([Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage
                ? ",\r\n                \"message\""
                : string.Empty;
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var expected = typeof(int);
            var actual = 42;

            ↓ClassicAssert.IsAssignableFrom(
                expected,
                actual{commaAndMessage}
            );
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var expected = typeof(int);
            var actual = 42;

            Assert.That(
                actual,
                Is.AssignableFrom(expected){commaAndMessage}
            );
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixForGenericMaintainsReasonableTriviaWithEndOfLineClosingParen([Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage
                ? ",\r\n                \"message\""
                : string.Empty;
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var actual = 42;

            ↓ClassicAssert.IsAssignableFrom<int>(
                actual{commaAndMessage});
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var actual = 42;

            Assert.That(
                actual,
                Is.AssignableFrom<int>(){commaAndMessage});
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixForGenericMaintainsReasonableTriviaWithNewLineClosingParen([Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage
                ? ",\r\n                \"message\""
                : string.Empty;
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var actual = 42;

            ↓ClassicAssert.IsAssignableFrom<int>(
                actual{commaAndMessage}
            );
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var actual = 42;

            Assert.That(
                actual,
                Is.AssignableFrom<int>(){commaAndMessage}
            );
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixMaintainsReasonableTriviaWithAllArgumentsOnSameLine([Values] bool newlineBeforeClosingParen)
        {
            var optionalNewline = newlineBeforeClosingParen ? "\r\n            " : string.Empty;
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var actual = 42;

            ↓ClassicAssert.IsAssignableFrom<int>(
                actual, ""message""{optionalNewline});
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var actual = 42;

            Assert.That(
                actual, Is.AssignableFrom<int>(), ""message""{optionalNewline});
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
