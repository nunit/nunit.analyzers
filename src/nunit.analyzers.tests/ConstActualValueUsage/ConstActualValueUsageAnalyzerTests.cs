using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.ConstActualValueUsage.ConstActualValueUsageAnalyzer>;

namespace NUnit.Analyzers.Tests.ConstActualValueUsage
{
    public class ConstActualValueUsageAnalyzerTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.ConstActualValueUsage);

        [Test]
        public async Task AnalyzeWhenLiteralArgumentIsProvidedForAreEqual()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        int expected = 5;
                        ClassicAssert.AreEqual(expected, {|#0:1|});
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenLiteralNamedArgumentIsProvidedForAreEqual()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        int expected = 5;
                        ClassicAssert.AreEqual(actual: {|#0:1|}, expected: expected);
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenLiteralArgumentIsProvidedForAssertThat()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        Assert.That({|#0:-1|}, Is.Positive);
                        Assert.That({|#1:(2 + 3) * 1024|}, Is.Positive);
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0), expectedDiagnostic.WithLocation(1));
        }

        [Test]
        public async Task AnalyzeWhenLocalConstArgumentIsProvidedForAreEqual()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        const string actual = "act";
                        string expected = "exp";
                        ClassicAssert.AreEqual(expected, {|#0:actual|});
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenLocalConstNamedArgumentIsProvidedForAreEqual()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        const string actual = "act";
                        string expected = "exp";
                        ClassicAssert.AreEqual(actual: {|#0:actual|}, expected: expected);
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenLocalConstArgumentIsProvidedForAssertThat()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        const string actual = "act";
                        string expected = "exp";
                        Assert.That({|#0:actual|}, Is.EqualTo(expected));
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenConstFieldArgumentIsProvidedForAreEqual()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class TestFixture
                {
                    private const string actual = "act";

                    public void Test()
                    {
                        string expected = "exp";
                        ClassicAssert.AreEqual(expected, {|#0:actual|});
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenConstFieldArgumentIsProvidedForAssertThat()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class TestFixture
                {
                    private const string actual = "act";

                    public void Test()
                    {
                        string expected = "exp";
                        Assert.That({|#0:actual|}, Is.EqualTo(expected));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenStringEmptyArgumentIsProvidedForAreEqual()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        string actual = "act";
                        ClassicAssert.AreEqual(actual, {|#0:string.Empty|});
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenStringEmptyArgumentIsProvidedForAssertThat()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        string actual = "act";
                        Assert.That({|#0:string.Empty|}, Is.EqualTo(actual));
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task ValidWhenNonConstValueIsProvidedAsActualArgumentForAreEqual()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class TestFixture
                {
                    private const string expected = "exp";

                    public void Test()
                    {
                        string actual = "act";
                        ClassicAssert.AreEqual(expected, actual);
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenConstValueIsProvidedAsActualAndExpectedArgumentForAreEqual()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class TestFixture
                {
                    private const string expected = "exp";

                    public void Test()
                    {
                        const string actual = "act";
                        ClassicAssert.AreEqual(expected, actual);
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenNonConstValueIsProvidedAsActualNamedArgumentForAreEqual()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class TestFixture
                {
                    private const string expected = "exp";

                    public void Test()
                    {
                        string actual = "act";
                        ClassicAssert.AreEqual(actual: actual, expected: expected);
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenConstValueIsProvidedAsActualAndExpectedNamedArgumentForAreEqual()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class TestFixture
                {
                    private const string expected = "exp";

                    public void Test()
                    {
                        const string actual = "act";
                        ClassicAssert.AreEqual(actual: actual, expected: expected);
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenNonConstValueIsProvidedAsActualArgumentForAssertThat()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class TestFixture
                {
                    private const string expected = "exp";

                    public void Test()
                    {
                        string actual = "act";
                        Assert.That(actual, Is.EqualTo(expected));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenConstValueIsProvidedAsActualAndExpectedArgumentForAssertThat()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class TestFixture
                {
                    private const string expected = "exp";

                    public void Test()
                    {
                        const string actual = "act";
                        Assert.That(actual, Is.EqualTo(expected));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenCheckingEnumerationValueForAssertThat()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class TestFixture
                {
                    private enum Answer { No = 0, Yes = 1 };

                    public void Test()
                    {
                        Assert.That(Answer.Yes, Is.EqualTo(1));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenCheckingAgainstStringEmptyAssertThat()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class TestFixture
                {
                    private const string actual = "act";

                    public void Test()
                    {
                        Assert.That(actual, Is.Not.EqualTo(string.Empty));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenCheckingEmptyAgainstNullAssertThat()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class TestFixture
                {
                    public void Test()
                    {
                        Assert.That(string.Empty, Is.Not.Null);
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenLiteralArgumentIsProvidedForStringAssertContains()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        string expected = "exp";
                        StringAssert.Contains(expected, {|#0:"act"|});
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenLiteralNamedArgumentIsProvidedForStringAssertContains()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        string expected = "exp";
                        StringAssert.Contains(actual: {|#0:"act"|}, expected: expected);
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenLocalConstArgumentIsProvidedForStringAssertContains()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        const string actual = "act";
                        string expected = "exp";
                        StringAssert.Contains(expected, {|#0:actual|});
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenLocalConstNamedArgumentIsProvidedForStringAssertContains()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        const string actual = "act";
                        string expected = "exp";
                        StringAssert.Contains(actual: {|#0:actual|}, expected: expected);
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenConstFieldArgumentIsProvidedForStringAssertContains()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class TestFixture
                {
                    private const string actual = "act";

                    public void Test()
                    {
                        string expected = "exp";
                        StringAssert.Contains(expected, {|#0:actual|});
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenStringEmptyArgumentIsProvidedForStringAssertContains()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        string actual = "act";
                        StringAssert.Contains(actual, {|#0:string.Empty|});
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task ValidWhenNonConstValueIsProvidedAsActualArgumentForStringAssertContains()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class TestFixture
                {
                    private const string expected = "exp";

                    public void Test()
                    {
                        string actual = "act";
                        StringAssert.Contains(expected, actual);
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenConstValueIsProvidedAsActualAndExpectedArgumentForStringAssertContains()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class TestFixture
                {
                    private const string expected = "exp";

                    public void Test()
                    {
                        const string actual = "act";
                        StringAssert.Contains(expected, actual);
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenNonConstValueIsProvidedAsActualNamedArgumentForStringAssertContains()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class TestFixture
                {
                    private const string expected = "exp";

                    public void Test()
                    {
                        string actual = "act";
                        StringAssert.Contains(actual: actual, expected: expected);
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenConstValueIsProvidedAsActualAndExpectedNamedArgumentForStringAssertContains()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class TestFixture
                {
                    private const string expected = "exp";

                    public void Test()
                    {
                        const string actual = "act";
                        StringAssert.Contains(actual: actual, expected: expected);
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenTypeOfArgumentIsProvidedForAssertThat()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                public void Test()
                {
                    Assert.That({|#0:typeof(string)|}, Is.EqualTo("".GetType()));
                }
                """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task TypeOfGenericIsNotAConstant()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class MyExplicitlyTypedTests
                {
                    [TestCaseSource(nameof(ExplicitTypeArgsTestCases))]
                    public void ExplicitTypeArgs<T>(T input)
                    {
                        Assert.That(typeof(T), Is.EqualTo(typeof(long)));
                    }

                    private static IEnumerable<TestCaseData> ExplicitTypeArgsTestCases()
                    {
                        yield return new TestCaseData(2);
                        yield return new TestCaseData(2L);
                    }
                }
                """, "using System.Collections.Generic;");
            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
