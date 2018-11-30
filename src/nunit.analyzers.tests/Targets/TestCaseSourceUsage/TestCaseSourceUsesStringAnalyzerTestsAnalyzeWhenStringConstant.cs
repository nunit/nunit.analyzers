using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseSourceUsage
{
    class TestCaseSourceUsesStringAnalyzerTestsAnalyzeWhenStringConstant
    {
        [TestCaseSource("Tests")]
        public void Test()
        {
        }
    }
}
