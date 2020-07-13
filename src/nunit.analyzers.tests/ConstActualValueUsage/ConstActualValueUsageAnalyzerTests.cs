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
                    Assert.AreEqual(expected, ↓1);
                }");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenLiteralNamedArgumentIsProvidedForAreEqual()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    int expected = 5;
                    Assert.AreEqual(actual: ↓1, expected: expected);
                }");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
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

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenLocalConstArgumentIsProvidedForAreEqual()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    const string actual = ""act"";
                    string expected = ""exp"";
                    Assert.AreEqual(expected, ↓actual);
                }");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenLocalConstNamedArgumentIsProvidedForAreEqual()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    const string actual = ""act"";
                    string expected = ""exp"";
                    Assert.AreEqual(actual: ↓actual, expected: expected);
                }");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
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

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenConstFieldArgumentIsProvidedForAreEqual()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixure
            {
                private const string actual = ""act"";

                public void Test()
                {
                    string expected = ""exp"";
                    Assert.AreEqual(expected, ↓actual);
                }
            }");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenConstFieldArgumentIsProvidedForAssertThat()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixure
            {
                private const string actual = ""act"";

                public void Test()
                {
                    string expected = ""exp"";
                    Assert.That(↓actual, Is.EqualTo(expected));
                }
            }");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void ValidWhenNonConstValueIsProvidedAsActualArgumentForAreEqual()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixure
            {
                private const string expected = ""exp"";

                public void Test()
                {
                    string actual = ""act"";
                    Assert.AreEqual(expected, actual);
                }
            }");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenConstValueIsProvidedAsActualAndExpectedArgumentForAreEqual()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixure
            {
                private const string expected = ""exp"";

                public void Test()
                {
                    const string actual = ""act"";
                    Assert.AreEqual(expected, actual);
                }
            }");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenNonConstValueIsProvidedAsActualNamedArgumentForAreEqual()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixure
            {
                private const string expected = ""exp"";

                public void Test()
                {
                    string actual = ""act"";
                    Assert.AreEqual(actual: actual, expected: expected);
                }
            }");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenConstValueIsProvidedAsActualAndExpectedNamedArgumentForAreEqual()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixure
            {
                private const string expected = ""exp"";

                public void Test()
                {
                    const string actual = ""act"";
                    Assert.AreEqual(actual: actual, expected: expected);
                }
            }");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenNonConstValueIsProvidedAsActualArgumentForAssertThat()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixure
            {
                private const string expected = ""exp"";

                public void Test()
                {
                    string actual = ""act"";
                    Assert.That(actual, Is.EqualTo(expected));
                }
            }");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenConstValueIsProvidedAsActualAndExpectedArgumentForAssertThat()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixure
            {
                private const string expected = ""exp"";

                public void Test()
                {
                    const string actual = ""act"";
                    Assert.That(actual, Is.EqualTo(expected));
                }
            }");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenCheckingEnumerationValueForAssertThat()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public class TestFixure
            {
                private enum Answer { No = 0, Yes = 1 };

                public void Test()
                {
                    Assert.That(Answer.Yes, Is.EqualTo(1));
                }
            }");

            AnalyzerAssert.Valid(analyzer, testCode);
        }
    }
}
