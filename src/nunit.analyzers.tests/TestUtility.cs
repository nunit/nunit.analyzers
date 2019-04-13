using System.Threading.Tasks;
using Gu.Roslyn.Asserts;
using NUnit.Framework;

[assembly: MetadataReferences(typeof(Assert), typeof(object), typeof(Task))]

namespace NUnit.Analyzers.Tests
{
    internal static class TestUtility
    {
        internal static string WrapClassInNamespaceAndAddUsing(string code)
        {
            return $@"
using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{{{code}
}}";
        }

        internal static string WrapMethodInClassNamespaceAndAddUsings(string method)
        {
            return WrapClassInNamespaceAndAddUsing($@"
    [TestFixture]
    public class TestClass
    {{{method}
    }}");
        }

        internal static string WrapInTestMethod(string code)
        {
            return WrapMethodInClassNamespaceAndAddUsings($@"
        [Test]
        public void TestMethod()
        {{{code}
        }}");
        }
    }
}
