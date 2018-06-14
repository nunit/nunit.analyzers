using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{
    class TestCaseUsageAnalyzerTestsAnalyzeWhenArgumentIsAReferenceToConstant
    {
        const int value = 42;

        [TestCase(value)]
        public void Test(int a) { }
    }
}
