using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.ConstActualValueUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ConstActualValueUsage
{
    public class ConstActualValueUsageAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ConstActualValueUsageAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.ConstActualValueUsage);

        [Test]
        public void AnalyzeWhenLiteralArgumentIsProvidedForAreEqual()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    int expected = 5;
                    ClassicAssert.AreEqual(expected, ↓1);
                }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenLiteralNamedArgumentIsProvidedForAreEqual()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    int expected = 5;
                    ClassicAssert.AreEqual(actual: ↓1, expected: expected);
                }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenLiteralArgumentIsProvidedForAssertThat()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    Assert.That(↓-1, Is.Positive);
                    Assert.That(↓(2 + 3) * 1024, Is.Positive);
                }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenLocalConstArgumentIsProvidedForAreEqual()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    const string actual = ""act"";
                    string expected = ""exp"";
                    ClassicAssert.AreEqual(expected, ↓actual);
                }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenLocalConstNamedArgumentIsProvidedForAreEqual()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    const string actual = ""act"";
                    string expected = ""exp"";
                    ClassicAssert.AreEqual(actual: ↓actual, expected: expected);
                }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenLocalConstArgumentIsProvidedForAssertThat()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    const string actual = ""act"";
                    string expected = ""exp"";
                    Assert.That(↓actual, Is.EqualTo(expected));
                }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenConstFieldArgumentIsProvidedForAreEqual()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixture
            {
                private const string actual = ""act"";

                public void Test()
                {
                    string expected = ""exp"";
                    ClassicAssert.AreEqual(expected, ↓actual);
                }
            }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenConstFieldArgumentIsProvidedForAssertThat()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixture
            {
                private const string actual = ""act"";

                public void Test()
                {
                    string expected = ""exp"";
                    Assert.That(↓actual, Is.EqualTo(expected));
                }
            }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenStringEmptyArgumentIsProvidedForAreEqual()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    string actual = ""act"";
                    ClassicAssert.AreEqual(actual, ↓string.Empty);
                }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenStringEmptyArgumentIsProvidedForAssertThat()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    string actual = ""act"";
                    Assert.That(↓string.Empty, Is.EqualTo(actual));
                }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void ValidWhenNonConstValueIsProvidedAsActualArgumentForAreEqual()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixture
            {
                private const string expected = ""exp"";

                public void Test()
                {
                    string actual = ""act"";
                    ClassicAssert.AreEqual(expected, actual);
                }
            }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenConstValueIsProvidedAsActualAndExpectedArgumentForAreEqual()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixture
            {
                private const string expected = ""exp"";

                public void Test()
                {
                    const string actual = ""act"";
                    ClassicAssert.AreEqual(expected, actual);
                }
            }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenNonConstValueIsProvidedAsActualNamedArgumentForAreEqual()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixture
            {
                private const string expected = ""exp"";

                public void Test()
                {
                    string actual = ""act"";
                    ClassicAssert.AreEqual(actual: actual, expected: expected);
                }
            }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenConstValueIsProvidedAsActualAndExpectedNamedArgumentForAreEqual()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixture
            {
                private const string expected = ""exp"";

                public void Test()
                {
                    const string actual = ""act"";
                    ClassicAssert.AreEqual(actual: actual, expected: expected);
                }
            }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenNonConstValueIsProvidedAsActualArgumentForAssertThat()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixture
            {
                private const string expected = ""exp"";

                public void Test()
                {
                    string actual = ""act"";
                    Assert.That(actual, Is.EqualTo(expected));
                }
            }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenConstValueIsProvidedAsActualAndExpectedArgumentForAssertThat()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixture
            {
                private const string expected = ""exp"";

                public void Test()
                {
                    const string actual = ""act"";
                    Assert.That(actual, Is.EqualTo(expected));
                }
            }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenCheckingEnumerationValueForAssertThat()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixture
            {
                private enum Answer { No = 0, Yes = 1 };

                public void Test()
                {
                    Assert.That(Answer.Yes, Is.EqualTo(1));
                }
            }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenCheckingAgainstStringEmptyAssertThat()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixture
            {
                private const string actual = ""act"";

                public void Test()
                {
                    Assert.That(actual, Is.Not.EqualTo(string.Empty));
                }
            }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenCheckingEmptyAgainstNullAssertThat()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixture
            {
                public void Test()
                {
                    Assert.That(string.Empty, Is.Not.Null);
                }
            }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenLiteralArgumentIsProvidedForStringAssertContains()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    string expected = ""exp"";
                    StringAssert.Contains(expected, ↓""act"");
                }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenLiteralNamedArgumentIsProvidedForStringAssertContains()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    string expected = ""exp"";
                    StringAssert.Contains(actual: ↓""act"", expected: expected);
                }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenLocalConstArgumentIsProvidedForStringAssertContains()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    const string actual = ""act"";
                    string expected = ""exp"";
                    StringAssert.Contains(expected, ↓actual);
                }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenLocalConstNamedArgumentIsProvidedForStringAssertContains()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    const string actual = ""act"";
                    string expected = ""exp"";
                    StringAssert.Contains(actual: ↓actual, expected: expected);
                }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenConstFieldArgumentIsProvidedForStringAssertContains()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixture
            {
                private const string actual = ""act"";

                public void Test()
                {
                    string expected = ""exp"";
                    StringAssert.Contains(expected, ↓actual);
                }
            }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenStringEmptyArgumentIsProvidedForStringAssertContains()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    string actual = ""act"";
                    StringAssert.Contains(actual, ↓string.Empty);
                }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void ValidWhenNonConstValueIsProvidedAsActualArgumentForStringAssertContains()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixture
            {
                private const string expected = ""exp"";

                public void Test()
                {
                    string actual = ""act"";
                    StringAssert.Contains(expected, actual);
                }
            }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenConstValueIsProvidedAsActualAndExpectedArgumentForStringAssertContains()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixture
            {
                private const string expected = ""exp"";

                public void Test()
                {
                    const string actual = ""act"";
                    StringAssert.Contains(expected, actual);
                }
            }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenNonConstValueIsProvidedAsActualNamedArgumentForStringAssertContains()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixture
            {
                private const string expected = ""exp"";

                public void Test()
                {
                    string actual = ""act"";
                    StringAssert.Contains(actual: actual, expected: expected);
                }
            }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenConstValueIsProvidedAsActualAndExpectedNamedArgumentForStringAssertContains()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixture
            {
                private const string expected = ""exp"";

                public void Test()
                {
                    const string actual = ""act"";
                    StringAssert.Contains(actual: actual, expected: expected);
                }
            }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenTypeOfArgumentIsProvidedForAssertThat()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                public void Test()
                {
                    Assert.That(↓typeof(string), Is.EqualTo("".GetType()));
                }
                """);

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void TypeOfGenericIsNotAConstant()
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
            RoslynAssert.Valid(analyzer, testCode);
        }
    }
}
