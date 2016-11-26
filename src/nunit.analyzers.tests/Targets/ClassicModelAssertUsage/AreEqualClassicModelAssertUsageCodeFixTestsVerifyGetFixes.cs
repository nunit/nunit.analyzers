using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.ClassicModelAssertUsage
{
	public sealed class AreEqualClassicModelAssertUsageCodeFixTestsVerifyGetFixes
	{
		public void Test()
		{
			Assert.AreEqual(2d, 2d);
		}
	}
}