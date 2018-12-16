using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TestCaseUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.TestCaseUsage
{
    [TestFixture]
    public sealed class TestCaseUsageAnalyzerTests
    {
        private static readonly string basePath =
            $@"{TestContext.CurrentContext.TestDirectory}\Targets\TestCaseUsage\{nameof(TestCaseUsageAnalyzerTests)}";

        [Test]
        public void VerifySupportedDiagnostics()
        {
            var analyzer = new TestCaseUsageAnalyzer();
            var diagnostics = analyzer.SupportedDiagnostics;

            var expectedIdentifiers = new List<string>
            {
                AnalyzerIdentifiers.TestCaseNotEnoughArgumentsUsage,
                AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage,
                AnalyzerIdentifiers.TestCaseTooManyArgumentsUsage
            };
            CollectionAssert.AreEquivalent(expectedIdentifiers, diagnostics.Select(d => d.Id));

            foreach (var diagnostic in diagnostics)
            {
                Assert.That(diagnostic.Title.ToString(), Is.EqualTo(TestCaseUsageAnalyzerConstants.Title),
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Title)}");
                Assert.That(diagnostic.Category, Is.EqualTo(Categories.Usage),
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Category)}");
                Assert.That(diagnostic.DefaultSeverity, Is.EqualTo(DiagnosticSeverity.Error),
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.DefaultSeverity)}");
            }

            var diagnosticMessage = diagnostics.Select(_ => _.MessageFormat.ToString()).ToImmutableArray();

            Assert.That(diagnosticMessage, Contains.Item(TestCaseUsageAnalyzerConstants.NotEnoughArgumentsMessage),
                $"{TestCaseUsageAnalyzerConstants.NotEnoughArgumentsMessage} is missing.");
            Assert.That(diagnosticMessage, Contains.Item(TestCaseUsageAnalyzerConstants.TooManyArgumentsMessage),
                $"{TestCaseUsageAnalyzerConstants.TooManyArgumentsMessage} is missing.");
            Assert.That(diagnosticMessage, Contains.Item(TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage),
                $"{TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage} is missing.");
        }

        [Test]
        public async Task AnalyzeWhenAttributeIsNotInNUnit()
        {
            await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
                $"{TestCaseUsageAnalyzerTests.basePath}{(nameof(this.AnalyzeWhenAttributeIsNotInNUnit))}.cs",
                Array.Empty<string>());
        }

        [Test]
        public async Task AnalyzeWhenAttributeIsTestAttribute()
        {
            await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
                $"{TestCaseUsageAnalyzerTests.basePath}{(nameof(this.AnalyzeWhenAttributeIsTestAttribute))}.cs",
                Array.Empty<string>());
        }

        [Test]
        public async Task AnalyzeWhenAttributeHasNoArguments()
        {
            await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
                $"{TestCaseUsageAnalyzerTests.basePath}{(nameof(this.AnalyzeWhenAttributeHasNoArguments))}.cs",
                Array.Empty<string>());
        }

        [Test]
        public async Task AnalyzeWhenArgumentIsCorrect()
        {
            await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
                $"{TestCaseUsageAnalyzerTests.basePath}{(nameof(this.AnalyzeWhenArgumentIsCorrect))}.cs",
                Array.Empty<string>());
        }

        [Test]
        public async Task AnalyzeWhenArgumentIsACast()
        {
            await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
                $"{TestCaseUsageAnalyzerTests.basePath}{(nameof(this.AnalyzeWhenArgumentIsACast))}.cs",
                Array.Empty<string>());
        }

        [Test]
        public async Task AnalyzeWhenArgumentIsAPrefixedValue()
        {
            await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
                $"{TestCaseUsageAnalyzerTests.basePath}{(nameof(this.AnalyzeWhenArgumentIsAPrefixedValue))}.cs",
                Array.Empty<string>());
        }

        [Test]
        public async Task AnalyzeWhenArgumentIsAReferenceToConstant()
        {
            await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
                $"{TestCaseUsageAnalyzerTests.basePath}{(nameof(this.AnalyzeWhenArgumentIsAReferenceToConstant))}.cs",
                Array.Empty<string>());
        }

        [Test]
        public async Task AnalyzeWhenArgumentTypeIsIncorrect()
        {
            await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
                $"{TestCaseUsageAnalyzerTests.basePath}{(nameof(this.AnalyzeWhenArgumentTypeIsIncorrect))}.cs",
                new[] { AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage },
                diagnostics =>
                {
                    var diagnostic = diagnostics[0];
                    Assert.That(diagnostic.GetMessage(), Is.EqualTo(
                        string.Format(TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage, "0", "a")),
                        nameof(diagnostic.GetMessage));
                });
        }

        [Test]
        public async Task AnalyzeWhenArgumentPassesNullToValueType()
        {
            await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
                $"{TestCaseUsageAnalyzerTests.basePath}{(nameof(this.AnalyzeWhenArgumentPassesNullToValueType))}.cs",
                new[] { AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage },
                diagnostics =>
                {
                    var diagnostic = diagnostics[0];
                    Assert.That(diagnostic.GetMessage(), Is.EqualTo(
                        string.Format(TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage, "0", "a")),
                        nameof(diagnostic.GetMessage));
                });
        }

        [Test]
        public async Task AnalyzeWhenArgumentPassesNullToNullableType()
        {
            await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
                $"{TestCaseUsageAnalyzerTests.basePath}{(nameof(this.AnalyzeWhenArgumentPassesNullToNullableType))}.cs",
                Array.Empty<string>());
        }

        [Test]
        public async Task AnalyzeWhenArgumentPassesValueToNullableType()
        {
            await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
                $"{TestCaseUsageAnalyzerTests.basePath}{(nameof(this.AnalyzeWhenArgumentPassesValueToNullableType))}.cs",
                Array.Empty<string>());
        }

        [Test]
        public async Task AnalyzeWhenNotEnoughRequiredArgumentsAreProvided()
        {
            await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
                $"{TestCaseUsageAnalyzerTests.basePath}{(nameof(this.AnalyzeWhenNotEnoughRequiredArgumentsAreProvided))}.cs",
                new[] { AnalyzerIdentifiers.TestCaseNotEnoughArgumentsUsage },
                diagnostics =>
                {
                    var diagnostic = diagnostics[0];
                    Assert.That(diagnostic.GetMessage(), Is.EqualTo(TestCaseUsageAnalyzerConstants.NotEnoughArgumentsMessage),
                        nameof(diagnostic.GetMessage));
                });
        }

        [Test]
        public async Task AnalyzeWhenTooManyRequiredArgumentsAreProvided()
        {
            await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
                $"{TestCaseUsageAnalyzerTests.basePath}{(nameof(this.AnalyzeWhenTooManyRequiredArgumentsAreProvided))}.cs",
                new[] { AnalyzerIdentifiers.TestCaseTooManyArgumentsUsage },
                diagnostics =>
                {
                    var diagnostic = diagnostics[0];
                    Assert.That(diagnostic.GetMessage(), Is.EqualTo(TestCaseUsageAnalyzerConstants.TooManyArgumentsMessage),
                        nameof(diagnostic.GetMessage));
                });
        }

        [Test]
        public async Task AnalyzeWhenTooManyRequiredAndOptionalArgumentsAreProvided()
        {
            await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
                $"{TestCaseUsageAnalyzerTests.basePath}{(nameof(this.AnalyzeWhenTooManyRequiredAndOptionalArgumentsAreProvided))}.cs",
                new[] { AnalyzerIdentifiers.TestCaseTooManyArgumentsUsage },
                diagnostics =>
                {
                    var diagnostic = diagnostics[0];
                    Assert.That(diagnostic.GetMessage(), Is.EqualTo(TestCaseUsageAnalyzerConstants.TooManyArgumentsMessage),
                        nameof(diagnostic.GetMessage));
                });
        }

        [Test]
        public async Task AnalyzeWhenMethodHasRequiredAndParamsAndMoreArgumentsThanParametersAreProvided()
        {
            await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
                $"{TestCaseUsageAnalyzerTests.basePath}{(nameof(this.AnalyzeWhenMethodHasRequiredAndParamsAndMoreArgumentsThanParametersAreProvided))}.cs",
                Array.Empty<string>());
        }

        [Test]
        public async Task AnalyzeWhenMethodHasOnlyParamsAndNoArgumentsAreProvided()
        {
            await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
                $"{TestCaseUsageAnalyzerTests.basePath}{(nameof(this.AnalyzeWhenMethodHasOnlyParamsAndNoArgumentsAreProvided))}.cs",
                Array.Empty<string>());
        }

        [Test]
        public async Task AnalyzeWhenMethodHasOnlyParamsAndArgumentTypeIsCorrect()
        {
            await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
                $"{TestCaseUsageAnalyzerTests.basePath}{(nameof(this.AnalyzeWhenMethodHasOnlyParamsAndArgumentTypeIsCorrect))}.cs",
                Array.Empty<string>());
        }

        [Test]
        public async Task AnalyzeWhenMethodHasOnlyParamsAndArgumentTypeIsIncorrect()
        {
            await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
                $"{TestCaseUsageAnalyzerTests.basePath}{(nameof(this.AnalyzeWhenMethodHasOnlyParamsAndArgumentTypeIsIncorrect))}.cs",
                new[] { AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage },
                diagnostics =>
                {
                    var diagnostic = diagnostics[0];
                    Assert.That(diagnostic.GetMessage(), Is.EqualTo(
                        string.Format(TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage, "0", "a")),
                        nameof(diagnostic.GetMessage));
                });
        }

        [Test]
        public async Task AnalyzeWhenMethodHasOnlyParamsAndArgumentPassesNullToValueType()
        {
            await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
                $"{TestCaseUsageAnalyzerTests.basePath}{(nameof(this.AnalyzeWhenMethodHasOnlyParamsAndArgumentPassesNullToValueType))}.cs",
                new[] { AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage },
                diagnostics =>
                {
                    var diagnostic = diagnostics[0];
                    Assert.That(diagnostic.GetMessage(), Is.EqualTo(
                        string.Format(TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage, "0", "a")),
                        nameof(diagnostic.GetMessage));
                });
        }
    }
}
