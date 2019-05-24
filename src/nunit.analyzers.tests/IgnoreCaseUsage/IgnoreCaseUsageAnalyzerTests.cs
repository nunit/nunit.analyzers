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
        public void AnalyzeWhenDictionaryWithNonStringValueProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new[] {""a"",""b""};
                var expected = new System.Collections.Generic.Dictionary<string, int>
                {
                    [""key1""] = 1
                };
                Assert.That(actual, Is.EqualTo(expected).↓IgnoreCase);");

            AnalyzerAssert.Diagnostics(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenValueTupleWithNoStringMemberProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = (1, 2, false);
                Assert.That(actual, Is.EqualTo((1, 2, false)).↓IgnoreCase);");

            AnalyzerAssert.Diagnostics(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenTupleWithNoStringMemberProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = System.Tuple.Create(1, 2);
                var expected = System.Tuple.Create(1, 2);
                Assert.That(actual, Is.EqualTo(expected).↓IgnoreCase);");

            AnalyzerAssert.Diagnostics(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenNonStringKeyValuePairProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var expected = new System.Collections.Generic.KeyValuePair<bool, int>(false, 1);
                var actual = new System.Collections.Generic.KeyValuePair<bool, int>(true, 1);
                Assert.That(actual, Is.EqualTo(expected).↓IgnoreCase);");

            AnalyzerAssert.Diagnostics(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenNonStringRecurseGenericArgumentProvided()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                class RecurseClass : System.Collections.Generic.List<RecurseClass>
                { }

                [Test]
                public void TestMethod()
                {
                    var actual = new RecurseClass();
                    var expected = new RecurseClass();
                    Assert.That(actual, Is.EqualTo(expected).↓IgnoreCase);
                }");

            AnalyzerAssert.Diagnostics(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenIgnoreCaseUsedInConstraintCombinedByOperators()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.That(1, Is.EqualTo(1).↓IgnoreCase | Is.EqualTo(true).↓IgnoreCase);");

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
        public void ValidWhenDictionaryWithStringValueProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new[] {""a"",""b""};
                var expected = new System.Collections.Generic.Dictionary<int, string>
                {
                    [1] = ""value1""
                };
                Assert.That(actual, Is.EqualTo(expected).IgnoreCase);");

            AnalyzerAssert.NoAnalyzerDiagnostics(analyzer, testCode);
        }

        [Test]
        public void ValidForValueTupleWithStringMember()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = (""a"", 2, false);
                Assert.That(actual, Is.EqualTo((""A"", 2, false)).IgnoreCase);");

            AnalyzerAssert.NoAnalyzerDiagnostics(analyzer, testCode);
        }

        [Test]
        public void ValidForTupleWithStringMember()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = System.Tuple.Create(1, ""a"");
                var expected = System.Tuple.Create(1, ""A"");
                Assert.That(actual, Is.EqualTo(expected).IgnoreCase);");

            AnalyzerAssert.NoAnalyzerDiagnostics(analyzer, testCode);
        }

        [Test]
        public void ValidForKeyValuePairWithStringKey()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var expected = new System.Collections.Generic.KeyValuePair<string, int>(""a"", 1);
                var actual = new System.Collections.Generic.KeyValuePair<string, int>(""A"", 1);
                Assert.That(actual, Is.EqualTo(expected).IgnoreCase);");

            AnalyzerAssert.NoAnalyzerDiagnostics(analyzer, testCode);
        }

        [Test]
        public void ValidForKeyValuePairWithStringValue()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var expected = new System.Collections.Generic.KeyValuePair<int, string>(1, ""a"");
                var actual = new System.Collections.Generic.KeyValuePair<int, string>(1, ""A"");
                Assert.That(actual, Is.EqualTo(expected).IgnoreCase);");

            AnalyzerAssert.NoAnalyzerDiagnostics(analyzer, testCode);
        }

        [Test]
        public void ValidForDictionaryEntry()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var expected = new System.Collections.DictionaryEntry(1, ""a"");
                var actual = new System.Collections.DictionaryEntry(1, ""A"");
                Assert.That(actual, Is.EqualTo(expected).IgnoreCase);");

            AnalyzerAssert.NoAnalyzerDiagnostics(analyzer, testCode);
        }

        [Test]
        public void ValidForDeepNesting()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = (1, new[] { new[] { ""a"" } });
                var expected = (1, new[] { new[] { ""A"" } });
                Assert.That(actual, Is.EqualTo(expected).IgnoreCase);");

            AnalyzerAssert.NoAnalyzerDiagnostics(analyzer, testCode);
        }
    }
}
