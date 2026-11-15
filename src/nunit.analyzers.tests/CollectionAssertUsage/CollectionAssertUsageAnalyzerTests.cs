using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.CollectionAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.CollectionAssertUsage.CollectionAssertUsageAnalyzer>;

namespace NUnit.Analyzers.Tests.CollectionAssertUsage
{
    [TestFixture]
    internal sealed class CollectionAssertUsageAnalyzerTests
    {
        private readonly DiagnosticResult diagnostic = Verify.Diagnostic(AnalyzerIdentifiers.CollectionAssertUsage);

        private static IEnumerable<string> OneCollectionParameterAsserts => CollectionAssertUsageAnalyzer.OneCollectionParameterAsserts.Keys;

        private static IEnumerable<string> TwoCollectionParameterAsserts => CollectionAssertUsageAnalyzer.TwoCollectionParameterAsserts.Keys;

        private static IEnumerable<string> CollectionAndItemParameterAsserts => CollectionAssertUsageAnalyzer.CollectionAndItemParameterAsserts.Keys;

        [TestCaseSource(nameof(OneCollectionParameterAsserts))]
        public async Task AnalyzeOneCollectionWhenNoMessageArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { 1, 2, 3 };
                        {|#0:CollectionAssert.{{method}}(collection)|};

            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(OneCollectionParameterAsserts))]
        public async Task AnalyzeOneCollectionWhenOnlyMessageArgumentIsUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { 1, 2, 3 };
                        {|#0:CollectionAssert.{{method}}(collection, "message")|};

            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(OneCollectionParameterAsserts))]
        public async Task AnalyzeOneCollectionWhenFormatAndOneParamsArgumentAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { 1, 2, 3 };
                        {|#0:CollectionAssert.{{method}}(collection, "Because of {0}", "message")|};

            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(OneCollectionParameterAsserts))]
        public async Task AnalyzeOneCollectionWhenFormatAndTwoParamsArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { 1, 2, 3 };
                        {|#0:CollectionAssert.{{method}}(collection, "{0}, {1}", "first", "second")|};

            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(TwoCollectionParameterAsserts))]
        public async Task AnalyzeTwoCollectionWhenNoMessageArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        {|#0:CollectionAssert.{{method}}(collection1, collection2)|};

            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(TwoCollectionParameterAsserts))]
        public async Task AnalyzeTwoCollectionWhenOnlyMessageArgumentIsUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        {|#0:CollectionAssert.{{method}}(collection1, collection2, "message")|};

            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(TwoCollectionParameterAsserts))]
        public async Task AnalyzeTwoCollectionWhenFormatAndOneParamsArgumentAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        {|#0:CollectionAssert.{{method}}(collection1, collection2, "Because of {0}", "message")|};

            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(TwoCollectionParameterAsserts))]
        public async Task AnalyzeTwoCollectionWhenFormatAndTwoParamsArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        {|#0:CollectionAssert.{{method}}(collection1, collection2, "{0}, {1}", "first", "second")|};

            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

        [TestCase(NUnitLegacyFrameworkConstants.NameOfCollectionAssertAreEqual)]
        [TestCase(NUnitLegacyFrameworkConstants.NameOfCollectionAssertAreNotEqual)]
        public async Task AnalyzeTwoCollectionWithComparerWhenFormatAndParamsArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void TestMethod()
                    {
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        var comparer = new AlwaysEqualComparer();
                        {|#0:CollectionAssert.{{method}}(collection1, collection2, comparer, "Because of {0}", "message")|};
                    }

                    private sealed class AlwaysEqualComparer : IComparer
                    {
                        public int Compare(object? x, object? y) => 0;
                    }

            """, "using System.Collections;");
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(CollectionAndItemParameterAsserts))]
        public async Task AnalyzeCollectionAndItemWhenNoMessageArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { typeof(byte), typeof(char) };
                        {|#0:CollectionAssert.{{method}}(collection, typeof(byte))|};

            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(CollectionAndItemParameterAsserts))]
        public async Task AnalyzeCollectionAndItemWhenOnlyMessageArgumentIsUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { typeof(byte), typeof(char) };
                        {|#0:CollectionAssert.{{method}}(collection, typeof(byte), "message")|};

            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(CollectionAndItemParameterAsserts))]
        public async Task AnalyzeCollectionAndItemWhenFormatAndOneParamsArgumentAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { typeof(byte), typeof(char) };
                        {|#0:CollectionAssert.{{method}}(collection, typeof(byte), "Because of {0}", "message")|};

            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(CollectionAndItemParameterAsserts))]
        public async Task AnalyzeCollectionAndItemWhenFormatAndTwoParamsArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { typeof(byte), typeof(char) };
                        {|#0:CollectionAssert.{{method}}(collection, typeof(byte), "{0}, {1}", "first", "second")|};

            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }
    }
}
