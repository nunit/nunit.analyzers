using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class IsNotAssignableFromClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ClassicModelAssertUsageAnalyzer();
        private static readonly CodeFixProvider fix = new IsNotAssignableFromClassicModelAssertUsageCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.IsNotAssignableFromUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new IsNotAssignableFromClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.IsNotAssignableFromUsage }));
        }

        [Test]
        public void VerifyIsNotAssignableFromFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            ↓ClassicAssert.IsNotAssignableFrom(expected, actual);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            Assert.That(actual, Is.Not.AssignableFrom(expected));
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotAssignableFromFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            ↓ClassicAssert.IsNotAssignableFrom(expected, actual, ""message"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            Assert.That(actual, Is.Not.AssignableFrom(expected), ""message"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotAssignableFromFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            ↓ClassicAssert.IsNotAssignableFrom(expected, actual, ""message-id: {0}"", Guid.NewGuid());
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            Assert.That(actual, Is.Not.AssignableFrom(expected), $""message-id: {Guid.NewGuid()}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotAssignableFromFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            ↓ClassicAssert.IsNotAssignableFrom(expected, actual, ""{0}, {1}"", ""first"", ""second"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            Assert.That(actual, Is.Not.AssignableFrom(expected), $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotAssignableFromFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            ↓ClassicAssert.IsNotAssignableFrom(args: new[] { ""first"", ""second"" },  message: ""{0}, {1}"", actual: actual, expected: expected);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var expected = typeof(int);
            var actual = 42;

            Assert.That(actual, Is.Not.AssignableFrom(expected), $""{""first""}, {""second""}"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotAssignableFromGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var actual = 42;

            ↓ClassicAssert.IsNotAssignableFrom<int>(actual);
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var actual = 42;

            Assert.That(actual, Is.Not.AssignableFrom<int>());
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotAssignableFromSingleNestedGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var wrapped = Create(42);
            ↓ClassicAssert.IsNotAssignableFrom<Wrapped<int>>(wrapped);
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
            Assert.That(wrapped, Is.Not.AssignableFrom<Wrapped<int>>());
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
        public void VerifyIsNotAssignableFromDoubleNestedGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var wrapped = Create(42);
            var nested = Create(wrapped);
            ↓ClassicAssert.IsNotAssignableFrom<Wrapped<Wrapped<int>>>(wrapped);
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
            Assert.That(wrapped, Is.Not.AssignableFrom<Wrapped<Wrapped<int>>>());
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
        public void VerifyIsNotAssignableFromGenericFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var actual = 42;

            ↓ClassicAssert.IsNotAssignableFrom<int>(actual, ""message"");
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var actual = 42;

            Assert.That(actual, Is.Not.AssignableFrom<int>(), ""message"");
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotAssignableFromGenericFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapInTestMethod(@"
            var actual = 42;

            ↓ClassicAssert.IsNotAssignableFrom<int>(actual, ""message-id: {0}"", Guid.NewGuid());");
            var fixedCode = TestUtility.WrapInTestMethod(@"
            var actual = 42;

            Assert.That(actual, Is.Not.AssignableFrom<int>(), $""message-id: {Guid.NewGuid()}"");");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotAssignableFromGenericFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapInTestMethod(@"
            var actual = 42;

            ↓ClassicAssert.IsNotAssignableFrom<int>(actual, ""{0}, {1}"", ""first"", ""second"");");
            var fixedCode = TestUtility.WrapInTestMethod(@"
            var actual = 42;

            Assert.That(actual, Is.Not.AssignableFrom<int>(), $""{""first""}, {""second""}"");");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void VerifyIsNotAssignableFromGenericFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapInTestMethod(@"
            var actual = 42;

            ↓ClassicAssert.IsNotAssignableFrom<int>(args: new[] { ""first"", ""second"" }, message: ""{0}, {1}"", actual: actual);");
            var fixedCode = TestUtility.WrapInTestMethod(@"
            var actual = 42;

            Assert.That(actual, Is.Not.AssignableFrom<int>(), $""{""first""}, {""second""}"");");
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

            ↓ClassicAssert.IsNotAssignableFrom(
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
                Is.Not.AssignableFrom(expected){commaAndMessage});
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

            ↓ClassicAssert.IsNotAssignableFrom(
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
                Is.Not.AssignableFrom(expected){commaAndMessage}
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

            ↓ClassicAssert.IsNotAssignableFrom<int>(
                actual{commaAndMessage});
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var actual = 42;

            Assert.That(
                actual,
                Is.Not.AssignableFrom<int>(){commaAndMessage});
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

            ↓ClassicAssert.IsNotAssignableFrom<int>(
                actual{commaAndMessage}
            );
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var actual = 42;

            Assert.That(
                actual,
                Is.Not.AssignableFrom<int>(){commaAndMessage}
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

            ↓ClassicAssert.IsNotAssignableFrom<int>(
                actual, ""message""{optionalNewline});
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var actual = 42;

            Assert.That(
                actual, Is.Not.AssignableFrom<int>(), ""message""{optionalNewline});
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
