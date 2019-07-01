namespace NUnit.Analyzers.Constants
{
    internal static class AnalyzerIdentifiers
    {
        #region Structure

        internal const string TestCaseParameterTypeMismatchUsage = "NUnit1001";
        internal const string TestCaseSourceStringUsage = "NUnit1002";
        internal const string TestCaseNotEnoughArgumentsUsage = "NUnit1003";
        internal const string TestCaseTooManyArgumentsUsage = "NUnit1004";
        internal const string TestMethodExpectedResultTypeMismatchUsage = "NUnit1005";
        internal const string TestMethodSpecifiedExpectedResultForVoidUsage = "NUnit1006";
        internal const string TestMethodNoExpectedResultButNonVoidReturnType = "NUnit1007";
        internal const string ParallelScopeSelfNoEffectOnAssemblyUsage = "NUnit1008";
        internal const string ParallelScopeChildrenOnNonParameterizedTestMethodUsage = "NUnit1009";
        internal const string ParallelScopeFixturesOnTestMethodUsage = "NUnit1010";
        internal const string TestCaseSourceIsMissing = "NUnit1011";
        internal const string TestMethodAsyncNoExpectedResultAndVoidReturnTypeUsage = "NUnit1012";
        internal const string TestMethodAsyncNoExpectedResultAndNonTaskReturnTypeUsage = "NUnit1013";
        internal const string TestMethodAsyncExpectedResultAndNonGenricTaskReturnTypeUsage = "NUnit1014";

        #endregion Structure

        #region Assertion

        internal const string FalseUsage = "NUnit2001";
        internal const string IsFalseUsage = "NUnit2002";
        internal const string IsTrueUsage = "NUnit2003";
        internal const string TrueUsage = "NUnit2004";
        internal const string AreEqualUsage = "NUnit2005";
        internal const string AreNotEqualUsage = "NUnit2006";
        internal const string ConstActualValueUsage = "NUnit2007";
        internal const string IgnoreCaseUsage = "NUnit2008";
        internal const string SameActualExpectedValue = "NUnit2009";
        internal const string IsEqualToConstraintUsage = "NUnit2010";
        internal const string IsNotEqualToConstraintUsage = "NUnit2011";

        #endregion Assertion
    }
}
