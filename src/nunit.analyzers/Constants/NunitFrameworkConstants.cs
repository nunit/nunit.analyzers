namespace NUnit.Analyzers.Constants
{
    /// <summary>
    /// String constants for relevant NUnit concepts (classes, fields, properties etc.)
    /// so that we do not need have a dependency on NUnit from the analyzer project.
    /// </summary>
    public static class NunitFrameworkConstants
    {
        public const string NameOfEqualConstraintWithin = "Within";

        public const string NameOfIs = "Is";
        public const string NameOfIsFalse = "False";
        public const string NameOfIsTrue = "True";
        public const string NameOfIsEqualTo = "EqualTo";
        public const string NameOfIsEquivalentTo = "EquivalentTo";
        public const string NameOfIsSubsetOf = "SubsetOf";
        public const string NameOfIsSupersetOf = "SupersetOf";
        public const string NameOfIsNot = "Not";
        public const string NameOfIsNotEqualTo = "EqualTo";
        public const string NameOfIsSameAs = "SameAs";
        public const string NameOfIsSamePath = "SamePath";

        public const string NameOfDoes = "Does";
        public const string NameOfDoesNot = "Not";
        public const string NameOfDoesContain = "Contain";
        public const string NameOfDoesStartWith = "StartWith";
        public const string NameOfDoesEndWith = "EndWith";

        public const string NameOfAssert = "Assert";
        public const string NameOfAssertIsTrue = "IsTrue";
        public const string NameOfAssertTrue = "True";
        public const string NameOfAssertIsFalse = "IsFalse";
        public const string NameOfAssertFalse = "False";
        public const string NameOfAssertAreEqual = "AreEqual";
        public const string NameOfAssertAreNotEqual = "AreNotEqual";
        public const string NameOfAssertAreSame = "AreSame";
        public const string NameOfAssertAreNotSame = "AreNotSame";
        public const string NameOfAssertThat = "That";

        public const string FullNameOfTypeIs = "NUnit.Framework.Is";
        public const string FullNameOfTypeTestCaseAttribute = "NUnit.Framework.TestCaseAttribute";
        public const string FullNameOfTypeTestCaseSourceAttribute = "NUnit.Framework.TestCaseSourceAttribute";
        public const string FullNameOfTypeTestAttribute = "NUnit.Framework.TestAttribute";
        public const string FullNameOfTypeParallelizableAttribute = "NUnit.Framework.ParallelizableAttribute";
        public const string FullNameOfTypeITestBuilder = "NUnit.Framework.Interfaces.ITestBuilder";

        public const string NameOfTestCaseAttribute = "TestCaseAttribute";
        public const string NameOfTestCaseSourceAttribute = "TestCaseSourceAttribute";
        public const string NameOfTestAttribute = "TestAttribute";
        public const string NameOfParallelizableAttribute = "ParallelizableAttribute";

        public const string NameOfExpectedResult = "ExpectedResult";

        public const string NameOfActualParameter = "actual";
        public const string NameOfConditionParameter = "condition";
        public const string NameOfExpectedParameter = "expected";
        public const string NameOfExpressionParameter = "expression";

        public const string NameOfIgnoreCase = "IgnoreCase";

        public const string AssemblyQualifiedNameOfTypeAssert =
            "NUnit.Framework.Assert, nunit.framework, Version=3.12.0.0, Culture=neutral, PublicKeyToken=2638cd05610744eb";
    }
}
