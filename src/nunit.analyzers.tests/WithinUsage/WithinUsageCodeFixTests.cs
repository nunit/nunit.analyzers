using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.WithinUsage;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.WithinUsage.WithinUsageAnalyzer, NUnit.Analyzers.WithinUsage.WithinUsageCodeFix>;

namespace NUnit.Analyzers.Tests.WithinUsage
{
    public class WithinUsageCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.WithinIncompatibleTypes);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var ids = new WithinUsageCodeFix().FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.WithinIncompatibleTypes }));
        }

#if !NUNIT4
        [TestCase(NUnitFrameworkConstants.NameOfIsEqualTo)]
#endif
        [TestCase(NUnitFrameworkConstants.NameOfIsLessThan)]
        [TestCase(NUnitFrameworkConstants.NameOfIsLessThanOrEqualTo)]
        [TestCase(NUnitFrameworkConstants.NameOfIsGreaterThan)]
        [TestCase(NUnitFrameworkConstants.NameOfIsGreaterThanOrEqualTo)]
        public async Task FixesWithinUsageOnConstraintWithIncompatibleExpectedType(string constraintName)
        {
            var code = TestUtility.WrapInTestMethod(
                $$"""Assert.That("1", Is.{{constraintName}}("1").{|#0:Within|}(1));""");

            var fixedCode = TestUtility.WrapInTestMethod(
                $$"""Assert.That("1", Is.{{constraintName}}("1"));""");

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, WithinUsageCodeFix.RemoveWithinDescription);
        }

#if !NUNIT4
        [TestCase(NUnitFrameworkConstants.NameOfIsEqualTo)]
#endif
        [TestCase(NUnitFrameworkConstants.NameOfIsLessThan)]
        [TestCase(NUnitFrameworkConstants.NameOfIsLessThanOrEqualTo)]
        [TestCase(NUnitFrameworkConstants.NameOfIsGreaterThan)]
        [TestCase(NUnitFrameworkConstants.NameOfIsGreaterThanOrEqualTo)]
        public async Task FixesWithinUsageOnInversedConstraintWithIncompatibleExpectedType(string constraintName)
        {
            var code = TestUtility.WrapInTestMethod(
                $$"""Assert.That("1", Is.Not.{{constraintName}}("1").{|#0:Within|}(1));""");

            var fixedCode = TestUtility.WrapInTestMethod(
                $$"""Assert.That("1", Is.Not.{{constraintName}}("1"));""");

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, WithinUsageCodeFix.RemoveWithinDescription);
        }

#if !NUNIT4
        [Test]
        public async Task FixesWithinUsageOnConstraintWithIncompatibleExpectedTypeWithMessage()
        {
            var code = TestUtility.WrapInTestMethod(
                """Assert.That("1", Is.EqualTo("1").{|#0:Within|}(1), "message");""");

            var fixedCode = TestUtility.WrapInTestMethod(
                """Assert.That("1", Is.EqualTo("1"), "message");""");

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, WithinUsageCodeFix.RemoveWithinDescription);
        }

        [Test]
        public async Task FixesWithinUsageOnConstraintWithIncompatibleExpectedTypeWithMessageAndParam()
        {
            var code = TestUtility.WrapInTestMethod(
                """Assert.That("1", Is.EqualTo("1").{|#0:Within|}(1), "message");""");

            var fixedCode = TestUtility.WrapInTestMethod(
                """Assert.That("1", Is.EqualTo("1"), "message");""");

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, WithinUsageCodeFix.RemoveWithinDescription);
        }
#endif
    }
}
