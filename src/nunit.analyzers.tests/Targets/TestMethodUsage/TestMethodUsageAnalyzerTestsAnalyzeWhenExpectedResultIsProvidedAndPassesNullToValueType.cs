using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestMethodUsage
{
    public sealed class TestMethodUsageUsageAnalyzerTestsAnalyzeWhenExpectedResultIsProvidedAndPassesNullToValueType
    {
        [TestCase(2, ExpectedResult = null)]
        public int Test(int a) { return 3; }
    }
}
