using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.InstanceOf;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.InstanceOf
{
    public class InstanceOfTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new InstanceOfAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.InstanceOf);

        [TestCase("\"some string\"", "string", "")]
        [TestCase("Task.FromResult(0)", "Task<int>", "")]
        [TestCase("\"some string\"", "string", ", Is.True")]
        [TestCase("Task.FromResult(0)", "Task<int>", ", Is.True")]
        [TestCase("\"some string\"", "string", ", Is.False")]
        [TestCase("Task.FromResult(0)", "Task<int>", ", Is.False")]
        public void AnalyzeWhenIsExpression(string instanceValue, string typeExpression, string constraintString)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var instance = {instanceValue};
                Assert.That(↓instance is {typeExpression}{constraintString});");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [TestCase("\"some string\"", "string", "")]
        [TestCase("Task.FromResult(0)", "Task<int>", "")]
        [TestCase("\"some string\"", "string", ", Is.True")]
        [TestCase("Task.FromResult(0)", "Task<int>", ", Is.True")]
        [TestCase("\"some string\"", "string", ", Is.False")]
        [TestCase("Task.FromResult(0)", "Task<int>", ", Is.False")]
        public void ValidWhenIsPatternExpression(string instanceValue, string typeExpression, string constraintString)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var instance = {instanceValue};
                Assert.That(instance is {typeExpression} myPatternVariable{constraintString});");

            RoslynAssert.Valid(analyzer, testCode);
        }
    }
}
