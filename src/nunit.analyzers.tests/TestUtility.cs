using System;
using Gu.Roslyn.Asserts;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests
{
    internal static class TestUtility
    {
        static TestUtility()
        {
            Settings.Default = Settings.Default.WithMetadataReferences(
                MetadataReferences.Transitive(typeof(Assert)));
        }

        internal static string WrapClassInNamespaceAndAddUsing(string code,
            string? additionalUsings = null)
        {
            return $@"
#pragma warning disable CS8019
using System;
using System.Threading.Tasks;
using NUnit.Framework;
#pragma warning restore CS8019
{additionalUsings}

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{{{code}
}}";
        }

        internal static string WrapMethodInClassNamespaceAndAddUsings(string method,
            string? additionalUsings = null)
        {
            return WrapClassInNamespaceAndAddUsing($@"
    [TestFixture]
    public class TestClass
    {{{method}
    }}", additionalUsings);
        }

        internal static string WrapInTestMethod(string code, string? additionalUsings = null)
        {
            return WrapMethodInClassNamespaceAndAddUsings($@"
        [Test]
        public void TestMethod()
        {{{code}
        }}", additionalUsings);
        }

        internal static string WrapInAsyncTestMethod(string code, string? additionalUsings = null)
        {
            return WrapMethodInClassNamespaceAndAddUsings($@"
        [Test]
        public async Task TestMethod()
        {{{code}
        }}", additionalUsings);
        }
    }
}
