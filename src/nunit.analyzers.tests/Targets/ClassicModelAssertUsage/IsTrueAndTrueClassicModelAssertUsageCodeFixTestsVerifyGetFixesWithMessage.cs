using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.ClassicModelAssertUsage
{
  public sealed class IsTrueAndTrueClassicModelAssertUsageCodeFixTestsVerifyGetFixesWithMessage
  {
    public void Test()
    {
      Assert.IsTrue(true, "message");
      Assert.True(true, "message");
    }
  }
}
