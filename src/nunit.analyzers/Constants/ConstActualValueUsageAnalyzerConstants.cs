namespace NUnit.Analyzers.Constants
{
    internal static class ConstActualValueUsageAnalyzerConstants
    {
        public const string Title = "Actual value should not be constant";
        public const string Message = "Actual value should not be constant - " +
            "perhaps the actual and expected values have switched places";
    }
}
