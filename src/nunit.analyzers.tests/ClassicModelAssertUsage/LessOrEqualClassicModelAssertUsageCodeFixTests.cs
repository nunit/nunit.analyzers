using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.ClassicModelAssertUsage.ClassicModelAssertUsageAnalyzer, NUnit.Analyzers.ClassicModelAssertUsage.LessOrEqualClassicModelAssertUsageCodeFix>;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class LessOrEqualClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.LessOrEqualUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new LessOrEqualClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.LessOrEqualUsage }));
        }

        [Test]
        public async Task VerifyLessOrEqualFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.LessOrEqual(2d, 3d)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(2d, Is.LessThanOrEqualTo(3d));
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyLessOrEqualFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.LessOrEqual(2d, 3d, "message")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(2d, Is.LessThanOrEqualTo(3d), "message");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyLessOrEqualFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.LessOrEqual(2d, 3d, "message-id: {0}", Guid.NewGuid())|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(2d, Is.LessThanOrEqualTo(3d), $"message-id: {Guid.NewGuid()}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyLessOrEqualFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.LessOrEqual(2d, 3d, "{0}, {1}", "first", "second")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(2d, Is.LessThanOrEqualTo(3d), $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyLessOrEqualFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.LessOrEqual(args: new[] { "first", "second" }, arg2: 3d, message: "{0}, {1}", arg1: 2d)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(2d, Is.LessThanOrEqualTo(3d), $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task CodeFixPreservesLineBreakBeforeMessage()
        {
            var code = TestUtility.WrapInTestMethod("""
                        {|#0:ClassicAssert.LessOrEqual(2d, 3d,
                            "message")|};
            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        Assert.That(2d, Is.LessThanOrEqualTo(3d),
                            "message");
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyLessOrEqualWithActualTypeImplicitConversionFix()
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
                        {|#0:ClassicAssert.LessOrEqual(x, y)|};
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
                        Assert.That((float)x, Is.LessThanOrEqualTo(y));
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyLessOrEqualWithExpectedTypeImplicitConversionFix()
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
                        {|#0:ClassicAssert.LessOrEqual(x, y)|};
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
                        Assert.That(x, Is.LessThanOrEqualTo((float)y));
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyLessOrEqualWithImplicitConversionFixInNonstandardOrder()
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
                        {|#0:ClassicAssert.LessOrEqual(args: new[] { "first", "second" }, message: "{0}, {1}", arg2: y, arg1: x)|};
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
                        Assert.That((float)x, Is.LessThanOrEqualTo((float)y), $"{"first"}, {"second"}");
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
                        {|#0:ClassicAssert.LessOrEqual(
                            2d,
                            3d{{commaAndMessage}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        Assert.That(
                            2d,
                            Is.LessThanOrEqualTo(3d){{commaAndMessage}});
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
                        {|#0:ClassicAssert.LessOrEqual(
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
                            Is.LessThanOrEqualTo(3d){{commaAndMessage}}
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
                        {|#0:ClassicAssert.LessOrEqual(
                            2d,
                            3d{{optionalNewline}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        Assert.That(
                            2d,
                            Is.LessThanOrEqualTo(3d){{optionalNewline}});
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
