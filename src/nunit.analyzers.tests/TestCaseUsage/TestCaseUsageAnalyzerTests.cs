using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TestCaseUsage;
using NUnit.Framework;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace NUnit.Analyzers.Tests.TestCaseUsage
{
	[TestFixture]
	public sealed class TestCaseUsageAnalyzerTests
	{
		private static readonly string BasePath =
			$@"{TestContext.CurrentContext.TestDirectory}\Targets\TestCaseUsage\{nameof(TestCaseUsageAnalyzerTests)}";

		[Test]
		public void VerifySupportedDiagnostics()
		{
			var analyzer = new TestCaseUsageAnalyzer();
			var diagnostics = analyzer.SupportedDiagnostics;

			Assert.That(diagnostics.Length, Is.EqualTo(5), nameof(DiagnosticAnalyzer.SupportedDiagnostics));

			foreach (var diagnostic in diagnostics)
			{
				Assert.That(diagnostic.Id, Is.EqualTo(AnalyzerIdentifiers.TestCaseUsage),
					$"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Id)}");
				Assert.That(diagnostic.Title.ToString(), Is.EqualTo(TestCaseUsageAnalyzerConstants.Title),
					$"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Title)}");
				Assert.That(diagnostic.Category, Is.EqualTo(Categories.Usage),
					$"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Category)}");
				Assert.That(diagnostic.DefaultSeverity, Is.EqualTo(DiagnosticSeverity.Error),
					$"{diagnostic.Id} : {nameof(DiagnosticDescriptor.DefaultSeverity)}");
			}

			var diagnosticMessage = diagnostics.Select(_ => _.MessageFormat.ToString()).ToImmutableArray();

			Assert.That(diagnosticMessage, Contains.Item(TestCaseUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage),
				$"{TestCaseUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage} is missing.");
			Assert.That(diagnosticMessage, Contains.Item(TestCaseUsageAnalyzerConstants.NotEnoughArgumentsMessage),
				$"{TestCaseUsageAnalyzerConstants.NotEnoughArgumentsMessage} is missing.");
			Assert.That(diagnosticMessage, Contains.Item(TestCaseUsageAnalyzerConstants.SpecifiedExpectedResultForVoidMethodMessage),
				$"{TestCaseUsageAnalyzerConstants.SpecifiedExpectedResultForVoidMethodMessage} is missing.");
			Assert.That(diagnosticMessage, Contains.Item(TestCaseUsageAnalyzerConstants.TooManyArgumentsMessage),
				$"{TestCaseUsageAnalyzerConstants.TooManyArgumentsMessage} is missing.");
			Assert.That(diagnosticMessage, Contains.Item(TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage),
				$"{TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage} is missing.");
		}

		[Test]
		public async Task AnalyzeWhenAttributeIsNotInNUnit()
		{
			await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
				$"{TestCaseUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenAttributeIsNotInNUnit))}.cs",
				Array.Empty<string>());
		}

		[Test]
		public async Task AnalyzeWhenAttributeIsTestAttribute()
		{
			await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
				$"{TestCaseUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenAttributeIsTestAttribute))}.cs",
				Array.Empty<string>());
		}

		[Test]
		public async Task AnalyzeWhenAttributeHasNoArguments()
		{
			await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
				$"{TestCaseUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenAttributeHasNoArguments))}.cs",
				Array.Empty<string>());
		}

		[Test]
		public async Task AnalyzeWhenArgumentIsCorrect()
		{
			await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
				$"{TestCaseUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenArgumentIsCorrect))}.cs",
				Array.Empty<string>());
		}

		[Test]
		public async Task AnalyzeWhenArgumentTypeIsIncorrect()
		{
			await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
				$"{TestCaseUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenArgumentTypeIsIncorrect))}.cs",
				new[] { AnalyzerIdentifiers.TestCaseUsage },
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
				$"{TestCaseUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenArgumentPassesNullToValueType))}.cs",
				new[] { AnalyzerIdentifiers.TestCaseUsage },
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
				$"{TestCaseUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenArgumentPassesNullToNullableType))}.cs",
				Array.Empty<string>());
		}

		[Test]
		public async Task AnalyzeWhenArgumentPassesValueToNullableType()
		{
			await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
				$"{TestCaseUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenArgumentPassesValueToNullableType))}.cs",
				Array.Empty<string>());
		}

		[Test]
		public async Task AnalyzeWhenNotEnoughRequiredArgumentsAreProvided()
		{
			await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
				$"{TestCaseUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenNotEnoughRequiredArgumentsAreProvided))}.cs",
				new[] { AnalyzerIdentifiers.TestCaseUsage },
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
				$"{TestCaseUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenTooManyRequiredArgumentsAreProvided))}.cs",
				new[] { AnalyzerIdentifiers.TestCaseUsage },
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
				$"{TestCaseUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenTooManyRequiredAndOptionalArgumentsAreProvided))}.cs",
				new[] { AnalyzerIdentifiers.TestCaseUsage },
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
				$"{TestCaseUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenMethodHasRequiredAndParamsAndMoreArgumentsThanParametersAreProvided))}.cs",
				Array.Empty<string>());
		}

		[Test]
		public async Task AnalyzeWhenMethodHasOnlyParamsAndNoArgumentsAreProvided()
		{
			await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
				$"{TestCaseUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenMethodHasOnlyParamsAndNoArgumentsAreProvided))}.cs",
				Array.Empty<string>());
		}

		[Test]
		public async Task AnalyzeWhenMethodHasOnlyParamsAndArgumentTypeIsCorrect()
		{
			await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
				$"{TestCaseUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenMethodHasOnlyParamsAndArgumentTypeIsCorrect))}.cs",
				Array.Empty<string>());
		}

		[Test]
		public async Task AnalyzeWhenMethodHasOnlyParamsAndArgumentTypeIsIncorrect()
		{
			await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
				$"{TestCaseUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenMethodHasOnlyParamsAndArgumentTypeIsIncorrect))}.cs",
				new[] { AnalyzerIdentifiers.TestCaseUsage },
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
				$"{TestCaseUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenMethodHasOnlyParamsAndArgumentPassesNullToValueType))}.cs",
				new[] { AnalyzerIdentifiers.TestCaseUsage },
				diagnostics =>
				{
					var diagnostic = diagnostics[0];
					Assert.That(diagnostic.GetMessage(), Is.EqualTo(
						string.Format(TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage, "0", "a")),
						nameof(diagnostic.GetMessage));
				});
		}

		[Test]
		public async Task AnalyzeWhenExpectedResultIsProvidedCorrectly()
		{
			await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
				$"{TestCaseUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenExpectedResultIsProvidedCorrectly))}.cs",
				Array.Empty<string>());
		}

		[Test]
		public async Task AnalyzeWhenExpectedResultIsProvidedAndReturnTypeIsVoid()
		{
			await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
				$"{TestCaseUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenExpectedResultIsProvidedAndReturnTypeIsVoid))}.cs",
				new[] { AnalyzerIdentifiers.TestCaseUsage },
				diagnostics =>
				{
					var diagnostic = diagnostics[0];
					Assert.That(diagnostic.GetMessage(), Is.EqualTo(
						TestCaseUsageAnalyzerConstants.SpecifiedExpectedResultForVoidMethodMessage),
						nameof(diagnostic.GetMessage));
				});
		}

		[Test]
		public async Task AnalyzeWhenExpectedResultIsProvidedAndTypeIsIncorrect()
		{
			await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
				$"{TestCaseUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenExpectedResultIsProvidedAndTypeIsIncorrect))}.cs",
				new[] { AnalyzerIdentifiers.TestCaseUsage },
				diagnostics =>
				{
					var diagnostic = diagnostics[0];
					Assert.That(diagnostic.GetMessage(), Is.EqualTo(
						string.Format(TestCaseUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage, "Int32")),
						nameof(diagnostic.GetMessage));
				});
		}

		[Test]
		public async Task AnalyzeWhenExpectedResultIsProvidedAndPassesNullToValueType()
		{
			await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
				$"{TestCaseUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenExpectedResultIsProvidedAndPassesNullToValueType))}.cs",
				new[] { AnalyzerIdentifiers.TestCaseUsage },
				diagnostics =>
				{
					var diagnostic = diagnostics[0];
					Assert.That(diagnostic.GetMessage(), Is.EqualTo(
						string.Format(TestCaseUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage, "Int32")),
						nameof(diagnostic.GetMessage));
				});
		}

		[Test]
		public async Task AnalyzeWhenExpectedResultIsProvidedAndPassesNullToNullableType()
		{
			await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
				$"{TestCaseUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenExpectedResultIsProvidedAndPassesNullToNullableType))}.cs",
				Array.Empty<string>());
		}

		[Test]
		public async Task AnalyzeWhenExpectedResultIsProvidedAndPassesValueToNullableType()
		{
			await TestHelpers.RunAnalysisAsync<TestCaseUsageAnalyzer>(
				$"{TestCaseUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenExpectedResultIsProvidedAndPassesValueToNullableType))}.cs",
				Array.Empty<string>());
		}
	}
}