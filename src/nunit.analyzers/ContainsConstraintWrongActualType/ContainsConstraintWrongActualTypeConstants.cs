namespace NUnit.Analyzers.ContainsConstraintWrongActualType
{
    internal static class ContainsConstraintWrongActualTypeConstants
    {
        public const string Title = "Wrong actual type used with ContainsConstraint";
        public const string Message = "The ContainsConstraint cannot be used with an actual value of type '{0}'";
        public const string Description = "The ContainsConstraint requires the type of the actual value to be either a string or a collection of strings.";
    }
}
