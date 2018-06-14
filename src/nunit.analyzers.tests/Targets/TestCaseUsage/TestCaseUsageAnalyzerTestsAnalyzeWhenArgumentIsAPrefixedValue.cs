using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{
    class TestCaseUsageAnalyzerTestsAnalyzeWhenArgumentIsAPrefixedValue
    {
        [TestCase(-2)]
        public void Test(int a) { }
    }
}
