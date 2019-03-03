namespace NUnit.Analyzers.Constants
{
    internal static class ConstActualValueUsageAnalyzerConstants
    {
        public const string Title = "Actual value should not be constant";
        public const string Message = "Actual value should not be constant - " +
            "possible that actual and expected values are switched places";
    }
}
