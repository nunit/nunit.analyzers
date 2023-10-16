namespace NUnit.Analyzers.UpdateStringFormatSpecification
{
    internal static class UpdateStringFormatSpecificationConstants
    {
        internal const string ExpectedResultTypeMismatchTitle = "The type of the value specified via ExpectedResult must match the return type of the method";
        internal const string ExpectedResultTypeMismatchMessage = "The type of the value specified via ExpectedResult cannot match the method's return type '{0}'";
        internal const string ExpectedResultTypeMismatchDescription = "The type of the value specified via ExpectedResult must match the return type of the method. " +
            "Otherwise, this will lead to an error at run-time.";
    }
}
