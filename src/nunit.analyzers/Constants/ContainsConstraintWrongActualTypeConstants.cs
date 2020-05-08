namespace NUnit.Analyzers.Constants
{
    internal static class ContainsConstraintWrongActualTypeConstants
    {
        public const string Title = "Wrong actual type used with ContainsConstraint.";
        public const string Message = "The ContainsConstraint cannot be used with '{0}' actual argument.";
        public const string Description = "The ContainsConstraint constraint requires actual argument to be either string, or collection of strings.";
    }
}
