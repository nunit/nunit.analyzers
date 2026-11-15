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
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.ClassicModelAssertUsage.ClassicModelAssertUsageAnalyzer, NUnit.Analyzers.ClassicModelAssertUsage.IsNotNullAndNotNullClassicModelAssertUsageCodeFix>;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class IsNotNullAndNotNullClassicModelAssertUsageCodeFixTests
    {
        private static readonly Dictionary<string, string> diagnosticIdsToAssertions = new()
        {
            { AnalyzerIdentifiers.NotNullUsage, nameof(ClassicAssert.NotNull) },
            { AnalyzerIdentifiers.IsNotNullUsage, nameof(ClassicAssert.IsNotNull) },
        };
        private static readonly string[] diagnosticIds = diagnosticIdsToAssertions.Keys.ToArray();

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new IsNotNullAndNotNullClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.IsNotNullUsage, AnalyzerIdentifiers.NotNullUsage }));
        }

        [TestCase("IsNotNull", AnalyzerIdentifiers.IsNotNullUsage)]
        [TestCase("NotNull", AnalyzerIdentifiers.NotNullUsage)]
        public async Task VerifyIsNotNullAndNotNullFixes(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = Verify.Diagnostic(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        object? obj = null;
                        {|#0:ClassicAssert.{{assertion}}(obj)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        object? obj = null;
                        Assert.That(obj, Is.Not.Null);
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsNotNull", AnalyzerIdentifiers.IsNotNullUsage)]
        [TestCase("NotNull", AnalyzerIdentifiers.NotNullUsage)]
        public async Task VerifyIsNotNullAndNotNullFixesWithMessage(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = Verify.Diagnostic(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        object? obj = null;
                        {|#0:ClassicAssert.{{assertion}}(obj, "message")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        object? obj = null;
                        Assert.That(obj, Is.Not.Null, "message");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsNotNull", AnalyzerIdentifiers.IsNotNullUsage)]
        [TestCase("NotNull", AnalyzerIdentifiers.NotNullUsage)]
        public async Task VerifyIsNotNullAndNotNullFixesWithMessageAndOneArgumentForParams(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = Verify.Diagnostic(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        object? obj = null;
                        {|#0:ClassicAssert.{{assertion}}(obj, "message-id: {0}", Guid.NewGuid())|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        object? obj = null;
                        Assert.That(obj, Is.Not.Null, $"message-id: {Guid.NewGuid()}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsNotNull", AnalyzerIdentifiers.IsNotNullUsage)]
        [TestCase("NotNull", AnalyzerIdentifiers.NotNullUsage)]
        public async Task VerifyIsNotNullAndNotNullFixesWithMessageAndTwoArgumentsForParams(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = Verify.Diagnostic(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        object? obj = null;
                        {|#0:ClassicAssert.{{assertion}}(obj, "{0}, {1}", "first", "second")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        object? obj = null;
                        Assert.That(obj, Is.Not.Null, $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("IsNotNull", AnalyzerIdentifiers.IsNotNullUsage)]
        [TestCase("NotNull", AnalyzerIdentifiers.NotNullUsage)]
        public async Task VerifyIsNotNullAndNotNullFixesWithMessageAndArrayParamsInNonstandardOrder(string assertion, string diagnosticId)
        {
            var expectedDiagnostic = Verify.Diagnostic(diagnosticId);

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        object? obj = null;
                        {|#0:ClassicAssert.{{assertion}}(args: new[] { "first", "second" }, message: "{0}, {1}", anObject: obj)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        object? obj = null;
                        Assert.That(obj, Is.Not.Null, $"{"first"}, {"second"}");
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
                        object? obj = null;
                        {|#0:ClassicAssert.{{assertion}}(
                            obj{{commaAndMessage}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        object? obj = null;
                        Assert.That(
                            obj,
                            Is.Not.Null{{commaAndMessage}});
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
                        object? obj = null;
                        {|#0:ClassicAssert.{{assertion}}(
                            obj{{commaAndMessage}}
                        )|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        object? obj = null;
                        Assert.That(
                            obj,
                            Is.Not.Null{{commaAndMessage}}
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
                        object? obj = null;
                        {|#0:ClassicAssert.{{assertion}}(
                            obj, "message"{{optionalNewline}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        object? obj = null;
                        Assert.That(
                            obj, Is.Not.Null, "message"{{optionalNewline}});
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
