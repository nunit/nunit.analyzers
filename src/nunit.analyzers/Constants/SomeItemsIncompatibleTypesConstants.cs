namespace NUnit.Analyzers.Constants
{
    internal static class SomeItemsIncompatibleTypesConstants
    {
        public const string Title = "Wrong actual type used with the SomeItemsConstraint with EqualConstraint";
        public const string Message = "The '{0}' constraint cannot be used with actual argument of type '{1}' and expected argument of type '{2}'";
        public const string Description = "The SomeItemsConstraint with EqualConstraint requires the actual argument to be a collection where the element type can match the type of the expected argument.";
    }
}
