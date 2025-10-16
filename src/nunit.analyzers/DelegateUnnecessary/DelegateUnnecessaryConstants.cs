namespace NUnit.Analyzers.DelegateUnnecessary
{
    internal static class DelegateUnnecessaryConstants
    {
        internal const string Title = "Remove unnecessary lambda expression";
        internal const string Message = "The type of the actual argument does not have to be a delegate";
        internal const string Description = "Only the argument value is needed by the Assert.";
    }
}
