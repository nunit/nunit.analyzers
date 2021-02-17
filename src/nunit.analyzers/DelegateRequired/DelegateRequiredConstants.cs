namespace NUnit.Analyzers.DelegateRequired
{
    internal static class DelegateRequiredConstants
    {
        internal const string Title = "Non-delegate actual parameter";
        internal const string Message = "The type of the actual argument must be a delegate";
        internal const string Description = "The actual argument needs to be evaluated by the Assert to catch any exceptions.";
    }
}
