using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestMethodUsage
{
    public sealed class TestMethodUsageAnalyzerTestsAnalyzeWhenExpectedResultIsProvidedAndPassesNullToNullableType
    {
        [TestCase(2, ExpectedResult = null)]
        public int? Test(int a) { return null; }
    }
}
