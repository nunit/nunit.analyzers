namespace NUnit.Analyzers.Tests
{
    internal static class TestUtility
    {
        internal static string WrapClassInNamespaceAndAddUsing(string code,
            string? additionalUsings = null)
        {
            return $@"
#pragma warning disable CS8019
using System;
using System.Threading.Tasks;
using NUnit.Framework;
#if NUNIT4
using NUnit.Framework.Legacy;
#else
using ClassicAssert = NUnit.Framework.Assert;
#endif

#pragma warning restore CS8019
{additionalUsings}

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{{{code}
}}";
        }

        internal static string WrapMethodInClassNamespaceAndAddUsings(string method,
            string? additionalUsings = null,
            bool isNUnit4Only = false)
        {
            var methodWithPreProcessor = isNUnit4Only
                ? $"""
                    
                    #if NUNIT4
                    {method}
                    #endif
                    """
                : method;
            return WrapClassInNamespaceAndAddUsing($@"
    [TestFixture]
    public class TestClass
    {{{methodWithPreProcessor}
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
