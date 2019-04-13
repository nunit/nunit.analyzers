using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.IgnoreCaseUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.IgnoreCaseUsage
{
    public class IgnoreCaseUsageAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new IgnoreCaseUsageAnalyzer();

        [Test]
        public void AnalyzeWhenIgnoreCaseUsedForNonStringEqualToArgument()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.That(1, Is.EqualTo(1).↓IgnoreCase);");

            AnalyzerAssert.Diagnostics(analyzer, testCode);
        }

        [TestCase(nameof(Is.EqualTo))]
        [TestCase(nameof(Is.EquivalentTo))]
        [TestCase(nameof(Is.SubsetOf))]
        [TestCase(nameof(Is.SupersetOf))]
        public void AnalyzeWhenIgnoreCaseUsedForNonStringCollection(string constraintMethod)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var actual = new[] {{1,2,3}};
                var expected = new[] {{3,2,1}};
                Assert.That(actual, Is.{constraintMethod}(expected).↓IgnoreCase);");

            AnalyzerAssert.Diagnostics(analyzer, testCode);
        }

        [Test]
        public void ValidWhenIgnoreCaseUsedForStringEqualToArgument()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.That(""A"", Is.EqualTo(""a"").IgnoreCase);");

            AnalyzerAssert.NoAnalyzerDiagnostics(analyzer, testCode);
        }

        [TestCase(nameof(Is.EqualTo))]
        [TestCase(nameof(Is.EquivalentTo))]
        [TestCase(nameof(Is.SubsetOf))]
        [TestCase(nameof(Is.SupersetOf))]
        public void ValidWhenIgnoreCaseUsedForStringCollection(string constraintMethod)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var actual = new[] {{""a"",""b"",""c""}};
                var expected = new[] {{""A"",""C"",""B""}};
                Assert.That(actual, Is.{constraintMethod}(expected).IgnoreCase);");

            AnalyzerAssert.NoAnalyzerDiagnostics(analyzer, testCode);
        }

        [Test]
        public void ValidWhenNonGenericIEnumerableProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new[] {""a"",""b"",""c""};
                System.Collections.IEnumerable expected = new[] {""A"",""C"",""B""};
                Assert.That(actual, Is.EqualTo(expected).IgnoreCase);");

            AnalyzerAssert.NoAnalyzerDiagnostics(analyzer, testCode);
        }

        [Test]
        public void ValidWhenStringDictionaryProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new[] {""a"",""b""};
                var expected = new System.Collections.Generic.Dictionary<string, string>
                {
                    [""key1""] = ""value1""
                };
                Assert.That(actual, Is.EqualTo(expected).IgnoreCase);");

            AnalyzerAssert.NoAnalyzerDiagnostics(analyzer, testCode);
        }
    }
}
