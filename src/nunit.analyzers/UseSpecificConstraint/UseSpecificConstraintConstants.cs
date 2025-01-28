namespace NUnit.Analyzers.UseSpecificConstraint
{
    internal static class UseSpecificConstraintConstants
    {
        internal const string UseSpecificConstraintTitle = "Use Specific constraint";
        internal const string UseSpecificConstraintMessage = "Replace 'Is.EqualTo({0})' with 'Is.{1}' constraint";
        internal const string UseSpecificConstraintDescription = "Replace 'EqualTo' with a keyword in the corresponding specific constraint.";
    }
}
