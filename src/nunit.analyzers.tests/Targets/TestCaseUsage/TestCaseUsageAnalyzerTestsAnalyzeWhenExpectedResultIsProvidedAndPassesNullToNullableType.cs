using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{
  public sealed class TestCaseUsageAnalyzerTestsAnalyzeWhenExpectedResultIsProvidedAndPassesNullToNullableType
  {
    [TestCase(2, ExpectedResult = null)]
    public int? Test(int a) { return null; }
  }
}
