namespace NUnit.Analyzers.Constants
{
    internal static class EqualToConstraintUsageConstants
    {
        internal const string IsEqualToTitle = "Use Is.EqualTo constraint.";
        internal const string IsEqualToMessage = "Use Is.EqualTo constraint instead of direct comparison.";
        internal const string IsEqualToDescription = "Using Is.EqualTo constraint will lead to better assertion messages in case of failure.";
        internal const string IsNotEqualToTitle = "Use Is.Not.EqualTo constraint.";
        internal const string IsNotEqualToMessage = "Use Is.Not.EqualTo constraint instead of direct comparison.";
        internal const string IsNotEqualToDescription = "Using Is.Not.EqualTo constraint will lead to better assertion messages in case of failure.";
    }
}
