using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{
  public sealed class TestCaseUsageAnalyzerTestsAnalyzeWhenArgumentPassesNullToNullableType
  {
    [TestCase(null)]
    public void Test(int? a) { }
  }
}
