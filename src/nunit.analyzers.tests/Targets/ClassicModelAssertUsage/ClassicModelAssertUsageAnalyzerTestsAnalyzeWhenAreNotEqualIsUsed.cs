using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.ClassicModelAssertUsage
{
	public sealed class ClassicModelAssertUsageAnalyzerTestsAnalyzeWhenAreNotEqualIsUsed
	{
		public void Test()
		{
			Assert.AreNotEqual(2, 3);
		}
	}
}