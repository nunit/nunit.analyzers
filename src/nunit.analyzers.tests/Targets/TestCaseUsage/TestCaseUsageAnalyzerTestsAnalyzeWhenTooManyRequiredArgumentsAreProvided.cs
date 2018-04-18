using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{
    public sealed class TestCaseUsageAnalyzerTestsAnalyzeWhenTooManyRequiredArgumentsAreProvided
    {
        [TestCase(2, 'b')]
        public void Test(int a) { }
    }
}
