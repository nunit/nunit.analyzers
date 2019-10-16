using System.Linq;
using System.Threading.Tasks;
using Gu.Roslyn.Asserts;
using NUnit.Framework;

[assembly: MetadataReferences(typeof(Assert), typeof(object), typeof(Task), typeof(Enumerable))]

namespace NUnit.Analyzers.Tests
{
    internal static class TestUtility
    {
        internal static string WrapClassInNamespaceAndAddUsing(string code,
            string additionalUsings = null)
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
            string additionalUsings = null)
        {
            return WrapClassInNamespaceAndAddUsing($@"
    [TestFixture]
    public class TestClass
    {{{method}
    }}", additionalUsings);
        }

        internal static string WrapInTestMethod(string code, string additionalUsings = null)
        {
            return WrapMethodInClassNamespaceAndAddUsings($@"
        [Test]
        public void TestMethod()
        {{{code}
        }}", additionalUsings);
        }
    }
}
