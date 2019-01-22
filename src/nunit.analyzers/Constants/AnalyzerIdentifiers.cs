namespace NUnit.Analyzers.Constants
{
    internal static class AnalyzerIdentifiers
    {
        internal const string TestCaseParameterTypeMismatchUsage = "NUNIT_7";
        internal const string TestCaseNotEnoughArgumentsUsage = "NUNIT_9";
        internal const string TestCaseTooManyArgumentsUsage = "NUNIT_10";
        internal const string TestMethodExpectedResultTypeMismatchUsage = "NUNIT_11";
        internal const string TestMethodSpecifiedExpectedResultForVoidUsage = "NUNIT_12";
        internal const string TestMethodNoExpectedResultButNonVoidReturnType = "NUNIT_13";
        internal const string ParallelScopeSelfNoEffectOnAssemblyUsage = "NUNIT_14";
        internal const string ParallelScopeChildrenOnNonParameterizedTestMethodUsage = "NUNIT_15";
        internal const string ParallelScopeFixturesOnTestMethodUsage = "NUNIT_16";
    }
}
