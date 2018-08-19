namespace NUnit.Analyzers.Constants
{
    internal static class TestCaseUsageAnalyzerConstants
    {
        internal const string ExpectedResultTypeMismatchMessage = "The ExpectedResult value cannot be assigned to the return type, {0}";
        internal const string NotEnoughArgumentsMessage = "There are not enough arguments provided from the TestCaseAttribute for the method";
        internal const string SpecifiedExpectedResultForVoidMethodMessage = "Cannot specify ExpectedResult when the method returns void";
        internal const string AsyncVoidMessage = "Async test method must have non-void return type";
        internal const string Title = "Find Incorrect TestCaseAttribute Usage";
        internal const string TooManyArgumentsMessage = "There are too many arguments provided from the TestCaseAttribute for the method";
        internal const string ParameterTypeMismatchMessage = "The value of the argument at position {0} cannot be assigned to the argument {1}";
    }
}
