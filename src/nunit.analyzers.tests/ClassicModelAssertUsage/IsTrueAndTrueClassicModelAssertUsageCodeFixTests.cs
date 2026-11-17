using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
#if NUNIT4
using NUnit.Framework.Legacy;
#else
using ClassicAssert = NUnit.Framework.Assert;
#endif
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.ClassicModelAssertUsage.ClassicModelAssertUsageAnalyzer, NUnit.Analyzers.ClassicModelAssertUsage.IsTrueAndTrueClassicModelAssertUsageCodeFix>;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class IsTrueAndTrueClassicModelAssertUsageCodeFixTests
    {
        private static readonly Dictionary<string, string> diagnosticIdsToAssertions = new()
        {
            { AnalyzerIdentifiers.TrueUsage, nameof(ClassicAssert.True) },
            { AnalyzerIdentifiers.IsTrueUsage, nameof(ClassicAssert.IsTrue) },
        };
        private static readonly string[] diagnosticIds = diagnosticIdsToAssertions.Keys.ToArray();

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new IsTrueAndTrueClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.IsTrueUsage, AnalyzerIdentifiers.TrueUsage }));
        }

        [TestCase("IsTrue", AnalyzerIdentifiers.IsTrueUsage)]
        [TestCase("True", AnalyzerIdentifiers.TrueUsage)]
        public async Task VerifyIsTrueAndTrueFixes(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = Verify.Diagnostic(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.{{assertion}}(true)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(true, Is.True);
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsTrue", AnalyzerIdentifiers.IsTrueUsage)]
        [TestCase("True", AnalyzerIdentifiers.TrueUsage)]
        public async Task VerifyIsTrueAndTrueFixesWithMessage(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = Verify.Diagnostic(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.{{assertion}}(true, "message")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(true, Is.True, "message");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsTrue", AnalyzerIdentifiers.IsTrueUsage)]
        [TestCase("True", AnalyzerIdentifiers.TrueUsage)]
        public async Task VerifyIsTrueAndTrueFixesWithMessageAndOneArgumentForParams(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = Verify.Diagnostic(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.{{assertion}}(true, "message-id: {0}", Guid.NewGuid())|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(true, Is.True, $"message-id: {Guid.NewGuid()}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsTrue", AnalyzerIdentifiers.IsTrueUsage)]
        [TestCase("True", AnalyzerIdentifiers.TrueUsage)]
        public async Task VerifyIsTrueAndTrueFixesWithMessageAndTwoArgumentsForParams(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = Verify.Diagnostic(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.{{assertion}}(true, "{0}, {1}", "first", "second")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(true, Is.True, $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsTrue", AnalyzerIdentifiers.IsTrueUsage)]
        [TestCase("True", AnalyzerIdentifiers.TrueUsage)]
        public async Task VerifyIsTrueAndTrueFixesWithMessageAndArrayParamsInNonstandardOrder(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = Verify.Diagnostic(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.{{assertion}}(args: new[] { "first", "second" }, message: "{0}, {1}", condition: true)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(true, Is.True, $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsTrue", AnalyzerIdentifiers.IsTrueUsage)]
        [TestCase("True", AnalyzerIdentifiers.TrueUsage)]
        public async Task VerifyIsTrueAndTrueWithImplicitTypeConversionFixes(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = Verify.Diagnostic(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private struct MyBool
                    {
                        private readonly bool _value;

                        public MyBool(bool value) => _value = value;

                        public static implicit operator bool(MyBool value) => value._value;
                        public static implicit operator MyBool(bool value) => new MyBool(value);
                    }
                    public void TestMethod()
                    {
                        MyBool x = true;
                        {|#0:ClassicAssert.{{assertion}}(x)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    private struct MyBool
                    {
                        private readonly bool _value;

                        public MyBool(bool value) => _value = value;

                        public static implicit operator bool(MyBool value) => value._value;
                        public static implicit operator MyBool(bool value) => new MyBool(value);
                    }
                    public void TestMethod()
                    {
                        MyBool x = true;
                        Assert.That((bool)x, Is.True);
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsTrue", AnalyzerIdentifiers.IsTrueUsage)]
        [TestCase("True", AnalyzerIdentifiers.TrueUsage)]
        public async Task VerifyIsTrueAndTrueWithImplicitTypeConversionFixesInNonstandardOrder(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = Verify.Diagnostic(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private struct MyBool
                    {
                        private readonly bool _value;

                        public MyBool(bool value) => _value = value;

                        public static implicit operator bool(MyBool value) => value._value;
                        public static implicit operator MyBool(bool value) => new MyBool(value);
                    }
                    public void TestMethod()
                    {
                        MyBool x = true;
                        {|#0:ClassicAssert.{{assertion}}(args: new[] { "first", "second" }, message: "{0}, {1}", condition: x)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    private struct MyBool
                    {
                        private readonly bool _value;

                        public MyBool(bool value) => _value = value;

                        public static implicit operator bool(MyBool value) => value._value;
                        public static implicit operator MyBool(bool value) => new MyBool(value);
                    }
                    public void TestMethod()
                    {
                        MyBool x = true;
                        Assert.That((bool)x, Is.True, $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWithEndOfLineClosingParen(
            [ValueSource(nameof(diagnosticIds))] string diagnosticId,
            [Values] bool hasMessage)
        {
            var assertion = diagnosticIdsToAssertions[diagnosticId];
            var commaAndMessage = hasMessage
                ? $",{Environment.NewLine}                \"message\""
                : string.Empty;
            var expectedDiagnostic = Verify.Diagnostic(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.{{assertion}}(
                            true{{commaAndMessage}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        Assert.That(
                            true,
                            Is.True{{commaAndMessage}});
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWithNewLineClosingParen(
            [ValueSource(nameof(diagnosticIds))] string diagnosticId,
            [Values] bool hasMessage)
        {
            var assertion = diagnosticIdsToAssertions[diagnosticId];
            var commaAndMessage = hasMessage
                ? $",{Environment.NewLine}                \"message\""
                : string.Empty;
            var expectedDiagnostic = Verify.Diagnostic(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.{{assertion}}(
                            true{{commaAndMessage}}
                        )|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        Assert.That(
                            true,
                            Is.True{{commaAndMessage}}
                        );
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWithAllArgumentsOnSameLine(
            [ValueSource(nameof(diagnosticIds))] string diagnosticId,
            [Values] bool newlineBeforeClosingParen)
        {
            var optionalNewline = newlineBeforeClosingParen ? $"{Environment.NewLine}            " : string.Empty;
            var assertion = diagnosticIdsToAssertions[diagnosticId];
            var expectedDiagnostic = Verify.Diagnostic(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.{{assertion}}(
                            true, "message"{{optionalNewline}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        Assert.That(
                            true, Is.True, "message"{{optionalNewline}});
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
