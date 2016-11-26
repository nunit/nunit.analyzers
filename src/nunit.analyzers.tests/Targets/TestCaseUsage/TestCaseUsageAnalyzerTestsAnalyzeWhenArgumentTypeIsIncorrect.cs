using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{
	public sealed class TestCaseUsageAnalyzerTestsAnalyzeWhenArgumentTypeIsIncorrect
	{
		[TestCase(2)]
		public void Test(char a) { }
	}
}