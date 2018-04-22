using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{
    public sealed class TestCaseUsageAnalyzerTestsAnalyzeWhenArgumentPassesNullToValueType
    {
        [TestCase(null)]
        public void Test(char a) { }
    }
}
