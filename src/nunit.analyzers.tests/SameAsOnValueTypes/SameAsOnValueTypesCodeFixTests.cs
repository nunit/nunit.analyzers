#if !NUNIT5 // NUnit 5 uses a class constraint hence these rules are not needed as the user code will not even compile.
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.SameAsOnValueTypes;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.SameAsOnValueTypes.SameAsOnValueTypesAnalyzer, NUnit.Analyzers.SameAsOnValueTypes.SameAsOnValueTypesCodeFix>;

namespace NUnit.Analyzers.Tests.SameAsOnValueTypes
{
    [TestFixture]
    public sealed class SameAsOnValueTypesCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.SameAsOnValueTypes);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new SameAsOnValueTypesCodeFix();
            var ids = new SameAsOnValueTypesCodeFix().FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.SameAsOnValueTypes }));
        }

        [Test]
        public async Task VerifySameAsIntoEqualToFix()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var expected = Guid.Empty;
                        var actual = expected;

                        Assert.That(actual, Is.SameAs({|#0:expected|}));

            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        var expected = Guid.Empty;
                        var actual = expected;

                        Assert.That(actual, Is.EqualTo(expected));

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, SameAsOnValueTypesCodeFix.UseIsEqualToDescription);
        }

        [Test]
        public async Task VerifySameAsIntoEqualToFixWithMessage()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var expected = Guid.Empty;

                        Assert.That(() => expected, Is.Not.SameAs({|#0:expected|}), "message");

            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        var expected = Guid.Empty;

                        Assert.That(() => expected, Is.Not.EqualTo(expected), "message");

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, SameAsOnValueTypesCodeFix.UseIsEqualToDescription);
        }

#if !NUNIT4
        [Test]
        public async Task VerifySameAsIntoEqualToFixWithMessageAndParams()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var expected = Guid.Empty;

                        Assert.That(Task.FromResult(expected), Is.SameAs({|#0:expected|}), "message", Guid.Empty);

            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        var expected = Guid.Empty;

                        Assert.That(Task.FromResult(expected), Is.EqualTo(expected), "message", Guid.Empty);

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, SameAsOnValueTypesCodeFix.UseIsEqualToDescription);
        }
#endif

        [Test]
        public async Task VerifyAreSameIntoAreEqualFix()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var expected = Guid.Empty;
                        var actual = expected;

                        ClassicAssert.AreSame({|#0:expected|}, actual);

            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        var expected = Guid.Empty;
                        var actual = expected;

                        ClassicAssert.AreEqual(expected, actual);

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, SameAsOnValueTypesCodeFix.UseIsEqualToDescription);
        }

        [Test]
        public async Task VerifyAreNotSameIntoAreNotEqualFixWithMessage()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var expected = Guid.Empty;

                        ClassicAssert.AreNotSame({|#0:expected|}, Guid.NewGuid(), "message");

            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        var expected = Guid.Empty;

                        ClassicAssert.AreNotEqual(expected, Guid.NewGuid(), "message");

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, SameAsOnValueTypesCodeFix.UseIsEqualToDescription);
        }

#if !NUNIT4
        [Test]
        public async Task VerifyAreSameIntoAreEqualFixWithMessageAndParams()
        {
            var code = TestUtility.WrapInTestMethod("""
                        var expected = Guid.Empty;

                        ClassicAssert.AreSame({|#0:expected|}, expected, "message", Guid.Empty);

            """);
            var fixedCode = TestUtility.WrapInTestMethod("""
                        var expected = Guid.Empty;

                        ClassicAssert.AreEqual(expected, expected, "message", Guid.Empty);

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, SameAsOnValueTypesCodeFix.UseIsEqualToDescription);
        }
#endif

        [Test]
        public async Task AnalyzeWhenMoreThanOneConstraintExpressionIsUsed()
        {
            var code = TestUtility.WrapInTestMethod(
                """Assert.That("1", Is.Not.SameAs("1") & Is.Not.SameAs({|#0:2|}));""");

            var fixedCode = TestUtility.WrapInTestMethod(
                """Assert.That("1", Is.Not.SameAs("1") & Is.Not.EqualTo(2));""");
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, SameAsOnValueTypesCodeFix.UseIsEqualToDescription);
        }
    }
}
#endif
