using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.IgnoreCaseUsage.IgnoreCaseUsageAnalyzer>;

namespace NUnit.Analyzers.Tests.IgnoreCaseUsage
{
    public class IgnoreCaseUsageAnalyzerTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.IgnoreCaseUsage);

#if !NUNIT4
        [Test]
        public async Task AnalyzeWhenIgnoreCaseUsedForNonStringEqualToArgument()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Assert.That(1, Is.EqualTo(1).{|#0:IgnoreCase|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }
#endif

        [TestCase(nameof(Is.EqualTo))]
        [TestCase(nameof(Is.EquivalentTo))]
        [TestCase(nameof(Is.SubsetOf))]
        [TestCase(nameof(Is.SupersetOf))]
        public async Task AnalyzeWhenIgnoreCaseUsedForNonStringCollection(string constraintMethod)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var actual = new[] {1,2,3};
                        var expected = new[] {3,2,1};
                        Assert.That(actual, Is.{{constraintMethod}}(expected).{|#0:IgnoreCase|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenDictionaryWithNonStringValueProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new[] {"a","b"};
                        var expected = new System.Collections.Generic.Dictionary<string, int>
                        {
                            ["key1"] = 1
                        };
                        Assert.That(actual, Is.EqualTo(expected).{|#0:IgnoreCase|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenValueTupleWithNoStringMemberProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = (1, 2, false);
                        Assert.That(actual, Is.EqualTo((1, 2, false)).{|#0:IgnoreCase|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenTupleWithNoStringMemberProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = System.Tuple.Create(1, 2);
                        var expected = System.Tuple.Create(1, 2);
                        Assert.That(actual, Is.EqualTo(expected).{|#0:IgnoreCase|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenNonStringKeyValuePairProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var expected = new System.Collections.Generic.KeyValuePair<bool, int>(false, 1);
                        var actual = new System.Collections.Generic.KeyValuePair<bool, int>(true, 1);
                        Assert.That(actual, Is.EqualTo(expected).{|#0:IgnoreCase|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenNonStringRecurseGenericArgumentProvided()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    class RecurseClass : System.Collections.Generic.List<RecurseClass>
                    { }

                    [Test]
                    public void TestMethod()
                    {
                        var actual = new RecurseClass();
                        var expected = new RecurseClass();
                        Assert.That(actual, Is.EqualTo(expected).{|#0:IgnoreCase|});
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenIgnoreCaseUsedInConstraintCombinedByOperators()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Assert.That(1 == 1, Is.EqualTo(false).{|#0:IgnoreCase|} | Is.EqualTo(true).{|#1:IgnoreCase|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0), expectedDiagnostic.WithLocation(1));
        }

        [Test]
        public async Task ValidWhenIgnoreCaseUsedForStringEqualToArgument()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Assert.That("A", Is.EqualTo("a").IgnoreCase);
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase(nameof(Is.EqualTo))]
        [TestCase(nameof(Is.EquivalentTo))]
        [TestCase(nameof(Is.SubsetOf))]
        [TestCase(nameof(Is.SupersetOf))]
        public async Task ValidWhenIgnoreCaseUsedForStringCollection(string constraintMethod)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var actual = new[] {"a","b","c"};
                        var expected = new[] {"A","C","B"};
                        Assert.That(actual, Is.{{constraintMethod}}(expected).IgnoreCase);
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenNonGenericIEnumerableProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new[] {"a","b","c"};
                        System.Collections.IEnumerable expected = new[] {"A","C","B"};
                        Assert.That(actual, Is.EqualTo(expected).IgnoreCase);
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenDictionaryWithStringValueProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new[] {"a","b"};
                        var expected = new System.Collections.Generic.Dictionary<int, string>
                        {
                            [1] = "value1"
                        };
                        Assert.That(actual, Is.EqualTo(expected).IgnoreCase);
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidForValueTupleWithStringMember()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = ("a", 2, false);
                        Assert.That(actual, Is.EqualTo(("A", 2, false)).IgnoreCase);
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidForTupleWithStringMember()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = System.Tuple.Create(1, "a");
                        var expected = System.Tuple.Create(1, "A");
                        Assert.That(actual, Is.EqualTo(expected).IgnoreCase);
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidForKeyValuePairWithStringKey()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var expected = new System.Collections.Generic.KeyValuePair<string, int>("a", 1);
                        var actual = new System.Collections.Generic.KeyValuePair<string, int>("A", 1);
                        Assert.That(actual, Is.EqualTo(expected).IgnoreCase);
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidForKeyValuePairWithStringValue()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var expected = new System.Collections.Generic.KeyValuePair<int, string>(1, "a");
                        var actual = new System.Collections.Generic.KeyValuePair<int, string>(1, "A");
                        Assert.That(actual, Is.EqualTo(expected).IgnoreCase);
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidForDictionaryEntry()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var expected = new System.Collections.DictionaryEntry(1, "a");
                        var actual = new System.Collections.DictionaryEntry(1, "A");
                        Assert.That(actual, Is.EqualTo(expected).IgnoreCase);
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidForDeepNesting()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = (1, new[] { new[] { "a" } });
                        var expected = (1, new[] { new[] { "A" } });
                        Assert.That(actual, Is.EqualTo(expected).IgnoreCase);
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
