namespace NUnit.Analyzers.Constants
{
    internal static class StringConstraintUsageConstants
    {
        internal const string ContainsTitle = "Use ContainsConstraint for better assertion messages in case of failure";
        internal const string StartsWithTitle = "Use StartsWithConstraint for better assertion messages in case of failure";
        internal const string EndsWithTitle = "Use EndsWithConstraint for better assertion messages in case of failure";
        internal const string Message = "Use {0} constraint instead of boolean method for better assertion messages in case of failure";
        internal const string Description = "Using constraints instead of boolean methods will lead to better assertion messages in case of failure.";
    }
}
