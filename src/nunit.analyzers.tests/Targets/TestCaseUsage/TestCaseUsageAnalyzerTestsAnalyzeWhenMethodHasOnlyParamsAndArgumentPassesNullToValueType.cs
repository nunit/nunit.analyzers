using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{
	public sealed class TestCaseUsageAnalyzerTestsAnalyzeWhenMethodHasOnlyParamsAndArgumentPassesNullToValueType
	{
		[TestCase(null)]
		public void Test(params int[] a) { }
	}
}