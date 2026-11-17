using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.ConstraintUsage.EqualConstraintUsageAnalyzer, NUnit.Analyzers.ConstraintUsage.EqualConstraintUsageCodeFix>;

namespace NUnit.Analyzers.Tests.ConstraintsUsage
{
    public class EqualConstraintUsageCodeFixTests
    {
        private static readonly DiagnosticResult equalConstraintDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.EqualConstraintUsage);

        [Test]
        public async Task FixesEqualsOperator()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:actual == "abc"|});
            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual, Is.EqualTo("abc"));
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.EqualTo"), fixedCode);
        }

        [Test]
        public async Task FixesNotEqualsOperator()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:actual != "abc"|});
            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual, Is.Not.EqualTo("abc"));
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.Not.EqualTo"), fixedCode);
        }

        [Test]
        public async Task FixesIsOperator()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:actual is "abc"|});
            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual, Is.EqualTo("abc"));
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.EqualTo"), fixedCode);
        }

        [Test]
        public async Task FixesIsNotOperator()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:actual is not "abc"|});
            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual, Is.Not.EqualTo("abc"));
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.Not.EqualTo"), fixedCode);
        }

        [Test]
        public async Task FixesComplexIsOperator()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:actual is "abc" or "def"|});
            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual, Is.EqualTo("abc").Or.EqualTo("def"));
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.EqualTo"), fixedCode);
        }

        [Test]
        public async Task FixesComplexIsNotOperator()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:actual is not "abc" and not "def"|});
            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual, Is.Not.EqualTo("abc").And.Not.EqualTo("def"));
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.EqualTo"), fixedCode);
        }

        [Test]
        public async Task FixesComplexRelationalIsOperator()
        {
            var code = TestUtility.WrapInTestMethod("""
                        double actual = 1.234;
                        Assert.That({|#0:actual is > 1 and <= 2 or 3 or > 4|});
            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        double actual = 1.234;
                        Assert.That(actual, Is.GreaterThan(1).And.LessThanOrEqualTo(2).Or.EqualTo(3).Or.GreaterThan(4));
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.EqualTo"), fixedCode);
        }

        [Test]
        public async Task FixesEqualsInstanceMethod()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:actual.Equals("abc")|});
            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual, Is.EqualTo("abc"));
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.EqualTo"), fixedCode);
        }

        [Test]
        public async Task FixesNegatedEqualsInstanceMethod()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:!actual.Equals("bcd")|});
            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual, Is.Not.EqualTo("bcd"));
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.Not.EqualTo"), fixedCode);
        }

        [Test]
        public async Task FixesEqualsStaticMethod()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:Equals(actual,"abc")|});
            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual, Is.EqualTo("abc"));
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.EqualTo"), fixedCode);
        }

        [Test]
        public async Task FixesNegatedEqualsStaticMethod()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:!Equals(actual,"bcd")|});
            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual, Is.Not.EqualTo("bcd"));
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.Not.EqualTo"), fixedCode);
        }

        [Test]
        public async Task FixesEqualsMethodWithIsTrue()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:actual.Equals("abc")|}, Is.True);
            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual, Is.EqualTo("abc"));
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.EqualTo"), fixedCode);
        }

        [Test]
        public async Task FixesEqualsMethodWithIsFalse()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:actual.Equals("bcd")|}, Is.False);
            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual, Is.Not.EqualTo("bcd"));
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.Not.EqualTo"), fixedCode);
        }

        [Test]
        public async Task FixesEqualsMethodWithAssertTrue()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        ClassicAssert.True({|#0:actual.Equals("abc")|});
            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual, Is.EqualTo("abc"));
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.EqualTo"), fixedCode);
        }

        [Test]
        public async Task FixesEqualsMethodWithAssertFalse()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        ClassicAssert.False({|#0:actual.Equals("bcd")|});
            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual, Is.Not.EqualTo("bcd"));
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.Not.EqualTo"), fixedCode);
        }

        [Test]
        public async Task FixesEqualsMethodWithAssertIsTrue()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        ClassicAssert.IsTrue({|#0:actual.Equals("abc")|});
            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual, Is.EqualTo("abc"));
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.EqualTo"), fixedCode);
        }

        [Test]
        public async Task FixesEqualsMethodWithAssertIsFalse()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        ClassicAssert.IsFalse({|#0:actual.Equals("bcd")|});
            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual, Is.Not.EqualTo("bcd"));
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.Not.EqualTo"), fixedCode);
        }

        [Test]
        public async Task FixesAssertThatWithMessage()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:actual == "abc"|}, "Assertion message");
            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual, Is.EqualTo("abc"), "Assertion message");
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.EqualTo"), fixedCode);
        }

        [Test]
        public async Task FixesEqualsMethodWithIsTrueWithMessage()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:actual.Equals("abc")|}, Is.True, "Assertion message");
            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual, Is.EqualTo("abc"), "Assertion message");
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.EqualTo"), fixedCode);
        }

        [Test]
        public async Task FixesEqualsMethodWithAssertTrueWithMessage()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        ClassicAssert.True({|#0:actual.Equals("abc")|}, "Assertion message");
            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual, Is.EqualTo("abc"), "Assertion message");
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.EqualTo"), fixedCode);
        }

        [Test]
        public async Task FixesEqualsMethodWithAssertFalseWithMessage()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        ClassicAssert.False({|#0:actual.Equals("bcd")|}, "Assertion message");
            """);

            var fixedCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual, Is.Not.EqualTo("bcd"), "Assertion message");
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.Not.EqualTo"), fixedCode);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWithEndOfLineClosingParen([Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage
                ? $",{Environment.NewLine}                \"message\""
                : string.Empty;
            var code = TestUtility.WrapInTestMethod($$"""
                        var actual = "abc";
                        ClassicAssert.False(
                            {|#0:actual.Equals("bcd")|}{{commaAndMessage}});
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var actual = "abc";
                        Assert.That(
                            actual,
                            Is.Not.EqualTo("bcd"){{commaAndMessage}});
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.Not.EqualTo"), fixedCode);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWithNewLineClosingParen([Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage
                ? $",{Environment.NewLine}                \"message\""
                : string.Empty;
            var code = TestUtility.WrapInTestMethod($$"""
                        var actual = "abc";
                        Assert.That(
                            {|#0:actual.Equals("abc")|},
                            Is.True{{commaAndMessage}}
                        );
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var actual = "abc";
                        Assert.That(
                            actual,
                            Is.EqualTo("abc"){{commaAndMessage}}
                        );
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.EqualTo"), fixedCode);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWithAllArgumentsOnSameLine([Values] bool newlineBeforeClosingParen)
        {
            var optionalNewline = newlineBeforeClosingParen ? $"{Environment.NewLine}            " : string.Empty;
            var code = TestUtility.WrapInTestMethod($$"""
                        var actual = "abc";
                        Assert.That(
                            {|#0:actual.Equals("abc")|}, Is.True{{optionalNewline}});
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var actual = "abc";
                        Assert.That(
                            actual, Is.EqualTo("abc"){{optionalNewline}});
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.EqualTo"), fixedCode);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWhenActualArgumentIsMultipleLines()
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        var someVeryLongActualVariableName = "abc";
                        Assert.That({|#0:someVeryLongActualVariableName
                                    .Equals("abc")|}, Is.True);
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var someVeryLongActualVariableName = "abc";
                        Assert.That(someVeryLongActualVariableName, Is.EqualTo("abc"));
            """);

            await Verify.VerifyCodeFixAsync(code, equalConstraintDiagnostic.WithLocation(0).WithArguments("Is.EqualTo"), fixedCode);
        }
    }
}
