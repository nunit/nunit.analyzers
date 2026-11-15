using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.ClassicModelAssertUsage.ClassicModelAssertUsageAnalyzer, NUnit.Analyzers.ClassicModelAssertUsage.IsInstanceOfClassicModelAssertUsageCodeFix>;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class IsInstanceOfClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.IsInstanceOfUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new IsInstanceOfClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.IsInstanceOfUsage }));
        }

        [Test]
        public async Task VerifyIsInstanceOfFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        {|#0:ClassicAssert.IsInstanceOf(expected, actual)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        Assert.That(actual, Is.InstanceOf(expected));
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsInstanceOfFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        {|#0:ClassicAssert.IsInstanceOf(expected, actual, "message")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        Assert.That(actual, Is.InstanceOf(expected), "message");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsInstanceOfFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        {|#0:ClassicAssert.IsInstanceOf(expected, actual, "message-id: {0}", Guid.NewGuid())|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        Assert.That(actual, Is.InstanceOf(expected), $"message-id: {Guid.NewGuid()}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsInstanceOfFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        {|#0:ClassicAssert.IsInstanceOf(expected, actual, "{0}, {1}", "first", "second")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        Assert.That(actual, Is.InstanceOf(expected), $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsInstanceOfFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        {|#0:ClassicAssert.IsInstanceOf(args: new[] { "first", "second" },  message: "{0}, {1}", actual: actual, expected: expected)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        Assert.That(actual, Is.InstanceOf(expected), $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsInstanceOfGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var actual = 42;

                        {|#0:ClassicAssert.IsInstanceOf<int>(actual)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var actual = 42;

                        Assert.That(actual, Is.InstanceOf<int>());
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsInstanceOfSingleNestedGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var wrapped = Create(42);
                        {|#0:ClassicAssert.IsInstanceOf<Wrapped<int>>(wrapped)|};
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
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsInstanceOfDoubleNestedGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var wrapped = Create(42);
                        var nested = Create(wrapped);
                        {|#0:ClassicAssert.IsInstanceOf<Wrapped<Wrapped<int>>>(wrapped)|};
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
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsInstanceOfGenericFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var actual = 42;

                        {|#0:ClassicAssert.IsInstanceOf<int>(actual, "message")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var actual = 42;

                        Assert.That(actual, Is.InstanceOf<int>(), "message");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsInstanceOfGenericFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        {|#0:ClassicAssert.IsInstanceOf<int>(actual, "message-id: {0}", Guid.NewGuid())|};
            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        Assert.That(actual, Is.InstanceOf<int>(), $"message-id: {Guid.NewGuid()}");
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsInstanceOfGenericFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        {|#0:ClassicAssert.IsInstanceOf<int>(actual, "{0}, {1}", "first", "second")|};
            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        Assert.That(actual, Is.InstanceOf<int>(), $"{"first"}, {"second"}");
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsInstanceOfGenericFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        {|#0:ClassicAssert.IsInstanceOf<int>(args: new[] { "first", "second" },  message: "{0}, {1}", actual: actual)|};
            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        Assert.That(actual, Is.InstanceOf<int>(), $"{"first"}, {"second"}");
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

                        {|#0:ClassicAssert.IsInstanceOf(
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
                            Is.InstanceOf(expected){{commaAndMessage}});
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

                        {|#0:ClassicAssert.IsInstanceOf(
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
                            Is.InstanceOf(expected){{commaAndMessage}}
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

                        {|#0:ClassicAssert.IsInstanceOf<int>(
                            actual{{commaAndMessage}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var actual = 42;

                        Assert.That(
                            actual,
                            Is.InstanceOf<int>(){{commaAndMessage}});
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

                        {|#0:ClassicAssert.IsInstanceOf<int>(
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
                            Is.InstanceOf<int>(){{commaAndMessage}}
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

                        {|#0:ClassicAssert.IsInstanceOf<int>(
                            actual, "message"{{optionalNewline}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var actual = 42;

                        Assert.That(
                            actual, Is.InstanceOf<int>(), "message"{{optionalNewline}});
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
