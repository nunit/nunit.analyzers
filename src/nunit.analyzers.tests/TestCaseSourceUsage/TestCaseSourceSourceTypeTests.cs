using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TestCaseSourceUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.TestCaseSourceUsage
{
    public sealed class TestCaseSourceSourceTypeTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new TestCaseSourceUsesStringAnalyzer();

        [Test]
        public void AnalyzeWhenTypeOfValidSourceType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenTypeOfValidSourceType
    {
        [TestCaseSource(typeof(MyTests))]
        public void Test(int input)
        {
        }
    }

    class MyTests : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new object[] { 12 };
        }
    }",
    additionalUsings: "using System.Collections;");
            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenTypeOfSourceNotImplementingIEnumerable()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenTypeOfSourceNotImplementingIEnumerable
    {
        [TestCaseSource(typeof(MyTests))]
        public void Test()
        {
        }
    }

    class MyTests
    {
    }");

            var expectedDiagnostic = ExpectedDiagnostic
                .Create(AnalyzerIdentifiers.TestCaseSourceSourceTypeNotIEnumerable)
                .WithMessage("The source type 'MyTests' does not implement IEnumerable");
            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenTypeOfSourceNoDefaultConstructor()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
        public class AnalyzeWhenTypeOfSourceNoDefaultConstructor
        {
            [TestCaseSource(typeof(MyTests))]
            public void Test()
            {
            }
        }

    class MyTests : IEnumerable
    {
        public MyTests(int i) {}

        public IEnumerator GetEnumerator()
        {
            yield return new object[] { 12 };
        }
    }",
    additionalUsings: "using System.Collections;");

            var expectedDiagnostic = ExpectedDiagnostic
                .Create(AnalyzerIdentifiers.TestCaseSourceSourceTypeNoDefaultConstructor)
                .WithMessage("The source type 'MyTests' does not have a default constructor");
            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }
    }
}
