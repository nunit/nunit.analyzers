#if !NUNIT5 // NUnit 5 uses a class constraint hence these rules are not needed as the user code will not even compile.
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.SameAsOnValueTypes.SameAsOnValueTypesAnalyzer>;

namespace NUnit.Analyzers.Tests.SameAsOnValueTypes
{
    public class SameAsOnValueTypesAnalyzerTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.SameAsOnValueTypes);

        [Test]
        public async Task AnalyzeWhenLiteralTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "Assert.That(1, Is.SameAs({|#0:1|}));");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenStructTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var expected = Guid.Empty;
                        var actual = expected;

                        Assert.That(actual, Is.SameAs({|#0:expected|}));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenStructDelegateProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var expected = Guid.Empty;

                        Assert.That(() => expected, Is.SameAs({|#0:expected|}));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenStructTaskProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var expected = Guid.Empty;

                        Assert.That(Task.FromResult(expected), Is.SameAs({|#0:expected|}));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenActualIsReferenceTypeAndExpectedIsValueType()
        {
            var testCode = TestUtility.WrapInTestMethod(
                """Assert.That("3", Is.Not.SameAs({|#0:3|}));""");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenActualIsValueTypeAndExpectedIsReferenceType()
        {
            var testCode = TestUtility.WrapInTestMethod(
                """Assert.That({|#0:3|}, Is.Not.SameAs("3"));""");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenBothArgumentsAreReferenceType()
        {
            var testCode = TestUtility.WrapInTestMethod(
                """Assert.That("3", Is.SameAs("3"));""");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenCollectionsWithValueTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new List<int> { 1 };
                        var expected = new [] { 1 };
                        Assert.That(actual, Is.SameAs(expected));
            """,
                additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeClassicWhenLiteralTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "ClassicAssert.AreSame({|#0:1|}, 1);");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeClassicWhenStructTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var expected = Guid.Empty;
                        var actual = expected;

                        ClassicAssert.AreSame({|#0:expected|}, actual);
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeClassicWhenActualIsReferenceTypeAndExpectedIsValueType()
        {
            var testCode = TestUtility.WrapInTestMethod(
                """ClassicAssert.AreNotSame({|#0:3|}, "3");""");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeClassicWhenActualIsValueTypeAndExpectedIsReferenceType()
        {
            var testCode = TestUtility.WrapInTestMethod(
                """ClassicAssert.AreNotSame("3", {|#0:3|});""");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeClassicWhenBothArgumentsAreReferenceType()
        {
            var testCode = TestUtility.WrapInTestMethod(
                """ClassicAssert.AreSame("3", "3");""");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenMoreThanOneConstraintExpressionIsUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(
                """Assert.That("1", Is.Not.SameAs("1") & Is.Not.SameAs({|#0:2|}));""");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }
    }
}
#endif
