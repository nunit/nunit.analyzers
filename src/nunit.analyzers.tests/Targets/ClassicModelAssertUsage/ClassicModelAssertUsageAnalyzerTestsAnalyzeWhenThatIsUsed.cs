using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.ClassicModelAssertUsage
{
	public sealed class ClassicModelAssertUsageAnalyzerTestsAnalyzeWhenThatIsUsed
	{
		public void Test()
		{
			Assert.That(true, Is.True);
		}
	}
}