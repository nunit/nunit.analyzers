using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{
    public sealed class TestCaseUsageAnalyzerTestsAnalyzeWhenMethodHasOnlyParamsAndArgumentTypeIsCorrect
    {
        [TestCase("a")]
        public void Test(params string[] a) { }
    }
}
