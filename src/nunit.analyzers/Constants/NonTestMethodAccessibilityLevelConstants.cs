namespace NUnit.Analyzers.Constants
{
    internal static class NonTestMethodAccessibilityLevelConstants
    {
        internal const string NonTestMethodIsPublicTitle = "The non-test method is public";
        internal const string NonTestMethodIsPublicMessage = "Only test methods should be public";
        internal const string NonTestMethodIsPublicDescription = "A fixture should not contain any public non-test methods.";
    }
}
