using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.ClassicModelAssertUsage
{
  public sealed class AreNotEqualClassicModelAssertUsageCodeFixTestsVerifyGetFixes
  {
    public void Test()
    {
      Assert.AreNotEqual(2d, 2d);
    }
  }
}
