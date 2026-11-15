using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.ClassicModelAssertUsage.ClassicModelAssertUsageAnalyzer, NUnit.Analyzers.ClassicModelAssertUsage.IsAssignableFromClassicModelAssertUsageCodeFix>;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class IsAssignableFromClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.IsAssignableFromUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new IsAssignableFromClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.IsAssignableFromUsage }));
        }

        [Test]
        public async Task VerifyIsAssignableFromFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        {|#0:ClassicAssert.IsAssignableFrom(expected, actual)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        Assert.That(actual, Is.AssignableFrom(expected));
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsAssignableFromFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        {|#0:ClassicAssert.IsAssignableFrom(expected, actual, "message")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        Assert.That(actual, Is.AssignableFrom(expected), "message");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsAssignableFromFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        {|#0:ClassicAssert.IsAssignableFrom(expected, actual, "message-id: {0}", Guid.NewGuid())|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        Assert.That(actual, Is.AssignableFrom(expected), $"message-id: {Guid.NewGuid()}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsAssignableFromFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        {|#0:ClassicAssert.IsAssignableFrom(expected, actual, "{0}, {1}", "first", "second")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        Assert.That(actual, Is.AssignableFrom(expected), $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsAssignableFromFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        {|#0:ClassicAssert.IsAssignableFrom(args: new[] { "first", "second" },  message: "{0}, {1}", actual: actual, expected: expected)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        Assert.That(actual, Is.AssignableFrom(expected), $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsAssignableFromGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var actual = 42;

                        {|#0:ClassicAssert.IsAssignableFrom<int>(actual)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var actual = 42;

                        Assert.That(actual, Is.AssignableFrom<int>());
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsAssignableFromSingleNestedGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var wrapped = Create(42);
                        {|#0:ClassicAssert.IsAssignableFrom<Wrapped<int>>(wrapped)|};
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
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsAssignableFromDoubleNestedGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var wrapped = Create(42);
                        var nested = Create(wrapped);
                        {|#0:ClassicAssert.IsAssignableFrom<Wrapped<Wrapped<int>>>(wrapped)|};
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
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsAssignableFromGenericFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var actual = 42;

                        {|#0:ClassicAssert.IsAssignableFrom<int>(actual, "message")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var actual = 42;

                        Assert.That(actual, Is.AssignableFrom<int>(), "message");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsAssignableFromGenericFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        {|#0:ClassicAssert.IsAssignableFrom<int>(actual, "message-id: {0}", Guid.NewGuid())|};
            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        Assert.That(actual, Is.AssignableFrom<int>(), $"message-id: {Guid.NewGuid()}");
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsAssignableFromGenericFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        {|#0:ClassicAssert.IsAssignableFrom<int>(actual, "{0}, {1}", "first", "second")|};
            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        Assert.That(actual, Is.AssignableFrom<int>(), $"{"first"}, {"second"}");
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsAssignableFromGenericFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        {|#0:ClassicAssert.IsAssignableFrom<int>(args: new[] { "first", "second" },  message: "{0}, {1}", actual: actual)|};
            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        Assert.That(actual, Is.AssignableFrom<int>(), $"{"first"}, {"second"}");
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

                        {|#0:ClassicAssert.IsAssignableFrom(
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
                            Is.AssignableFrom(expected){{commaAndMessage}});
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

                        {|#0:ClassicAssert.IsAssignableFrom(
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
                            Is.AssignableFrom(expected){{commaAndMessage}}
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

                        {|#0:ClassicAssert.IsAssignableFrom<int>(
                            actual{{commaAndMessage}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var actual = 42;

                        Assert.That(
                            actual,
                            Is.AssignableFrom<int>(){{commaAndMessage}});
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

                        {|#0:ClassicAssert.IsAssignableFrom<int>(
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
                            Is.AssignableFrom<int>(){{commaAndMessage}}
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

                        {|#0:ClassicAssert.IsAssignableFrom<int>(
                            actual, "message"{{optionalNewline}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var actual = 42;

                        Assert.That(
                            actual, Is.AssignableFrom<int>(), "message"{{optionalNewline}});
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
