using System.Threading.Tasks;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.TestCaseSourceUsage.TestCaseSourceUsesStringAnalyzer>;

namespace NUnit.Analyzers.Tests.TestCaseSourceUsage
{
    public sealed class TestCaseSourceSourceTypeTests
    {
        [Test]
        public async Task AnalyzeWhenTypeOfValidSourceType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
                }
            """,
    additionalUsings: "using System.Collections;");
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenTypeOfSourceNotImplementingIEnumerable()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class AnalyzeWhenTypeOfSourceNotImplementingIEnumerable
                {
                    [TestCaseSource({|#0:typeof(MyTests)|})]
                    public void Test()
                    {
                    }
                }

                class MyTests
                {
                }
            """);

            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseSourceSourceTypeNotIEnumerable)
                .WithMessage("The source type 'MyTests' does not implement I(Async)Enumerable");
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenTypeOfSourceImplementsIAsyncEnumerable()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
                }
            """,
        additionalUsings: "using System.Collections.Generic;using System.Threading;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenTypeOfSourceNoDefaultConstructor()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                    public class AnalyzeWhenTypeOfSourceNoDefaultConstructor
                    {
                        [TestCaseSource({|#0:typeof(MyTests)|})]
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
                }
            """,
    additionalUsings: "using System.Collections;");

            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseSourceSourceTypeNoDefaultConstructor)
                .WithMessage("The source type 'MyTests' does not have a default constructor");
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }
    }
}
