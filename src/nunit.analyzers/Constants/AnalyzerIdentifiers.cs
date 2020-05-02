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
        internal const string TestCaseSourceSourceTypeNotIEnumerable = "NUnit1015";
        internal const string TestCaseSourceSourceTypeNoDefaultConstructor = "NUnit1016";
        internal const string TestCaseSourceSourceIsNotStatic = "NUnit1017";
        internal const string TestCaseSourceMismatchInNumberOfParameters = "NUnit1018";
        internal const string TestCaseSourceDoesNotReturnIEnumerable = "NUnit1019";
        internal const string TestCaseSourceSuppliesParametersToFieldOrProperty = "NUnit1020";

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
        internal const string EqualConstraintUsage = "NUnit2010";
        internal const string StringContainsConstraintUsage = "NUnit2011";
        internal const string StringStartsWithConstraintUsage = "NUnit2012";
        internal const string StringEndsWithConstraintUsage = "NUnit2013";
        internal const string CollectionContainsConstraintUsage = "NUnit2014";
        internal const string AreSameUsage = "NUnit2015";
        internal const string NullUsage = "NUnit2016";
        internal const string IsNullUsage = "NUnit2017";
        internal const string NotNullUsage = "NUnit2018";
        internal const string IsNotNullUsage = "NUnit2019";
        internal const string SameAsIncompatibleTypes = "NUnit2020";
        internal const string EqualToIncompatibleTypes = "NUnit2021";
        internal const string MissingProperty = "NUnit2022";
        internal const string NullConstraintUsage = "NUnit2023";
        internal const string StringConstraintWrongActualType = "NUnit2024";

        #endregion Assertion
    }
}
