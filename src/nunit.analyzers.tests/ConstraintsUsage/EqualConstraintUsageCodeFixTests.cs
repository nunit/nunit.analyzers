using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.ConstraintUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ConstraintsUsage
{
    public class EqualConstraintUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new EqualConstraintUsageAnalyzer();
        private static readonly CodeFixProvider fix = new EqualConstraintUsageCodeFix();
        private static readonly ExpectedDiagnostic equalConstraintDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.EqualConstraintUsage);

        [Test]
        public void FixesEqualsOperator()
        {
            var code = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual == ""abc"");");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual, Is.EqualTo(""abc""));");

            AnalyzerAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesNotEqualsOperator()
        {
            var code = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual != ""abc"");");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual, Is.Not.EqualTo(""abc""));");

            AnalyzerAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesEqualsInstanceMethod()
        {
            var code = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual.Equals(""abc""));");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual, Is.EqualTo(""abc""));");

            AnalyzerAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesNegatedEqualsInstanceMethod()
        {
            var code = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(!actual.Equals(""bcd""));");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual, Is.Not.EqualTo(""bcd""));");

            AnalyzerAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesEqualsStaticMethod()
        {
            var code = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(Equals(actual,""abc""));");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual, Is.EqualTo(""abc""));");

            AnalyzerAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesNegatedEqualsStaticMethod()
        {
            var code = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(!Equals(actual,""bcd""));");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual, Is.Not.EqualTo(""bcd""));");

            AnalyzerAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesEqualsMethodWithIsTrue()
        {
            var code = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual.Equals(""abc""), Is.True);");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual, Is.EqualTo(""abc""));");

            AnalyzerAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesEqualsMethodWithIsFalse()
        {
            var code = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual.Equals(""bcd""), Is.False);");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual, Is.Not.EqualTo(""bcd""));");

            AnalyzerAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesEqualsMethodWithAssertTrue()
        {
            var code = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.True(actual.Equals(""abc""));");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual, Is.EqualTo(""abc""));");

            AnalyzerAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesEqualsMethodWithAssertFalse()
        {
            var code = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.False(actual.Equals(""bcd""));");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual, Is.Not.EqualTo(""bcd""));");

            AnalyzerAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesEqualsMethodWithAssertIsTrue()
        {
            var code = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.IsTrue(actual.Equals(""abc""));");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual, Is.EqualTo(""abc""));");

            AnalyzerAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesEqualsMethodWithAssertIsFalse()
        {
            var code = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.IsFalse(actual.Equals(""bcd""));");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual, Is.Not.EqualTo(""bcd""));");

            AnalyzerAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesAssertThatWithMessage()
        {
            var code = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual == ""abc"", ""Assertion message"");");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual, Is.EqualTo(""abc""), ""Assertion message"");");

            AnalyzerAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesEqualsMethodWithIsTrueWithMessage()
        {
            var code = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual.Equals(""abc""), Is.True, ""Assertion message"");");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual, Is.EqualTo(""abc""), ""Assertion message"");");

            AnalyzerAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesEqualsMethodWithAssertTrueWithMessage()
        {
            var code = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.True(actual.Equals(""abc""), ""Assertion message"");");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual, Is.EqualTo(""abc""), ""Assertion message"");");

            AnalyzerAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesEqualsMethodWithAssertFalseWithMessage()
        {
            var code = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.False(actual.Equals(""bcd""), ""Assertion message"");");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual, Is.Not.EqualTo(""bcd""), ""Assertion message"");");

            AnalyzerAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void CodeFixPreservesLineBreakBeforeMessage()
        {
            var code = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";

                Assert.False(actual.Equals(""bcd""),
                    ""Assertion message from new line"");");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";

                Assert.That(actual, Is.Not.EqualTo(""bcd""),
                    ""Assertion message from new line"");");

            AnalyzerAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }
    }
}
