using System.Globalization;
using Gu.Roslyn.Asserts;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.ValuesUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ValuesUsage
{
    public class ValuesUsageAnalyzerTests
    {
        private readonly ValuesUsageAnalyzer analyzer = new ValuesUsageAnalyzer();

        [Test]
        public void AnalyzeWhenArgumentTypeIsCorrect()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class Foo
    {
        [Test]
        public void ATest([Values(true, false)] bool blah) { }
    }");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenArgumentTypeIsIncorrect()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.ValuesParameterTypeMismatchUsage,
                                                               string.Format(CultureInfo.InvariantCulture,
                                                                             ValuesUsageAnalyzerConstants.ParameterTypeMismatchMessage,
                                                                             1, "object", "blah", "bool"));
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class Foo
    {
        [Test]
        public void ATest([Values(true, null)] bool blah) { }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }
    }
}
