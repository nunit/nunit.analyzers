namespace NUnit.Analyzers.CollectionAssertUsage
{
    internal class CollectionAssertUsageConstants
    {
        internal const string CollectionAssertTitle = "Consider using Assert.That(...) instead of CollectionAssert(...)";
        internal const string CollectionAssertMessage = "Consider using the constraint model, Assert.That(...), instead of the classic model, CollectionAssert(...)";
        internal const string CollectionAssertDescription = "Consider using the constraint model, Assert.That(actual, {0}(expected)), instead of the classic model, CollectionAssert.{1}(expected, actual).";
    }
}
