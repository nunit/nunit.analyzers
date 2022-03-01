namespace NUnit.Analyzers.ConstraintUsage
{
    internal static class ComparisonConstraintUsageConstants
    {
        internal const string Title = "Use ComparisonConstraint for better assertion messages in case of failure";
        internal const string Message = "Use {0} constraint instead of direct comparison for better assertion messages in case of failure";
        internal const string Description = "Using ComparisonConstraint will lead to better assertion messages in case of failure.";
    }
}
