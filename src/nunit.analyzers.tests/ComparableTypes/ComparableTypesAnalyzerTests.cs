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

        private static readonly string[] NumericTypes = new[]
        {
            "decimal", "double", "float", "int", "uint", "long", "ulong", "short", "ushort"
        };

        [Test]
        public void AnalyzeWhenIncompatibleTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "Assert.That(1, Is.LessThan(↓\"1\"));");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenObjectTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(
                @"
        object o = 0;
        Assert.That(o, Is.LessThan(↓1));
        ");

            RoslynAssert.Diagnostics(analyzer,
                ExpectedDiagnostic.Create(AnalyzerIdentifiers.ComparableOnObject), testCode);
        }

        [Test]
        public void NoDiagnosticsWhenIComparableTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(
                @"
        IComparable smallValue = 0;
        IComparable bigValue = 9;
        Assert.That(smallValue, Is.LessThan(bigValue));
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticsWhenGenericIComparableTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(
                @"
        IComparable<int> smallValue = 0;
        int bigValue = 9;
        Assert.That(smallValue, Is.LessThan(bigValue));
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenNonComparableTypesProvidedWithLambdaActualValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class A { }

    public class Tests
    {
        [Test]
        public void AnalyzeWhenNonComparableTypesProvidedWithLambdaActualValue()
        {
            var expected = new A();
            Assert.That(() => new A(), Is.GreaterThanOrEqualTo(↓expected));
        }
    }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIncompatibleTypesProvidedWithLocalFunctionActualValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class A { }

    public class Tests
    {
        [Test]
        public void AnalyzeWhenIncompatibleTypesProvidedWithLocalFunctionActualValue()
        {
            A actual() => new A();
            var expected = new A();
            Assert.That(actual, Is.Not.GreaterThan(↓expected));
        }
    }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIncompatibleTypesProvidedWithFuncActualValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class A { }

    public class Tests
    {
        [Test]
        public void AnalyzeWhenIncompatibleTypesProvidedWithNegatedAssert()
        {
            Func<A> actual = () => new A();
            var expected = new A();
            Assert.That(actual, Is.LessThanOrEqualTo(↓expected));
        }
    }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIncompatibleTypesProvidedWithTaskActualValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class A { }

    public class Tests
    {
        [Test]
        public void AnalyzeWhenIncompatibleTypesProvidedWithNegatedAssert()
        {
            var actual = Task.FromResult(new A());
            var expected = new A();
            Assert.That(actual, Is.GreaterThan(↓expected));
        }
    }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
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

            RoslynAssert.Valid(analyzer, testCode);
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

            RoslynAssert.Valid(analyzer, testCode);
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

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void NoDiagnosticWhenActualHasIComparableOfExpected()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
                class A : IComparable<B>
                {
                    public int CompareTo(B? other) => 0;
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

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenExpectedHasIComparableOfActual()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
                class A { }

                class B : IComparable<A>
                {
                    public int CompareTo(A? other) => 0;
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

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticForGenericWithIComparableConstraint()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public static class LightWeightAssert
            {
                public static void Less<T>(T value, T threshold)
                    where T : IComparable
                {
                    if (value.CompareTo(threshold) >= 0)
                    {
                        Assert.That(value, Is.LessThan(threshold));
                    }
                }
            }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticForGenericWithIComparableTConstraint()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
            public static class LightWeightAssert
            {
                public static void Less<T>(T value, T threshold)
                    where T : IComparable<T>
                {
                    if (value.CompareTo(threshold) >= 0)
                    {
                        Assert.That(value, Is.LessThan(threshold));
                    }
                }
            }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenCustomComparerProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class A { }
    class B { }
    class ABComparer : IComparer
    {
        public int Compare(object? x, object? y) => 0;
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

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenExpectedIsMatchingNullableType()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
            int actual = 5;
            int? expected = 5;
            Assert.That(actual, Is.GreaterThan(expected));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenActualIsNumericNullableType()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
            int? actual = 5;
            double expected = 5.0;
            Assert.That(actual, Is.GreaterThan(expected));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenActualIsDelegate()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
            int actual = 6;
            double expected = 5.0;
            Assert.That(() => actual, Is.GreaterThan(expected));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenMoreThanOneConstraintExpressionIsUsed()
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                Assert.That(5, Is.GreaterThan(4) & Is.LessThan(↓""6""));");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenFirstConstraintExpressionUsesComparer()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                class AsInt32Comparer : IComparer
                {
                    public int Compare(object? x, object? y) => Convert.ToInt32(x).CompareTo(Convert.ToInt32(y));
                }

                [Test]
                public void Test()
                {
                    Assert.That(5, Is.GreaterThan(""4"").Using(new AsInt32Comparer()) & Is.LessThan(↓""6""));
                }", "using System.Collections;");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenDifferentComparableTypesProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
                class A : IComparable<A>
                {
                    public int CompareTo(A? other) => 0;
                }
                class B : IComparable<B>
                {
                    public int CompareTo(B? other) => 0;
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

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenTypeImplementsComparableToOtherTypeProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
                class A : IComparable<A>, IComparable<double>
                {
                    public int CompareTo(A? other) => 0;
                    public int CompareTo(double other) => 0;
                }

                class TestClass
                {
                    [Test]
                    public void TestMethod()
                    {
                        var actual = new A();
                        double expected = 123.4;
                        Assert.That(actual, Is.LessThanOrEqualTo(expected));
                        // Even though an 'int' is implicit convertible to a 'double', NUnit is not doing this.
                        int implicitConversion = 567;
                        Assert.That(actual, Is.LessThanOrEqualTo(↓implicitConversion));
                    }
                }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenDifferentNonGenericComparableTypesProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
                class A : IComparable
                {
                    public int CompareTo(object? other) => 0;
                }
                class B : IComparable
                {
                    public int CompareTo(object? other) => 0;
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

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenSameNonGenericComparableTypesProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
                class A : IComparable
                {
                    public int CompareTo(object? other) => 0;
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

            RoslynAssert.Valid(analyzer, testCode);
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
                    public int CompareTo(A? other) => 0;
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

            RoslynAssert.Valid(analyzer, testCode);
        }
    }
}
