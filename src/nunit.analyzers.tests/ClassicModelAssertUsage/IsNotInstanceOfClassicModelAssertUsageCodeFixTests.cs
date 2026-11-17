using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.ClassicModelAssertUsage.ClassicModelAssertUsageAnalyzer, NUnit.Analyzers.ClassicModelAssertUsage.IsNotInstanceOfClassicModelAssertUsageCodeFix>;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class IsNotInstanceOfClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.IsNotInstanceOfUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new IsNotInstanceOfClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.IsNotInstanceOfUsage }));
        }

        [Test]
        public async Task VerifyIsNotInstanceOfFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotInstanceOf(expected, actual)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        Assert.That(actual, Is.Not.InstanceOf(expected));
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotInstanceOfFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotInstanceOf(expected, actual, "message")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        Assert.That(actual, Is.Not.InstanceOf(expected), "message");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotInstanceOfFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotInstanceOf(expected, actual, "message-id: {0}", Guid.NewGuid())|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        Assert.That(actual, Is.Not.InstanceOf(expected), $"message-id: {Guid.NewGuid()}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotInstanceOfFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotInstanceOf(expected, actual, "{0}, {1}", "first", "second")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        Assert.That(actual, Is.Not.InstanceOf(expected), $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotInstanceOfFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotInstanceOf(args: new[] { "first", "second" },  message: "{0}, {1}", actual: actual, expected: expected)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var expected = typeof(int);
                        var actual = 42;

                        Assert.That(actual, Is.Not.InstanceOf(expected), $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotInstanceOfGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotInstanceOf<int>(actual)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var actual = 42;

                        Assert.That(actual, Is.Not.InstanceOf<int>());
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotInstanceOfSingleNestedGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var wrapped = Create(42);
                        {|#0:ClassicAssert.IsNotInstanceOf<Wrapped<int>>(wrapped)|};
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
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotInstanceOfDoubleNestedGenericFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var wrapped = Create(42);
                        var nested = Create(wrapped);
                        {|#0:ClassicAssert.IsNotInstanceOf<Wrapped<Wrapped<int>>>(wrapped)|};
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
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotInstanceOfGenericFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotInstanceOf<int>(actual, "message")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var actual = 42;

                        Assert.That(actual, Is.Not.InstanceOf<int>(), "message");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotInstanceOfGenericFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotInstanceOf<int>(actual, "message-id: {0}", Guid.NewGuid())|};
            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        Assert.That(actual, Is.Not.InstanceOf<int>(), $"message-id: {Guid.NewGuid()}");
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotInstanceOfGenericFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotInstanceOf<int>(actual, "{0}, {1}", "first", "second")|};
            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        Assert.That(actual, Is.Not.InstanceOf<int>(), $"{"first"}, {"second"}");
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotInstanceOfGenericFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        {|#0:ClassicAssert.IsNotInstanceOf<int>(args: new[] { "first", "second" }, message: "{0}, {1}", actual: actual)|};
            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = 42;

                        Assert.That(actual, Is.Not.InstanceOf<int>(), $"{"first"}, {"second"}");
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

                        {|#0:ClassicAssert.IsNotInstanceOf(
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
                            Is.Not.InstanceOf(expected){{commaAndMessage}});
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

                        {|#0:ClassicAssert.IsNotInstanceOf(
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
                            Is.Not.InstanceOf(expected){{commaAndMessage}}
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

                        {|#0:ClassicAssert.IsNotInstanceOf<int>(
                            actual{{commaAndMessage}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var actual = 42;

                        Assert.That(
                            actual,
                            Is.Not.InstanceOf<int>(){{commaAndMessage}});
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

                        {|#0:ClassicAssert.IsNotInstanceOf<int>(
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
                            Is.Not.InstanceOf<int>(){{commaAndMessage}}
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

                        {|#0:ClassicAssert.IsNotInstanceOf<int>(
                            actual, "message"{{optionalNewline}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var actual = 42;

                        Assert.That(
                            actual, Is.Not.InstanceOf<int>(), "message"{{optionalNewline}});
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
