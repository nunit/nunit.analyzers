using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.UseSpecificConstraint;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.UseSpecificConstraint
{
    public class UseSpecificConstraintAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new UseSpecificConstraintAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.UseSpecificConstraint);

        private static readonly string[][] EqualToSpecificConstraint =
        [
            ["false", "False"],
#if NUNIT4
            ["default(bool)", "Default"],
#else
            ["default(bool)", "False"],
#endif
            ["true", "True"],

            ["null", "Null"],
            ["default(object)", "Null"],
            ["default(string)", "Null"],
#if NUNIT4
            ["default(int)", "Default"],
#endif
            ["0", "Zero"],
        ];

        [TestCaseSource(nameof(EqualToSpecificConstraint))]
        public void AnalyzeForSpecificConstraint(string literal, string constraint) => AnalyzeForEqualTo(literal, constraint);

#if NUNIT4
        [Test]
        public void AnalyzeForIsDefault() => AnalyzeForEqualTo("default", "Default",
            Settings.Default.WithAllowedCompilerDiagnostics(AllowedCompilerDiagnostics.WarningsAndErrors));
#endif

        private static void AnalyzeForEqualTo(string literal, string constraint, Settings? settings = null)
        {
            AnalyzeForEqualTo("Is", string.Empty, literal, constraint, settings);
            AnalyzeForEqualTo("Is", ".And.Not.Empty", literal, constraint, settings);
            AnalyzeForEqualTo("Is.Not", string.Empty, literal, constraint, settings);
            AnalyzeForEqualTo("Is.EqualTo(4).Or", string.Empty, literal, constraint, settings);
        }

        private static void AnalyzeForEqualTo(string prefix, string suffix, string literal, string constraint, Settings? settings = null)
        {
            var testCode = TestUtility.WrapInTestMethod(
                $"Assert.That(false, ↓{prefix}.EqualTo({literal}){suffix});");

            RoslynAssert.Diagnostics(analyzer,
                expectedDiagnostic.WithMessage($"Replace 'Is.EqualTo({literal})' with 'Is.{constraint}' constraint"),
                testCode, settings);
        }
    }
}
