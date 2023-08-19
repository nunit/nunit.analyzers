namespace NUnit.Analyzers.DisposeFieldsInTearDown
{
    internal static class DisposeFieldsInTearDownConstants
    {
        internal const string FieldIsNotDisposedInTearDownTitle = "An IDisposable field should be Disposed in a TearDown method";
        internal const string FieldIsNotDisposedInTearDownDescription = "An IDisposable field should be Disposed in a TearDown method.";
        internal const string FieldIsNotDisposedInTearDownMessageFormat = "The field {0} should be Disposed in the {1} method";
    }
}
