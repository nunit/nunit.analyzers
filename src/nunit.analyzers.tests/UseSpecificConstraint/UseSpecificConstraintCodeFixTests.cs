using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.UseSpecificConstraint.UseSpecificConstraintAnalyzer, NUnit.Analyzers.UseSpecificConstraint.UseSpecificConstraintCodeFix>;

namespace NUnit.Analyzers.Tests.UseSpecificConstraint
{
    public class UseSpecificConstraintCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.UseSpecificConstraint);

        private static readonly string[][] EqualToSpecificConstraint =
        [
            ["false", "False"],
#if NUNIT4
            ["default(bool)", "Default"],
#else
            ["default(bool)", "False"],
#endif
            ["true", "True"],

            ["(object?)null", "Null"],
            ["default(object)", "Null"],
            ["default(string)", "Null"],
#if NUNIT4
            ["default(int)", "Default"],
#endif
            ["0", "Zero"],
            ["0.0", "Zero"],
        ];

        [TestCaseSource(nameof(EqualToSpecificConstraint))]
        public Task AnalyzeForSpecificConstraint(string literal, string constraint) => AnalyzeForEqualTo(literal, constraint);

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

        private static async Task AnalyzeForEqualTo(string literal, string constraint)
        {
            await AnalyzeForEqualTo("Is", string.Empty, literal, constraint);
            await AnalyzeForEqualTo("Is", ".And.Not.Empty", literal, constraint);
            await AnalyzeForEqualTo("Is.Not", string.Empty, literal, constraint);
            await AnalyzeForEqualTo("Is.EqualTo(4).Or", string.Empty, literal, constraint);
        }

        private static async Task AnalyzeForEqualTo(string prefix, string suffix, string literal, string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var actual = {{literal}};
                        Assert.That(actual, {|#0:{{prefix}}.EqualTo({{literal}})|}{{suffix}});
            """);

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var actual = {{literal}};
                        Assert.That(actual, {{prefix}}.{{constraint}}{{suffix}});
            """);

            await Verify.VerifyCodeFixAsync(testCode,
                expectedDiagnostic.WithLocation(0).WithMessage($"Replace 'Is.EqualTo({literal})' with 'Is.{constraint}' constraint"),
                fixedCode);
        }
    }
}
