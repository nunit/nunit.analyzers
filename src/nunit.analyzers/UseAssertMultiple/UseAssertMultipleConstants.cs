namespace NUnit.Analyzers.UseAssertMultiple
{
    internal static class UseAssertMultipleConstants
    {
        internal const string Title = "Use Assert.Multiple";
        internal const string Message = "Call independent Assert statements from inside an Assert.Multiple";
        internal const string Description = "Hosting Asserts inside an Assert.Multiple allows detecting more than one failure.";
    }
}
