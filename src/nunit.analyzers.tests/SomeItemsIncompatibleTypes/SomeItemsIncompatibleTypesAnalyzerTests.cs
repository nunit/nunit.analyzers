using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.SomeItemsIncompatibleTypes;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.SomeItemsIncompatibleTypes
{
    [TestFixtureSource(nameof(ConstraintExpressions))]
    public class SomeItemsIncompatibleTypesAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new SomeItemsIncompatibleTypesAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.SomeItemsIncompatibleTypes);

        private static readonly string[] ConstraintExpressions = new[] { "Does.Contain", "Contains.Item" };

        private readonly string constraint;

        public SomeItemsIncompatibleTypesAnalyzerTests(string constraintExpession)
        {
            this.constraint = constraintExpession;
        }

        [Test]
        public void AnalyzeWhenNonCollectionActualArgumentProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(
                $"Assert.That(123, ↓{this.constraint}(1));");

            RoslynAssert.Diagnostics(analyzer,
                expectedDiagnostic.WithMessage($"The '{this.constraint}' constraint cannot be used with actual argument of type 'int' and expected argument of type 'int'"),
                testCode);
        }

        [Test]
        public void AnalyzeWhenInvalidCollectionActualArgumentProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(
                $"Assert.That(new[] {{\"1\", \"2\"}}, ↓{this.constraint}(1));");

            RoslynAssert.Diagnostics(analyzer,
                expectedDiagnostic.WithMessage($"The '{this.constraint}' constraint cannot be used with actual argument of type 'string[]' and expected argument of type 'int'"),
                testCode);
        }

        [Test]
        public void AnalyzeWhenActualIsCollectionTask()
        {
            var testCode = TestUtility.WrapInTestMethod(
                $"Assert.That(Task.FromResult(new[] {{1,2,3}}), ↓{this.constraint}(1));");

            RoslynAssert.Diagnostics(analyzer,
                expectedDiagnostic.WithMessage($"The '{this.constraint}' constraint cannot be used with actual argument of type 'Task<int[]>' and expected argument of type 'int'"),
                testCode);
        }

        [Test]
        public void ValidWhenCollectionIsProvidedAsActualWithMatchingExpectedType()
        {
            var testCode = TestUtility.WrapInTestMethod(
                $"Assert.That(new [] {{1, 2, 3}}, {this.constraint}(2));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenCollectionOfCollectionsIsProvidedAsActualAndCollectionAsExpected()
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var actual = new List<IEnumerable<int>>
                {{
                    new [] {{ 1, 2 }},
                    new List<int> {{ 2, 3 }},
                    new [] {{ 3, 4 }}
                }};
                Assert.That(actual, {this.constraint}(new[] {{ 2, 3 }}));",
                additionalUsings: "using System.Collections.Generic;");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenCollectionItemTypeAndExpectedTypeAreNumeric()
        {
            var testCode = TestUtility.WrapInTestMethod(
                $"Assert.That(new [] {{1.1, 2.0, 3.2}}, {this.constraint}(2));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenActualIsNonGenericCollection()
        {
            var testCode = TestUtility.WrapInTestMethod(
                $"Assert.That(new ArrayList {{ 1, 2, 3 }}, {this.constraint}(2));",
                additionalUsings: "using System.Collections;");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenUsedWithAllOperator()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "Assert.That(new[] { new[] { 1 }, new[] { 1, 2 } }, Has.All.Contain(1));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenActualIsCollectionDelegate()
        {
            var testCode = TestUtility.WrapInTestMethod(
                $"Assert.That(() => new[] {{1,2,3}}, {this.constraint}(1));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenCustomEqualityMethodIsUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                var actual = new[] {{ ""1"", ""2"", ""3"" }};
                Func<string, int, bool> func = (a, b) => true;
                Assert.That(actual, {this.constraint}(1).Using(func));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void IgnoredWhenUntypedCustomComparerProvided()
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var actual = new[] {{ ""1"", ""2"", ""3"" }};
                Assert.That(actual, {this.constraint}(1).Using((IEqualityComparer)StringComparer.Ordinal));",
                "using System.Collections;");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenTypedCustomComparerProvided()
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var actual = new[] {{ ""1"", ""2"", ""3"" }};
                Assert.That(actual, ↓{this.constraint}(1).Using((IEqualityComparer<string>)StringComparer.Ordinal));",
                "using System.Collections.Generic;");

            RoslynAssert.Diagnostics(analyzer,
                expectedDiagnostic.WithMessage($"The '{this.constraint}' constraint cannot be used with actual argument of type 'string[]' and expected argument of type 'int'"),
                testCode);
        }

#if NUNIT4
        [Test]
        public void AnalyzeWhenUsingPropertiesComparerOverloadWithoutArgument()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [Test]
                public void TestMethod()
                {
                    var actual = new[] { new Record("Test1"), new Record("Test2"), new Record("Test3") };
                    var expected = new AnotherRecord("Test2");

                    Assert.That(actual, ↓{{this.constraint}}(expected).UsingPropertiesComparer());
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

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Theory]
        [TestCase("c => c.AllowDifferentTypes()")]
        [TestCase("c => c.CompareOnlyCommonProperties()")]
        [TestCase("c => c.Map<Record, AnotherRecord>(r => r.Name, a => a.Name)")]
        public void AnalyzeWhenUsingPropertiesComparerOverloadWithArgument(string configure)
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

            RoslynAssert.Valid(analyzer, testCode);
        }
#endif
    }
}
