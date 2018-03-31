using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{
  public sealed class TestCaseUsageAnalyzerTestsAnalyzeWhenMethodHasOnlyParamsAndNoArgumentsAreProvided
  {
    [TestCase]
    public void Test(params object[] a) { }
  }
}
