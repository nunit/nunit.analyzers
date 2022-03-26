using Gu.Roslyn.Asserts;
using NUnit.Framework;

[assembly: TransitiveMetadataReferences(typeof(Assert))]

namespace NUnit.Analyzers.Tests
{
    internal static class TestUtility
    {
        internal static string WrapClassInNamespaceAndAddUsing(string code,
            string? additionalUsings = null)
        {
            return $@"
using System;
using System.Threading.Tasks;
using NUnit.Framework;
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
