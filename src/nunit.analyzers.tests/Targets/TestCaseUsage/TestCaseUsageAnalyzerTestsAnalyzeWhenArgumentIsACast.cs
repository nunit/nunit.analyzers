using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{
    class TestCaseUsageAnalyzerTestsAnalyzeWhenArgumentIsACast
    {
        [TestCase((byte)2)]
        public void Test(byte a) { }
    }
}
