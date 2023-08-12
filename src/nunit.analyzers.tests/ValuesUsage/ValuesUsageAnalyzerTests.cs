using Gu.Roslyn.Asserts;
using NUnit.Analyzers.ValuesUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ValuesUsage
{
    public class ValuesUsageAnalyzerTests
    {
        private readonly ValuesUsageAnalyzer analyzer = new ValuesUsageAnalyzer();

        [Test]
        public void Blah()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class Foo
    {
        [Test]
        public void ATest([Values(true, false)] bool blah) { }
    }");
            RoslynAssert.Valid(this.analyzer, testCode);
        }
    }
}
