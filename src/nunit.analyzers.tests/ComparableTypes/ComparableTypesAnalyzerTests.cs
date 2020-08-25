using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.ComparableTypes;
using NUnit.Analyzers.Constants;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ComparableTypes
{
    public class ComparableTypesAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ComparableTypesAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.ComparableTypes);

        private static readonly string[] NumericTypes = new[] {
            "decimal", "double", "float", "int", "uint", "long", "ulong", "short", "ushort"
        };

        [Test]
        public void AnalyzeWhenIncompatibleTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "Assert.That(1, Is.LessThan(↓\"1\"));");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenNonComparableTypesProvided_WithLambdaActualValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class A { }

    public class Tests
    {
        [Test]
        public void AnalyzeWhenNonComparableTypesProvided_WithLambdaActualValue()
        {
            var expected = new A();
            Assert.That(() => new A(), Is.GreaterThanOrEqualTo(↓expected));
        }
    }");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIncompatibleTypesProvided_WithLocalFunctionActualValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class A { }

    public class Tests
    {
        [Test]
        public void AnalyzeWhenIncompatibleTypesProvided_WithLocalFunctionActualValue()
        {
            A actual() => new A();
            var expected = new A();
            Assert.That(actual, Is.Not.GreaterThan(↓expected));
        }
    }");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIncompatibleTypesProvided_WithFuncActualValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class A { }

    public class Tests
    {
        [Test]
        public void AnalyzeWhenIncompatibleTypesProvided_WithNegatedAssert()
        {
            Func<A> actual = () => new A();
            var expected = new A();
            Assert.That(actual, Is.LessThanOrEqualTo(↓expected));
        }
    }");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIncompatibleTypesProvided_WithTaskActualValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class A { }

    public class Tests
    {
        [Test]
        public void AnalyzeWhenIncompatibleTypesProvided_WithNegatedAssert()
        {
            var actual = Task.FromResult(new A());
            var expected = new A();
            Assert.That(actual, Is.GreaterThan(↓expected));
        }
    }");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void NoDiagnosticForNumericTypes(
            [ValueSource(nameof(NumericTypes))] string actualType,
            [ValueSource(nameof(NumericTypes))] string expectedType)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                {actualType} actual = 1;
                {expectedType} expected = 1;
                Assert.That(actual, Is.LessThanOrEqualTo(expected));");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenSameEnumsProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
                enum EnumOne { A, B, C }

                class TestClass
                {
                    [Test]
                    public void EnumTest()
                    {
                        var actual = EnumOne.B;
                        var expected = EnumOne.C;
                        Assert.That(actual, Is.LessThan(expected));
                    }
                }");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenDifferentEnumsProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
                enum EnumOne { A, B, C }
                enum EnumTwo { A, B, C }

                class TestClass
                {
                    [Test]
                    public void EnumTest()
                    {
                        var actual = EnumOne.B;
                        var expected = EnumTwo.B;
                        Assert.That(actual, Is.LessThan(↓expected));
                    }
                }");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void NoDiagnosticWhenActualHasIComparableOfExpected()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
                class A : IComparable<B>
                {
                    public int CompareTo(B other) => 0;
                }

                class B { }

                class Tests
                {
                    [Test]
                    public void TestMethod()
                    {
                        var actual = new A();
                        var expected = new B();
                        Assert.That(actual, Is.GreaterThanOrEqualTo(expected));
                    }
                }");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenExpectedHasIComparableOfActual()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
                class A { }

                class B : IComparable<A>
                {
                    public int CompareTo(A other) => 0;
                }

                class Tests
                {
                    [Test]
                    public void TestMethod()
                    {
                        var actual = new A();
                        var expected = new B();
                        Assert.That(actual, Is.LessThanOrEqualTo(expected));
                    }
                }");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenCustomComparerProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class A { }
    class B { }
    class ABComparer : IComparer
    {
        public int Compare(object x, object y) => 0;
    }

    public class Tests
    {
        [Test]
        public void TestMethod()
        {
            var actual = new A();
            var expected = new B();
            Assert.That(actual, Is.GreaterThan(expected).Using(new ABComparer()));
        }
    }", "using System.Collections;");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenExpectedIsMatchingNullableType()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
            int actual = 5;
            int? expected = 5;
            Assert.That(actual, Is.GreaterThan(expected));");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenActualIsNumericNullableType()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
            int? actual = 5;
            double expected = 5.0;
            Assert.That(actual, Is.GreaterThan(expected));");

            AnalyzerAssert.Valid(analyzer, testCode);
        }


        [Test]
        public void NoDiagnosticWhenActualIsDelegate()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
            int actual = 6;
            double expected = 5.0;
            Assert.That(() => actual, Is.GreaterThan(expected));");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenMoreThanOneConstraintExpressionIsUsed()
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                Assert.That(5, Is.GreaterThan(4) & Is.LessThan(↓""6""));");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenFirstConstraintExpressionUsesComparer()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
                class AsInt32Comparer : IComparer
                {
                    public int Compare(object x, object y) => Convert.ToInt32(x).CompareTo(Convert.ToInt32(y));
                }

                [Test]
                public void Test()
                {
                    Assert.That(5, Is.GreaterThan(""4"").Using(new AsInt32Comparer()) & Is.LessThan(↓""6""));
                }", "using System.Collections;");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenDifferentComparableTypesProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
                class A : IComparable<A>
                {
                    public int CompareTo(A other) => 0;
                }
                class B : IComparable<B>
                {
                    public int CompareTo(B other) => 0;
                }

                class TestClass
                {
                    [Test]
                    public void TestMethod()
                    {
                        var actual = new A();
                        var expected = new B();
                        Assert.That(actual, Is.LessThanOrEqualTo(↓expected));
                    }
                }");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenDifferentNonGenericComparableTypesProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
                class A : IComparable
                {
                    public int CompareTo(object other) => 0;
                }
                class B : IComparable
                {
                    public int CompareTo(object other) => 0;
                }

                class TestClass
                {
                    [Test]
                    public void TestMethod()
                    {
                        var actual = new A();
                        var expected = new B();
                        Assert.That(actual, Is.LessThanOrEqualTo(↓expected));
                    }
                }");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenSameNonGenericComparableTypesProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
                class A : IComparable
                {
                    public int CompareTo(object other) => 0;
                }

                class TestClass
                {
                    [Test]
                    public void TestMethod()
                    {
                        var actual = new A();
                        var expected = new A();
                        Assert.That(actual, Is.LessThanOrEqualTo(expected));
                    }
                }");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenTypesHaveCompareToClassMethod()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
                class A
                {
                }
                class B
                {
                    public int CompareTo(A other) => 0;
                }

                class TestClass
                {
                    [Test]
                    public void TestMethod()
                    {
                        var actual = new A();
                        var expected = new B();
                        Assert.That(actual, Is.LessThanOrEqualTo(expected));
                    }
                }");

            AnalyzerAssert.Valid(analyzer, testCode);
        }
    }
}
