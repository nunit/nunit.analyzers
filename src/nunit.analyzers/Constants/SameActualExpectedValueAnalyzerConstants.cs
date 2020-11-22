namespace NUnit.Analyzers.Constants
{
    internal static class SameActualExpectedValueAnalyzerConstants
    {
        internal const string Title = "The same value has been provided as both the actual and the expected argument";
        internal const string Message = "The actual and the expected argument is the same '{0}'";
        internal const string Description = "The same value has been provided as both the actual and the expected argument. " +
            "This indicates a coding error.";
    }
}
