using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.CollectionAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.CollectionAssertUsage.CollectionAssertUsageAnalyzer, NUnit.Analyzers.CollectionAssertUsage.CollectionAssertUsageCodeFix>;

namespace NUnit.Analyzers.Tests.CollectionAssertUsage
{
    [TestFixture]
    internal sealed class CollectionAssertUsageCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.CollectionAssertUsage);

        private static IEnumerable<string> OneCollectionParameterAsserts => CollectionAssertUsageAnalyzer.OneCollectionParameterAsserts.Keys;

        private static IEnumerable<string> TwoCollectionParameterAsserts => CollectionAssertUsageAnalyzer.TwoCollectionParameterAsserts.Keys;

        private static IEnumerable<string> CollectionAndItemParameterAsserts => CollectionAssertUsageAnalyzer.CollectionAndItemParameterAsserts.Keys;

        [TestCaseSource(nameof(OneCollectionParameterAsserts))]
        public async Task AnalyzeOneCollectionWhenNoMessageArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { 1, 2, 3 };
                        {|#0:CollectionAssert.{{method}}(collection)|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { 1, 2, 3 };
                        Assert.That(collection, {{CollectionAssertUsageAnalyzer.OneCollectionParameterAsserts[method]}});

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(OneCollectionParameterAsserts))]
        public async Task AnalyzeOneCollectionWhenOnlyMessageArgumentIsUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { 1, 2, 3 };
                        {|#0:CollectionAssert.{{method}}(collection, "message")|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { 1, 2, 3 };
                        Assert.That(collection, {{CollectionAssertUsageAnalyzer.OneCollectionParameterAsserts[method]}}, "message");

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(OneCollectionParameterAsserts))]
        public async Task AnalyzeOneCollectionWhenFormatAndOneParamsArgumentAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { 1, 2, 3 };
                        {|#0:CollectionAssert.{{method}}(collection, "Because of {0}", "message")|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { 1, 2, 3 };
                        Assert.That(collection, {{CollectionAssertUsageAnalyzer.OneCollectionParameterAsserts[method]}}, $"Because of {"message"}");

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(OneCollectionParameterAsserts))]
        public async Task AnalyzeOneCollectionWhenFormatAndTwoParamsArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { 1, 2, 3 };
                        {|#0:CollectionAssert.{{method}}(collection, "{0}, {1}", "first", "second")|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { 1, 2, 3 };
                        Assert.That(collection, {{CollectionAssertUsageAnalyzer.OneCollectionParameterAsserts[method]}}, $"{"first"}, {"second"}");

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(OneCollectionParameterAsserts))]
        public async Task AnalyzeOneCollectionWhenFormatAndParamsArgumentsAreUsedOutOfOrder(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { 1, 2, 3 };
                        {|#0:CollectionAssert.{{method}}(args: new[] { "first", "second" }, message: "{0}, {1}", collection: collection)|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { 1, 2, 3 };
                        Assert.That(collection, {{CollectionAssertUsageAnalyzer.OneCollectionParameterAsserts[method]}}, $"{"first"}, {"second"}");

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCase(NUnitLegacyFrameworkConstants.NameOfCollectionAssertIsOrdered)]
        public async Task AnalyzeOneCollectionWithComparerWhenFormatAndParamsArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { 2, 4, 6 };
                        IComparer comparer = Comparer<int>.Default;
                        {|#0:CollectionAssert.{{method}}(collection, comparer, "Because of {0}", "message")|};

            """, "using System.Collections;using System.Collections.Generic;");
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { 2, 4, 6 };
                        IComparer comparer = Comparer<int>.Default;
                        Assert.That(collection, {{CollectionAssertUsageAnalyzer.OneCollectionParameterAsserts[method]}}.Using(comparer), $"Because of {"message"}");

            """, "using System.Collections;using System.Collections.Generic;");
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(TwoCollectionParameterAsserts))]
        public async Task AnalyzeTwoCollectionWhenNoMessageArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        {|#0:CollectionAssert.{{method}}(collection1, collection2)|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        Assert.That({{GetAdjustedTwoCollectionConstraint(method)}});

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task CodeFixForOneCollectionParameterAssertMaintainsReasonableTriviaWithEndOfLineClosingParen(
            [ValueSource(nameof(OneCollectionParameterAsserts))] string method,
            [Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage ? $",{Environment.NewLine}                \"message\"" : string.Empty;
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { 1, 2, 3 };
                        {|#0:CollectionAssert.{{method}}(
                            collection{{commaAndMessage}})|};
            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { 1, 2, 3 };
                        Assert.That(
                            collection,
                            {{CollectionAssertUsageAnalyzer.OneCollectionParameterAsserts[method]}}{{commaAndMessage}});
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task CodeFixForOneCollectionParameterAssertMaintainsReasonableTriviaWithNewLineClosingParen(
            [ValueSource(nameof(OneCollectionParameterAsserts))] string method,
            [Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage ? $",{Environment.NewLine}                \"message\"" : string.Empty;
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { 1, 2, 3 };
                        {|#0:CollectionAssert.{{method}}(
                            collection{{commaAndMessage}}
                        )|};
            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { 1, 2, 3 };
                        Assert.That(
                            collection,
                            {{CollectionAssertUsageAnalyzer.OneCollectionParameterAsserts[method]}}{{commaAndMessage}}
                        );
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task CodeFixForTwoCollectionParameterAssertMaintainsReasonableTriviaWithEndOfLineClosingParen(
            [ValueSource(nameof(TwoCollectionParameterAsserts))] string method,
            [Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage ? $",{Environment.NewLine}                \"message\"" : string.Empty;
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        {|#0:CollectionAssert.{{method}}(
                            collection1,
                            collection2{{commaAndMessage}})|};
            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        Assert.That(
                            {{GetAdjustedTwoCollectionConstraint(method, insertNewline: true)}}{{commaAndMessage}});
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task CodeFixForTwoCollectionParameterAssertMaintainsReasonableTriviaWithNewLineClosingParen(
            [ValueSource(nameof(TwoCollectionParameterAsserts))] string method,
            [Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage ? $",{Environment.NewLine}                \"message\"" : string.Empty;
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        {|#0:CollectionAssert.{{method}}(
                            collection1,
                            collection2{{commaAndMessage}}
                        )|};
            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        Assert.That(
                            {{GetAdjustedTwoCollectionConstraint(method, insertNewline: true)}}{{commaAndMessage}}
                        );
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task CodeFixForCollectionAndItemParameterAssertMaintainsReasonableTriviaWithEndOfLineClosingParen(
            [ValueSource(nameof(CollectionAndItemParameterAsserts))] string method,
            [Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage ? $",{Environment.NewLine}                \"message\"" : string.Empty;
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { typeof(byte), typeof(char) };
                        var expected = typeof(byte);
                        {|#0:CollectionAssert.{{method}}(
                            collection,
                            expected{{commaAndMessage}})|};
            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { typeof(byte), typeof(char) };
                        var expected = typeof(byte);
                        Assert.That(
                            collection,
                            {{CollectionAssertUsageAnalyzer.CollectionAndItemParameterAsserts[method]}}{{commaAndMessage}});
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task CodeFixForCollectionAndItemParameterAssertMaintainsReasonableTriviaWithNewLineClosingParen(
            [ValueSource(nameof(CollectionAndItemParameterAsserts))] string method,
            [Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage ? $",{Environment.NewLine}                \"message\"" : string.Empty;
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { typeof(byte), typeof(char) };
                        var expected = typeof(byte);
                        {|#0:CollectionAssert.{{method}}(
                            collection,
                            expected{{commaAndMessage}}
                        )|};
            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { typeof(byte), typeof(char) };
                        var expected = typeof(byte);
                        Assert.That(
                            collection,
                            {{CollectionAssertUsageAnalyzer.CollectionAndItemParameterAsserts[method]}}{{commaAndMessage}}
                        );
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(TwoCollectionParameterAsserts))]
        public async Task AnalyzeTwoCollectionWhenOnlyMessageArgumentIsUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        {|#0:CollectionAssert.{{method}}(collection1, collection2, "message")|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        Assert.That({{GetAdjustedTwoCollectionConstraint(method)}}, "message");

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(TwoCollectionParameterAsserts))]
        public async Task AnalyzeTwoCollectionWhenFormatAndOneParamsArgumentAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        {|#0:CollectionAssert.{{method}}(collection1, collection2, "Because of {0}", "message")|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        Assert.That({{GetAdjustedTwoCollectionConstraint(method)}}, $"Because of {"message"}");

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(TwoCollectionParameterAsserts))]
        public async Task AnalyzeTwoCollectionWhenFormatAndTwoParamsArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        {|#0:CollectionAssert.{{method}}(collection1, collection2, "{0}, {1}", "first", "second")|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        Assert.That({{GetAdjustedTwoCollectionConstraint(method)}}, $"{"first"}, {"second"}");

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(TwoCollectionParameterAsserts))]
        public async Task AnalyzeTwoCollectionWhenFormatAndArgsArrayAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        var args = new[] { "first", "second" };
                        {|#0:CollectionAssert.{{method}}(collection1, collection2, "{0}, {1}", args)|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        var args = new[] { "first", "second" };
                        Assert.That({{GetAdjustedTwoCollectionConstraint(method)}}, () => string.Format("{0}, {1}", args));

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(TwoCollectionParameterAsserts))]
        public async Task AnalyzeTwoCollectionWhenFormatVariableAndTwoParamsArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        const string formatSpecification = "{0}, {1}";
                        {|#0:CollectionAssert.{{method}}(collection1, collection2, formatSpecification, "first", "second")|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        const string formatSpecification = "{0}, {1}";
                        Assert.That({{GetAdjustedTwoCollectionConstraint(method)}}, () => string.Format(formatSpecification, "first", "second"));

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(TwoCollectionParameterAsserts))]
        public async Task AnalyzeTwoCollectionWhenFormatAndParamsArgumentsAreUsedOutOfOrder(string method)
        {
            var firstParameterName = CollectionAssertUsageCodeFix.CollectionAssertToFirstParameterName[method];
            var secondParameterName = CollectionAssertUsageCodeFix.CollectionAssertToSecondParameterName[method];
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        {|#0:CollectionAssert.{{method}}(args: new[] { "first", "second" }, message: "{0}, {1}", {{secondParameterName}}: collection2, {{firstParameterName}}: collection1)|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        Assert.That({{GetAdjustedTwoCollectionConstraint(method)}}, $"{"first"}, {"second"}");

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCase(NUnitLegacyFrameworkConstants.NameOfCollectionAssertAreEqual)]
        [TestCase(NUnitLegacyFrameworkConstants.NameOfCollectionAssertAreNotEqual)]
        public async Task AnalyzeTwoCollectionWithComparerWhenFormatAndParamsArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        IComparer comparer = Comparer<int>.Default;
                        {|#0:CollectionAssert.{{method}}(collection1, collection2, comparer, "Because of {0}", "message")|};

            """, "using System.Collections;using System.Collections.Generic;");
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        IComparer comparer = Comparer<int>.Default;
                        Assert.That({{GetAdjustedTwoCollectionConstraint(method).Replace(".AsCollection", ".Using(comparer)")}}, $"Because of {"message"}");

            """, "using System.Collections;using System.Collections.Generic;");
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(CollectionAndItemParameterAsserts))]
        public async Task AnalyzeCollectionAndItemWhenNoMessageArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { typeof(byte), typeof(char) };
                        var expected = typeof(byte);
                        {|#0:CollectionAssert.{{method}}(collection, expected)|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { typeof(byte), typeof(char) };
                        var expected = typeof(byte);
                        Assert.That(collection, {{CollectionAssertUsageAnalyzer.CollectionAndItemParameterAsserts[method]}});

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(CollectionAndItemParameterAsserts))]
        public async Task AnalyzeCollectionAndItemWhenOnlyMessageArgumentIsUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { typeof(byte), typeof(char) };
                        var expected = typeof(byte);
                        {|#0:CollectionAssert.{{method}}(collection, expected, "message")|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { typeof(byte), typeof(char) };
                        var expected = typeof(byte);
                        Assert.That(collection, {{CollectionAssertUsageAnalyzer.CollectionAndItemParameterAsserts[method]}}, "message");

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(CollectionAndItemParameterAsserts))]
        public async Task AnalyzeCollectionAndItemWhenFormatAndOneParamsArgumentAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { typeof(byte), typeof(char) };
                        var expected = typeof(byte);
                        {|#0:CollectionAssert.{{method}}(collection, expected, "Because of {0}", "message")|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { typeof(byte), typeof(char) };
                        var expected = typeof(byte);
                        Assert.That(collection, {{CollectionAssertUsageAnalyzer.CollectionAndItemParameterAsserts[method]}}, $"Because of {"message"}");

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(CollectionAndItemParameterAsserts))]
        public async Task AnalyzeCollectionAndItemWhenFormatAndTwoParamsArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { typeof(byte), typeof(char) };
                        var expected = typeof(byte);
                        {|#0:CollectionAssert.{{method}}(collection, expected, "{0}, {1}", "first", "second")|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { typeof(byte), typeof(char) };
                        var expected = typeof(byte);
                        Assert.That(collection, {{CollectionAssertUsageAnalyzer.CollectionAndItemParameterAsserts[method]}}, $"{"first"}, {"second"}");

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(CollectionAndItemParameterAsserts))]
        public async Task AnalyzeCollectionAndItemWhenFormatAndParamsArgumentsAreUsedOutOfOrder(string method)
        {
            var secondParameterName = CollectionAssertUsageCodeFix.CollectionAssertToSecondParameterName[method];
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { typeof(byte), typeof(char) };
                        var expected = typeof(byte);
                        {|#0:CollectionAssert.{{method}}(args: new[] { "first", "second" }, message: "{0}, {1}", {{secondParameterName}}: expected, collection: collection)|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { typeof(byte), typeof(char) };
                        var expected = typeof(byte);
                        Assert.That(collection, {{CollectionAssertUsageAnalyzer.CollectionAndItemParameterAsserts[method]}}, $"{"first"}, {"second"}");

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task CodeFixForTwoCollectionParameterAssertsMaintainsReasonableTriviaWithAllArgumentsOnSameLine(
            [ValueSource(nameof(TwoCollectionParameterAsserts))] string method,
            [Values] bool newlineBeforeClosingParen)
        {
            var optionalNewline = newlineBeforeClosingParen ? $"{Environment.NewLine}            " : string.Empty;
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        {|#0:CollectionAssert.{{method}}(
                            collection1,
                            collection2{{optionalNewline}})|};
            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection1 = new[] { 1, 2, 3 };
                        var collection2 = new[] { 2, 4, 6 };
                        Assert.That(
                            {{GetAdjustedTwoCollectionConstraint(method, insertNewline: true)}}{{optionalNewline}});
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task CodeFixForCollectionAndItemParameterAssertMaintainsReasonableTriviaWithAllArgumentsOnSameLine(
            [ValueSource(nameof(CollectionAndItemParameterAsserts))] string method,
            [Values] bool newlineBeforeClosingParen)
        {
            var optionalNewline = newlineBeforeClosingParen ? $"{Environment.NewLine}            " : string.Empty;
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { typeof(byte), typeof(char) };
                        var expected = typeof(byte);
                        {|#0:CollectionAssert.{{method}}(
                            collection,
                            expected{{optionalNewline}})|};
            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { typeof(byte), typeof(char) };
                        var expected = typeof(byte);
                        Assert.That(
                            collection,
                            {{CollectionAssertUsageAnalyzer.CollectionAndItemParameterAsserts[method]}}{{optionalNewline}});
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
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
                ? $$"""
                {{actualArgument}},
                                {{constraint}}
                """
                : $"{actualArgument}, {constraint}";
        }
    }
}
