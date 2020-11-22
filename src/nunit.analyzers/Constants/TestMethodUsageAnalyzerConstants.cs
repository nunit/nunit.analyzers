namespace NUnit.Analyzers.Constants
{
    internal static class TestMethodUsageAnalyzerConstants
    {
        internal const string ExpectedResultTypeMismatchTitle = "The type of the value specified via ExpectedResult must match the return type of the method";
        internal const string ExpectedResultTypeMismatchMessage = "The type of the value specified via ExpectedResult cannot match the method's return type '{0}'";
        internal const string ExpectedResultTypeMismatchDescription = "The type of the value specified via ExpectedResult must match the return type of the method. " +
            "Otherwise, this will lead to an error at run-time.";

        internal const string SpecifiedExpectedResultForVoidMethodTitle = "ExpectedResult must not be specified when the method returns void";
        internal const string SpecifiedExpectedResultForVoidMethodMessage = "Don't specify ExpectedResult when the method returns void";
        internal const string SpecifiedExpectedResultForVoidMethodDescription = "ExpectedResult must not be specified when the method returns void. This will lead to an error at run-time.";

        internal const string NoExpectedResultButNonVoidReturnTypeTitle = "The method has non-void return type, but no result is expected in ExpectedResult";
        internal const string NoExpectedResultButNonVoidReturnTypeMessage = "The method has non-void return type '{0}', but no result is expected in ExpectedResult";
        internal const string NoExpectedResultButNonVoidReturnTypeDescription = "The method has non-void return type, but no result is expected in ExpectedResult.";

        internal const string AsyncNoExpectedResultAndVoidReturnTypeTitle = "The async test method must have a non-void return type";
        internal const string AsyncNoExpectedResultAndVoidReturnTypeMessage = "The async test method must have a non-void return type";
        internal const string AsyncNoExpectedResultAndVoidReturnTypeDescription = "The async test method must have a non-void return type.";

        internal const string AsyncNoExpectedResultAndNonTaskReturnTypeTitle = "The async test method must have a non-generic Task return type when no result is expected";
        internal const string AsyncNoExpectedResultAndNonTaskReturnTypeMessage = "The async test method must have a non-generic Task return type when no result is expected, but the return type was '{0}'";
        internal const string AsyncNoExpectedResultAndNonTaskReturnTypeDescription = "The async test method must have a non-generic Task return type when no result is expected.";

        internal const string AsyncExpectedResultAndNonGenericTaskReturnTypeTitle = "The async test method must have a Task<T> return type when a result is expected";
        internal const string AsyncExpectedResultAndNonGenericTaskReturnTypeMessage = "The async test method must have a Task<T> return type when a result is expected, but the return type was '{0}'";
        internal const string AsyncExpectedResultAndNonGenericTaskReturnTypeDescription = "The async test method must have a Task<T> return type when a result is expected.";

        internal const string SimpleTestMethodHasParametersTitle = "The test method has parameters, but no arguments are supplied by attributes";
        internal const string SimpleTestMethodHasParametersMessage = "The test method has '{0}' parameter(s), but only '{1}' argument(s) are supplied by attributes";
        internal const string SimpleTestMethodHasParametersDescription = "The test method has parameters, but no arguments are supplied by attributes.";
    }
}
