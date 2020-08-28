namespace NUnit.Analyzers.Constants
{
    internal static class SameAsOnValueTypesConstants
    {
        internal const string Title = "Non-reference types for SameAs constraint.";
        internal const string Message = "The SameAs constraint always fails on value types as the actual and the expected value cannot be the same reference.";
        internal const string Description = "The SameAs constraint always fails on value types as the actual and the expected value cannot be the same reference.";
    }
}
