using System.Globalization;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.ConstraintUsage;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.ConstraintUsage.SomeItemsConstraintUsageAnalyzer>;

namespace NUnit.Analyzers.Tests.ConstraintsUsage
{
    public class SomeItemsConstraintUsageAnalyzerTests
    {
        private static readonly DiagnosticResult doesContainDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.CollectionContainsConstraintUsage).WithMessage(string.Format(CultureInfo.InvariantCulture, SomeItemsConstraintUsageConstants.Message, "Does.Contain"));

        private static readonly DiagnosticResult doesNotContainDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.CollectionContainsConstraintUsage).WithMessage(string.Format(CultureInfo.InvariantCulture, SomeItemsConstraintUsageConstants.Message, "Does.Not.Contain"));

        [Test]
        public async Task AnalyzeWhenListContainsUsedAssertThat()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Assert.That({|#0:new List<int> {1, 2, 3}.Contains(1)|});
            """,
                additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode, doesContainDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenListContainsUsedAssertIsTrue()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        ClassicAssert.IsTrue({|#0:new List<int> {1, 2, 3}.Contains(1)|});
            """,
                additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode, doesContainDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenListContainsUsedAssertIsFalse()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        ClassicAssert.IsFalse({|#0:new List<int> {1, 2, 3}.Contains(1)|});
            """,
                additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode, doesNotContainDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenLinqContainsUsedAssertThat()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Assert.That({|#0:new[] {1, 2, 3}.Contains(1)|});
            """,
                additionalUsings: "using System.Linq;");

            await Verify.VerifyAnalyzerAsync(testCode, doesContainDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenLinqContainsUsedAssertIsTrue()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        ClassicAssert.IsTrue({|#0:new[] {1, 2, 3}.Contains(1)|});
            """,
                additionalUsings: "using System.Linq;");

            await Verify.VerifyAnalyzerAsync(testCode, doesContainDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenLinqContainsUsedAssertIsFalse()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        ClassicAssert.IsFalse({|#0:new[] {1, 2, 3}.Contains(1)|});
            """,
                additionalUsings: "using System.Linq;");

            await Verify.VerifyAnalyzerAsync(testCode, doesNotContainDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task NoDiagnosticWhenListOtherMethodsUsed()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Assert.That(new List<int> { 1, 2, 3 }.Remove(1));
            """,
                additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenOtherTypesUsed()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Assert.That("1, 2, 3".Contains("1"));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
