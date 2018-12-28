namespace NUnit.Analyzers.Constants
{
    class ParallelizableUsageAnalyzerConstants
    {
        internal const string Title = "Find Incorrect ParallelizableAttribute Usage";
        internal const string ParallelScopeSelfNoEffectOnAssemblyMessage = "Specifying ParallelScope.Self on assembly level has no effect";
        internal const string ParallelScopeChildrenOnNonParameterizedTestMethodMessage = "One may not specify ParallelScope.Children on a non-parameterized test method";
        internal const string ParallelScopeFixturesOnTestMethodMessage = "One may not specify ParallelScope.Fixtures on a test method";

        internal class ParallelScope
        {
            internal const int Self = 1;
            internal const int Children = 256;
            internal const int Fixtures = 512;
        }
    }
}
