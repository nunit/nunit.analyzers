using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.SameActualExpectedValue.SameActualExpectedValueAnalyzer>;

namespace NUnit.Analyzers.Tests.SameActualExpectedValue
{
    public class SameActualExpectedValueAnalyzerTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.SameActualExpectedValue);

        [TestCase(nameof(Is.EqualTo))]
        [TestCase(nameof(Is.EquivalentTo))]
        public async Task AnalyzeWhenSameVariableProvided(string constraintMethod)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var str = "test";
                        Assert.That(str, Is.{{constraintMethod}}({|#0:str|}));
            """);

            var message = "The actual and the expected argument is the same 'str'";
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithMessage(message).WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenSameExpressionProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var str = "test";
                        Assert.That(str.Trim(), Is.EqualTo({|#0:str.Trim()|}));
            """);

            var message = "The actual and the expected argument is the same 'str.Trim()'";
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithMessage(message).WithLocation(0));
        }

        [Test]
        public async Task AnalyzeForTwoCombinedConstraintsViaProperty()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var str = "test";
                        Assert.That(str, Is.EqualTo({|#0:str|}).And.EquivalentTo({|#1:str|}));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("str"), expectedDiagnostic.WithLocation(1).WithArguments("str"));
        }

        [Test]
        public async Task AnalyzeForMultipleCombinedConstraintsViaProperty()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var str = "test";
                        var str2 = "test2";
                        Assert.That(str, Is.EqualTo({|#0:str|})
                            .Or.Contains(str2)
                            .Or.EquivalentTo({|#1:str|})
                            .And.StartsWith({|#2:str|}));
            """);

            var message = "The actual and the expected argument is the same 'str'";
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithMessage(message).WithLocation(0), expectedDiagnostic.WithMessage(message).WithLocation(1), expectedDiagnostic.WithMessage(message).WithLocation(2));
        }

        [Test]
        public async Task AnalyzeForTwoCombinedConstraintsViaOperator()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var str = "test";
                        Assert.That(str, Is.EqualTo({|#0:str|}) | Is.EquivalentTo({|#1:str|}));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("str"), expectedDiagnostic.WithLocation(1).WithArguments("str"));
        }

        [Test]
        public async Task AnalyzeForMultipleCombinedConstraintsViaOperator()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var str = "test";
                        var str2 = "test2";
                        Assert.That(str, Is.EqualTo({|#0:str|}) 
                            | Is.EquivalentTo(str2)
                            & Does.Contain({|#1:str|})
                            | Does.StartWith({|#2:str|}));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("str"), expectedDiagnostic.WithLocation(1).WithArguments("str"), expectedDiagnostic.WithLocation(2).WithArguments("str"));
        }

        [TestCase(nameof(Is.EqualTo))]
        [TestCase(nameof(Is.EquivalentTo))]
        public async Task ValidWhenDifferentVariablesProvided(string constraintMethod)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var str1 = "test1";
                        var str2 = "test2";
                        Assert.That(str1, Is.{{constraintMethod}}(str2));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenDifferentExpressionsProvided()
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var str = "test";
                        Assert.That(str.Trim(), Is.EqualTo(str.TrimEnd()));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
