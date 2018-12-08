using System;
using System.Threading.Tasks;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TestCaseSourceUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.TestCaseSourceUsage
{
    [TestFixture]
    public sealed class TestCaseSourceUsesStringAnalyzerTests
    {
        private static readonly string BasePath =
            $@"{TestContext.CurrentContext.TestDirectory}\Targets\TestCaseSourceUsage\{nameof(TestCaseSourceUsesStringAnalyzerTests)}";

        [Test]
        public async Task AnalyzeWhenNameOf()
        {
            await TestHelpers.RunAnalysisAsync<TestCaseSourceUsesStringAnalyzer>(
                $"{BasePath}{(nameof(this.AnalyzeWhenNameOf))}.cs",
                Array.Empty<string>());
        }

        [Test]
        public async Task AnalyzeWhenTypeOf()
        {
            await TestHelpers.RunAnalysisAsync<TestCaseSourceUsesStringAnalyzer>(
                $"{BasePath}{(nameof(this.AnalyzeWhenTypeOf))}.cs",
                Array.Empty<string>());
        }

        [Test]
        public async Task AnalyzeWhenStringConstant()
        {
            string expectedMessage = string.Format(TestCaseSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantMessage, "Tests");
            await TestHelpers.RunAnalysisAsync<TestCaseSourceUsesStringAnalyzer>(
                $"{BasePath}{(nameof(this.AnalyzeWhenStringConstant))}.cs",
                new[] { AnalyzerIdentifiers.TestCaseSourceStringUsage },
                diagnostics =>
                {
                    var diagnostic = diagnostics[0];
                    Assert.That(diagnostic.GetMessage(), Is.EqualTo(expectedMessage),
                        nameof(diagnostic.GetMessage));
                });
        }

        [Test]
        public async Task AnalyzeWhenMultipleUnrelatedAttributes()
        {
            string expectedMessage = string.Format(TestCaseSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantMessage, "StringConstant");
            await TestHelpers.RunAnalysisAsync<TestCaseSourceUsesStringAnalyzer>(
                $"{BasePath}{(nameof(this.AnalyzeWhenMultipleUnrelatedAttributes))}.cs",
                new[] { AnalyzerIdentifiers.TestCaseSourceStringUsage },
                diagnostics =>
                {
                    var diagnostic = diagnostics[0];
                    Assert.That(diagnostic.GetMessage(), Is.EqualTo(expectedMessage),
                        nameof(diagnostic.GetMessage));
                });
        }
    }
}
