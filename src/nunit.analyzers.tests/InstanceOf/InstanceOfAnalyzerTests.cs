using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.InstanceOf.InstanceOfAnalyzer>;

namespace NUnit.Analyzers.Tests.InstanceOf
{
    public class InstanceOfAnalyzerTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.InstanceOf);

        [TestCase("\"some string\"", "string", "")]
        [TestCase("Task.FromResult(0)", "Task<int>", "")]
        [TestCase("\"some string\"", "string", ", Is.True")]
        [TestCase("\"some string\"", "string", ", Is.Not.False")]
        [TestCase("Task.FromResult(0)", "Task<int>", ", Is.True")]
        [TestCase("\"some string\"", "string", ", Is.False")]
        [TestCase("Task.FromResult(0)", "Task<int>", ", Is.False")]
        public async Task AnalyzeWhenIsExpression(string instanceValue, string typeExpression, string constraintString)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var instance = {{instanceValue}};
                        Assert.That({|#0:instance is {{typeExpression}}|}{{constraintString}});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments(DisplayName(typeExpression)));
        }

        [TestCase("\"some string\"", "string", "")]
        [TestCase("Task.FromResult(0)", "Task<int>", "")]
        [TestCase("\"some string\"", "string", ", Is.True")]
        [TestCase("Task.FromResult(0)", "Task<int>", ", Is.True")]
        [TestCase("\"some string\"", "string", ", Is.False")]
        [TestCase("Task.FromResult(0)", "Task<int>", ", Is.False")]
        public async Task ValidWhenIsPatternExpression(string instanceValue, string typeExpression, string constraintString)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var instance = {{instanceValue}};
                        Assert.That(instance is {{typeExpression}} myPatternVariable{{constraintString}});
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ShouldNotRaiseForClassicAssert()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var instance = "some string";
                        ClassicAssert.IsTrue(instance is string);
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        // The diagnostic message contains the type's display name.
        private static string DisplayName(string typeExpression)
            => typeExpression == "Task<int>" ? "System.Threading.Tasks.Task<int>" : typeExpression;
    }
}
