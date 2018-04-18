using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{
    public sealed class TestCaseUsageAnalyzerTestsAnalyzeWhenTooManyRequiredAndOptionalArgumentsAreProvided
    {
        [TestCase(2, 'b', 2d)]
        public void Test(int a, char b = 'c') { }
    }
}
