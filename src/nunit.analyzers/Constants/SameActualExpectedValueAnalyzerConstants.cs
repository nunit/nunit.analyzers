namespace NUnit.Analyzers.Constants
{
    internal static class SameActualExpectedValueAnalyzerConstants
    {
        internal const string Title = "Same value provided as actual and expected argument.";
        internal const string Message = "Actual and expected arguments are the same '{0}'.";
        internal const string Description = "The same value has been provided as both actual and expected argument. " +
            "This indicates a coding error.";
    }
}
