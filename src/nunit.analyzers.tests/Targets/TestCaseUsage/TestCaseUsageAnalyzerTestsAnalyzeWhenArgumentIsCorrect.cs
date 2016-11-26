using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{
	public sealed class TestCaseUsageAnalyzerTestsAnalyzeWhenArgumentIsCorrect
	{
		[TestCase(2)]
		public void Test(int a) { }
	}
}