using NUnit.Framework;
using System;

namespace NUnit.Analyzers.Tests.Targets.ClassicModelAssertUsage
{
  public sealed class IsFalseAndFalseClassicModelAssertUsageCodeFixTestsVerifyGetFixesWithMessageAndParams
  {
    public void Test()
    {
      Assert.IsFalse(false, "message", Guid.NewGuid());
      Assert.False(false, "message", Guid.NewGuid());
    }
  }
}
