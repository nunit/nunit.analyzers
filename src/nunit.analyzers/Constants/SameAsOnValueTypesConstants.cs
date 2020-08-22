namespace NUnit.Analyzers.Constants
{
    internal static class SameAsOnValueTypesConstants
    {
        internal static readonly string Title = "Non-reference types for SameAs constraint.";
        internal static readonly string Message = "The SameAs constraint always fails on value types as the actual and the expected value cannot be the same reference.";
        internal static readonly string Description = "The SameAs constraint always fails on value types as the actual and the expected value cannot be the same reference.";
    }
}
