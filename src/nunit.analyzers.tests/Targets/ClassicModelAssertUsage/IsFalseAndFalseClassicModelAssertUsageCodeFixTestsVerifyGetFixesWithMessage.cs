using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.ClassicModelAssertUsage
{
  public sealed class IsFalseAndFalseClassicModelAssertUsageCodeFixTestsVerifyGetFixesWithMessage
  {
    public void Test()
    {
      Assert.IsFalse(false, "message");
      Assert.False(false, "message");
    }
  }
}
