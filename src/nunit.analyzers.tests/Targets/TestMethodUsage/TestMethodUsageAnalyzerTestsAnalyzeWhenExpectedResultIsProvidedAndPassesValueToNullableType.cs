using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestMethodUsage
{
    public sealed class TestMethodUsageAnalyzerTestsAnalyzeWhenExpectedResultIsProvidedAndPassesValueToNullableType
    {
        [TestCase(2, ExpectedResult = 2)]
        public int? Test(int a) { return 2; }
    }
}
