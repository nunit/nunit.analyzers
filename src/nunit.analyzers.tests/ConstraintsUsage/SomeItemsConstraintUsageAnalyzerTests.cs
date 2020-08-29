using System.Globalization;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.ConstraintUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ConstraintsUsage
{
    public class SomeItemsConstraintUsageAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new SomeItemsConstraintUsageAnalyzer();

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

            AnalyzerAssert.Diagnostics(analyzer, doesContainDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenListContainsUsedAssertIsTrue()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.IsTrue(↓new List<int> {1, 2, 3}.Contains(1));",
                additionalUsings: "using System.Collections.Generic;");

            AnalyzerAssert.Diagnostics(analyzer, doesContainDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenListContainsUsedAssertIsFalse()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.IsFalse(↓new List<int> {1, 2, 3}.Contains(1));",
                additionalUsings: "using System.Collections.Generic;");

            AnalyzerAssert.Diagnostics(analyzer, doesNotContainDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenLinqContainsUsedAssertThat()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.That(↓new[] {1, 2, 3}.Contains(1));",
                additionalUsings: "using System.Linq;");

            AnalyzerAssert.Diagnostics(analyzer, doesContainDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenLinqContainsUsedAssertIsTrue()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.IsTrue(↓new[] {1, 2, 3}.Contains(1));",
                additionalUsings: "using System.Linq;");

            AnalyzerAssert.Diagnostics(analyzer, doesContainDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenLinqContainsUsedAssertIsFalse()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.IsFalse(↓new[] {1, 2, 3}.Contains(1));",
                additionalUsings: "using System.Linq;");

            AnalyzerAssert.Diagnostics(analyzer, doesNotContainDiagnostic, testCode);
        }

        [Test]
        public void NoDiagnosticWhenListOtherMethodsUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.That(new List<int> { 1, 2, 3 }.Remove(1));",
                additionalUsings: "using System.Collections.Generic;");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenOtherTypesUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.That(""1, 2, 3"".Contains(""1""));");

            AnalyzerAssert.Valid(analyzer, testCode);
        }
    }
}
