using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.UseSpecificConstraint;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.UseSpecificConstraint
{
    public class UseSpecificConstraintCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new UseSpecificConstraintAnalyzer();
        private static readonly CodeFixProvider fix = new UseSpecificConstraintCodeFix();
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
            ["0.0", "Zero"],
        ];

        [TestCaseSource(nameof(EqualToSpecificConstraint))]
        public void AnalyzeForSpecificConstraint(string literal, string constraint) => AnalyzeForEqualTo(literal, constraint);

#if NUNIT4
        /*
         *  Is.EqualTo(default) no longer compiles with NUnit 4.3
         *  As 'default' is untyped, the call is ambigous between the specificy typed overloads.
         *
            [Test]
            public void AnalyzeForIsDefault() => AnalyzeForEqualTo("default", "Default",
                Settings.Default.WithAllowedCompilerDiagnostics(AllowedCompilerDiagnostics.WarningsAndErrors));
          */
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

            var fixedCode = TestUtility.WrapInTestMethod(
                $"Assert.That(false, {prefix}.{constraint}{suffix});");

            RoslynAssert.CodeFix(analyzer, fix,
                expectedDiagnostic.WithMessage($"Replace 'Is.EqualTo({literal})' with 'Is.{constraint}' constraint"),
                testCode, fixedCode,
                settings: settings);
        }
    }
}
