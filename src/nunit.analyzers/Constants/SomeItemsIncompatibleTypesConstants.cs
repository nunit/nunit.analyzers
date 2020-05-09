namespace NUnit.Analyzers.Constants
{
    internal static class SomeItemsIncompatibleTypesConstants
    {
        public const string Title = "Wrong actual type used with SomeItemsConstraint.";
        public const string Message = "The SomeItemsConstraint cannot be used with '{0}' actual and '{1}' expected arguments.";
        public const string Description = "The SomeItemsConstraint requires actual argument to be a collection with matching element type to the expected argument.";
    }
}
