namespace NUnit.Analyzers.UseAssertMultiple
{
    internal static class UseAssertMultipleConstants
    {
        internal const string Title = "Use Assert.EnterMultipleScope or Assert.Multiple";
        internal const string Message = "Call independent Assert statements from inside an Assert.EnterMultipleScope or Assert.Multiple";
        internal const string Description = "Hosting Asserts inside an Assert.EnterMultipleScope or Assert.Multiple allows detecting more than one failure.";
    }
}
