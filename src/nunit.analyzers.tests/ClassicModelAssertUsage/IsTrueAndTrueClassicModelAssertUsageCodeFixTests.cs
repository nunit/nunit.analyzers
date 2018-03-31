using System.Collections.Immutable;
using System.Threading.Tasks;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class IsTrueAndTrueClassicModelAssertUsageCodeFixTests
      : CodeFixTests
    {
        private static readonly string BasePath =
          $@"{TestContext.CurrentContext.TestDirectory}\Targets\ClassicModelAssertUsage\{nameof(IsTrueAndTrueClassicModelAssertUsageCodeFixTests)}";

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new IsTrueAndTrueClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds.ToImmutableArray();

            Assert.That(ids.Length, Is.EqualTo(2), nameof(ids.Length));
            Assert.That(ids, Contains.Item(AnalyzerIdentifiers.IsTrueUsage),
              nameof(AnalyzerIdentifiers.IsTrueUsage));
            Assert.That(ids, Contains.Item(AnalyzerIdentifiers.TrueUsage),
              nameof(AnalyzerIdentifiers.TrueUsage));
        }

        [Test]
        public async Task VerifyGetFixes()
        {
            await this.VerifyGetFixes<ClassicModelAssertUsageAnalyzer, IsTrueAndTrueClassicModelAssertUsageCodeFix>(
              $"{IsTrueAndTrueClassicModelAssertUsageCodeFixTests.BasePath}{(nameof(this.VerifyGetFixes))}.cs",
              1, CodeFixConstants.TransformToConstraintModelDescription,
              new[] { "That", ", Is.True" }.ToImmutableArray());
        }

        [Test]
        public async Task VerifyGetFixesWithMessage()
        {
            await this.VerifyGetFixes<ClassicModelAssertUsageAnalyzer, IsTrueAndTrueClassicModelAssertUsageCodeFix>(
              $"{IsTrueAndTrueClassicModelAssertUsageCodeFixTests.BasePath}{(nameof(this.VerifyGetFixesWithMessage))}.cs",
              1, CodeFixConstants.TransformToConstraintModelDescription,
              new[] { "That", "Is.True, " }.ToImmutableArray());
        }

        [Test]
        public async Task VerifyGetFixesWithMessageAndParams()
        {
            await this.VerifyGetFixes<ClassicModelAssertUsageAnalyzer, IsTrueAndTrueClassicModelAssertUsageCodeFix>(
              $"{IsTrueAndTrueClassicModelAssertUsageCodeFixTests.BasePath}{(nameof(this.VerifyGetFixesWithMessageAndParams))}.cs",
              1, CodeFixConstants.TransformToConstraintModelDescription,
              new[] { "That", "Is.True, " }.ToImmutableArray());
        }
    }
}
