using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{
	public sealed class TestCaseUsageAnalyzerTestsAnalyzeWhenMethodHasRequiredAndParamsAndMoreArgumentsThanParametersAreProvided
	{
		[TestCase(1, 2, 3, 4)]
		public void Test(int a, int b, params int[] c) { }
	}
}