using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.ContainsConstraintWrongActualType.ContainsConstraintWrongActualTypeAnalyzer>;

namespace NUnit.Analyzers.Tests.ContainsConstraintWrongActualType
{
    public class ContainsConstraintWrongActualTypeAnalyzerTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.ContainsConstraintWrongActualType);

        [Test]
        public async Task AnalyzeWhenNonStringAndNonCollectionActualArgumentProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "Assert.That(123, {|#0:Does.Contain(\"1\")|});");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithMessage("The ContainsConstraint cannot be used with an actual value of type 'int'").WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenNonStringCollectionActualArgumentProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new[] {1, 2, 3};
                        Assert.That(actual, {|#0:Does.Contain("1")|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithMessage("The ContainsConstraint cannot be used with an actual value of type 'int[]'").WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenActualIsStringTask()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "Assert.That(Task.FromResult(\"1234\"), {|#0:Does.Contain(\"1\")|});");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithMessage("The ContainsConstraint cannot be used with an actual value of type 'Task<string>'").WithLocation(0));
        }

        [Test]
        public async Task ValidWhenStringProvidedAsActual()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "Assert.That(\"123\", Does.Contain(\"1\"));");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenStringArrayProvidedAsActual()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new[] {"1", "2", "3"};
                        Assert.That(actual, Does.Contain("1"));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenUsedWithAllOperator()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "Assert.That(new [] {\"Aa\", \"Ba\", \"Ca\"}, Has.All.Contains(\"a\"));");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenActualIsStringDelegate()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "Assert.That(() => \"1234\", Does.Contain(\"1\"));");

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
