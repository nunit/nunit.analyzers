using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{
    public sealed class TestCaseUsageAnalyzerTestsAnalyzeWhenExpectedResultIsProvidedCorrectly
    {
        [TestCase(2, ExpectedResult = 3)]
        public int Test(int a) { return 3; }
    }
}
