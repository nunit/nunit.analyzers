namespace NUnit.Analyzers.Constants
{
    /// <summary>
    /// String constants for relevant NUnit concepts (classes, fields, properties etc.)
    /// so that we do not need have a dependency on NUnit from the analyzer project.
    /// These are constants for NUnit v3 and later.
    /// </summary>
    internal abstract class NUnitPreV5FrameworkConstants
    {
        public const string FullNameOfSameAsConstraint = "NUnit.Framework.Constraints.SameAsConstraint";

        public const string FullNameOfActualValueDelegate = "NUnit.Framework.Constraints.ActualValueDelegate`1";
        public const string FullNameOfTestDelegate = "NUnit.Framework.TestDelegate";
        public const string FullNameOfAsyncTestDelegate = "NUnit.Framework.AsyncTestDelegate";
    }
}
