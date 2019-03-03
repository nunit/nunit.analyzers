using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.ConstActualValueUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ActualConstValueUsage
{
    public class ActualConstValueUsageAnalyzerTests
    {
        private readonly DiagnosticAnalyzer analyzer = new ActualConstValueUsageAnalyzer();


        [Test]
        public void AnalyzeWhenLiteralArgumentIsProvidedForAreEqual()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    int expected = 5;
                    Assert.AreEqual(expected, ↓1);
                }");

            AnalyzerAssert.Diagnostics(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenLiteralArgumentIsProvidedForAssertThat()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public void Test()
                {
                    Assert.That(↓true, Is.EqualTo(false));
                }");

            AnalyzerAssert.Diagnostics(this.analyzer, testCode);
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

            AnalyzerAssert.Diagnostics(this.analyzer, testCode);
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

            AnalyzerAssert.Diagnostics(this.analyzer, testCode);
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

            AnalyzerAssert.Diagnostics(this.analyzer, testCode);
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

            AnalyzerAssert.Diagnostics(this.analyzer, testCode);
        }

        [Test]
        public void ValidWhenNonConstValueIsProvidedAsActualForAreEqual()
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

            AnalyzerAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void ValidWhenNonConstValueIsProvidedAsActualForAssertThat()
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

            AnalyzerAssert.Valid(this.analyzer, testCode);
        }
    }
}
