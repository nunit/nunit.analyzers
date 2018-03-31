using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.ClassicModelAssertUsage
{
  public sealed class AreNotEqualClassicModelAssertUsageCodeFixTestsVerifyGetFixesWhenToleranceDoesNotExist
  {
    public void Test()
    {
      Assert.AreNotEqual(2d, 2d, "message");
    }
  }
}
