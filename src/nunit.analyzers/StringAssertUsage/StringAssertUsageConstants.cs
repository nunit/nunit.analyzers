namespace NUnit.Analyzers.StringAssertUsage
{
    internal class StringAssertUsageConstants
    {
        internal const string StringAssertTitle = "Consider using Assert.That(...) instead of StringAssert(...)";
        internal const string StringAssertMessage = "Consider using the constraint model, Assert.That(...), instead of the classic model, StringAssert(...)";
        internal const string StringAssertDescription = "Consider using the constraint model, Assert.That(actual, {0}(expected)), instead of the classic model, StringAssert.{1}(expected, actual).";
    }
}
