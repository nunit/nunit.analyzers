namespace NUnit.Analyzers.Constants
{
    /// <summary>
    /// String constants for relevant NUnit concepts (classes, fields, properties etc.)
    /// so that we do not need have a dependency on NUnit from the analyzer project.
    /// These are constants for NUnit Legacy Framework.
    /// </summary>
    internal abstract class NUnitLegacyFrameworkConstants
    {
        public const string NameOfAssertIsTrue = "IsTrue";
        public const string NameOfAssertTrue = "True";
        public const string NameOfAssertIsFalse = "IsFalse";
        public const string NameOfAssertFalse = "False";
        public const string NameOfAssertAreEqual = "AreEqual";
        public const string NameOfAssertAreNotEqual = "AreNotEqual";
        public const string NameOfAssertAreSame = "AreSame";
        public const string NameOfAssertAreNotSame = "AreNotSame";
        public const string NameOfAssertNull = "Null";
        public const string NameOfAssertIsNull = "IsNull";
        public const string NameOfAssertNotNull = "NotNull";
        public const string NameOfAssertIsNotNull = "IsNotNull";
        public const string NameOfAssertGreater = "Greater";
        public const string NameOfAssertGreaterOrEqual = "GreaterOrEqual";
        public const string NameOfAssertLess = "Less";
        public const string NameOfAssertLessOrEqual = "LessOrEqual";
        public const string NameOfAssertZero = "Zero";
        public const string NameOfAssertNotZero = "NotZero";
        public const string NameOfAssertIsNaN = "IsNaN";
        public const string NameOfAssertIsEmpty = "IsEmpty";
        public const string NameOfAssertIsNotEmpty = "IsNotEmpty";
        public const string NameOfAssertContains = "Contains";
        public const string NameOfAssertIsInstanceOf = "IsInstanceOf";
        public const string NameOfAssertIsNotInstanceOf = "IsNotInstanceOf";
        public const string NameOfAssertIsAssignableFrom = "IsAssignableFrom";
        public const string NameOfAssertIsNotAssignableFrom = "IsNotAssignableFrom";
        public const string NameOfAssertPositive = "Positive";
        public const string NameOfAssertNegative = "Negative";

        public const string NameOfStringAssert = "StringAssert";
        public const string NameOfStringAssertContains = "Contains";
        public const string NameOfStringAssertDoesNotContain = "DoesNotContain";
        public const string NameOfStringAssertStartsWith = "StartsWith";
        public const string NameOfStringAssertDoesNotStartWith = "DoesNotStartWith";
        public const string NameOfStringAssertEndsWith = "EndsWith";
        public const string NameOfStringAssertDoesNotEndWith = "DoesNotEndWith";
        public const string NameOfStringAssertAreEqualIgnoringCase = "AreEqualIgnoringCase";
        public const string NameOfStringAssertAreNotEqualIgnoringCase = "AreNotEqualIgnoringCase";
        public const string NameOfStringAssertIsMatch = "IsMatch";
        public const string NameOfStringAssertDoesNotMatch = "DoesNotMatch";

        public const string NameOfCollectionAssert = "CollectionAssert";
        public const string NameOfCollectionAssertAllItemsAreInstancesOfType = "AllItemsAreInstancesOfType";
        public const string NameOfCollectionAssertAllItemsAreNotNull = "AllItemsAreNotNull";
        public const string NameOfCollectionAssertAllItemsAreUnique = "AllItemsAreUnique";
        public const string NameOfCollectionAssertAreEqual = "AreEqual";
        public const string NameOfCollectionAssertAreEquivalent = "AreEquivalent";
        public const string NameOfCollectionAssertAreNotEqual = "AreNotEqual";
        public const string NameOfCollectionAssertAreNotEquivalent = "AreNotEquivalent";
        public const string NameOfCollectionAssertContains = "Contains";
        public const string NameOfCollectionAssertDoesNotContain = "DoesNotContain";
        public const string NameOfCollectionAssertIsNotSubsetOf = "IsNotSubsetOf";
        public const string NameOfCollectionAssertIsSubsetOf = "IsSubsetOf";
        public const string NameOfCollectionAssertIsNotSupersetOf = "IsNotSupersetOf";
        public const string NameOfCollectionAssertIsSupersetOf = "IsSupersetOf";
        public const string NameOfCollectionAssertIsEmpty = "IsEmpty";
        public const string NameOfCollectionAssertIsNotEmpty = "IsNotEmpty";
        public const string NameOfCollectionAssertIsOrdered = "IsOrdered";

        public const string NUnitFrameworkLegacyAssemblyName = "nunit.framework.legacy";
        public const string NameOfClassicAssert = "ClassicAssert";
    }
}
