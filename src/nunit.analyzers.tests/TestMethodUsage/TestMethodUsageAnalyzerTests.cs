using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TestCaseUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.TestCaseUsage
{
    [TestFixture]
    public sealed class TestMethodUsageAnalyzerTests
    {
        private static readonly string BasePath =
            $@"{TestContext.CurrentContext.TestDirectory}\Targets\TestMethodUsage\{nameof(TestMethodUsageAnalyzerTests)}";

        [Test]
        public void VerifySupportedDiagnostics()
        {
            var analyzer = new TestMethodUsageAnalyzer();
            var diagnostics = analyzer.SupportedDiagnostics;

            Assert.That(diagnostics.Length, Is.EqualTo(2), nameof(DiagnosticAnalyzer.SupportedDiagnostics));

            foreach (var diagnostic in diagnostics)
            {
                Assert.That(diagnostic.Id, Is.EqualTo(AnalyzerIdentifiers.TestCaseUsage),
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Id)}");
                Assert.That(diagnostic.Title.ToString(), Is.EqualTo(TestMethodUsageAnalyzerConstants.Title),
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Title)}");
                Assert.That(diagnostic.Category, Is.EqualTo(Categories.Usage),
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Category)}");
                Assert.That(diagnostic.DefaultSeverity, Is.EqualTo(DiagnosticSeverity.Error),
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.DefaultSeverity)}");
            }

            var diagnosticMessage = diagnostics.Select(_ => _.MessageFormat.ToString()).ToImmutableArray();

            Assert.That(diagnosticMessage, Contains.Item(TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage),
                $"{TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage} is missing.");
            Assert.That(diagnosticMessage, Contains.Item(TestMethodUsageAnalyzerConstants.SpecifiedExpectedResultForVoidMethodMessage),
                $"{TestMethodUsageAnalyzerConstants.SpecifiedExpectedResultForVoidMethodMessage} is missing.");
        }

        [Test]
        public async Task AnalyzeWhenExpectedResultIsProvidedCorrectly()
        {
            await TestHelpers.RunAnalysisAsync<TestMethodUsageAnalyzer>(
                $"{TestMethodUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenExpectedResultIsProvidedCorrectly))}.cs",
                Array.Empty<string>());
        }

        [Test]
        public async Task AnalyzeWhenExpectedResultIsProvidedAndReturnTypeIsVoid()
        {
            await TestHelpers.RunAnalysisAsync<TestMethodUsageAnalyzer>(
                $"{TestMethodUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenExpectedResultIsProvidedAndReturnTypeIsVoid))}.cs",
                new[] { AnalyzerIdentifiers.TestCaseUsage },
                diagnostics =>
                {
                    var diagnostic = diagnostics[0];
                    Assert.That(diagnostic.GetMessage(), Is.EqualTo(
                        TestMethodUsageAnalyzerConstants.SpecifiedExpectedResultForVoidMethodMessage),
                        nameof(diagnostic.GetMessage));
                });
        }

        [Test]
        public async Task AnalyzeWhenExpectedResultIsProvidedAndTypeIsIncorrect()
        {
            await TestHelpers.RunAnalysisAsync<TestMethodUsageAnalyzer>(
                $"{TestMethodUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenExpectedResultIsProvidedAndTypeIsIncorrect))}.cs",
                new[] { AnalyzerIdentifiers.TestCaseUsage },
                diagnostics =>
                {
                    var diagnostic = diagnostics[0];
                    Assert.That(diagnostic.GetMessage(), Is.EqualTo(
                        string.Format(TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage, "Int32")),
                        nameof(diagnostic.GetMessage));
                });
        }

        [Test]
        public async Task AnalyzeWhenExpectedResultIsProvidedAndPassesNullToValueType()
        {
            await TestHelpers.RunAnalysisAsync<TestMethodUsageAnalyzer>(
                $"{TestMethodUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenExpectedResultIsProvidedAndPassesNullToValueType))}.cs",
                new[] { AnalyzerIdentifiers.TestCaseUsage },
                diagnostics =>
                {
                    var diagnostic = diagnostics[0];
                    Assert.That(diagnostic.GetMessage(), Is.EqualTo(
                        string.Format(TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage, "Int32")),
                        nameof(diagnostic.GetMessage));
                });
        }

        [Test]
        public async Task AnalyzeWhenExpectedResultIsProvidedAndPassesNullToNullableType()
        {
            await TestHelpers.RunAnalysisAsync<TestMethodUsageAnalyzer>(
                $"{TestMethodUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenExpectedResultIsProvidedAndPassesNullToNullableType))}.cs",
                Array.Empty<string>());
        }

        [Test]
        public async Task AnalyzeWhenExpectedResultIsProvidedAndPassesValueToNullableType()
        {
            await TestHelpers.RunAnalysisAsync<TestMethodUsageAnalyzer>(
                $"{TestMethodUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenExpectedResultIsProvidedAndPassesValueToNullableType))}.cs",
                Array.Empty<string>());
        }
    }
}
