using System.Collections.Immutable;
using System.Threading.Tasks;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class AreEqualClassicModelAssertUsageCodeFixTests
        : CodeFixTests
    {
        private static readonly string BasePath =
            $@"{TestContext.CurrentContext.TestDirectory}\Targets\ClassicModelAssertUsage\{nameof(AreEqualClassicModelAssertUsageCodeFixTests)}";

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new AreEqualClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds.ToImmutableArray();

            Assert.That(ids.Length, Is.EqualTo(1), nameof(ids.Length));
            Assert.That(ids[0], Is.EqualTo(AnalyzerIdentifiers.AreEqualUsage),
                nameof(AnalyzerIdentifiers.AreEqualUsage));
        }

        [Test]
        public async Task VerifyGetFixes()
        {
            await this.VerifyGetFixes<ClassicModelAssertUsageAnalyzer, AreEqualClassicModelAssertUsageCodeFix>(
                $"{AreEqualClassicModelAssertUsageCodeFixTests.BasePath}{(nameof(this.VerifyGetFixes))}.cs",
                1, CodeFixConstants.TransformToConstraintModelDescription,
                new[] { "That(", ", Is.EqualTo(2d)" }.ToImmutableArray());
        }

        [Test]
        public async Task VerifyGetFixesWithMessage()
        {
            await this.VerifyGetFixes<ClassicModelAssertUsageAnalyzer, AreEqualClassicModelAssertUsageCodeFix>(
                $"{AreEqualClassicModelAssertUsageCodeFixTests.BasePath}{(nameof(this.VerifyGetFixesWithMessage))}.cs",
                1, CodeFixConstants.TransformToConstraintModelDescription,
                new[] { "That(", "Is.EqualTo(2d), " }.ToImmutableArray());
        }

        [Test]
        public async Task VerifyGetFixesWithMessageAndParams()
        {
            await this.VerifyGetFixes<ClassicModelAssertUsageAnalyzer, AreEqualClassicModelAssertUsageCodeFix>(
                $"{AreEqualClassicModelAssertUsageCodeFixTests.BasePath}{(nameof(this.VerifyGetFixesWithMessageAndParams))}.cs",
                1, CodeFixConstants.TransformToConstraintModelDescription,
                new[] { "That(", "Is.EqualTo(2d), " }.ToImmutableArray());
        }

        [Test]
        public async Task VerifyGetFixesWhenToleranceExists()
        {
            await this.VerifyGetFixes<ClassicModelAssertUsageAnalyzer, AreEqualClassicModelAssertUsageCodeFix>(
                $"{AreEqualClassicModelAssertUsageCodeFixTests.BasePath}{(nameof(this.VerifyGetFixesWhenToleranceExists))}.cs",
                1, CodeFixConstants.TransformToConstraintModelDescription,
                new[] { "That(", "Is.EqualTo(2d).Within(0.0000001d)" }.ToImmutableArray());
        }

        [Test]
        public async Task VerifyGetFixesWhenToleranceExistsWithMessage()
        {
            await this.VerifyGetFixes<ClassicModelAssertUsageAnalyzer, AreEqualClassicModelAssertUsageCodeFix>(
                $"{AreEqualClassicModelAssertUsageCodeFixTests.BasePath}{(nameof(this.VerifyGetFixesWhenToleranceExistsWithMessage))}.cs",
                1, CodeFixConstants.TransformToConstraintModelDescription,
                new[] { "That(", "Is.EqualTo(2d).Within(0.0000001d)" }.ToImmutableArray());
        }

        [Test]
        public async Task VerifyGetFixesWhenToleranceExistsWithMessageAndParams()
        {
            await this.VerifyGetFixes<ClassicModelAssertUsageAnalyzer, AreEqualClassicModelAssertUsageCodeFix>(
                $"{AreEqualClassicModelAssertUsageCodeFixTests.BasePath}{(nameof(this.VerifyGetFixesWhenToleranceExistsWithMessageAndParams))}.cs",
                1, CodeFixConstants.TransformToConstraintModelDescription,
                new[] { "That(", "Is.EqualTo(2d).Within(0.0000001d)" }.ToImmutableArray());
        }
    }
}
