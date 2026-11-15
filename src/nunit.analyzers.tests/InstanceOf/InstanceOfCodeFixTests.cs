using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.InstanceOf;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.InstanceOf.InstanceOfAnalyzer, NUnit.Analyzers.InstanceOf.InstanceOfCodeFix>;

namespace NUnit.Analyzers.Tests.InstanceOf
{
    public class InstanceOfCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.InstanceOf);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var ids = new InstanceOfCodeFix().FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.InstanceOf }));
        }

        [TestCase("\"some string\"", "string", "")]
        [TestCase("Task.FromResult(0)", "Task<int>", "")]
        [TestCase("\"some string\"", "string", ", Is.True")]
        [TestCase("\"some string\"", "string", ", Is.Not.Not.True")]
        [TestCase("Task.FromResult(0)", "Task<int>", ", Is.True")]
        [TestCase("\"some string\"", "string", ", Is.Not.False")]
        public async Task VerifyInstanceOfCodeFix(string instanceValue, string typeExpression, string actualConstraint)
        {
            await VerifyInstanceOfCodeFix(instanceValue, typeExpression, actualConstraint, "Is.InstanceOf");
        }

        [TestCase("\"some string\"", "string", ", Is.Not.True")]
        [TestCase("\"some string\"", "string", ", Is.False")]
        [TestCase("\"some string\"", "string", ", Is.Not.Not.False")]
        [TestCase("Task.FromResult(0)", "Task<int>", ", Is.False")]
        public async Task VerifyNotInstanceOfCodeFix(string instanceValue, string typeExpression, string actualConstraint)
        {
            await VerifyInstanceOfCodeFix(instanceValue, typeExpression, actualConstraint, "Is.Not.InstanceOf");
        }

        private static async Task VerifyInstanceOfCodeFix(string instanceValue, string typeExpression, string actualConstraint, string expectedConstraint)
        {
            var code = TestUtility.WrapInTestMethod(
                $$"""
                            var instance = {{instanceValue}};
                                            Assert.That({|#0:instance is {{typeExpression}}|}{{actualConstraint}});
                """);

            var fixedCode = TestUtility.WrapInTestMethod(
                $$"""
                            var instance = {{instanceValue}};
                                            Assert.That(instance, {{expectedConstraint}}<{{typeExpression}}>());
                """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0).WithArguments(DisplayName(typeExpression)), fixedCode);
        }

        // The diagnostic message contains the type's display name.
        private static string DisplayName(string typeExpression)
            => typeExpression == "Task<int>" ? "System.Threading.Tasks.Task<int>" : typeExpression;
    }
}
