using Gu.Roslyn.Asserts;
using NUnit.Framework;

[assembly: MetadataReferences(typeof(Assert), typeof(object))]

namespace NUnit.Analyzers.Tests
{
    internal static class TestUtility
    {
        internal static string WrapClassInNamespaceAndAddUsing(string code)
        {
            return $@"
using System;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{{{code}
}}";
        }

    }
}
