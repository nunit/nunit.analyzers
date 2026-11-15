using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.ClassicModelAssertUsage.ClassicModelAssertUsageAnalyzer, NUnit.Analyzers.ClassicModelAssertUsage.AreNotEqualClassicModelAssertUsageCodeFix>;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class AreNotEqualClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.AreNotEqualUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new AreNotEqualClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.AreNotEqualUsage }));
        }

        [Test]
        public async Task VerifyAreNotEqualFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.AreNotEqual(2d, 3d)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(3d, Is.Not.EqualTo(2d));
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyAreNotEqualFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.AreNotEqual(2d, 3d, "message")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(3d, Is.Not.EqualTo(2d), "message");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyAreNotEqualFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.AreNotEqual(2d, 3d, "message-id: {0}", Guid.NewGuid())|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(3d, Is.Not.EqualTo(2d), $"message-id: {Guid.NewGuid()}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyAreNotEqualFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.AreNotEqual(2d, 3d, "{0}, {1}", "first", "second")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(3d, Is.Not.EqualTo(2d), $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyAreNotEqualFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.AreNotEqual(args: new[] { "first", "second" }, actual: "actual", message: "{0}, {1}", expected: "expected")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That("actual", Is.Not.EqualTo("expected"), $"{"first"}, {"second"}");
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
            var code = TestUtility.WrapInTestMethod($$"""
                        {|#0:ClassicAssert.AreNotEqual(
                            2d,
                            3d{{commaAndMessage}})|};
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That(
                            3d,
                            Is.Not.EqualTo(2d){{commaAndMessage}});
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWithNewLineClosingParen([Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage
                ? $",{Environment.NewLine}                \"message\""
                : string.Empty;
            var code = TestUtility.WrapInTestMethod($$"""
                        {|#0:ClassicAssert.AreNotEqual(
                            2d,
                            3d{{commaAndMessage}}
                        )|};
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That(
                            3d,
                            Is.Not.EqualTo(2d){{commaAndMessage}}
                        );
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWithAllArgumentsOnSameLine([Values] bool newlineBeforeClosingParen)
        {
            var optionalNewline = newlineBeforeClosingParen ? $"{Environment.NewLine}            " : string.Empty;
            var code = TestUtility.WrapInTestMethod($$"""
                        {|#0:ClassicAssert.AreNotEqual(
                            2d, 3d{{optionalNewline}})|};
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That(
                            3d, Is.Not.EqualTo(2d){{optionalNewline}});
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("string.Empty")]
        [TestCase("String.Empty")]
        [TestCase("Guid.Empty")]
        [TestCase("\"\"")]
        [TestCase("Array.Empty<int>()")]
        [TestCase("Enumerable.Empty<int>()", "using System.Linq;")]
        public async Task CodeFixUsesIsEmpty(string expected, string? additionalUsings = null)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        string value = "Value";
                        {|#0:ClassicAssert.AreNotEqual({{expected}}, value)|};
            """,
            additionalUsings);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        string value = "Value";
                        Assert.That(value, Is.Not.Empty);
            """,
            additionalUsings);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [TestCase("ImmutableArray<int>.Empty")]
        [TestCase("ImmutableList<int>.Empty")]
        [TestCase("ImmutableHashSet<int>.Empty")]
        public async Task CodeFixUsesIsEmpty(string expected)
        {
            const string usingSystemCollectionsImmutable = "using System.Collections.Immutable;";

            var code = TestUtility.WrapInTestMethod($$"""
                        string value = "Value";
                        {|#0:ClassicAssert.AreNotEqual({{expected}}, value)|};
            """,
            usingSystemCollectionsImmutable);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        string value = "Value";
                        Assert.That(value, Is.Not.Empty);
            """,
            usingSystemCollectionsImmutable);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task CodeFixDoesNotUseIsEmpty()
        {
            var code = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class TestClass
                {
                    [Test]
                    public void TestMethod()
                    {
                        var rangeValue = Range.Empty;
                        {|#0:ClassicAssert.AreNotEqual(Range.Empty, rangeValue)|};
                    }

                    public record struct Range(int Start, int End)
                    {
                        public static Range Empty { get; } = new Range(0, 0);
                    }
                }
            """);

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class TestClass
                {
                    [Test]
                    public void TestMethod()
                    {
                        var rangeValue = Range.Empty;
                        Assert.That(rangeValue, Is.Not.EqualTo(Range.Empty));
                    }

                    public record struct Range(int Start, int End)
                    {
                        public static Range Empty { get; } = new Range(0, 0);
                    }
                }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
