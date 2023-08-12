namespace NUnit.Analyzers.ValuesUsage
{
    internal static class ValuesUsageAnalyzerConstants
    {
        internal const string ParameterTypeMismatchTitle = "The individual arguments provided by a ValuesAttribute must match the type of the corresponding parameter of the method";
        internal const string ParameterTypeMismatchMessage = "The value of the argument at position '{0}' of type {1} cannot be assigned to the parameter '{2}' of type {3}";
        internal const string ParameterTypeMismatchDescription = "The individual arguments provided by a ValuesAttribute must match the type of the corresponding parameter of the method.";
    }
}
