using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.ClassicModelAssertUsage.ClassicModelAssertUsageAnalyzer, NUnit.Analyzers.ClassicModelAssertUsage.IsNotAssignableFromClassicModelAssertUsageCodeFix>;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class IsNotAssignableFromClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.IsNotAssignableFromUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new IsNotAssignableFromClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.IsNotAssignableFromUsage }));
        }

        [Test]
        public async Task VerifyIsNotAssignableFromFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotAssignableFrom(expected, actual)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        Assert.That(actual, Is.Not.AssignableFrom(expected));
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotAssignableFromFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotAssignableFrom(expected, actual, "message")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        Assert.That(actual, Is.Not.AssignableFrom(expected), "message");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotAssignableFromFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotAssignableFrom(expected, actual, "message-id: {0}", Guid.NewGuid())|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        Assert.That(actual, Is.Not.AssignableFrom(expected), $"message-id: {Guid.NewGuid()}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotAssignableFromFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotAssignableFrom(expected, actual, "{0}, {1}", "first", "second")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        Assert.That(actual, Is.Not.AssignableFrom(expected), $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotAssignableFromFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotAssignableFrom(args: new[] { "first", "second" },  message: "{0}, {1}", actual: actual, expected: expected)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        Assert.That(actual, Is.Not.AssignableFrom(expected), $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotAssignableFromGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotAssignableFrom<int>(actual)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var actual = 42;

                        Assert.That(actual, Is.Not.AssignableFrom<int>());
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotAssignableFromSingleNestedGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var wrapped = Create(42);
                        {|#0:ClassicAssert.IsNotAssignableFrom<Wrapped<int>>(wrapped)|};
                    }

                    private Wrapped<T> Create<T>(T value) => new Wrapped<T>(value);

                    private class Wrapped<T>
                    {
                        public Wrapped(T value)
                        {
                            Value = value;
                        }

                        public T Value { get; }
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
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
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotAssignableFromDoubleNestedGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var wrapped = Create(42);
                        var nested = Create(wrapped);
                        {|#0:ClassicAssert.IsNotAssignableFrom<Wrapped<Wrapped<int>>>(wrapped)|};
                    }

                    private Wrapped<T> Create<T>(T value) => new Wrapped<T>(value);

                    private class Wrapped<T>
                    {
                        public Wrapped(T value)
                        {
                            Value = value;
                        }

                        public T Value { get; }
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
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
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotAssignableFromGenericFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotAssignableFrom<int>(actual, "message")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var actual = 42;

                        Assert.That(actual, Is.Not.AssignableFrom<int>(), "message");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotAssignableFromGenericFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotAssignableFrom<int>(actual, "message-id: {0}", Guid.NewGuid())|};
            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        Assert.That(actual, Is.Not.AssignableFrom<int>(), $"message-id: {Guid.NewGuid()}");
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotAssignableFromGenericFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotAssignableFrom<int>(actual, "{0}, {1}", "first", "second")|};
            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        Assert.That(actual, Is.Not.AssignableFrom<int>(), $"{"first"}, {"second"}");
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotAssignableFromGenericFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotAssignableFrom<int>(args: new[] { "first", "second" }, message: "{0}, {1}", actual: actual)|};
            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        Assert.That(actual, Is.Not.AssignableFrom<int>(), $"{"first"}, {"second"}");
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWithEndOfLineClosingParen([Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage
                ? $",{Environment.NewLine}                \"message\""
                : string.Empty;
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotAssignableFrom(
                            expected,
                            actual{{commaAndMessage}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        Assert.That(
                            actual,
                            Is.Not.AssignableFrom(expected){{commaAndMessage}});
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWithNewLineClosingParen([Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage
                ? $",{Environment.NewLine}                \"message\""
                : string.Empty;
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotAssignableFrom(
                            expected,
                            actual{{commaAndMessage}}
                        )|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        Assert.That(
                            actual,
                            Is.Not.AssignableFrom(expected){{commaAndMessage}}
                        );
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task CodeFixForGenericMaintainsReasonableTriviaWithEndOfLineClosingParen([Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage
                ? $",{Environment.NewLine}                \"message\""
                : string.Empty;
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotAssignableFrom<int>(
                            actual{{commaAndMessage}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var actual = 42;

                        Assert.That(
                            actual,
                            Is.Not.AssignableFrom<int>(){{commaAndMessage}});
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task CodeFixForGenericMaintainsReasonableTriviaWithNewLineClosingParen([Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage
                ? $",{Environment.NewLine}                \"message\""
                : string.Empty;
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotAssignableFrom<int>(
                            actual{{commaAndMessage}}
                        )|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var actual = 42;

                        Assert.That(
                            actual,
                            Is.Not.AssignableFrom<int>(){{commaAndMessage}}
                        );
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWithAllArgumentsOnSameLine([Values] bool newlineBeforeClosingParen)
        {
            var optionalNewline = newlineBeforeClosingParen ? $"{Environment.NewLine}            " : string.Empty;
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotAssignableFrom<int>(
                            actual, "message"{{optionalNewline}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var actual = 42;

                        Assert.That(
                            actual, Is.Not.AssignableFrom<int>(), "message"{{optionalNewline}});
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
