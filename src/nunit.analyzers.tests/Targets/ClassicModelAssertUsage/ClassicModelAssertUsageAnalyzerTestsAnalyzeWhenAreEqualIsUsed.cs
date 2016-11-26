using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.ClassicModelAssertUsage
{
	public sealed class ClassicModelAssertUsageAnalyzerTestsAnalyzeWhenAreEqualIsUsed
	{
		public void Test()
		{
			Assert.AreEqual(2, 2);
		}
	}
}