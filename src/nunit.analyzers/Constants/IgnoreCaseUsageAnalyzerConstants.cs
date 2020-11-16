namespace NUnit.Analyzers.Constants
{
    internal static class IgnoreCaseUsageAnalyzerConstants
    {
        internal const string Title = "Incorrect IgnoreCase usage";
        internal const string Message = "The IgnoreCase modifier should only be used for string or char arguments";
        internal const string Description = "The IgnoreCase modifier should only be used for string or char arguments. " +
            "Using it on another type will not have any effect.";
    }
}
