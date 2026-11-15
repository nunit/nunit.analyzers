using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.SomeItemsIncompatibleTypes.SomeItemsIncompatibleTypesAnalyzer>;

namespace NUnit.Analyzers.Tests.SomeItemsIncompatibleTypes
{
    [TestFixtureSource(nameof(ConstraintExpressions))]
    public class SomeItemsIncompatibleTypesAnalyzerTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.SomeItemsIncompatibleTypes);

        private static readonly string[] ConstraintExpressions = ["Does.Contain", "Contains.Item"];

        private readonly string constraint;

        public SomeItemsIncompatibleTypesAnalyzerTests(string constraintExpession)
        {
            this.constraint = constraintExpession;
        }

        [Test]
        public async Task AnalyzeWhenNonCollectionActualArgumentProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(
                $"Assert.That(123, {{|#0:{this.constraint}(1)|}});");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithMessage($"The '{this.constraint}' constraint cannot be used with actual argument of type 'int' and expected argument of type 'int'").WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenInvalidCollectionActualArgumentProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(
                $"Assert.That(new[] {{\"1\", \"2\"}}, {{|#0:{this.constraint}(1)|}});");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithMessage($"The '{this.constraint}' constraint cannot be used with actual argument of type 'string[]' and expected argument of type 'int'").WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenActualIsCollectionTask()
        {
            var testCode = TestUtility.WrapInTestMethod(
                $"Assert.That(Task.FromResult(new[] {{1,2,3}}), {{|#0:{this.constraint}(1)|}});");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithMessage($"The '{this.constraint}' constraint cannot be used with actual argument of type 'Task<int[]>' and expected argument of type 'int'").WithLocation(0));
        }

        [Test]
        public async Task ValidWhenCollectionIsProvidedAsActualWithMatchingExpectedType()
        {
            var testCode = TestUtility.WrapInTestMethod(
                $"Assert.That(new [] {{1, 2, 3}}, {this.constraint}(2));");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenCollectionOfCollectionsIsProvidedAsActualAndCollectionAsExpected()
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var actual = new List<IEnumerable<int>>
                        {
                            new [] { 1, 2 },
                            new List<int> { 2, 3 },
                            new [] { 3, 4 }
                        };
                        Assert.That(actual, {{this.constraint}}(new[] { 2, 3 }));
            """,
                additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenCollectionItemTypeAndExpectedTypeAreNumeric()
        {
            var testCode = TestUtility.WrapInTestMethod(
                $"Assert.That(new [] {{1.1, 2.0, 3.2}}, {this.constraint}(2));");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenActualIsNonGenericCollection()
        {
            var testCode = TestUtility.WrapInTestMethod(
                $"Assert.That(new ArrayList {{ 1, 2, 3 }}, {this.constraint}(2));",
                additionalUsings: "using System.Collections;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenUsedWithAllOperator()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "Assert.That(new[] { new[] { 1 }, new[] { 1, 2 } }, Has.All.Contain(1));");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenActualIsCollectionDelegate()
        {
            var testCode = TestUtility.WrapInTestMethod(
                $"Assert.That(() => new[] {{1,2,3}}, {this.constraint}(1));");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenCustomEqualityMethodIsUsed()
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var actual = new[] { "1", "2", "3" };
                        Func<string, int, bool> func = (a, b) => true;
                        Assert.That(actual, {{this.constraint}}(1).Using(func));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task IgnoredWhenUntypedCustomComparerProvided()
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var actual = new[] { "1", "2", "3" };
                        Assert.That(actual, {{this.constraint}}(1).Using((IEqualityComparer)StringComparer.Ordinal));
            """,
                "using System.Collections;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenTypedCustomComparerProvided()
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var actual = new[] { "1", "2", "3" };
                        Assert.That(actual, {|#0:{{this.constraint}}(1).Using((IEqualityComparer<string>)StringComparer.Ordinal)|});
            """,
                "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithMessage($"The '{this.constraint}' constraint cannot be used with actual argument of type 'string[]' and expected argument of type 'int'").WithLocation(0));
        }

#if NUNIT4
        [Test]
        public async Task AnalyzeWhenUsingPropertiesComparerOverloadWithoutArgument()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [Test]
                public void TestMethod()
                {
                    var actual = new[] { new Record("Test1"), new Record("Test2"), new Record("Test3") };
                    var expected = new AnotherRecord("Test2");

                    Assert.That(actual, {|#0:{{this.constraint}}(expected).UsingPropertiesComparer()|});
                }

                private class Record
                {
                    public string Name { get; }
                    public Record(string name)
                    {
                        Name = name;
                    }
                }

                private class AnotherRecord
                {
                    public string Name { get; }
                    public AnotherRecord(string name)
                    {
                        Name = name;
                    }
                }
                """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments(this.constraint, "Record[]", "AnotherRecord"));
        }

        [Theory]
        [TestCase("c => c.AllowDifferentTypes()")]
        [TestCase("c => c.CompareOnlyCommonProperties()")]
        [TestCase("c => c.Map<Record, AnotherRecord>(r => r.Name, a => a.Name)")]
        public async Task AnalyzeWhenUsingPropertiesComparerOverloadWithArgument(string configure)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [Test]
                public void TestMethod()
                {
                    var actual = new[] { new Record("Test1"), new Record("Test2"), new Record("Test3") };
                    var expected = new AnotherRecord("Test2");

                    Assert.That(actual, {{this.constraint}}(expected).UsingPropertiesComparer(
                        {{configure}}));
                }

                private class Record
                {
                    public string Name { get; }

                    public Record(string name)
                    {
                        Name = name;
                    }
                }

                private class AnotherRecord
                {
                    public string Name { get; }

                    public AnotherRecord(string name)
                    {
                        Name = name;
                    }
                }
                """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }
#endif
    }
}
