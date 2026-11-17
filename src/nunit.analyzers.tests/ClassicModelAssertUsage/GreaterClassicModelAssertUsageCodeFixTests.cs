using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.ClassicModelAssertUsage.ClassicModelAssertUsageAnalyzer, NUnit.Analyzers.ClassicModelAssertUsage.GreaterClassicModelAssertUsageCodeFix>;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class GreaterClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.GreaterUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new GreaterClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.GreaterUsage }));
        }

        [Test]
        public async Task VerifyGreaterFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.Greater(2d, 3d)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(2d, Is.GreaterThan(3d));
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyGreaterFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.Greater(2d, 3d, "message")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(2d, Is.GreaterThan(3d), "message");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyGreaterFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.Greater(2d, 3d, "message-id: {0}", Guid.NewGuid())|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(2d, Is.GreaterThan(3d), $"message-id: {Guid.NewGuid()}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyGreaterFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.Greater(2d, 3d, "{0}, {1}", "first", "second")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(2d, Is.GreaterThan(3d), $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyGreaterFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.Greater(args: new[] { "first", "second" }, arg2: 3d, message: "{0}, {1}", arg1: 2d)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(2d, Is.GreaterThan(3d), $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task CodeFixPreservesLineBreakBeforeMessage()
        {
            var code = TestUtility.WrapInTestMethod("""
                        {|#0:ClassicAssert.Greater(2d, 3d,
                            "message")|};
            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        Assert.That(2d, Is.GreaterThan(3d),
                            "message");
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyGreaterWithActualTypeImplicitConversionFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
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
                        {|#0:ClassicAssert.Greater(x, y)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
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
                        Assert.That((float)x, Is.GreaterThan(y));
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyGreaterWithExpectedTypeImplicitConversionFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
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
                        {|#0:ClassicAssert.Greater(x, y)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
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
                        Assert.That(x, Is.GreaterThan((float)y));
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyGreaterWithImplicitConversionFixInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
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
                        {|#0:ClassicAssert.Greater(args: new[] { "first", "second" }, message: "{0}, {1}", arg2: y, arg1: x)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
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
                        Assert.That((float)x, Is.GreaterThan((float)y), $"{"first"}, {"second"}");
                    }
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
                        {|#0:ClassicAssert.Greater(
                            2d,
                            3d{{commaAndMessage}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        Assert.That(
                            2d,
                            Is.GreaterThan(3d){{commaAndMessage}});
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
                        {|#0:ClassicAssert.Greater(
                            2d,
                            3d{{commaAndMessage}}
                        )|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        Assert.That(
                            2d,
                            Is.GreaterThan(3d){{commaAndMessage}}
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
                        {|#0:ClassicAssert.Greater(
                            2d,
                            3d{{optionalNewline}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        Assert.That(
                            2d,
                            Is.GreaterThan(3d){{optionalNewline}});
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
