namespace NUnit.Analyzers.UseCollectionConstraint
{
    internal static class UseCollectionConstraintConstants
    {
        internal const string Title = "Use CollectionConstraint for better assertion messages in case of failure";
        internal const string MessageFormat = "Use Assert.That(<collection>, Has.{0}.EqualTo(<value>)";
        internal const string Description = "Use Has.Length/Has.Count/Is.Empty instead of testing property directly.";
    }
}
