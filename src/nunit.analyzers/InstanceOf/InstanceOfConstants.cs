namespace NUnit.Analyzers.InstanceOf
{
    internal static class InstanceOfConstants
    {
        public const string Title = "Consider using Is.InstanceOf<T> constraint instead of an 'is T' expression";
        public const string Message = "Consider using Is.InstanceOf<{0}> instead of 'is {0}' expression for better assertion messages";
        public const string Description = "Consider using Is.InstanceOf<T> instead of 'is T' expression for better assertion messages.";
    }
}
