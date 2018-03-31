using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.ClassicModelAssertUsage
{
  public sealed class ClassicModelAssertUsageAnalyzerTestsAnalyzeWhenTrueIsUsed
  {
    public void Test()
    {
      Assert.True(true);
    }
  }
}
