using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{
  public sealed class TestCaseUsageAnalyzerTestsAnalyzeWhenAttributeHasNoArguments
  {
    [TestCase]
    public void ATest() { }
  }
}
