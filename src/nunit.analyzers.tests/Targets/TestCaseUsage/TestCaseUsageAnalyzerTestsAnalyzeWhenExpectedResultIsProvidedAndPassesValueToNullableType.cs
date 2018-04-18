using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{
    public sealed class TestCaseUsageAnalyzerTestsAnalyzeWhenExpectedResultIsProvidedAndPassesValueToNullableType
    {
        [TestCase(2, ExpectedResult = 2)]
        public int? Test(int a) { return 2; }
    }
}
