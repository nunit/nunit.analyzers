using Gu.Roslyn.Asserts;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests
{
    [SetUpFixture]
    internal class SetUpFixture
    {
        [OneTimeSetUp]
        public void SetDefaults()
        {
            Settings.Default = Settings.Default.WithMetadataReferences(MetadataReferences.Transitive(typeof(Assert)));
        }
    }
}
