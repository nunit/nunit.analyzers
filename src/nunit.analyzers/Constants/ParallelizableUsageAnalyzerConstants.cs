namespace NUnit.Analyzers.Constants
{
    class ParallelizableUsageAnalyzerConstants
    {
        internal const string ParallelScopeSelfNoEffectOnAssemblyTitle = "Specifying ParallelScope.Self on assembly level has no effect.";
        internal const string ParallelScopeSelfNoEffectOnAssemblyMessage = "Specifying ParallelScope.Self on assembly level has no effect.";
        internal const string ParallelScopeSelfNoEffectOnAssemblyDescription = "Specifying ParallelScope.Self on assembly level has no effect.";

        internal const string ParallelScopeChildrenOnNonParameterizedTestMethodTitle = "One may not specify ParallelScope.Children on a non-parameterized test method.";
        internal const string ParallelScopeChildrenOnNonParameterizedTestMethodMessage = "One may not specify ParallelScope.Children on a non-parameterized test method.";
        internal const string ParallelScopeChildrenOnNonParameterizedTestMethodDescription = "One may not specify ParallelScope.Children on a non-parameterized test method.";

        internal const string ParallelScopeFixturesOnTestMethodTitle = "One may not specify ParallelScope.Fixtures on a test method.";
        internal const string ParallelScopeFixturesOnTestMethodMessage = "One may not specify ParallelScope.Fixtures on a test method.";
        internal const string ParallelScopeFixturesOnTestMethodDescription = "One may not specify ParallelScope.Fixtures on a test method.";

        internal class ParallelScope
        {
            internal const int Self = 1;
            internal const int Children = 256;
            internal const int Fixtures = 512;
        }
    }
}
