using System.Globalization;
using System.Threading.Tasks;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.ConstraintUsage;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.ConstraintUsage.StringConstraintUsageAnalyzer>;

namespace NUnit.Analyzers.Tests.ConstraintsUsage
{
    public class StringConstraintUsageAnalyzerTests
    {
        private static readonly object[] PositiveAssertData = new[]
        {
            new[] { nameof(string.Contains), AnalyzerIdentifiers.StringContainsConstraintUsage, "Does.Contain" },
            [nameof(string.StartsWith), AnalyzerIdentifiers.StringStartsWithConstraintUsage, "Does.StartWith"],
            [nameof(string.EndsWith), AnalyzerIdentifiers.StringEndsWithConstraintUsage, "Does.EndWith"]
        };

        private static readonly object[] NegativeAssertData = new[]
        {
            new[] { nameof(string.Contains), AnalyzerIdentifiers.StringContainsConstraintUsage, "Does.Not.Contain" },
            [nameof(string.StartsWith), AnalyzerIdentifiers.StringStartsWithConstraintUsage, "Does.Not.StartWith"],
            [nameof(string.EndsWith), AnalyzerIdentifiers.StringEndsWithConstraintUsage, "Does.Not.EndWith"]
        };

        [TestCaseSource(nameof(PositiveAssertData))]
        public async Task AnalyzeStringBooleanMethodAssertTrue(string method, string analyzerId, string suggestedConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""ClassicAssert.True({|#0:"abc".{{method}}("ab")|});""");

            var diagnostic = Verify.Diagnostic(analyzerId).WithMessage(string.Format(CultureInfo.InvariantCulture, StringConstraintUsageConstants.Message, suggestedConstraint));
            await Verify.VerifyAnalyzerAsync(testCode, diagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(PositiveAssertData))]
        public async Task AnalyzeStringBooleanMethodAssertIsTrue(string method, string analyzerId, string suggestedConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""ClassicAssert.IsTrue({|#0:"abc".{{method}}("ab")|});""");

            var diagnostic = Verify.Diagnostic(analyzerId).WithMessage(string.Format(CultureInfo.InvariantCulture, StringConstraintUsageConstants.Message, suggestedConstraint));
            await Verify.VerifyAnalyzerAsync(testCode, diagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(PositiveAssertData))]
        public async Task AnalyzeStringBooleanMethodAssertThat(string method, string analyzerId, string suggestedConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""Assert.That({|#0:"abc".{{method}}("ab")|});""");

            var diagnostic = Verify.Diagnostic(analyzerId).WithMessage(string.Format(CultureInfo.InvariantCulture, StringConstraintUsageConstants.Message, suggestedConstraint));
            await Verify.VerifyAnalyzerAsync(testCode, diagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(PositiveAssertData))]
        public async Task AnalyzeStringBooleanMethodAssertThatIsTrue(string method, string analyzerId, string suggestedConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""Assert.That({|#0:"abc".{{method}}("ab")|}, Is.True);""");

            var diagnostic = Verify.Diagnostic(analyzerId).WithMessage(string.Format(CultureInfo.InvariantCulture, StringConstraintUsageConstants.Message, suggestedConstraint));
            await Verify.VerifyAnalyzerAsync(testCode, diagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(NegativeAssertData))]
        public async Task AnalyzeStringBooleanMethodAssertFalse(string method, string analyzerId, string suggestedConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""ClassicAssert.False({|#0:"abc".{{method}}("ab")|});""");

            var diagnostic = Verify.Diagnostic(analyzerId).WithMessage(string.Format(CultureInfo.InvariantCulture, StringConstraintUsageConstants.Message, suggestedConstraint));
            await Verify.VerifyAnalyzerAsync(testCode, diagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(NegativeAssertData))]
        public async Task AnalyzeStringBooleanMethodAssertIsFalse(string method, string analyzerId, string suggestedConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""ClassicAssert.IsFalse({|#0:"abc".{{method}}("ab")|});""");

            var diagnostic = Verify.Diagnostic(analyzerId).WithMessage(string.Format(CultureInfo.InvariantCulture, StringConstraintUsageConstants.Message, suggestedConstraint));
            await Verify.VerifyAnalyzerAsync(testCode, diagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(NegativeAssertData))]
        public async Task AnalyzeStringBooleanMethodAssertThatIsFalse(string method, string analyzerId, string suggestedConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""Assert.That({|#0:"abc".{{method}}("ab")|}, Is.False);""");

            var diagnostic = Verify.Diagnostic(analyzerId).WithMessage(string.Format(CultureInfo.InvariantCulture, StringConstraintUsageConstants.Message, suggestedConstraint));
            await Verify.VerifyAnalyzerAsync(testCode, diagnostic.WithLocation(0));
        }

        [Test]
        public async Task ValidForUnsupportedStringMethods()
        {
            var testCode = TestUtility.WrapInTestMethod("""Assert.That("abc".IsNormalized());""");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidForNonStringMethods()
        {
            var testCode = TestUtility.WrapInTestMethod(
                """Assert.That(new System.Collections.Generic.List<string> { "a","ab"}.Contains("ab"));""");

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
