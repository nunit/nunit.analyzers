namespace NUnit.Analyzers.Constants
{
    internal static class TestMethodUsageAnalyzerConstants
    {
        internal const string ExpectedResultTypeMismatchMessage = "The ExpectedResult value cannot be assigned to the return type, {0}";
        internal const string SpecifiedExpectedResultForVoidMethodMessage = "Cannot specify ExpectedResult when the method returns void";
        internal const string NoExpectedResultButNonVoidReturnType = "Method has non-void return type, but no result is expected in ExpectedResult";
        internal const string AsyncNoExpectedResultAndVoidReturnType = "Async test method must have non-void return type";
        internal const string AsyncNoExpectedResultAndNonTaskReturnType = "Async test method must have non-generic Task return type when no result is expected";

        internal const string Title = "Find Incorrect TestAttribute or TestCaseAttribute Usage";
    }
}
