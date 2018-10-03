using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestMethodUsage
{
    public sealed class TestMethodUsageAnalyzerTestsAnalyzeWhenExpectedResultIsProvidedCorrectly
    {
        [TestCase(2, ExpectedResult = 3)]
        public int Test(int a) { return 3; }
    }
}
