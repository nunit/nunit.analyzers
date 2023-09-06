namespace NUnit.Analyzers.DisposeFieldsInTearDown
{
    internal static class DisposeFieldsAndPropertiesInTearDownConstants
    {
        internal const string FieldOrPropertyIsNotDisposedInTearDownTitle = "An IDisposable field/property should be Disposed in a TearDown method";
        internal const string FieldOrPropertyIsNotDisposedInTearDownDescription = "An IDisposable field/property should be Disposed in a TearDown method.";
        internal const string FieldOrPropertyIsNotDisposedInTearDownMessageFormat = "The {0} {1} should be Disposed in a method annotated with [{2}]";
    }
}
