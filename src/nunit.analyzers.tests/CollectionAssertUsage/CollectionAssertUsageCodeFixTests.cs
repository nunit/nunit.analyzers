using System.Collections.Generic;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.ClassicModelAssertUsage;
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
        public void AnalyzeOneCollectionWhenOnlyMessageArgumentIsUsed(string method)
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
        public void AnalyzeOneCollectionWhenFormatAndOneParamsArgumentAreUsed(string method)
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

        [TestCaseSource(nameof(OneCollectionParameterAsserts))]
        public void AnalyzeOneCollectionWhenFormatAndTwoParamsArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ 1, 2, 3 }};
            ↓CollectionAssert.{method}(collection, ""{{0}}, {{1}}"", ""first"", ""second"");
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ 1, 2, 3 }};
            Assert.That(collection, {CollectionAssertUsageAnalyzer.OneCollectionParameterAsserts[method]}, $""{{""first""}}, {{""second""}}"");
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(OneCollectionParameterAsserts))]
        public void AnalyzeOneCollectionWhenFormatAndParamsArgumentsAreUsedOutOfOrder(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ 1, 2, 3 }};
            ↓CollectionAssert.{method}(args: new[] {{ ""first"", ""second"" }}, message: ""{{0}}, {{1}}"", collection: collection);
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ 1, 2, 3 }};
            Assert.That(collection, {CollectionAssertUsageAnalyzer.OneCollectionParameterAsserts[method]}, $""{{""first""}}, {{""second""}}"");
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

        [Test]
        public void CodeFixForOneCollectionParameterAssertMaintainsReasonableTriviaWithEndOfLineClosingParen(
            [ValueSource(nameof(OneCollectionParameterAsserts))] string method,
            [Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage ? ",\r\n                \"message\"" : string.Empty;
            var code = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ 1, 2, 3 }};
            ↓CollectionAssert.{method}(
                collection{commaAndMessage});");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ 1, 2, 3 }};
            Assert.That(
                collection,
                {CollectionAssertUsageAnalyzer.OneCollectionParameterAsserts[method]}{commaAndMessage});");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void CodeFixForOneCollectionParameterAssertMaintainsReasonableTriviaWithNewLineClosingParen(
            [ValueSource(nameof(OneCollectionParameterAsserts))] string method,
            [Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage ? ",\r\n                \"message\"" : string.Empty;
            var code = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ 1, 2, 3 }};
            ↓CollectionAssert.{method}(
                collection{commaAndMessage}
            );");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ 1, 2, 3 }};
            Assert.That(
                collection,
                {CollectionAssertUsageAnalyzer.OneCollectionParameterAsserts[method]}{commaAndMessage}
            );");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void CodeFixForTwoCollectionParameterAssertMaintainsReasonableTriviaWithEndOfLineClosingParen(
            [ValueSource(nameof(TwoCollectionParameterAsserts))] string method,
            [Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage ? ",\r\n                \"message\"" : string.Empty;
            var code = TestUtility.WrapInTestMethod(@$"
            var collection1 = new[] {{ 1, 2, 3 }};
            var collection2 = new[] {{ 2, 4, 6 }};
            ↓CollectionAssert.{method}(
                collection1,
                collection2{commaAndMessage});");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection1 = new[] {{ 1, 2, 3 }};
            var collection2 = new[] {{ 2, 4, 6 }};
            Assert.That(
                {GetAdjustedTwoCollectionConstraint(method, insertNewline: true)}{commaAndMessage});");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void CodeFixForTwoCollectionParameterAssertMaintainsReasonableTriviaWithNewLineClosingParen(
            [ValueSource(nameof(TwoCollectionParameterAsserts))] string method,
            [Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage ? ",\r\n                \"message\"" : string.Empty;
            var code = TestUtility.WrapInTestMethod(@$"
            var collection1 = new[] {{ 1, 2, 3 }};
            var collection2 = new[] {{ 2, 4, 6 }};
            ↓CollectionAssert.{method}(
                collection1,
                collection2{commaAndMessage}
            );");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection1 = new[] {{ 1, 2, 3 }};
            var collection2 = new[] {{ 2, 4, 6 }};
            Assert.That(
                {GetAdjustedTwoCollectionConstraint(method, insertNewline: true)}{commaAndMessage}
            );");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void CodeFixForCollectionAndItemParameterAssertMaintainsReasonableTriviaWithEndOfLineClosingParen(
            [ValueSource(nameof(CollectionAndItemParameterAsserts))] string method,
            [Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage ? ",\r\n                \"message\"" : string.Empty;
            var code = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ typeof(byte), typeof(char) }};
            var expected = typeof(byte);
            ↓CollectionAssert.{method}(
                collection,
                expected{commaAndMessage});");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ typeof(byte), typeof(char) }};
            var expected = typeof(byte);
            Assert.That(
                collection,
                {CollectionAssertUsageAnalyzer.CollectionAndItemParameterAsserts[method]}{commaAndMessage});");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void CodeFixForCollectionAndItemParameterAssertMaintainsReasonableTriviaWithNewLineClosingParen(
            [ValueSource(nameof(CollectionAndItemParameterAsserts))] string method,
            [Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage ? ",\r\n                \"message\"" : string.Empty;
            var code = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ typeof(byte), typeof(char) }};
            var expected = typeof(byte);
            ↓CollectionAssert.{method}(
                collection,
                expected{commaAndMessage}
            );");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ typeof(byte), typeof(char) }};
            var expected = typeof(byte);
            Assert.That(
                collection,
                {CollectionAssertUsageAnalyzer.CollectionAndItemParameterAsserts[method]}{commaAndMessage}
            );");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(TwoCollectionParameterAsserts))]
        public void AnalyzeTwoCollectionWhenOnlyMessageArgumentIsUsed(string method)
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
        public void AnalyzeTwoCollectionWhenFormatAndOneParamsArgumentAreUsed(string method)
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

        [TestCaseSource(nameof(TwoCollectionParameterAsserts))]
        public void AnalyzeTwoCollectionWhenFormatAndTwoParamsArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            var collection1 = new[] {{ 1, 2, 3 }};
            var collection2 = new[] {{ 2, 4, 6 }};
            ↓CollectionAssert.{method}(collection1, collection2, ""{{0}}, {{1}}"", ""first"", ""second"");
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection1 = new[] {{ 1, 2, 3 }};
            var collection2 = new[] {{ 2, 4, 6 }};
            Assert.That({GetAdjustedTwoCollectionConstraint(method)}, $""{{""first""}}, {{""second""}}"");
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(TwoCollectionParameterAsserts))]
        public void AnalyzeTwoCollectionWhenFormatAndArgsArrayAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            var collection1 = new[] {{ 1, 2, 3 }};
            var collection2 = new[] {{ 2, 4, 6 }};
            var args = new[] {{ ""first"", ""second"" }};
            ↓CollectionAssert.{method}(collection1, collection2, ""{{0}}, {{1}}"", args);
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection1 = new[] {{ 1, 2, 3 }};
            var collection2 = new[] {{ 2, 4, 6 }};
            var args = new[] {{ ""first"", ""second"" }};
            Assert.That({GetAdjustedTwoCollectionConstraint(method)}, () => string.Format(""{{0}}, {{1}}"", args));
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(TwoCollectionParameterAsserts))]
        public void AnalyzeTwoCollectionWhenFormatVariableAndTwoParamsArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            var collection1 = new[] {{ 1, 2, 3 }};
            var collection2 = new[] {{ 2, 4, 6 }};
            const string formatSpecification = ""{{0}}, {{1}}"";
            ↓CollectionAssert.{method}(collection1, collection2, formatSpecification, ""first"", ""second"");
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection1 = new[] {{ 1, 2, 3 }};
            var collection2 = new[] {{ 2, 4, 6 }};
            const string formatSpecification = ""{{0}}, {{1}}"";
            Assert.That({GetAdjustedTwoCollectionConstraint(method)}, () => string.Format(formatSpecification, ""first"", ""second""));
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(TwoCollectionParameterAsserts))]
        public void AnalyzeTwoCollectionWhenFormatAndParamsArgumentsAreUsedOutOfOrder(string method)
        {
            var firstParameterName = CollectionAssertUsageCodeFix.CollectionAssertToFirstParameterName[method];
            var secondParameterName = CollectionAssertUsageCodeFix.CollectionAssertToSecondParameterName[method];
            var code = TestUtility.WrapInTestMethod(@$"
            var collection1 = new[] {{ 1, 2, 3 }};
            var collection2 = new[] {{ 2, 4, 6 }};
            ↓CollectionAssert.{method}(args: new[] {{ ""first"", ""second"" }}, message: ""{{0}}, {{1}}"", {secondParameterName}: collection2, {firstParameterName}: collection1);
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection1 = new[] {{ 1, 2, 3 }};
            var collection2 = new[] {{ 2, 4, 6 }};
            Assert.That({GetAdjustedTwoCollectionConstraint(method)}, $""{{""first""}}, {{""second""}}"");
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
        public void AnalyzeCollectionAndItemWhenOnlyMessageArgumentIsUsed(string method)
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
        public void AnalyzeCollectionAndItemWhenFormatAndOneParamsArgumentAreUsed(string method)
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

        [TestCaseSource(nameof(CollectionAndItemParameterAsserts))]
        public void AnalyzeCollectionAndItemWhenFormatAndTwoParamsArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ typeof(byte), typeof(char) }};
            var expected = typeof(byte);
            ↓CollectionAssert.{method}(collection, expected, ""{{0}}, {{1}}"", ""first"", ""second"");
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ typeof(byte), typeof(char) }};
            var expected = typeof(byte);
            Assert.That(collection, {CollectionAssertUsageAnalyzer.CollectionAndItemParameterAsserts[method]}, $""{{""first""}}, {{""second""}}"");
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(CollectionAndItemParameterAsserts))]
        public void AnalyzeCollectionAndItemWhenFormatAndParamsArgumentsAreUsedOutOfOrder(string method)
        {
            var secondParameterName = CollectionAssertUsageCodeFix.CollectionAssertToSecondParameterName[method];
            var code = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ typeof(byte), typeof(char) }};
            var expected = typeof(byte);
            ↓CollectionAssert.{method}(args: new[] {{ ""first"", ""second"" }}, message: ""{{0}}, {{1}}"", {secondParameterName}: expected, collection: collection);
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ typeof(byte), typeof(char) }};
            var expected = typeof(byte);
            Assert.That(collection, {CollectionAssertUsageAnalyzer.CollectionAndItemParameterAsserts[method]}, $""{{""first""}}, {{""second""}}"");
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void CodeFixForTwoCollectionParameterAssertsMaintainsReasonableTriviaWithAllArgumentsOnSameLine(
            [ValueSource(nameof(TwoCollectionParameterAsserts))] string method,
            [Values] bool newlineBeforeClosingParen)
        {
            var optionalNewline = newlineBeforeClosingParen ? "\r\n            " : string.Empty;
            var code = TestUtility.WrapInTestMethod(@$"
            var collection1 = new[] {{ 1, 2, 3 }};
            var collection2 = new[] {{ 2, 4, 6 }};
            ↓CollectionAssert.{method}(
                collection1,
                collection2{optionalNewline});");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection1 = new[] {{ 1, 2, 3 }};
            var collection2 = new[] {{ 2, 4, 6 }};
            Assert.That(
                {GetAdjustedTwoCollectionConstraint(method, insertNewline: true)}{optionalNewline});");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public void CodeFixForCollectionAndItemParameterAssertMaintainsReasonableTriviaWithAllArgumentsOnSameLine(
            [ValueSource(nameof(CollectionAndItemParameterAsserts))] string method,
            [Values] bool newlineBeforeClosingParen)
        {
            var optionalNewline = newlineBeforeClosingParen ? "\r\n            " : string.Empty;
            var code = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ typeof(byte), typeof(char) }};
            var expected = typeof(byte);
            ↓CollectionAssert.{method}(
                collection,
                expected{optionalNewline});");
            var fixedCode = TestUtility.WrapInTestMethod(@$"
            var collection = new[] {{ typeof(byte), typeof(char) }};
            var expected = typeof(byte);
            Assert.That(
                collection,
                {CollectionAssertUsageAnalyzer.CollectionAndItemParameterAsserts[method]}{optionalNewline});");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        private static string GetAdjustedTwoCollectionConstraint(string method, bool insertNewline = false)
        {
            (string actualArgument, string constraintArgument) =
                CollectionAssertUsageCodeFix.CollectionAssertToOneUnswappedParameterConstraints.ContainsKey(method)
                    ? ("collection1", "collection2")
                    : ("collection2", "collection1");

            string constraint = CollectionAssertUsageAnalyzer.TwoCollectionParameterAsserts[method]
                                                             .Replace("expected", constraintArgument);
            return insertNewline
                ? $@"{actualArgument},
                {constraint}"
                : $"{actualArgument}, {constraint}";
        }
    }
}
