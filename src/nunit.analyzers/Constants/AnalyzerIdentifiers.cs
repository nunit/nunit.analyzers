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
        internal const string ValueSourceStringUsage = "NUnit1021";
        internal const string ValueSourceIsNotStatic = "NUnit1022";
        internal const string ValueSourceMethodExpectParameters = "NUnit1023";
        internal const string ValueSourceDoesNotReturnIEnumerable = "NUnit1024";
        internal const string ValueSourceIsMissing = "NUnit1025";
        internal const string TestMethodIsNotPublic = "NUnit1026";
        internal const string SimpleTestMethodHasParameters = "NUnit1027";
        internal const string NonTestMethodIsPublic = "NUnit1028";
        internal const string TestCaseSourceMismatchInNumberOfTestMethodParameters = "NUnit1029";
        internal const string TestCaseSourceMismatchWithTestMethodParameterType = "NUnit1030";
        internal const string ValuesParameterTypeMismatchUsage = "NUnit1031";
        internal const string FieldIsNotDisposedInTearDown = "NUnit1032";
        internal const string TestContextWriteIsObsolete = "NUnit1033";

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
        internal const string ContainsConstraintWrongActualType = "NUnit2025";
        internal const string SomeItemsIncompatibleTypes = "NUnit2026";
        internal const string GreaterUsage = "NUnit2027";
        internal const string GreaterOrEqualUsage = "NUnit2028";
        internal const string LessUsage = "NUnit2029";
        internal const string LessOrEqualUsage = "NUnit2030";
        internal const string AreNotSameUsage = "NUnit2031";
        internal const string ZeroUsage = "NUnit2032";
        internal const string NotZeroUsage = "NUnit2033";
        internal const string IsNaNUsage = "NUnit2034";
        internal const string IsEmptyUsage = "NUnit2035";
        internal const string IsNotEmptyUsage = "NUnit2036";
        internal const string ContainsUsage = "NUnit2037";
        internal const string IsInstanceOfUsage = "NUnit2038";
        internal const string IsNotInstanceOfUsage = "NUnit2039";
        internal const string SameAsOnValueTypes = "NUnit2040";
        internal const string ComparableTypes = "NUnit2041";
        internal const string ComparableOnObject = "NUnit2042";
        internal const string ComparisonConstraintUsage = "NUnit2043";
        internal const string DelegateRequired = "NUnit2044";
        internal const string UseAssertMultiple = "NUnit2045";
        internal const string UsePropertyConstraint = "NUnit2046";
        internal const string WithinIncompatibleTypes = "NUnit2047";
        internal const string StringAssertUsage = "NUnit2048";
        internal const string CollectionAssertUsage = "NUnit2049";
        internal const string UpdateStringFormatToInterpolatableString = "NUnit2050";
        internal const string PositiveUsage = "NUnit2051";
        internal const string NegativeUsage = "NUnit2052";
        internal const string IsAssignableFromUsage = "NUnit2053";
        internal const string IsNotAssignableFromUsage = "NUnit2054";

        #endregion Assertion

        #region Suppression

        internal const string DereferencePossibleNullReference = "NUnit3001";
        internal const string NonNullableFieldOrPropertyIsUninitialized = "NUnit3002";
        internal const string AvoidUninstantiatedInternalClasses = "NUnit3003";
        internal const string TypesThatOwnDisposableFieldsShouldBeDisposable = "NUnit3004";

        #endregion

        #region Style

        internal const string SimplifyValues = "NUnit4001";
        internal const string UseSpecificConstraint = "NUnit4002";

        #endregion
    }
}
