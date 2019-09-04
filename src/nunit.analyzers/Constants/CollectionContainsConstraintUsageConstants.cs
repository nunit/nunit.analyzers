namespace NUnit.Analyzers.Constants
{
    internal static class CollectionContainsConstraintUsageConstants
    {
        internal const string Title = "Use SomeItemsConstraint.";
        internal const string Message = "Use {0} constraint instead of direct comparison.";
        internal const string Description = "Using SomeItemsConstraint will lead to better assertion messages in case of failure.";
    }
}
