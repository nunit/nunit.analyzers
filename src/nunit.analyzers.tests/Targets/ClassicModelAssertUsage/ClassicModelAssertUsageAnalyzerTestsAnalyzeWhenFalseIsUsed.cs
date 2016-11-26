using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.ClassicModelAssertUsage
{
	public sealed class ClassicModelAssertUsageAnalyzerTestsAnalyzeWhenFalseIsUsed
	{
		public void Test()
		{
			Assert.False(false);
		}
	}
}