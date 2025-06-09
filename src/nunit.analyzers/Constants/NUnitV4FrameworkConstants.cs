namespace NUnit.Analyzers.Constants
{
    /// <summary>
    /// String constants for relevant NUnit concepts (classes, fields, properties etc.)
    /// so that we do not need have a dependency on NUnit from the analyzer project.
    /// These are constants for NUnit v4.
    /// </summary>
    internal abstract class NUnitV4FrameworkConstants
    {
        public const string NameOfIsDefault = "Default";

        public const string NameOfMultipleAsync = "MultipleAsync";
        public const string NameOfEnterMultipleScope = "EnterMultipleScope";

        public const string NameOfCancelAfterAttribute = "CancelAfterAttribute";

        public const string FullNameOfCancelAfterAttribute = "NUnit.Framework.CancelAfterAttribute";
        public const string FullNameOfCancellationToken = "System.Threading.CancellationToken";

        public const string NameOfUsingPropertiesComparer = "UsingPropertiesComparer";

        public const string FullNameOfSomeItemsConstraintGeneric = "NUnit.Framework.Constraints.SomeItemsConstraint`1";
    }
}
