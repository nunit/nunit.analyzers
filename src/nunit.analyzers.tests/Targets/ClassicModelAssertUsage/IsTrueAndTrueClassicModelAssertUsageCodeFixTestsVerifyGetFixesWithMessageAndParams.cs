using NUnit.Framework;
using System;

namespace NUnit.Analyzers.Tests.Targets.ClassicModelAssertUsage
{
	public sealed class IsTrueAndTrueClassicModelAssertUsageCodeFixTestsVerifyGetFixesWithMessageAndParams
	{
		public void Test()
		{
			Assert.IsTrue(true, "message", Guid.NewGuid());
			Assert.True(true, "message", Guid.NewGuid());
		}
	}
}