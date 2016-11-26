using NUnit.Framework;
using System;

namespace NUnit.Analyzers.Tests.Targets.ClassicModelAssertUsage
{
	public sealed class AreEqualClassicModelAssertUsageCodeFixTestsVerifyGetFixesWhenToleranceExistsWithMessageAndParams
	{
		public void Test()
		{
			Assert.AreEqual(2d, 2d, 0.0000001d, "message", Guid.NewGuid());
		}
	}
}