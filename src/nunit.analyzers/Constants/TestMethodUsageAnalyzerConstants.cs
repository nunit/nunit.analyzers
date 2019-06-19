namespace NUnit.Analyzers.Constants
{
    internal static class TestMethodUsageAnalyzerConstants
    {
        internal const string ExpectedResultTypeMismatchTitle = "The type of ExpectedResult must match the return type.";
        internal const string ExpectedResultTypeMismatchMessage = "The ExpectedResult value cannot be assigned to the return type '{0}'.";
        internal const string ExpectedResultTypeMismatchDescription = "The type of ExpectedResult must match the return type. This will lead to an error at run-time.";

        internal const string SpecifiedExpectedResultForVoidMethodTitle = "ExpectedResult must not be specified when the method returns void.";
        internal const string SpecifiedExpectedResultForVoidMethodMessage = "Don't specify ExpectedResult when the method returns void";
        internal const string SpecifiedExpectedResultForVoidMethodDescription= "ExpectedResult must not be specified when the method returns void. This will lead to an error at run-time.";

        internal const string NoExpectedResultButNonVoidReturnTypeTitle = "Method has non-void return type, but no result is expected in ExpectedResult.";
        internal const string NoExpectedResultButNonVoidReturnTypeMessage = "Method has non-void return type '{0}', but no result is expected in ExpectedResult.";
        internal const string NoExpectedResultButNonVoidReturnTypeDescription = "Method has non-void return type, but no result is expected in ExpectedResult.";

        internal const string AsyncNoExpectedResultAndVoidReturnTypeTitle = "Async test method must have non-void return type.";
        internal const string AsyncNoExpectedResultAndVoidReturnTypeMessage = "Async test method must have non-void return type.";
        internal const string AsyncNoExpectedResultAndVoidReturnTypeDescription = "Async test method must have non-void return type.";

        internal const string AsyncNoExpectedResultAndNonTaskReturnTypeTitle = "Async test method must have non-generic Task return type when no result is expected.";
        internal const string AsyncNoExpectedResultAndNonTaskReturnTypeMessage = "Async test method must have non-generic Task return type when no result is expected, but the return type was '{0}'.";
        internal const string AsyncNoExpectedResultAndNonTaskReturnTypeDescription = "Async test method must have non-generic Task return type when no result is expected.";

        internal const string AsyncExpectedResultAndNonGenericTaskReturnTypeTitle = "Async test method must have Task<T> return type when a result is expected";
        internal const string AsyncExpectedResultAndNonGenericTaskReturnTypeMessage = "Async test method must have Task<T> return type when a result is expected, but the return type was '{0}'.";
        internal const string AsyncExpectedResultAndNonGenericTaskReturnTypeDescription = "Async test method must have Task<T> return type when a result is expected";
    }
}
