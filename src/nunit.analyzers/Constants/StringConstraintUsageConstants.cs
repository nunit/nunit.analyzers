namespace NUnit.Analyzers.Constants
{
    internal static class StringConstraintUsageConstants
    {
        internal const string ContainsTitle = "Use ContainsConstraint.";
        internal const string StartsWithTitle = "Use StartsWithConstraint.";
        internal const string EndsWithTitle = "Use EndsWithConstraint.";
        internal const string Message = "Use {0} constraint instead of boolean method.";
        internal const string Description = "Using constraints instead of boolean methods will lead to better assertion messages in case of failure.";
    }
}
