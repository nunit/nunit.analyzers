using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.ClassicModelAssertUsage
{
	public sealed class IsFalseAndFalseClassicModelAssertUsageCodeFixTestsVerifyGetFixes
	{
		public void Test()
		{
			Assert.IsFalse(false);
			Assert.False(false);
		}
	}
}