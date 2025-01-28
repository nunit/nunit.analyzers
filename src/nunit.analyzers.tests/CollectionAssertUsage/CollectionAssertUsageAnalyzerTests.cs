using System.Collections.Generic;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.CollectionAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.CollectionAssertUsage
{
    [TestFixture]
    internal sealed class CollectionAssertUsageAnalyzerTests
    {
        private readonly DiagnosticAnalyzer analyzer = new CollectionAssertUsageAnalyzer();
        private readonly ExpectedDiagnostic diagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.CollectionAssertUsage);

        private static IEnumerable<string> OneCollectionParameterAsserts => CollectionAssertUsageAnalyzer.OneCollectionParameterAsserts.Keys;

        private static IEnumerable<string> TwoCollectionParameterAsserts => CollectionAssertUsageAnalyzer.TwoCollectionParameterAsserts.Keys;

        private static IEnumerable<string> CollectionAndItemParameterAsserts => CollectionAssertUsageAnalyzer.CollectionAndItemParameterAsserts.Keys;

        [TestCaseSource(nameof(OneCollectionParameterAsserts))]
        public void AnalyzeOneCollectionWhenNoMessageArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                var collection = new[] {{ 1, 2, 3 }};
                ↓CollectionAssert.{method}(collection);
            ");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [TestCaseSource(nameof(OneCollectionParameterAsserts))]
        public void AnalyzeOneCollectionWhenOnlyMessageArgumentIsUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                var collection = new[] {{ 1, 2, 3 }};
                ↓CollectionAssert.{method}(collection, ""message"");
            ");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [TestCaseSource(nameof(OneCollectionParameterAsserts))]
        public void AnalyzeOneCollectionWhenFormatAndOneParamsArgumentAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                var collection = new[] {{ 1, 2, 3 }};
                ↓CollectionAssert.{method}(collection, ""Because of {{0}}"", ""message"");
            ");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [TestCaseSource(nameof(OneCollectionParameterAsserts))]
        public void AnalyzeOneCollectionWhenFormatAndTwoParamsArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                var collection = new[] {{ 1, 2, 3 }};
                ↓CollectionAssert.{method}(collection, ""{{0}}, {{1}}"", ""first"", ""second"");
            ");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [TestCaseSource(nameof(TwoCollectionParameterAsserts))]
        public void AnalyzeTwoCollectionWhenNoMessageArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                var collection1 = new[] {{ 1, 2, 3 }};
                var collection2 = new[] {{ 2, 4, 6 }};
                ↓CollectionAssert.{method}(collection1, collection2);
            ");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [TestCaseSource(nameof(TwoCollectionParameterAsserts))]
        public void AnalyzeTwoCollectionWhenOnlyMessageArgumentIsUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                var collection1 = new[] {{ 1, 2, 3 }};
                var collection2 = new[] {{ 2, 4, 6 }};
                ↓CollectionAssert.{method}(collection1, collection2, ""message"");
            ");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [TestCaseSource(nameof(TwoCollectionParameterAsserts))]
        public void AnalyzeTwoCollectionWhenFormatAndOneParamsArgumentAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                var collection1 = new[] {{ 1, 2, 3 }};
                var collection2 = new[] {{ 2, 4, 6 }};
                ↓CollectionAssert.{method}(collection1, collection2, ""Because of {{0}}"", ""message"");
            ");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [TestCaseSource(nameof(TwoCollectionParameterAsserts))]
        public void AnalyzeTwoCollectionWhenFormatAndTwoParamsArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                var collection1 = new[] {{ 1, 2, 3 }};
                var collection2 = new[] {{ 2, 4, 6 }};
                ↓CollectionAssert.{method}(collection1, collection2, ""{{0}}, {{1}}"", ""first"", ""second"");
            ");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [TestCase(NUnitLegacyFrameworkConstants.NameOfCollectionAssertAreEqual)]
        [TestCase(NUnitLegacyFrameworkConstants.NameOfCollectionAssertAreNotEqual)]
        public void AnalyzeTwoCollectionWithComparerWhenFormatAndParamsArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
            [Test]
            public void TestMethod()
            {{
                var collection1 = new[] {{ 1, 2, 3 }};
                var collection2 = new[] {{ 2, 4, 6 }};
                var comparer = new AlwaysEqualComparer();
                ↓CollectionAssert.{method}(collection1, collection2, comparer, ""Because of {{0}}"", ""message"");
            }}

            private sealed class AlwaysEqualComparer : IComparer
            {{
                public int Compare(object? x, object? y) => 0;
            }}
            ", "using System.Collections;");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [TestCaseSource(nameof(CollectionAndItemParameterAsserts))]
        public void AnalyzeCollectionAndItemWhenNoMessageArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                var collection = new[] {{ typeof(byte), typeof(char) }};
                ↓CollectionAssert.{method}(collection, typeof(byte));
            ");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [TestCaseSource(nameof(CollectionAndItemParameterAsserts))]
        public void AnalyzeCollectionAndItemWhenOnlyMessageArgumentIsUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                var collection = new[] {{ typeof(byte), typeof(char) }};
                ↓CollectionAssert.{method}(collection, typeof(byte), ""message"");
            ");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [TestCaseSource(nameof(CollectionAndItemParameterAsserts))]
        public void AnalyzeCollectionAndItemWhenFormatAndOneParamsArgumentAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                var collection = new[] {{ typeof(byte), typeof(char) }};
                ↓CollectionAssert.{method}(collection, typeof(byte), ""Because of {{0}}"", ""message"");
            ");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [TestCaseSource(nameof(CollectionAndItemParameterAsserts))]
        public void AnalyzeCollectionAndItemWhenFormatAndTwoParamsArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                var collection = new[] {{ typeof(byte), typeof(char) }};
                ↓CollectionAssert.{method}(collection, typeof(byte), ""{{0}}, {{1}}"", ""first"", ""second"");
            ");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }
    }
}
