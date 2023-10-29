using System.Collections.Generic;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.CollectionAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.CollectionAssertUsage
{
    [TestFixture]
    internal sealed class CollectionAssertUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new CollectionAssertUsageAnalyzer();
        private static readonly CodeFixProvider fix = new CollectionAssertUsageCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.CollectionAssertUsage);

        private static IEnumerable<string> OneCollectionParameterAsserts => CollectionAssertUsageAnalyzer.OneCollectionParameterAsserts.Keys;

        private static IEnumerable<string> TwoCollectionParameterAsserts => CollectionAssertUsageAnalyzer.TwoCollectionParameterAsserts.Keys;

        private static IEnumerable<string> CollectionAndItemParameterAsserts => CollectionAssertUsageAnalyzer.CollectionAndItemParameterAsserts.Keys;

        [TestCaseSource(nameof(OneCollectionParameterAsserts))]
        public void AnalyzeOneCollectionWhenNoMessageArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ 1, 2, 3 }};
            ↓CollectionAssert.{method}(collection);
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ 1, 2, 3 }};
            Assert.That(collection, {CollectionAssertUsageAnalyzer.OneCollectionParameterAsserts[method]});
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(OneCollectionParameterAsserts))]
        public void AnalyzeOneCollectionWhenOnlyMessageArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ 1, 2, 3 }};
            ↓CollectionAssert.{method}(collection, ""message"");
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ 1, 2, 3 }};
            Assert.That(collection, {CollectionAssertUsageAnalyzer.OneCollectionParameterAsserts[method]}, ""message"");
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(OneCollectionParameterAsserts))]
        public void AnalyzeOneCollectionWhenFormatAndParamsArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ 1, 2, 3 }};
            ↓CollectionAssert.{method}(collection, ""Because of {{0}}"", ""message"");
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ 1, 2, 3 }};
            Assert.That(collection, {CollectionAssertUsageAnalyzer.OneCollectionParameterAsserts[method]}, $""Because of {{""message""}}"");
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCase(NUnitFrameworkConstants.NameOfCollectionAssertIsOrdered)]
        public void AnalyzeOneCollectionWithComparerWhenFormatAndParamsArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ 2, 4, 6 }};
            IComparer comparer = Comparer<int>.Default;
            ↓CollectionAssert.{method}(collection, comparer, ""Because of {{0}}"", ""message"");
            ", "using System.Collections;using System.Collections.Generic;");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ 2, 4, 6 }};
            IComparer comparer = Comparer<int>.Default;
            Assert.That(collection, {CollectionAssertUsageAnalyzer.OneCollectionParameterAsserts[method]}.Using(comparer), $""Because of {{""message""}}"");
            ", "using System.Collections;using System.Collections.Generic;");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(TwoCollectionParameterAsserts))]
        public void AnalyzeTwoCollectionWhenNoMessageArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            var collection1 = new[] {{ 1, 2, 3 }};
            var collection2 = new[] {{ 2, 4, 6 }};
            ↓CollectionAssert.{method}(collection1, collection2);
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection1 = new[] {{ 1, 2, 3 }};
            var collection2 = new[] {{ 2, 4, 6 }};
            Assert.That({GetAdjustedTwoCollectionConstraint(method)});
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(TwoCollectionParameterAsserts))]
        public void AnalyzeTwoCollectionWhenOnlyMessageArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            var collection1 = new[] {{ 1, 2, 3 }};
            var collection2 = new[] {{ 2, 4, 6 }};
            ↓CollectionAssert.{method}(collection1, collection2, ""message"");
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection1 = new[] {{ 1, 2, 3 }};
            var collection2 = new[] {{ 2, 4, 6 }};
            Assert.That({GetAdjustedTwoCollectionConstraint(method)}, ""message"");
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(TwoCollectionParameterAsserts))]
        public void AnalyzeTwoCollectionWhenFormatAndParamsArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            var collection1 = new[] {{ 1, 2, 3 }};
            var collection2 = new[] {{ 2, 4, 6 }};
            ↓CollectionAssert.{method}(collection1, collection2, ""Because of {{0}}"", ""message"");
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection1 = new[] {{ 1, 2, 3 }};
            var collection2 = new[] {{ 2, 4, 6 }};
            Assert.That({GetAdjustedTwoCollectionConstraint(method)}, $""Because of {{""message""}}"");
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCase(NUnitFrameworkConstants.NameOfCollectionAssertAreEqual)]
        [TestCase(NUnitFrameworkConstants.NameOfCollectionAssertAreNotEqual)]
        public void AnalyzeTwoCollectionWithComparerWhenFormatAndParamsArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            var collection1 = new[] {{ 1, 2, 3 }};
            var collection2 = new[] {{ 2, 4, 6 }};
            IComparer comparer = Comparer<int>.Default;
            ↓CollectionAssert.{method}(collection1, collection2, comparer, ""Because of {{0}}"", ""message"");
            ", "using System.Collections;using System.Collections.Generic;");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection1 = new[] {{ 1, 2, 3 }};
            var collection2 = new[] {{ 2, 4, 6 }};
            IComparer comparer = Comparer<int>.Default;
            Assert.That({GetAdjustedTwoCollectionConstraint(method).Replace(".AsCollection", ".Using(comparer)")}, $""Because of {{""message""}}"");
            ", "using System.Collections;using System.Collections.Generic;");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(CollectionAndItemParameterAsserts))]
        public void AnalyzeCollectionAndItemWhenNoMessageArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ typeof(byte), typeof(char) }};
            var expected = typeof(byte);
            ↓CollectionAssert.{method}(collection, expected);
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ typeof(byte), typeof(char) }};
            var expected = typeof(byte);
            Assert.That(collection, {CollectionAssertUsageAnalyzer.CollectionAndItemParameterAsserts[method]});
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(CollectionAndItemParameterAsserts))]
        public void AnalyzeCollectionAndItemWhenOnlyMessageArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ typeof(byte), typeof(char) }};
            var expected = typeof(byte);
            ↓CollectionAssert.{method}(collection, expected, ""message"");
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ typeof(byte), typeof(char) }};
            var expected = typeof(byte);
            Assert.That(collection, {CollectionAssertUsageAnalyzer.CollectionAndItemParameterAsserts[method]}, ""message"");
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(CollectionAndItemParameterAsserts))]
        public void AnalyzeCollectionAndItemWhenFormatAndParamsArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ typeof(byte), typeof(char) }};
            var expected = typeof(byte);
            ↓CollectionAssert.{method}(collection, expected, ""Because of {{0}}"", ""message"");
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ typeof(byte), typeof(char) }};
            var expected = typeof(byte);
            Assert.That(collection, {CollectionAssertUsageAnalyzer.CollectionAndItemParameterAsserts[method]}, $""Because of {{""message""}}"");
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        private static string GetAdjustedTwoCollectionConstraint(string method)
        {
            string actualArgument;
            string constraintArgument;

            if (CollectionAssertUsageCodeFix.CollectionAssertToOneUnswappedParameterConstraints.ContainsKey(method))
            {
                actualArgument = "collection1";
                constraintArgument = "collection2";
            }
            else
            {
                actualArgument = "collection2";
                constraintArgument = "collection1";
            }

            string constraint = CollectionAssertUsageAnalyzer.TwoCollectionParameterAsserts[method]
                                                             .Replace("expected", constraintArgument);
            return $"{actualArgument}, {constraint}";
        }
    }
}
