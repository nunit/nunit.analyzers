using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.ClassicModelAssertUsage
{
	public sealed class ClassicModelAssertUsageAnalyzerTestsAnalyzeWhenIsFalseIsUsed
	{
		public void Test()
		{
			Assert.IsFalse(false);
		}
	}
}