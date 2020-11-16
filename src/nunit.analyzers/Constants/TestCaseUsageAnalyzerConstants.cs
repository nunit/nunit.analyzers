namespace NUnit.Analyzers.Constants
{
    internal static class TestCaseUsageAnalyzerConstants
    {
        internal const string NotEnoughArgumentsTitle = "The TestCaseAttribute provided too few arguments";
        internal const string NotEnoughArgumentsMessage = "The TestCaseAttribute provided too few arguments. Expected '{0}', but got '{1}'.";
        internal const string NotEnoughArgumentsDescription = "The number of arguments provided by a TestCaseAttribute must match the number of parameters of the method.";

        internal const string TooManyArgumentsTitle = "The TestCaseAttribute provided too many arguments";
        internal const string TooManyArgumentsMessage = "The TestCaseAttribute provided too many arguments. Expected '{0}', but got '{1}'.";
        internal const string TooManyArgumentsDescription = "The number of arguments provided by a TestCaseAttribute must match the number of parameters of the method.";

        internal const string ParameterTypeMismatchTitle = "The individual arguments provided by a TestCaseAttribute must match the type of the corresponding parameter of the method";
        internal const string ParameterTypeMismatchMessage = "The value of the argument at position '{0}' of type {1} cannot be assigned to the parameter '{2}' of type {3}";
        internal const string ParameterTypeMismatchDescription = "The individual arguments provided by a TestCaseAttribute must match the type of the corresponding parameter of the method.";
    }
}
