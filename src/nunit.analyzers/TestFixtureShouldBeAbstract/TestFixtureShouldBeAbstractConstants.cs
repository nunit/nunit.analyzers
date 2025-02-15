namespace NUnit.Analyzers.TestFixtureShouldBeAbstract
{
    internal static class TestFixtureShouldBeAbstractConstants
    {
        internal const string Title = "Base TestFixtures should be abstract";
        internal const string Message = "Class {0} is used as a base class and should be abstract";
        internal const string Description = "Base TestFixtures should be abstract to prevent base class tests executing separately.";
    }
}
