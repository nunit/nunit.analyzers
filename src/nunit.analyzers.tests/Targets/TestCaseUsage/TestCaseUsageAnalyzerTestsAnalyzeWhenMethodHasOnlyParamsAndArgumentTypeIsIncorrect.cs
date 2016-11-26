using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{
	public sealed class TestCaseUsageAnalyzerTestsAnalyzeWhenMethodHasOnlyParamsAndArgumentTypeIsIncorrect
	{
		[TestCase(2)]
		public void Test(params string[] a) { }
	}
}