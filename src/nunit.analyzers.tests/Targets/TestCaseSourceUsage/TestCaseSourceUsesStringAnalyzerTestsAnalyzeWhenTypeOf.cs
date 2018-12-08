using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseSourceUsage
{
    class TestCaseSourceUsesStringAnalyzerTestsAnalyzeWhenTypeOf
    {
        [TestCaseSource(typeof(MyTests))]
        public void Test()
        {
        }
    }

    class MyTests
    {
    }
}
