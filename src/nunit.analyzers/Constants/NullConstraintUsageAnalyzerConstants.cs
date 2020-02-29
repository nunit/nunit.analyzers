namespace NUnit.Analyzers.Constants
{
    internal static class NullConstraintUsageAnalyzerConstants
    {
        internal const string Title = "Invalid NullConstraint usage.";
        internal const string Message = "Actual argument of type '{0}' can never be null.";
        internal const string Description = "NullConstraint is allowed only for reference types or nullable value types.";
    }
}
