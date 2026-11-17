using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.ConstraintUsage;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.ConstraintUsage.SomeItemsConstraintUsageAnalyzer, NUnit.Analyzers.ConstraintUsage.SomeItemsConstraintUsageCodeFix>;

namespace NUnit.Analyzers.Tests.ConstraintsUsage
{
    public class SomeItemsConstraintUsageCodeFixTests
    {
        private static readonly DiagnosticResult doesContainDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.CollectionContainsConstraintUsage).WithMessage(string.Format(CultureInfo.InvariantCulture, SomeItemsConstraintUsageConstants.Message, "Does.Contain"));

        private static readonly DiagnosticResult doesNotContainDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.CollectionContainsConstraintUsage).WithMessage(string.Format(CultureInfo.InvariantCulture, SomeItemsConstraintUsageConstants.Message, "Does.Not.Contain"));

        [Test]
        public async Task AnalyzeWhenListContainsUsedAssertThat()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Assert.That({|#0:new List<int> { 1, 2, 3 }.Contains(1)|});
            """,
                additionalUsings: "using System.Collections.Generic;");

            var fixedCode = TestUtility.WrapInTestMethod("""
                        Assert.That(new List<int> { 1, 2, 3 }, Does.Contain(1));
            """,
                additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyCodeFixAsync(testCode, doesContainDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task AnalyzeWhenListContainsUsedAssertIsTrue()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        ClassicAssert.IsTrue({|#0:new List<int> { 1, 2, 3 }.Contains(1)|});
            """,
                additionalUsings: "using System.Collections.Generic;");

            var fixedCode = TestUtility.WrapInTestMethod("""
                        Assert.That(new List<int> { 1, 2, 3 }, Does.Contain(1));
            """,
                additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyCodeFixAsync(testCode, doesContainDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task AnalyzeWhenListContainsUsedAssertIsFalse()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        ClassicAssert.IsFalse({|#0:new List<int> { 1, 2, 3 }.Contains(1)|});
            """,
                additionalUsings: "using System.Collections.Generic;");

            var fixedCode = TestUtility.WrapInTestMethod("""
                        Assert.That(new List<int> { 1, 2, 3 }, Does.Not.Contain(1));
            """,
                additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyCodeFixAsync(testCode, doesNotContainDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task AnalyzeWhenLinqContainsUsedAssertThat()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Assert.That({|#0:new[] { 1, 2, 3 }.Contains(1)|});
            """,
                additionalUsings: "using System.Linq;");

            var fixedCode = TestUtility.WrapInTestMethod("""
                        Assert.That(new[] { 1, 2, 3 }, Does.Contain(1));
            """,
                additionalUsings: "using System.Linq;");

            await Verify.VerifyCodeFixAsync(testCode, doesContainDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task AnalyzeWhenLinqContainsUsedAssertIsTrue()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        ClassicAssert.IsTrue({|#0:new[] { 1, 2, 3 }.Contains(1)|});
            """,
                additionalUsings: "using System.Linq;");

            var fixedCode = TestUtility.WrapInTestMethod("""
                        Assert.That(new[] { 1, 2, 3 }, Does.Contain(1));
            """,
                additionalUsings: "using System.Linq;");

            await Verify.VerifyCodeFixAsync(testCode, doesContainDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task AnalyzeWhenLinqContainsUsedAssertIsFalse()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        ClassicAssert.IsFalse({|#0:new[] { 1, 2, 3 }.Contains(1)|});
            """,
                additionalUsings: "using System.Linq;");

            var fixedCode = TestUtility.WrapInTestMethod("""
                        Assert.That(new[] { 1, 2, 3 }, Does.Not.Contain(1));
            """,
                additionalUsings: "using System.Linq;");

            await Verify.VerifyCodeFixAsync(testCode, doesNotContainDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWithEndOfLineClosingParen()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        ClassicAssert.IsFalse(
                            {|#0:new[] { 1, 2, 3 }.Contains(1)|});
            """,
                additionalUsings: "using System.Linq;");

            var fixedCode = TestUtility.WrapInTestMethod("""
                        Assert.That(
                            new[] { 1, 2, 3 },
                            Does.Not.Contain(1));
            """,
                additionalUsings: "using System.Linq;");

            await Verify.VerifyCodeFixAsync(testCode, doesNotContainDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWithNewLineClosingParen()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        ClassicAssert.IsFalse(
                            {|#0:new[] { 1, 2, 3 }.Contains(1)|}
                        );
            """,
                additionalUsings: "using System.Linq;");

            var fixedCode = TestUtility.WrapInTestMethod("""
                        Assert.That(
                            new[] { 1, 2, 3 },
                            Does.Not.Contain(1)
                        );
            """,
                additionalUsings: "using System.Linq;");

            await Verify.VerifyCodeFixAsync(testCode, doesNotContainDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWithAllArgumentsOnSameLine([Values] bool newlineBeforeClosingParen)
        {
            var optionalNewline = newlineBeforeClosingParen ? $"{Environment.NewLine}            " : string.Empty;
            var testCode = TestUtility.WrapInTestMethod($$"""
                        ClassicAssert.IsFalse(
                            {|#0:new[] { 1, 2, 3 }.Contains(1)|}, "message"{{optionalNewline}});
            """,
                additionalUsings: "using System.Linq;");

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That(
                            new[] { 1, 2, 3 }, Does.Not.Contain(1), "message"{{optionalNewline}});
            """,
                additionalUsings: "using System.Linq;");

            await Verify.VerifyCodeFixAsync(testCode, doesNotContainDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWhenActualArgumentIsMultipleLines()
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        ClassicAssert.IsFalse({|#0:new[] { 1, 2, 3 }
                                              .Contains(1)|}, "message");
            """,
                additionalUsings: "using System.Linq;");

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That(new[] { 1, 2, 3 }, Does.Not.Contain(1), "message");
            """,
                additionalUsings: "using System.Linq;");

            await Verify.VerifyCodeFixAsync(testCode, doesNotContainDiagnostic.WithLocation(0), fixedCode);
        }
    }
}
