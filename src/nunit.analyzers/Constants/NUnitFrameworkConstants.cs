namespace NUnit.Analyzers.Constants
{
    /// <summary>
    /// String constants for relevant NUnit concepts (classes, fields, properties etc.)
    /// so that we do not need have a dependency on NUnit from the analyzer project.
    /// These are constants for NUnit v3 and later.
    /// </summary>
    internal abstract class NUnitFrameworkConstants
    {
        public const string NameOfIs = "Is";
        public const string NameOfIsFalse = "False";
        public const string NameOfIsTrue = "True";
        public const string NameOfIsEqualTo = "EqualTo";
        public const string NameOfIsEquivalentTo = "EquivalentTo";
        public const string NameOfIsSubsetOf = "SubsetOf";
        public const string NameOfIsSupersetOf = "SupersetOf";
        public const string NameOfIsNot = "Not";
        public const string NameOfIsSameAs = "SameAs";
        public const string NameOfIsSamePath = "SamePath";
        public const string NameOfIsNull = "Null";
        public const string NameOfIsGreaterThan = "GreaterThan";
        public const string NameOfIsGreaterThanOrEqualTo = "GreaterThanOrEqualTo";
        public const string NameOfIsLessThan = "LessThan";
        public const string NameOfIsLessThanOrEqualTo = "LessThanOrEqualTo";
        public const string NameOfIsPositive = "Positive";
        public const string NameOfIsNegative = "Negative";
        public const string NameOfIsZero = "Zero";
        public const string NameOfIsNaN = "NaN";
        public const string NameOfIsEmpty = "Empty";
        public const string NameOfIsInstanceOf = "InstanceOf";
        public const string NameOfIsAssignableFrom = "AssignableFrom";
        public const string NameOfIsAll = "All";
        public const string NameOfIsUnique = "Unique";
        public const string NameOfIsOrdered = "Ordered";

        public const string NameOfContains = "Contains";
        public const string NameOfContainsItem = "Item";

        public const string NameOfDoes = "Does";
        public const string NameOfDoesNot = "Not";
        public const string NameOfDoesContain = "Contain";
        public const string NameOfDoesStartWith = "StartWith";
        public const string NameOfDoesEndWith = "EndWith";
        public const string NameOfDoesMatch = "Match";

        public const string NameOfHas = "Has";
        public const string NameOfHasProperty = "Property";
        public const string NameOfHasCount = "Count";
        public const string NameOfHasLength = "Length";
        public const string NameOfHasMessage = "Message";
        public const string NameOfHasInnerException = "InnerException";
        public const string NameOfHasNo = "No";
        public const string NameOfHasMember = "Member";

        public const string NameOfMultiple = "Multiple";

        public const string NameOfOut = "Out";
        public const string NameOfWrite = "Write";
        public const string NameOfWriteLine = "WriteLine";

        public const string NameOfThrows = "Throws";
        public const string NameOfThrowsArgumentException = "ArgumentException";
        public const string NameOfThrowsArgumentNullException = "ArgumentNullException";
        public const string NameOfThrowsInvalidOperationException = "InvalidOperationException";
        public const string NameOfThrowsTargetInvocationException = "TargetInvocationException";

        public const string NameOfAssert = "Assert";
        public const string NameOfAssume = "Assume";

        public const string NameOfAssertPass = "Pass";
        public const string NameOfAssertFail = "Fail";
        public const string NameOfAssertWarn = "Warn";
        public const string NameOfAssertIgnore = "Ignore";
        public const string NameOfAssertInconclusive = "Inconclusive";

        public const string NameOfAssertThat = "That";

        public const string NameOfAssertCatch = "Catch";
        public const string NameOfAssertCatchAsync = "CatchAsync";
        public const string NameOfAssertThrows = "Throws";
        public const string NameOfAssertThrowsAsync = "ThrowsAsync";

        public const string FullNameOfTypeIs = "NUnit.Framework.Is";
        public const string FullNameOfTypeTestCaseAttribute = "NUnit.Framework.TestCaseAttribute";
        public const string FullNameOfTypeTestCaseSourceAttribute = "NUnit.Framework.TestCaseSourceAttribute";
        public const string FullNameOfTypeTestAttribute = "NUnit.Framework.TestAttribute";
        public const string FullNameOfTypeParallelizableAttribute = "NUnit.Framework.ParallelizableAttribute";
        public const string FullNameOfTypeITestBuilder = "NUnit.Framework.Interfaces.ITestBuilder";
        public const string FullNameOfTypeIFixtureBuilder = "NUnit.Framework.Interfaces.IFixtureBuilder";
        public const string FullNameOfTypeISimpleTestBuilder = "NUnit.Framework.Interfaces.ISimpleTestBuilder";
        public const string FullNameOfTypeRangeAttribute = "NUnit.Framework.RangeAttribute";
        public const string FullNameOfTypeValuesAttribute = "NUnit.Framework.ValuesAttribute";
        public const string FullNameOfTypeValueSourceAttribute = "NUnit.Framework.ValueSourceAttribute";

        public const string FullNameOfTypeIParameterDataSource = "NUnit.Framework.Interfaces.IParameterDataSource";
        public const string FullNameOfTypeTestCaseData = "NUnit.Framework.TestCaseData";
        public const string FullNameOfTypeTestCaseParameters = "NUnit.Framework.Internal.TestCaseParameters";

        public const string FullNameOfTypeOneTimeSetUpAttribute = "NUnit.Framework.OneTimeSetUpAttribute";
        public const string FullNameOfTypeOneTimeTearDownAttribute = "NUnit.Framework.OneTimeTearDownAttribute";
        public const string FullNameOfTypeSetUpAttribute = "NUnit.Framework.SetUpAttribute";
        public const string FullNameOfTypeTearDownAttribute = "NUnit.Framework.TearDownAttribute";

        public const string FullNameOfFixtureLifeCycleAttribute = "NUnit.Framework.FixtureLifeCycleAttribute";
        public const string FullNameOfLifeCycle = "NUnit.Framework.LifeCycle";

        public const string FullNameOfTypeTestContext = "NUnit.Framework.TestContext";

        public const string NameOfConstraint = "Constraint";

        public const string FullNameOfSameAsConstraint = "NUnit.Framework.Constraints.SameAsConstraint";
        public const string FullNameOfSomeItemsConstraint = "NUnit.Framework.Constraints.SomeItemsConstraint";
        public const string FullNameOfEqualToConstraint = "NUnit.Framework.Constraints.EqualConstraint";
        public const string FullNameOfEndsWithConstraint = "NUnit.Framework.Constraints.EndsWithConstraint";
        public const string FullNameOfRegexConstraint = "NUnit.Framework.Constraints.RegexConstraint";
        public const string FullNameOfEmptyStringConstraint = "NUnit.Framework.Constraints.EmptyStringConstraint";
        public const string FullNameOfSamePathConstraint = "NUnit.Framework.Constraints.SamePathConstraint";
        public const string FullNameOfSamePathOrUnderConstraint = "NUnit.Framework.Constraints.SamePathOrUnderConstraint";
        public const string FullNameOfStartsWithConstraint = "NUnit.Framework.Constraints.StartsWithConstraint";
        public const string FullNameOfSubPathConstraint = "NUnit.Framework.Constraints.SubPathConstraint";
        public const string FullNameOfSubstringConstraint = "NUnit.Framework.Constraints.SubstringConstraint";
        public const string FullNameOfContainsConstraint = "NUnit.Framework.Constraints.ContainsConstraint";
        public const string FullNameOfActualValueDelegate = "NUnit.Framework.Constraints.ActualValueDelegate`1";
        public const string FullNameOfDelayedConstraint = "NUnit.Framework.Constraints.DelayedConstraint";
        public const string FullNameOfTestDelegate = "NUnit.Framework.TestDelegate";
        public const string FullNameOfThrows = "NUnit.Framework.Throws";

        public const string PrefixOfAllEqualToConstraints = "NUnit.Framework.Constraints.Equal";

        public const string NameOfTestCaseAttribute = "TestCaseAttribute";
        public const string NameOfTestCaseSourceAttribute = "TestCaseSourceAttribute";
        public const string NameOfTestAttribute = "TestAttribute";
        public const string NameOfParallelizableAttribute = "ParallelizableAttribute";
        public const string NameOfRangeAttribute = "RangeAttribute";
        public const string NameOfValuesAttribute = "ValuesAttribute";
        public const string NameOfValueSourceAttribute = "ValueSourceAttribute";

        public const string NameOfOneTimeSetUpAttribute = "OneTimeSetUpAttribute";
        public const string NameOfOneTimeTearDownAttribute = "OneTimeTearDownAttribute";
        public const string NameOfSetUpAttribute = "SetUpAttribute";
        public const string NameOfTearDownAttribute = "TearDownAttribute";

        public const string NameOfExpectedResult = "ExpectedResult";

        public const string NameOfActualParameter = "actual";
        public const string NameOfADoubleParameter = "aDouble";
        public const string NameOfAnObjectParameter = "anObject";
        public const string NameOfArg1Parameter = "arg1";
        public const string NameOfArg2Parameter = "arg2";
        public const string NameOfArgsParameter = "args";
        public const string NameOfAStringParameter = "aString";
        public const string NameOfCollectionParameter = "collection";
        public const string NameOfComparerParameter = "comparer";
        public const string NameOfConditionParameter = "condition";
        public const string NameOfDeltaParameter = "delta";
        public const string NameOfExpectedParameter = "expected";
        public const string NameOfExpectedTypeParameter = "expectedType";
        public const string NameOfExpressionParameter = "expression";
        public const string NameOfMessageParameter = "message";
        public const string NameOfPatternParameter = "pattern";
        public const string NameOfSubsetParameter = "subset";
        public const string NameOfSupersetParameter = "superset";

        public const string NameOfConstraintExpressionAnd = "And";
        public const string NameOfConstraintExpressionOr = "Or";
        public const string NameOfConstraintExpressionWith = "With";

        public const string NameOfEqualConstraintIgnoreCase = "IgnoreCase";
        public const string NameOfEqualConstraintUsing = "Using";
        public const string NameOfEqualConstraintWithin = "Within";
        public const string NameOfEqualConstraintAsCollection = "AsCollection";

        public const string NUnitFrameworkAssemblyName = "nunit.framework";
    }
}
