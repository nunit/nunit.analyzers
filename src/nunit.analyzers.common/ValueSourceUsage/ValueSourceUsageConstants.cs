namespace NUnit.Analyzers.ValueSourceUsage
{
    internal static class ValueSourceUsageConstants
    {
        internal const string SourceDoesNotSpecifyAnExistingMemberTitle = "The ValueSource argument does not specify an existing member";
        internal const string SourceDoesNotSpecifyAnExistingMemberMessage = "The ValueSource argument '{0}' does not specify an existing member";
        internal const string SourceDoesNotSpecifyAnExistingMemberDescription = "The ValueSource argument does not specify an existing member. This will lead to an error at run-time.";

        internal const string ConsiderNameOfInsteadOfStringConstantAnalyzerTitle = "The ValueSource should use nameof operator to specify target";
        internal const string ConsiderNameOfInsteadOfStringConstantMessage = "Consider using nameof({0}) instead of \"{1}\"";
        internal const string ConsiderNameOfInsteadOfStringConstantDescription = "The ValueSource should use nameof operator to specify target.";

        internal const string SourceIsNotStaticTitle = "The specified source is not static";
        internal const string SourceIsNotStaticMessage = "The specified source '{0}' is not static";
        internal const string SourceIsNotStaticDescription = "The specified source must be static.";

        internal const string MethodExpectParametersTitle = "The target method expects parameters which cannot be supplied by the ValueSource";
        internal const string MethodExpectParametersMessage = "The ValueSource cannot supply parameters, but the target method expects '{0}' parameter(s)";
        internal const string MethodExpectParametersDescription = "The target method expects parameters which cannot be supplied by the ValueSource.";

        internal const string SourceDoesNotReturnIEnumerableTitle = "The source specified by the ValueSource does not return an IEnumerable or a type that implements IEnumerable";
        internal const string SourceDoesNotReturnIEnumerableMessage = "The ValueSource does not return an IEnumerable or a type that implements IEnumerable. Instead it returns a '{0}'.";
        internal const string SourceDoesNotReturnIEnumerableDescription = "The source specified by the ValueSource must return an IEnumerable or a type that implements IEnumerable.";
    }
}
