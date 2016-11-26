using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
	[TestFixture]
	public sealed class AreNotEqualClassicModelAssertUsageCodeFixTests
		: CodeFixTests
	{
		private static readonly string BasePath =
			$@"{TestContext.CurrentContext.TestDirectory}\Targets\ClassicModelAssertUsage\{nameof(AreNotEqualClassicModelAssertUsageCodeFixTests)}";

		[Test]
		public void VerifyGetFixableDiagnosticIds()
		{
			var fix = new AreNotEqualClassicModelAssertUsageCodeFix();
			var ids = fix.FixableDiagnosticIds.ToImmutableArray();

			Assert.That(ids.Length, Is.EqualTo(1), nameof(ids.Length));
			Assert.That(ids[0], Is.EqualTo(AnalyzerIdentifiers.AreNotEqualUsage),
				nameof(AnalyzerIdentifiers.AreNotEqualUsage));
		}

		[Test]
		public async Task VerifyGetFixes()
		{
			await this.VerifyGetFixes<ClassicModelAssertUsageAnalyzer, AreNotEqualClassicModelAssertUsageCodeFix>(
				$"{AreNotEqualClassicModelAssertUsageCodeFixTests.BasePath}{(nameof(this.VerifyGetFixes))}.cs",
				1, CodeFixConstants.TransformToConstraintModelDescription,
				new[] { "That(", ", Is.Not.EqualTo(2d)" }.ToImmutableArray());
		}

		[Test]
		public async Task VerifyGetFixesWithMessage()
		{
			await this.VerifyGetFixes<ClassicModelAssertUsageAnalyzer, AreNotEqualClassicModelAssertUsageCodeFix>(
				$"{AreNotEqualClassicModelAssertUsageCodeFixTests.BasePath}{(nameof(this.VerifyGetFixesWithMessage))}.cs",
				1, CodeFixConstants.TransformToConstraintModelDescription,
				new[] { "That(", "Is.Not.EqualTo(2d), " }.ToImmutableArray());
		}

		[Test]
		public async Task VerifyGetFixesWithMessageAndParams()
		{
			await this.VerifyGetFixes<ClassicModelAssertUsageAnalyzer, AreNotEqualClassicModelAssertUsageCodeFix>(
				$"{AreNotEqualClassicModelAssertUsageCodeFixTests.BasePath}{(nameof(this.VerifyGetFixesWithMessageAndParams))}.cs",
				1, CodeFixConstants.TransformToConstraintModelDescription,
				new[] { "That(", "Is.Not.EqualTo(2d), " }.ToImmutableArray());
		}
	}
}
