using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseSourceUsage
{
    [TestFixture]
    class TestCaseSourceUsesStringAnalyzerTestsAnalyzeWhenMultipleUnrelatedAttributes
    {
        [Test]
        public void UnrelatedTest()
        {
        }

        [TestCaseSource("StringConstant")]
        public void Test()
        {
        }
    }
}
