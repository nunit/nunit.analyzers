using System.Collections.Generic;
using System.Linq;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
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
                .WithMessage("The source type 'MyTests' does not implement I(Async)Enumerable");
            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenTypeOfSourceImplementsIAsyncEnumerable()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenTypeOfSourceImplementsIAsyncEnumerable
    {
        [TestCaseSource(typeof(MyTests))]
        public void Test(int i)
        {
        }
    }

    public sealed class MyTests : IAsyncEnumerable<int>
    {
        public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }",
        additionalUsings: "using System.Collections.Generic;using System.Threading;");

            IEnumerable<MetadataReference> asyncEnumerableMetadata = MetadataReferences.Transitive(typeof(IAsyncEnumerable<>));
            IEnumerable<MetadataReference> metadataReferences = (Settings.Default.MetadataReferences ?? Enumerable.Empty<MetadataReference>()).Concat(asyncEnumerableMetadata);

            RoslynAssert.Valid(analyzer, testCode, Settings.Default.WithMetadataReferences(metadataReferences));
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
