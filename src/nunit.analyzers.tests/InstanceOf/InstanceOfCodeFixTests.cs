using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.InstanceOf;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.InstanceOf
{
    public class InstanceOfCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new InstanceOfAnalyzer();
        private static readonly CodeFixProvider fix = new InstanceOfCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.InstanceOf);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.InstanceOf }));
        }

        [TestCase("\"some string\"", "string", "")]
        [TestCase("Task.FromResult(0)", "Task<int>", "")]
        [TestCase("\"some string\"", "string", ", Is.True")]
        [TestCase("\"some string\"", "string", ", Is.Not.True")]
        [TestCase("Task.FromResult(0)", "Task<int>", ", Is.True")]
        [TestCase("\"some string\"", "string", ", Is.False")]
        [TestCase("\"some string\"", "string", ", Is.Not.True")]
        [TestCase("Task.FromResult(0)", "Task<int>", ", Is.False")]
        public void VerifyInstanceOfCodeFix(string instanceValue, string typeExpression, string constraintString)
        {
            var code = TestUtility.WrapInTestMethod(
                @$"var instance = {instanceValue};
                Assert.That(↓instance is {typeExpression}{constraintString});");

            var expectedConstraint = constraintString is ", Is.False" or ", Is.Not.True" ?
                "Is.Not.InstanceOf" : "Is.InstanceOf";
            var fixedCode = TestUtility.WrapInTestMethod(
                @$"var instance = {instanceValue};
                Assert.That(instance, {expectedConstraint}<{typeExpression}>());");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }
    }
}
