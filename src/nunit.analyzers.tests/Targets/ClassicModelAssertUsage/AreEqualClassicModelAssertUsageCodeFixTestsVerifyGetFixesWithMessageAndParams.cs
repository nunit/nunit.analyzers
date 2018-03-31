using NUnit.Framework;
using System;

namespace NUnit.Analyzers.Tests.Targets.ClassicModelAssertUsage
{
  public sealed class AreEqualClassicModelAssertUsageCodeFixTestsVerifyGetFixesWithMessageAndParams
  {
    public void Test()
    {
      Assert.AreEqual(2d, 2d, "message", Guid.NewGuid());
    }
  }
}
