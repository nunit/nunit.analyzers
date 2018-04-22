using System.Collections.Immutable;
using System.Threading.Tasks;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class IsFalseAndFalseClassicModelAssertUsageCodeFixTests
        : CodeFixTests
    {
        private static readonly string BasePath =
            $@"{TestContext.CurrentContext.TestDirectory}\Targets\ClassicModelAssertUsage\{nameof(IsFalseAndFalseClassicModelAssertUsageCodeFixTests)}";

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new IsFalseAndFalseClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds.ToImmutableArray();

            Assert.That(ids.Length, Is.EqualTo(2), nameof(ids.Length));
            Assert.That(ids, Contains.Item(AnalyzerIdentifiers.IsFalseUsage),
                nameof(AnalyzerIdentifiers.IsFalseUsage));
            Assert.That(ids, Contains.Item(AnalyzerIdentifiers.FalseUsage),
                nameof(AnalyzerIdentifiers.FalseUsage));
        }

        [Test]
        public async Task VerifyGetFixes()
        {
            await this.VerifyGetFixes<ClassicModelAssertUsageAnalyzer, IsFalseAndFalseClassicModelAssertUsageCodeFix>(
                $"{IsFalseAndFalseClassicModelAssertUsageCodeFixTests.BasePath}{(nameof(this.VerifyGetFixes))}.cs",
                1, CodeFixConstants.TransformToConstraintModelDescription,
                new[] { "That", ", Is.False" }.ToImmutableArray());
        }

        [Test]
        public async Task VerifyGetFixesWithMessage()
        {
            await this.VerifyGetFixes<ClassicModelAssertUsageAnalyzer, IsFalseAndFalseClassicModelAssertUsageCodeFix>(
                $"{IsFalseAndFalseClassicModelAssertUsageCodeFixTests.BasePath}{(nameof(this.VerifyGetFixesWithMessage))}.cs",
                1, CodeFixConstants.TransformToConstraintModelDescription,
                new[] { "That", "Is.False, " }.ToImmutableArray());
        }

        [Test]
        public async Task VerifyGetFixesWithMessageAndParams()
        {
            await this.VerifyGetFixes<ClassicModelAssertUsageAnalyzer, IsFalseAndFalseClassicModelAssertUsageCodeFix>(
                $"{IsFalseAndFalseClassicModelAssertUsageCodeFixTests.BasePath}{(nameof(this.VerifyGetFixesWithMessageAndParams))}.cs",
                1, CodeFixConstants.TransformToConstraintModelDescription,
                new[] { "That", "Is.False, " }.ToImmutableArray());
        }
    }
}
