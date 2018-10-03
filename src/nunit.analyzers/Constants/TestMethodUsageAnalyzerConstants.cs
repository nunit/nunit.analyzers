namespace NUnit.Analyzers.Constants
{
    internal static class TestMethodUsageAnalyzerConstants
    {
        internal const string ExpectedResultTypeMismatchMessage = "The ExpectedResult value cannot be assigned to the return type, {0}";
        internal const string SpecifiedExpectedResultForVoidMethodMessage = "Cannot specify ExpectedResult when the method returns void";
        internal const string Title = "Find Incorrect TestAttribute or TestCaseAttribute Usage";
    }
}
