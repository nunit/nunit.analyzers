namespace NUnit.Analyzers.TestCaseSourceUsage
{
    internal static class TestCaseSourceUsageConstants
    {
        internal const string ConsiderNameOfInsteadOfStringConstantAnalyzerTitle = "The TestCaseSource should use nameof operator to specify target";
        internal const string ConsiderNameOfInsteadOfStringConstantMessage = "Consider using nameof({0}) instead of \"{1}\"";
        internal const string ConsiderNameOfInsteadOfStringConstantDescription = "The TestCaseSource should use nameof operator to specify target.";

        internal const string SourceTypeNotIEnumerableTitle = "The source type does not implement IEnumerable";
        internal const string SourceTypeNotIEnumerableMessage = "The source type '{0}' does not implement IEnumerable";
        internal const string SourceTypeNotIEnumerableDescription = "The source type must implement IEnumerable in order to provide test cases.";

        internal const string SourceTypeNoDefaultConstructorTitle = "The source type does not have a default constructor";
        internal const string SourceTypeNoDefaultConstructorMessage = "The source type '{0}' does not have a default constructor";
        internal const string SourceTypeNoDefaultConstructorDescription = "The source type must have a default constructor in order to provide test cases.";

        internal const string SourceIsNotStaticTitle = "The specified source is not static";
        internal const string SourceIsNotStaticMessage = "The specified source '{0}' is not static";
        internal const string SourceIsNotStaticDescription = "The specified source must be static.";

        internal const string MismatchInNumberOfParametersTitle = "The number of parameters provided by the TestCaseSource does not match the number of parameters in the target method";
        internal const string MismatchInNumberOfParametersMessage = "The TestCaseSource provides '{0}' parameter(s), but the target method expects '{1}' parameter(s)";
        internal const string MismatchInNumberOfParametersDescription = "The number of parameters provided by the TestCaseSource must match the number of parameters in the target method.";

        internal const string SourceDoesNotReturnIEnumerableTitle = "The source specified by the TestCaseSource does not return an IEnumerable or a type that implements IEnumerable";
        internal const string SourceDoesNotReturnIEnumerableMessage = "The TestCaseSource does not return an IEnumerable or a type that implements IEnumerable. Instead it returns a '{0}'.";
        internal const string SourceDoesNotReturnIEnumerableDescription = "The source specified by the TestCaseSource must return an IEnumerable or a type that implements IEnumerable.";

        internal const string TestCaseSourceSuppliesParametersTitle = "The TestCaseSource provides parameters to a source - field or property - that expects no parameters";
        internal const string TestCaseSourceSuppliesParametersMessage = "The TestCaseSource provides '{0}' parameter(s), but {1} cannot take parameters";
        internal const string TestCaseSourceSuppliesParametersDescription = "The TestCaseSource must not provide any parameters when the source is a field or a property.";
    }
}
