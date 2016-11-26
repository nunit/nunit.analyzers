namespace NUnit.Analyzers.Constants
{
	internal static class TestCaseUsageAnalyzerConstants
	{
		internal const string ExpectedResultCannotBeNullMessage = "The ExpectedResult value cannot be null for the method as it returns a value type";
		internal const string ExpectedResultTypeMismatchMessage = "The ExpectedResult type, {0}, does not match the return type of the method, {1}";
		internal const string NotEnoughArgumentsMessage = "There are not enough arguments provided from the TestCaseAttribute for the method";
		internal const string NullUsageMessage = "The argument at position {0} is null, which cannot be passed to the argument {1}, which is of type {2}";
		internal const string SpecifiedExpectedResultForVoidMethodMessage = "Cannot specify ExpectedResult when the method returns void";
		internal const string Title = "Find Incorrect TestCaseAttribute Usage";
		internal const string TooManyArgumentsMessage = "There are too many arguments provided from the TestCaseAttribute for the method";
		internal const string TypeMismatchMessage = "The type of argument at position {0}, {1}, does not match the type of the argument {2}, which is {3}";
	}
}
