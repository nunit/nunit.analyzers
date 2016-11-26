using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{
	public sealed class TestCaseUsageAnalyzerTestsAnalyzeWhenExpectedResultIsProvidedAndReturnTypeIsVoid
	{
		[TestCase(2, ExpectedResult = '3')]
		public void Test(int a) { }
	}
}