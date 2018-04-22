using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.ClassicModelAssertUsage
{
    public sealed class IsTrueAndTrueClassicModelAssertUsageCodeFixTestsVerifyGetFixes
    {
        public void Test()
        {
            Assert.IsTrue(true);
            Assert.True(true);
        }
    }
}
