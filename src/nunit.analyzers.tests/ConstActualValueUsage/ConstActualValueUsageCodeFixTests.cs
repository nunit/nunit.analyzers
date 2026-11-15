using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.ConstActualValueUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using NUnit.Framework.Internal;

#if NUNIT4
using NUnit.Framework.Legacy;
#else
using ClassicAssert = NUnit.Framework.Assert;
#endif
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.ConstActualValueUsage.ConstActualValueUsageAnalyzer, NUnit.Analyzers.ConstActualValueUsage.ConstActualValueUsageCodeFix>;

namespace NUnit.Analyzers.Tests.ConstActualValueUsage
{
    public class ConstActualValueUsageCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.ConstActualValueUsage);

        [TestCase(nameof(ClassicAssert.AreEqual))]
        [TestCase(nameof(ClassicAssert.AreNotEqual))]
        [TestCase(nameof(ClassicAssert.AreSame))]
        [TestCase(nameof(ClassicAssert.AreNotSame))]
        public async Task LiteralArgumentIsProvidedForClassicAssertCodeFix(string classicAssertMethod)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void Test()
                    {
                        int expected = 5;
                        ClassicAssert.{{classicAssertMethod}}(expected, {|#0:1|});
                    }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void Test()
                    {
                        int expected = 5;
                        ClassicAssert.{{classicAssertMethod}}(1, expected);
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ConstActualValueUsageCodeFix.SwapArgumentsDescription);
        }

        [TestCase(nameof(ClassicAssert.AreEqual))]
        [TestCase(nameof(ClassicAssert.AreNotEqual))]
        [TestCase(nameof(ClassicAssert.AreSame))]
        [TestCase(nameof(ClassicAssert.AreNotSame))]
        public async Task LiteralArgumentIsProvidedForClassicAssertCodeFixWithNewLine(string classicAssertMethod)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void Test()
                    {
                        int expected = 5;
                        ClassicAssert.{{classicAssertMethod}}(expected,
                            {|#0:1|});
                    }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void Test()
                    {
                        int expected = 5;
                        ClassicAssert.{{classicAssertMethod}}(1,
                            expected);
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ConstActualValueUsageCodeFix.SwapArgumentsDescription);
        }

        [TestCase(nameof(ClassicAssert.AreEqual))]
        public async Task LiteralArgumentIsProvidedForClassicAssertWithDeltaCodeFix(string classicAssertMethod)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void Test()
                    {
                        int expected = 5;
                        ClassicAssert.{{classicAssertMethod}}(expected, {|#0:1|}, double.Epsilon);
                    }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void Test()
                    {
                        int expected = 5;
                        ClassicAssert.{{classicAssertMethod}}(1, expected, double.Epsilon);
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ConstActualValueUsageCodeFix.SwapArgumentsDescription);
        }

        [Test]
        public async Task LiteralNamedArgumentIsProvidedForAreEqualCodeFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        int expected = 5;
                        ClassicAssert.AreEqual(actual: {|#0:1|}, expected: expected);
                    }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        int expected = 5;
                        ClassicAssert.AreEqual(actual: expected, expected: 1);
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ConstActualValueUsageCodeFix.SwapArgumentsDescription);
        }

        [TestCase(nameof(Is.EqualTo))]
        [TestCase(nameof(Is.SameAs))]
        [TestCase(nameof(Is.SamePath))]
        [TestCase("Not.EqualTo")]
        [TestCase("Not.SameAs")]
        [TestCase("Not.SamePath")]
        public async Task LiteralArgumentIsProvidedForAssertThatCodeFix(string isConstraint)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void Test()
                    {
                        var expected = "abc";
                        Assert.That({|#0:"a"|}, Is.{{isConstraint}}(expected));
                    }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void Test()
                    {
                        var expected = "abc";
                        Assert.That(expected, Is.{{isConstraint}}("a"));
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ConstActualValueUsageCodeFix.SwapArgumentsDescription);
        }

        [Test]
        public async Task LiteralArgumentIsProvidedForAssertThatWithinCodeFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void Test()
                    {
                        double expected = 5d;
                        Assert.That({|#0:1d|}, Is.EqualTo(expected).Within(double.Epsilon));
                    }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void Test()
                    {
                        double expected = 5d;
                        Assert.That(expected, Is.EqualTo(1d).Within(double.Epsilon));
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ConstActualValueUsageCodeFix.SwapArgumentsDescription);
        }

        [TestCase(nameof(Is.GreaterThan))]
        [TestCase(nameof(Is.LessThan))]
        public async Task NoCodeFixForNonSymmetricAssertThat(string isConstraint)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void Test()
                    {
                        var expected = 5;
                        Assert.That({|#0:4|}, Is.{{isConstraint}}(expected));
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), code);
        }

        [Test]
        public async Task NoCodeFixForCombinedAssertThat()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void Test()
                    {
                        var expected = 5;
                        Assert.That({|#0:4|}, Is.Not.EqualTo(3).And.Not.EqualTo(expected));
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), code);
        }

        [TestCase(nameof(StringAssert.AreEqualIgnoringCase))]
        [TestCase(nameof(StringAssert.AreNotEqualIgnoringCase))]
        [TestCase(nameof(StringAssert.Contains))]
        [TestCase(nameof(StringAssert.EndsWith))]
        [TestCase(nameof(StringAssert.IsMatch))]
        [TestCase(nameof(StringAssert.StartsWith))]
        [TestCase(nameof(StringAssert.DoesNotContain))]
        [TestCase(nameof(StringAssert.DoesNotMatch))]
        [TestCase(nameof(StringAssert.DoesNotEndWith))]
        [TestCase(nameof(StringAssert.DoesNotStartWith))]
        public async Task LiteralArgumentIsProvidedForClassicStringAssertCodeFix(string classicAssertMethod)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void Test()
                    {
                        string actual = "act";
                        StringAssert.{{classicAssertMethod}}(actual, {|#0:"exp"|});
                    }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void Test()
                    {
                        string actual = "act";
                        StringAssert.{{classicAssertMethod}}("exp", actual);
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ConstActualValueUsageCodeFix.SwapArgumentsDescription);
        }

        [Test]
        public async Task LiteralNamedArgumentIsProvidedForStringAssertContainsCodeFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        string actual = "act";
                        StringAssert.Contains(actual: {|#0:"exp"|}, expected: actual);
                    }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        string actual = "act";
                        StringAssert.Contains(actual: actual, expected: "exp");
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ConstActualValueUsageCodeFix.SwapArgumentsDescription);
        }
    }
}
