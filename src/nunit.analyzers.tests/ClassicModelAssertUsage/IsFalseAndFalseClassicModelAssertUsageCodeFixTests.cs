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
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.ClassicModelAssertUsage.ClassicModelAssertUsageAnalyzer, NUnit.Analyzers.ClassicModelAssertUsage.IsFalseAndFalseClassicModelAssertUsageCodeFix>;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class IsFalseAndFalseClassicModelAssertUsageCodeFixTests
    {
        private static readonly Dictionary<string, string> diagnosticIdsToAssertions = new()
        {
            { AnalyzerIdentifiers.FalseUsage, nameof(ClassicAssert.False) },
            { AnalyzerIdentifiers.IsFalseUsage, nameof(ClassicAssert.IsFalse) },
        };
        private static readonly string[] diagnosticIds = diagnosticIdsToAssertions.Keys.ToArray();

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new IsFalseAndFalseClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.IsFalseUsage, AnalyzerIdentifiers.FalseUsage }));
        }

        [TestCase("IsFalse", AnalyzerIdentifiers.IsFalseUsage)]
        [TestCase("False", AnalyzerIdentifiers.FalseUsage)]
        public async Task VerifyIsFalseAndFalseFixes(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = Verify.Diagnostic(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.{{assertion}}(false)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(false, Is.False);
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsFalse", AnalyzerIdentifiers.IsFalseUsage)]
        [TestCase("False", AnalyzerIdentifiers.FalseUsage)]
        public async Task VerifyIsFalseAndFalseFixesWithMessage(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = Verify.Diagnostic(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.{{assertion}}(false, "message")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(false, Is.False, "message");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsFalse", AnalyzerIdentifiers.IsFalseUsage)]
        [TestCase("False", AnalyzerIdentifiers.FalseUsage)]
        public async Task VerifyIsFalseAndFalseFixesWithMessageAndOneArgumentForParams(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = Verify.Diagnostic(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.{{assertion}}(false, "message-id: {0}", Guid.NewGuid())|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(false, Is.False, $"message-id: {Guid.NewGuid()}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsFalse", AnalyzerIdentifiers.IsFalseUsage)]
        [TestCase("False", AnalyzerIdentifiers.FalseUsage)]
        public async Task VerifyIsFalseAndFalseFixesWithMessageAndTwoArgumentsForParams(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = Verify.Diagnostic(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.{{assertion}}(false, "{0}, {1}", "first", "second")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(false, Is.False, $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsFalse", AnalyzerIdentifiers.IsFalseUsage)]
        [TestCase("False", AnalyzerIdentifiers.FalseUsage)]
        public async Task VerifyIsFalseAndFalseFixesWithMessageAndArrayParamsInNonstandardOrder(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = Verify.Diagnostic(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.{{assertion}}(args: new[] { "first", "second" }, message: "{0}, {1}", condition: false)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(false, Is.False, $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsFalse", AnalyzerIdentifiers.IsFalseUsage)]
        [TestCase("False", AnalyzerIdentifiers.FalseUsage)]
        public async Task VerifyIsFalseAndFalseWithImplicitTypeConversionFixes(string assertion, string diagnosticId)
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
                        MyBool x = false;
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
                        MyBool x = false;
                        Assert.That((bool)x, Is.False);
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
                            false{{commaAndMessage}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        Assert.That(
                            false,
                            Is.False{{commaAndMessage}});
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
                            false{{commaAndMessage}}
                        )|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        Assert.That(
                            false,
                            Is.False{{commaAndMessage}}
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
                            false, "message"{{optionalNewline}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        Assert.That(
                            false, Is.False, "message"{{optionalNewline}});
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
