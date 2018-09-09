using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestMethodUsage
{
    public sealed class TestMethodUsageAnalyzerTestsAnalyzeWhenExpectedResultIsProvidedAndReturnTypeIsVoid
    {
        [TestCase(2, ExpectedResult = '3')]
        public void Test(int a) { }
    }
}
