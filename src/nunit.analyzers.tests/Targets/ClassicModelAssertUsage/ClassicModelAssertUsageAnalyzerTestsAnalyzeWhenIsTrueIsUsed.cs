using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.ClassicModelAssertUsage
{
	public sealed class ClassicModelAssertUsageAnalyzerTestsAnalyzeWhenIsTrueIsUsed
	{
		public void Test()
		{
			Assert.IsTrue(true);
		}
	}
}