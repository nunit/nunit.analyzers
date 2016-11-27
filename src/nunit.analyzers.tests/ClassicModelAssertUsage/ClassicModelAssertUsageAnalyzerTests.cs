using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
	[TestFixture]
	public sealed class ClassicModelAssertUsageAnalyzerTests
	{
		private static readonly string BasePath =
			$@"{TestContext.CurrentContext.TestDirectory}\Targets\ClassicModelAssertUsage\{nameof(ClassicModelAssertUsageAnalyzerTests)}";

		[Test]
		public void VerifySupportedDiagnostics()
		{
			var analyzer = new ClassicModelAssertUsageAnalyzer();
			var diagnostics = analyzer.SupportedDiagnostics;

			Assert.That(diagnostics.Length, Is.EqualTo(6), nameof(DiagnosticAnalyzer.SupportedDiagnostics));

			foreach (var diagnostic in diagnostics)
			{
				Assert.That(diagnostic.Title.ToString(), Is.EqualTo(ClassicModelUsageAnalyzerConstants.Title),
					$"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Title)}");
				Assert.That(diagnostic.MessageFormat.ToString(), Is.EqualTo(ClassicModelUsageAnalyzerConstants.Message),
					$"{diagnostic.Id} : {nameof(DiagnosticDescriptor.MessageFormat)}");
				Assert.That(diagnostic.Category, Is.EqualTo(Categories.Usage),
					$"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Category)}");
				Assert.That(diagnostic.DefaultSeverity, Is.EqualTo(DiagnosticSeverity.Warning),
					$"{diagnostic.Id} : {nameof(DiagnosticDescriptor.DefaultSeverity)}");
			}

			var diagnosticIds = diagnostics.Select(_ => _.Id).ToImmutableArray();

			Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.AreEqualUsage),
				$"{AnalyzerIdentifiers.AreEqualUsage} is missing.");
			Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.AreNotEqualUsage),
				$"{AnalyzerIdentifiers.AreNotEqualUsage} is missing.");
			Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.FalseUsage),
				$"{AnalyzerIdentifiers.FalseUsage} is missing.");
			Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.IsFalseUsage),
				$"{AnalyzerIdentifiers.IsFalseUsage} is missing.");
			Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.IsTrueUsage),
				$"{AnalyzerIdentifiers.IsTrueUsage} is missing.");
			Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.TrueUsage),
				$"{AnalyzerIdentifiers.TrueUsage} is missing.");
		}

		[Test]
		public async Task AnalyzeWhenThatIsUsed()
		{
			await TestHelpers.RunAnalysisAsync<ClassicModelAssertUsageAnalyzer>(
				$"{ClassicModelAssertUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenThatIsUsed))}.cs",
				Array.Empty<string>());
		}

		[Test]
		public async Task AnalyzeWhenDiagnosticIssuesExist()
		{
			await TestHelpers.RunAnalysisAsync<ClassicModelAssertUsageAnalyzer>(
				$"{ClassicModelAssertUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenDiagnosticIssuesExist))}.cs",
				Array.Empty<string>());
		}

		[Test]
		public async Task AnalyzeWhenInvocationIsNotFromAssert()
		{
			await TestHelpers.RunAnalysisAsync<ClassicModelAssertUsageAnalyzer>(
				$"{ClassicModelAssertUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenInvocationIsNotFromAssert))}.cs",
				Array.Empty<string>());
		}

		[Test]
		public async Task AnalyzeWhenIsTrueIsUsed()
		{
			await TestHelpers.RunAnalysisAsync<ClassicModelAssertUsageAnalyzer>(
				$"{ClassicModelAssertUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenIsTrueIsUsed))}.cs",
				new[] { AnalyzerIdentifiers.IsTrueUsage }, 
				diagnostics =>
				{
					var diagnostic = diagnostics[0];
					Assert.That(diagnostic.Properties[AnalyzerPropertyKeys.ModelName], Is.EqualTo(nameof(Assert.IsTrue)),
						AnalyzerPropertyKeys.ModelName);
					Assert.That(diagnostic.Properties[AnalyzerPropertyKeys.HasToleranceValue], Is.EqualTo(false.ToString()),
						AnalyzerPropertyKeys.HasToleranceValue);
				});
		}

		[Test]
		public async Task AnalyzeWhenTrueIsUsed()
		{
			await TestHelpers.RunAnalysisAsync<ClassicModelAssertUsageAnalyzer>(
				$"{ClassicModelAssertUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenTrueIsUsed))}.cs",
				new[] { AnalyzerIdentifiers.TrueUsage },
				diagnostics =>
				{
					var diagnostic = diagnostics[0];
					Assert.That(diagnostic.Properties[AnalyzerPropertyKeys.ModelName], Is.EqualTo(nameof(Assert.True)),
						AnalyzerPropertyKeys.ModelName);
					Assert.That(diagnostic.Properties[AnalyzerPropertyKeys.HasToleranceValue], Is.EqualTo(false.ToString()),
						AnalyzerPropertyKeys.HasToleranceValue);
				});
		}

		[Test]
		public async Task AnalyzeWhenIsFalseIsUsed()
		{
			await TestHelpers.RunAnalysisAsync<ClassicModelAssertUsageAnalyzer>(
				$"{ClassicModelAssertUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenIsFalseIsUsed))}.cs",
				new[] { AnalyzerIdentifiers.IsFalseUsage },
				diagnostics =>
				{
					var diagnostic = diagnostics[0];
					Assert.That(diagnostic.Properties[AnalyzerPropertyKeys.ModelName], Is.EqualTo(nameof(Assert.IsFalse)),
						AnalyzerPropertyKeys.ModelName);
					Assert.That(diagnostic.Properties[AnalyzerPropertyKeys.HasToleranceValue], Is.EqualTo(false.ToString()),
						AnalyzerPropertyKeys.HasToleranceValue);
				});
		}

		[Test]
		public async Task AnalyzeWhenFalseIsUsed()
		{
			await TestHelpers.RunAnalysisAsync<ClassicModelAssertUsageAnalyzer>(
				$"{ClassicModelAssertUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenFalseIsUsed))}.cs",
				new[] { AnalyzerIdentifiers.FalseUsage },
				diagnostics =>
				{
					var diagnostic = diagnostics[0];
					Assert.That(diagnostic.Properties[AnalyzerPropertyKeys.ModelName], Is.EqualTo(nameof(Assert.False)),
						AnalyzerPropertyKeys.ModelName);
					Assert.That(diagnostic.Properties[AnalyzerPropertyKeys.HasToleranceValue], Is.EqualTo(false.ToString()),
						AnalyzerPropertyKeys.HasToleranceValue);
				});
		}

		[Test]
		public async Task AnalyzeWhenAreEqualIsUsed()
		{
			await TestHelpers.RunAnalysisAsync<ClassicModelAssertUsageAnalyzer>(
				$"{ClassicModelAssertUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenAreEqualIsUsed))}.cs",
				new[] { AnalyzerIdentifiers.AreEqualUsage },
				diagnostics =>
				{
					var diagnostic = diagnostics[0];
					Assert.That(diagnostic.Properties[AnalyzerPropertyKeys.ModelName], Is.EqualTo(nameof(Assert.AreEqual)),
						AnalyzerPropertyKeys.ModelName);
					Assert.That(diagnostic.Properties[AnalyzerPropertyKeys.HasToleranceValue], Is.EqualTo(false.ToString()),
						AnalyzerPropertyKeys.HasToleranceValue);
				});
		}

		[Test]
		public async Task AnalyzeWhenAreEqualIsUsedWithTolerance()
		{
			await TestHelpers.RunAnalysisAsync<ClassicModelAssertUsageAnalyzer>(
				$"{ClassicModelAssertUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenAreEqualIsUsedWithTolerance))}.cs",
				new[] { AnalyzerIdentifiers.AreEqualUsage },
				diagnostics =>
				{
					var diagnostic = diagnostics[0];
					Assert.That(diagnostic.Properties[AnalyzerPropertyKeys.ModelName], Is.EqualTo(nameof(Assert.AreEqual)),
						AnalyzerPropertyKeys.ModelName);
					Assert.That(diagnostic.Properties[AnalyzerPropertyKeys.HasToleranceValue], Is.EqualTo(true.ToString()),
						AnalyzerPropertyKeys.HasToleranceValue);
				});
		}

		[Test]
		public async Task AnalyzeWhenAreNotEqualIsUsed()
		{
			await TestHelpers.RunAnalysisAsync<ClassicModelAssertUsageAnalyzer>(
				$"{ClassicModelAssertUsageAnalyzerTests.BasePath}{(nameof(this.AnalyzeWhenAreNotEqualIsUsed))}.cs",
				new[] { AnalyzerIdentifiers.AreNotEqualUsage },
				diagnostics =>
				{
					var diagnostic = diagnostics[0];
					Assert.That(diagnostic.Properties[AnalyzerPropertyKeys.ModelName], Is.EqualTo(nameof(Assert.AreNotEqual)),
						AnalyzerPropertyKeys.ModelName);
					Assert.That(diagnostic.Properties[AnalyzerPropertyKeys.HasToleranceValue], Is.EqualTo(false.ToString()),
						AnalyzerPropertyKeys.HasToleranceValue);
				});
		}
	}
}