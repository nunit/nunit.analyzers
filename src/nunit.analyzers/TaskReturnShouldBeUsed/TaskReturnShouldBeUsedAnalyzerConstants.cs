namespace NUnit.Analyzers.TaskReturnShouldBeUsed
{
    internal static class TaskReturnShouldBeUsedAnalyzerConstants
    {
        public const string Title = "Task result of method should be used";
        public const string Message = "Method '{0}' returns a Task and is not being observed for completion or errors";
        public const string Description = "Methods that return a Task should have their results observed to ensure proper handling of completion and errors.";
    }
}
