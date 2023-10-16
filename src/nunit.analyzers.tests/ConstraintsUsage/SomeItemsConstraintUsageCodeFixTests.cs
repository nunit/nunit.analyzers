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
        public void AnalyzeWhenListContainsUsedAssertThat()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
            Assert.That(↓new List<int> {1, 2, 3}.Contains(1));",
                additionalUsings: "using System.Collections.Generic;");

            var fixedCode = TestUtility.WrapInTestMethod(@"
            Assert.That(new List<int> {1, 2, 3}, Does.Contain(1));",
                additionalUsings: "using System.Collections.Generic;");

            RoslynAssert.CodeFix(analyzer, fix, doesContainDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void AnalyzeWhenListContainsUsedAssertIsTrue()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
            ClassicAssert.IsTrue(↓new List<int> {1, 2, 3}.Contains(1));",
                additionalUsings: "using System.Collections.Generic;");

            var fixedCode = TestUtility.WrapInTestMethod(@"
            Assert.That(new List<int> {1, 2, 3}, Does.Contain(1));",
                additionalUsings: "using System.Collections.Generic;");

            RoslynAssert.CodeFix(analyzer, fix, doesContainDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void AnalyzeWhenListContainsUsedAssertIsFalse()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
            ClassicAssert.IsFalse(↓new List<int> {1, 2, 3}.Contains(1));",
                additionalUsings: "using System.Collections.Generic;");

            var fixedCode = TestUtility.WrapInTestMethod(@"
            Assert.That(new List<int> {1, 2, 3}, Does.Not.Contain(1));",
                additionalUsings: "using System.Collections.Generic;");

            RoslynAssert.CodeFix(analyzer, fix, doesNotContainDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void AnalyzeWhenLinqContainsUsedAssertThat()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
            Assert.That(↓new[] {1, 2, 3}.Contains(1));",
                additionalUsings: "using System.Linq;");

            var fixedCode = TestUtility.WrapInTestMethod(@"
            Assert.That(new[] {1, 2, 3}, Does.Contain(1));",
                additionalUsings: "using System.Linq;");

            RoslynAssert.CodeFix(analyzer, fix, doesContainDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void AnalyzeWhenLinqContainsUsedAssertIsTrue()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
            ClassicAssert.IsTrue(↓new[] {1, 2, 3}.Contains(1));",
                additionalUsings: "using System.Linq;");

            var fixedCode = TestUtility.WrapInTestMethod(@"
            Assert.That(new[] {1, 2, 3}, Does.Contain(1));",
                additionalUsings: "using System.Linq;");

            RoslynAssert.CodeFix(analyzer, fix, doesContainDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void AnalyzeWhenLinqContainsUsedAssertIsFalse()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
            ClassicAssert.IsFalse(↓new[] {1, 2, 3}.Contains(1));",
                additionalUsings: "using System.Linq;");

            var fixedCode = TestUtility.WrapInTestMethod(@"
            Assert.That(new[] {1, 2, 3}, Does.Not.Contain(1));",
                additionalUsings: "using System.Linq;");

            RoslynAssert.CodeFix(analyzer, fix, doesNotContainDiagnostic, testCode, fixedCode);
        }
    }
}
