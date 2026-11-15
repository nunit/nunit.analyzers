using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.ClassicModelAssertUsage.ClassicModelAssertUsageAnalyzer, NUnit.Analyzers.ClassicModelAssertUsage.AreEqualClassicModelAssertUsageCodeFix>;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class AreEqualClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.AreEqualUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new AreEqualClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.AreEqualUsage }));
        }

        [Test]
        public async Task VerifyAreEqualFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.AreEqual(2d, 3d)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(3d, Is.EqualTo(2d));
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyAreEqualFixWhenToleranceExistsWithNamedArgumentButInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.AreEqual(delta: 0.0000001d, actual: 3d, expected: 2d)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(3d, Is.EqualTo(2d).Within(0.0000001d));
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyAreEqualFixWithNamedParametersInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.AreEqual(actual: 3d, expected: 2d)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(3d, Is.EqualTo(2d));
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyAreEqualFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.AreEqual(2d, 3d, "message")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(3d, Is.EqualTo(2d), "message");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyAreEqualFixWithNullMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.AreEqual(2d, 3d, null)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(3d, Is.EqualTo(2d));
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyAreEqualFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.AreEqual(2d, 3d, "message-id: {0}", Guid.NewGuid())|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(3d, Is.EqualTo(2d), $"message-id: {Guid.NewGuid()}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyAreEqualFixWithMessageAnd2ParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var actual = 3d;
                        {|#0:ClassicAssert.AreEqual(delta: 0.0000001d, expected: 2d, actual: actual, args: new[] { "Guid.NewGuid()", Guid.NewGuid().ToString() }, message: "message-id: {0}, {1}")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var actual = 3d;
                        Assert.That(actual, Is.EqualTo(2d).Within(0.0000001d), $"message-id: {"Guid.NewGuid()"}, {Guid.NewGuid().ToString()}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyAreEqualFixWithMessageAnd2ParamsInStandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var actual = 3d;
                        {|#0:ClassicAssert.AreEqual(expected: 2d, actual: actual, delta: 0.0000001d, "message-id: {0}, {1}", "Guid.NewGuid()", Guid.NewGuid())|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var actual = 3d;
                        Assert.That(actual, Is.EqualTo(2d).Within(0.0000001d), $"message-id: {"Guid.NewGuid()"}, {Guid.NewGuid()}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyAreEqualFixWhenToleranceExists()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.AreEqual(2d, 3d, 0.0000001d)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(3d, Is.EqualTo(2d).Within(0.0000001d));
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyAreEqualFixWhenToleranceExistsWithNamedArgument()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.AreEqual(expected: 2d, actual: 3d, delta: 0.0000001d)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(3d, Is.EqualTo(2d).Within(0.0000001d));
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyAreEqualFixWhenToleranceExistsWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.AreEqual(2d, 3d, 0.0000001d, "message")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(3d, Is.EqualTo(2d).Within(0.0000001d), "message");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyAreEqualFixWhenToleranceExistsWithNullMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.AreEqual(2d, 3d, 0.1, null)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(3d, Is.EqualTo(2d).Within(0.1));
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyAreEqualFixWhenToleranceExistsWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.AreEqual(2d, 3d, 0.0000001d, "message-id: {0}", Guid.NewGuid())|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(3d, Is.EqualTo(2d).Within(0.0000001d), $"message-id: {Guid.NewGuid()}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyAreEqualFixWhenToleranceExistsWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        {|#0:ClassicAssert.AreEqual(2d, 3d, 0.0000001d, "{0}, {1}", "first", "second")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(3d, Is.EqualTo(2d).Within(0.0000001d), $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyAreEqualFixWhenWithMessageAndArgsArray()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [TestCase(3.0, 3)]
                    public void Test(object actual, object expected)
                    {
                        object[] args = { expected, actual };
                        {|#0:ClassicAssert.AreEqual(expected, actual, "Expected: {0} Got: {1}", args)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [TestCase(3.0, 3)]
                    public void Test(object actual, object expected)
                    {
                        object[] args = { expected, actual };
                        Assert.That(actual, Is.EqualTo(expected), () => string.Format("Expected: {0} Got: {1}", args));
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyAreEqualFixWhenWithMessageVariable()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [TestCase(3.0, 3)]
                    public void Test(object actual, object expected)
                    {
                        {|#0:ClassicAssert.AreEqual(expected, actual, GetLocalizedFormatSpecification(), expected, actual)|};
                    }
                    private static string GetLocalizedFormatSpecification() => "Expected: {0} Got: {1}";

            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [TestCase(3.0, 3)]
                    public void Test(object actual, object expected)
                    {
                        Assert.That(actual, Is.EqualTo(expected), () => string.Format(GetLocalizedFormatSpecification(), expected, actual));
                    }
                    private static string GetLocalizedFormatSpecification() => "Expected: {0} Got: {1}";

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
                        {|#0:ClassicAssert.AreEqual(
                            2d,
                            3d,
                            0.0000001d{{commaAndMessage}})|};
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That(
                            3d,
                            Is.EqualTo(2d).Within(0.0000001d){{commaAndMessage}});
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
                        {|#0:ClassicAssert.AreEqual(
                            2d,
                            3d,
                            0.0000001d{{commaAndMessage}}
                        )|};
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That(
                            3d,
                            Is.EqualTo(2d).Within(0.0000001d){{commaAndMessage}}
                        );
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWithAllArgumentsOnSameLine([Values] bool newlineBeforeClosingParen)
        {
            var optionalNewline = newlineBeforeClosingParen ? $"{Environment.NewLine}            " : string.Empty;
            var code = TestUtility.WrapInTestMethod($$"""
                        {|#0:ClassicAssert.AreEqual(
                            2d, 3d, 0.0000001d{{optionalNewline}})|};
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That(
                            3d, Is.EqualTo(2d).Within(0.0000001d){{optionalNewline}});
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
                        {|#0:ClassicAssert.AreEqual({{expected}}, value)|};
            """,
            additionalUsings);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        string value = "Value";
                        Assert.That(value, Is.Empty);
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
                        {|#0:ClassicAssert.AreEqual({{expected}}, value)|};
            """,
            usingSystemCollectionsImmutable);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        string value = "Value";
                        Assert.That(value, Is.Empty);
            """,
            usingSystemCollectionsImmutable);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task CodeFixUsesIsEmpty()
        {
            const string usingSystemCollectionsGeneric = "using System.Collections.Generic;";

            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void TestMethod()
                    {
                        MyCollection value = new MyCollection();
                        {|#0:ClassicAssert.AreEqual(MyCollection.Empty, value)|};
                    }

                    public class MyCollection : List<int>
                    {
                        public static readonly MyCollection Empty = new MyCollection();
                    }
            """,
            usingSystemCollectionsGeneric);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void TestMethod()
                    {
                        MyCollection value = new MyCollection();
                        Assert.That(value, Is.Empty);
                    }

                    public class MyCollection : List<int>
                    {
                        public static readonly MyCollection Empty = new MyCollection();
                    }
            """,
            usingSystemCollectionsGeneric);

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
                        {|#0:ClassicAssert.AreEqual(Range.Empty, rangeValue)|};
                    }

                    public enum Range
                    {
                        Empty,
                        Full
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
                        Assert.That(rangeValue, Is.EqualTo(Range.Empty));
                    }

                    public enum Range
                    {
                        Empty,
                        Full
                    }
                }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
