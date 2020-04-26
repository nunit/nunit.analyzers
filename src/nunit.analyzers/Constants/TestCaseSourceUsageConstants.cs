namespace NUnit.Analyzers.Constants
{
    internal static class TestCaseSourceUsageConstants
    {
        internal const string ConsiderNameOfInsteadOfStringConstantAnalyzerTitle = "TestCaseSource should use nameof operator to specify target.";
        internal const string ConsiderNameOfInsteadOfStringConstantMessage = "Consider using nameof({0}) instead of \"{0}\".";
        internal const string ConsiderNameOfInsteadOfStringConstantDescription = "TestCaseSource should use nameof operator to specify target.";

        internal const string SourceTypeNotINumerableTitle = "Source type does not implement IEnumerable.";
        internal const string SourceTypeNotINumerableMessage = "Source type '{0}' does not implement IEnumerable.";
        internal const string SourceTypeNotINumerableDescription = "The source type must implement IEnumerable in order to provide test cases.";

        internal const string SourceTypeNoDefaultConstructorTitle = "Source type does not have a default constructor.";
        internal const string SourceTypeNoDefaultConstructorMessage = "Source type '{0}' does not have a default constructor.";
        internal const string SourceTypeNoDefaultConstructorDescription = "The source type must have a default implement constructor in order to provide test cases.";

        internal const string SourceIsNotStaticTitle = "Specified source is not static.";
        internal const string SourceIsNotStaticMessage = "Specified source '{0}' is not static.";
        internal const string SourceIsNotStaticDescription = "The specified source must be static.";
    }
}
