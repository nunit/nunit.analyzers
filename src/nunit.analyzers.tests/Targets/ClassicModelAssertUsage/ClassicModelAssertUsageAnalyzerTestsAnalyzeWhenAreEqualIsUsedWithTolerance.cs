using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.ClassicModelAssertUsage
{
  public sealed class ClassicModelAssertUsageAnalyzerTestsAnalyzeWhenAreEqualIsUsedWithTolerance
  {
    public void Test()
    {
      Assert.AreEqual(2d, 2d, 0.0000001d);
    }
  }
}
