namespace NUnit.Analyzers.StringConstraintWrongActualType
{
    internal static class StringConstraintWrongActualTypeConstants
    {
        public const string Title = "Wrong actual type used with String Constraint";
        public const string Message = "The '{0}' constraint cannot be used with actual argument of type '{1}'";
        public const string Description = "The type of the actual argument is not a string and hence cannot be used with a String Constraint.";
    }
}
