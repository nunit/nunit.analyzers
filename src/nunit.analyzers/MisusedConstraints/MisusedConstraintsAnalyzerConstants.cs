namespace NUnit.Analyzers.MisusedConstraints
{
    internal static class MisusedConstraintsAnalyzerConstants
    {
        public const string Title = "The constraint is misused";
        public const string Message = "The constraint might not be what is intended; {0}";
        public const string Description = "The constraint didn't take the operator priority into account.";
    }
}
