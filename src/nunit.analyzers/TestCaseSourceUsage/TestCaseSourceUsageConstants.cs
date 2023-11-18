namespace NUnit.Analyzers.TestCaseSourceUsage
{
    internal static class TestCaseSourceUsageConstants
    {
        internal const string ConsiderNameOfInsteadOfStringConstantAnalyzerTitle = "The TestCaseSource should use nameof operator to specify target";
        internal const string ConsiderNameOfInsteadOfStringConstantMessage = "Consider using nameof({0}) instead of \"{1}\"";
        internal const string ConsiderNameOfInsteadOfStringConstantDescription = "The TestCaseSource should use nameof operator to specify target.";

        internal const string SourceTypeNotIEnumerableTitle = "The source type does not implement I(Async)Enumerable";
        internal const string SourceTypeNotIEnumerableMessage = "The source type '{0}' does not implement I(Async)Enumerable";
        internal const string SourceTypeNotIEnumerableDescription = "The source type must implement I(Async)Enumerable in order to provide test cases.";

        internal const string SourceTypeNoDefaultConstructorTitle = "The source type does not have a default constructor";
        internal const string SourceTypeNoDefaultConstructorMessage = "The source type '{0}' does not have a default constructor";
        internal const string SourceTypeNoDefaultConstructorDescription = "The source type must have a default constructor in order to provide test cases.";

        internal const string SourceIsNotStaticTitle = "The specified source is not static";
        internal const string SourceIsNotStaticMessage = "The specified source '{0}' is not static";
        internal const string SourceIsNotStaticDescription = "The specified source must be static.";

        internal const string MismatchInNumberOfParametersTitle = "The number of parameters provided by the TestCaseSource does not match the number of parameters in the target method";
        internal const string MismatchInNumberOfParametersMessage = "The TestCaseSource provides '{0}' parameter(s), but the target method expects '{1}' parameter(s)";
        internal const string MismatchInNumberOfParametersDescription = "The number of parameters provided by the TestCaseSource must match the number of parameters in the target method.";

        internal const string SourceDoesNotReturnIEnumerableTitle = "The source specified by the TestCaseSource does not return an I(Async)Enumerable or a type that implements I(Async)Enumerable";
        internal const string SourceDoesNotReturnIEnumerableMessage = "The TestCaseSource does not return an I(Async)Enumerable or a type that implements I(Async)Enumerable. Instead it returns a '{0}'.";
        internal const string SourceDoesNotReturnIEnumerableDescription = "The source specified by the TestCaseSource must return an I(Async)Enumerable or a type that implements I(Async)Enumerable.";

        internal const string TestCaseSourceSuppliesParametersTitle = "The TestCaseSource provides parameters to a source - field or property - that expects no parameters";
        internal const string TestCaseSourceSuppliesParametersMessage = "The TestCaseSource provides '{0}' parameter(s), but {1} cannot take parameters";
        internal const string TestCaseSourceSuppliesParametersDescription = "The TestCaseSource must not provide any parameters when the source is a field or a property.";

        internal const string MismatchInNumberOfTestMethodParametersTitle = "The number of parameters provided by the TestCaseSource does not match the number of parameters in the Test method";
        internal const string MismatchInNumberOfTestMethodParametersMessage = "The TestCaseSource provides '{0}' parameter(s), but the Test method expects '{1}' parameter(s)";
        internal const string MismatchInNumberOfTestMethodParametersDescription = "The number of parameters provided by the TestCaseSource must match the number of parameters in the Test method.";

        internal const string MismatchWithTestMethodParameterTypeTitle = "The type of parameter provided by the TestCaseSource does not match the type of the parameter in the Test method";
        internal const string MismatchWithTestMethodParameterTypeMessage = "The TestCaseSource provides type '{0}', but the Test method expects type '{1}' for parameter '{2}'";
        internal const string MismatchWithTestMethodParameterTypeDescription = "The type of parameters provided by the TestCaseSource must match the type of parameters in the Test method.";
    }
}
