using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TestCaseSourceUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.TestCaseSourceUsage
{
    public sealed class TestCaseSourceExistsTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new TestCaseSourceUsesStringAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.TestCaseSourceIsMissing);

        [Test]
        public void ErrorsWhenSourceDoesNotExists()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class Tests
    {
        [TestCaseSource(↓""Missing"")]
        public void Test()
        {
        }
    }");
            var message = "The TestCaseSource argument 'Missing' does not specify an existing member";
            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic.WithMessage(message), testCode);
        }

        [Test]
        public void NoErrorsWhenSourceExists()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    internal class Base
    {
        protected static IEnumerable<string> TargetFrameworks { get; } = new[] { ""net48"", ""netcoreapp3.1"" };
    }

    internal class Tests : Base
    {
        [TestCaseSource(nameof(TargetFrameworks))]
        public void Test(string targetFramework)
        {
            Assert.IsNotNull(targetFramework);
        }
    }", "using System.Collections.Generic;");

            RoslynAssert.Valid(analyzer, testCode);
        }
    }
}
