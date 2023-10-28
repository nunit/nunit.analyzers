namespace NUnit.Analyzers.ConstActualValueUsage
{
    internal static class ConstActualValueUsageAnalyzerConstants
    {
        public const string Title = "The actual value should not be a constant";
        public const string Message = "The actual value should not be a constant - " +
            "perhaps the actual value and the expected value have switched places";
        public const string Description = "The actual value should not be a constant. " +
            "This indicates that the actual value and the expected value have switched places.";
    }
}
