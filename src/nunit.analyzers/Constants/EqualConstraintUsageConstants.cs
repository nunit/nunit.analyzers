namespace NUnit.Analyzers.Constants
{
    internal static class EqualConstraintUsageConstants
    {
        internal const string Title = "Use EqualConstraint.";
        internal const string Message = "Use {0} constraint instead of direct comparison.";
        internal const string Description = "Using EqualConstraint will lead to better assertion messages in case of failure.";
    }
}
