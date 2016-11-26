using NUnit.Framework;

namespace NUnit.Analyzers.IntegrationTests.TestCaseUsage
{
	public sealed class UsingTestCase
	{
		[TestCase(3d, "a value", 123, ExpectedResult = 22d)]
		public double RunTest(double a, string b, int c, char d = 'd')
		{
			return 22d;
		}

		[TestCase(2d, "3", ExpectedResult = "22")]
		public string RunTestWithParams(double a, params object[] values)
		{
			return "22";
		}

		[TestCase('a', 22, ExpectedResult = 22d)]
		public double RunTestWithOptionalsAndParams(char d = 'd', params int[] values)
		{
			return 22d;
		}
	}
}
