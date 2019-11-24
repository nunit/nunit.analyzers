namespace NUnit.Analyzers.Constants
{
    internal static class EqualToIncompatibleTypesConstants
    {
        internal static string Title = "Incompatible types for EqualTo constraint.";
        internal static string Message = "Provided actual and expected arguments for EqualTo constraint cannot be equal.";
        internal static string Description = "Provided actual and expected arguments cannot be equal, therefore EqualTo assertion will always fail.";
    }
}
