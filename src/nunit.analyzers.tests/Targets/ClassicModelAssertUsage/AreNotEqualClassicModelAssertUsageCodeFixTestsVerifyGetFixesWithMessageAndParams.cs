using NUnit.Framework;
using System;

namespace NUnit.Analyzers.Tests.Targets.ClassicModelAssertUsage
{
	public sealed class AreNotEqualClassicModelAssertUsageCodeFixTestsVerifyGetFixesWithMessageAndParams
	{
		public void Test()
		{
			Assert.AreNotEqual(2d, 2d, "message", Guid.NewGuid());
		}
	}
}