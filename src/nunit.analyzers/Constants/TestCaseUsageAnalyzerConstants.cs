namespace NUnit.Analyzers.Constants
{
    internal static class TestCaseUsageAnalyzerConstants
    {
        internal const string NotEnoughArgumentsTitle = "Too few arguments provided by TestCaseAttribute.";
        internal const string NotEnoughArgumentsMessage = "There are not enough arguments provided by the TestCaseAttribute. Expected '{0}', but got '{1}'.";
        internal const string NotEnoughArgumentsDescription = "The number of arguments provided by a TestCaseAttribute must match the number of parameters of the method.";

        internal const string TooManyArgumentsTitle = "Too many arguments provided by TestCaseAttribute.";
        internal const string TooManyArgumentsMessage = "There are too many arguments provided by the TestCaseAttribute. Expected '{0}', but got '{1}'.";
        internal const string TooManyArgumentsDescription = "The number of arguments provided by a TestCaseAttribute must match the number of parameters of the method.";

        internal const string ParameterTypeMismatchTitle = "The individual arguments provided by a TestCaseAttribute must match the type of the matching parameter of the method.";
        internal const string ParameterTypeMismatchMessage = "The value of the argument at position '{0}' of type {1} cannot be assigned to the argument '{2}' of type {3}.";
        internal const string ParameterTypeMismatchDescription = "The individual arguments provided by a TestCaseAttribute must match the type of the matching parameter of the method.";
    }
}
