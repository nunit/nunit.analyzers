using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.ClassicModelAssertUsage
{
    public sealed class AreEqualClassicModelAssertUsageCodeFixTestsVerifyGetFixesWhenToleranceExistsWithMessage
    {
        public void Test()
        {
            Assert.AreEqual(2d, 2d, 0.0000001d, "message");
        }
    }
}
