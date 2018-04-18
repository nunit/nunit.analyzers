using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.ClassicModelAssertUsage
{
    public sealed class ClassicModelAssertUsageAnalyzerTestsAnalyzeWhenDiagnosticIssuesExist
    {
        public void Test()
        {
            Assert.That(true,
            Assert.That(true, Is.True);
        }
    }
}
