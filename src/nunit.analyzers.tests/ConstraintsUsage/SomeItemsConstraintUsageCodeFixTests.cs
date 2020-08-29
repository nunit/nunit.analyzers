using System.Globalization;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.ConstraintUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ConstraintsUsage
{
    public class SomeItemsConstraintUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new SomeItemsConstraintUsageAnalyzer();

        private static readonly CodeFixProvider fix = new SomeItemsConstraintUsageCodeFix();

        private static readonly ExpectedDiagnostic doesContainDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.CollectionContainsConstraintUsage,
            string.Format(CultureInfo.InvariantCulture, SomeItemsConstraintUsageConstants.Message, "Does.Contain"));

        private static readonly ExpectedDiagnostic doesNotContainDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.CollectionContainsConstraintUsage,
            string.Format(CultureInfo.InvariantCulture, SomeItemsConstraintUsageConstants.Message, "Does.Not.Contain"));

        [Test]
        public void AnalyzeWhenListContainsUsed_AssertThat()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.That(↓new List<int> {1, 2, 3}.Contains(1));",
                additionalUsings: "using System.Collections.Generic;");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                Assert.That(new List<int> {1, 2, 3}, Does.Contain(1));",
                additionalUsings: "using System.Collections.Generic;");

            AnalyzerAssert.CodeFix(analyzer, fix, doesContainDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void AnalyzeWhenListContainsUsed_AssertIsTrue()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.IsTrue(↓new List<int> {1, 2, 3}.Contains(1));",
                additionalUsings: "using System.Collections.Generic;");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                Assert.That(new List<int> {1, 2, 3}, Does.Contain(1));",
                additionalUsings: "using System.Collections.Generic;");

            AnalyzerAssert.CodeFix(analyzer, fix, doesContainDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void AnalyzeWhenListContainsUsed_AssertIsFalse()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.IsFalse(↓new List<int> {1, 2, 3}.Contains(1));",
                additionalUsings: "using System.Collections.Generic;");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                Assert.That(new List<int> {1, 2, 3}, Does.Not.Contain(1));",
                additionalUsings: "using System.Collections.Generic;");

            AnalyzerAssert.CodeFix(analyzer, fix, doesNotContainDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void AnalyzeWhenLinqContainsUsed_AssertThat()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.That(↓new[] {1, 2, 3}.Contains(1));",
                additionalUsings: "using System.Linq;");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                Assert.That(new[] {1, 2, 3}, Does.Contain(1));",
                additionalUsings: "using System.Linq;");

            AnalyzerAssert.CodeFix(analyzer, fix, doesContainDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void AnalyzeWhenLinqContainsUsed_AssertIsTrue()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.IsTrue(↓new[] {1, 2, 3}.Contains(1));",
                additionalUsings: "using System.Linq;");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                Assert.That(new[] {1, 2, 3}, Does.Contain(1));",
                additionalUsings: "using System.Linq;");

            AnalyzerAssert.CodeFix(analyzer, fix, doesContainDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void AnalyzeWhenLinqContainsUsed_AssertIsFalse()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.IsFalse(↓new[] {1, 2, 3}.Contains(1));",
                additionalUsings: "using System.Linq;");

            var fixedCode = TestUtility.WrapInTestMethod(@"
                Assert.That(new[] {1, 2, 3}, Does.Not.Contain(1));",
                additionalUsings: "using System.Linq;");

            AnalyzerAssert.CodeFix(analyzer, fix, doesNotContainDiagnostic, testCode, fixedCode);
        }
    }
}
