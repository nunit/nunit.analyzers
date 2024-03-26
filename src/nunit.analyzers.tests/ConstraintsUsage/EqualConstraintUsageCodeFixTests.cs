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

            RoslynAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
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

            RoslynAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesIsOperator()
        {
            var code = TestUtility.WrapInTestMethod(@"
            var actual = ""abc"";
            Assert.That(actual is ""abc"");");

            var fixedCode = TestUtility.WrapInTestMethod(@"
            var actual = ""abc"";
            Assert.That(actual, Is.EqualTo(""abc""));");

            RoslynAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesIsNotOperator()
        {
            var code = TestUtility.WrapInTestMethod(@"
            var actual = ""abc"";
            Assert.That(actual is not ""abc"");");

            var fixedCode = TestUtility.WrapInTestMethod(@"
            var actual = ""abc"";
            Assert.That(actual, Is.Not.EqualTo(""abc""));");

            RoslynAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesComplexIsOperator()
        {
            var code = TestUtility.WrapInTestMethod(@"
            var actual = ""abc"";
            Assert.That(actual is ""abc"" or ""def"");");

            var fixedCode = TestUtility.WrapInTestMethod(@"
            var actual = ""abc"";
            Assert.That(actual, Is.EqualTo(""abc"").Or.EqualTo(""def""));");

            RoslynAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesComplexIsNotOperator()
        {
            var code = TestUtility.WrapInTestMethod(@"
            var actual = ""abc"";
            Assert.That(actual is not ""abc"" and not ""def"");");

            var fixedCode = TestUtility.WrapInTestMethod(@"
            var actual = ""abc"";
            Assert.That(actual, Is.Not.EqualTo(""abc"").And.Not.EqualTo(""def""));");

            RoslynAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesComplexRelationalIsOperator()
        {
            var code = TestUtility.WrapInTestMethod(@"
            double actual = 1.234;
            Assert.That(actual is > 1 and <= 2 or 3 or > 4);");

            var fixedCode = TestUtility.WrapInTestMethod(@"
            double actual = 1.234;
            Assert.That(actual, Is.GreaterThan(1).And.LessThanOrEqualTo(2).Or.EqualTo(3).Or.GreaterThan(4));");

            RoslynAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
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

            RoslynAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
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

            RoslynAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
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

            RoslynAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
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

            RoslynAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
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

            RoslynAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
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

            RoslynAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesEqualsMethodWithAssertTrue()
        {
            var code = TestUtility.WrapInTestMethod(@"
            var actual = ""abc"";
            ClassicAssert.True(actual.Equals(""abc""));");

            var fixedCode = TestUtility.WrapInTestMethod(@"
            var actual = ""abc"";
            Assert.That(actual, Is.EqualTo(""abc""));");

            RoslynAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesEqualsMethodWithAssertFalse()
        {
            var code = TestUtility.WrapInTestMethod(@"
            var actual = ""abc"";
            ClassicAssert.False(actual.Equals(""bcd""));");

            var fixedCode = TestUtility.WrapInTestMethod(@"
            var actual = ""abc"";
            Assert.That(actual, Is.Not.EqualTo(""bcd""));");

            RoslynAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesEqualsMethodWithAssertIsTrue()
        {
            var code = TestUtility.WrapInTestMethod(@"
            var actual = ""abc"";
            ClassicAssert.IsTrue(actual.Equals(""abc""));");

            var fixedCode = TestUtility.WrapInTestMethod(@"
            var actual = ""abc"";
            Assert.That(actual, Is.EqualTo(""abc""));");

            RoslynAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesEqualsMethodWithAssertIsFalse()
        {
            var code = TestUtility.WrapInTestMethod(@"
            var actual = ""abc"";
            ClassicAssert.IsFalse(actual.Equals(""bcd""));");

            var fixedCode = TestUtility.WrapInTestMethod(@"
            var actual = ""abc"";
            Assert.That(actual, Is.Not.EqualTo(""bcd""));");

            RoslynAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
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

            RoslynAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
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

            RoslynAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesEqualsMethodWithAssertTrueWithMessage()
        {
            var code = TestUtility.WrapInTestMethod(@"
            var actual = ""abc"";
            ClassicAssert.True(actual.Equals(""abc""), ""Assertion message"");");

            var fixedCode = TestUtility.WrapInTestMethod(@"
            var actual = ""abc"";
            Assert.That(actual, Is.EqualTo(""abc""), ""Assertion message"");");

            RoslynAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void FixesEqualsMethodWithAssertFalseWithMessage()
        {
            var code = TestUtility.WrapInTestMethod(@"
            var actual = ""abc"";
            ClassicAssert.False(actual.Equals(""bcd""), ""Assertion message"");");

            var fixedCode = TestUtility.WrapInTestMethod(@"
            var actual = ""abc"";
            Assert.That(actual, Is.Not.EqualTo(""bcd""), ""Assertion message"");");

            RoslynAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }

        [Test]
        public void CodeFixPreservesLineBreakBeforeMessage()
        {
            var code = TestUtility.WrapInTestMethod(@"
            var actual = ""abc"";

            ClassicAssert.False(actual.Equals(""bcd""),
                ""Assertion message from new line"");");

            var fixedCode = TestUtility.WrapInTestMethod(@"
            var actual = ""abc"";

            Assert.That(actual, Is.Not.EqualTo(""bcd""),
                ""Assertion message from new line"");");

            RoslynAssert.CodeFix(analyzer, fix, equalConstraintDiagnostic, code, fixedCode);
        }
    }
}
