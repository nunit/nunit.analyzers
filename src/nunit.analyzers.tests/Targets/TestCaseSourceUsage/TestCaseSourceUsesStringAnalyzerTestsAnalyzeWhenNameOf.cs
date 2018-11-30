using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseSourceUsage
{
    class TestCaseSourceUsesStringAnalyzerTestsAnalyzeWhenNameOf
    {
        string Tests;

        [TestCaseSource(nameof(Tests))]
        public void Test()
        {
        }
    }
}
