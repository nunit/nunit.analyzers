namespace NUnit.Analyzers.WithinUsage
{
    internal static class WithinUsageAnalyzerConstants
    {
        internal const string Title = "Incompatible types for Within constraint";
        internal const string Message = "The Within constraint makes no sense as tolerance cannot be applied to values of compared types";
        internal const string Description = "The Within modifier should only be used for numeric or Date/Time arguments or tuples containing only these element types. Using it on other types will not have any effect.";
    }
}
