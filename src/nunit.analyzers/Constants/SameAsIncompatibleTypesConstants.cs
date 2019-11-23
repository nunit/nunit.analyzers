namespace NUnit.Analyzers
{
    internal class SameAsIncompatibleTypesConstants
    {
        internal static string Title = "Incompatible types for SameAs constraint.";
        internal static string Message = "Provided actual and expected arguments for SameAs constraint cannot have same type.";
        internal static string Description = "Provided actual and expected arguments cannot have same type, therefore SameAs assertion will always fail.";
    }
}
