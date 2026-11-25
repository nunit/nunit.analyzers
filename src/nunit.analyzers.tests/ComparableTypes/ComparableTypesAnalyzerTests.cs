using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.ComparableTypes.ComparableTypesAnalyzer>;

namespace NUnit.Analyzers.Tests.ComparableTypes
{
    public class ComparableTypesAnalyzerTests
    {
        private static readonly string[] NumericTypes =
        [
            "decimal", "double", "float", "int", "uint", "long", "ulong", "short", "ushort"
        ];

        private static readonly DiagnosticResult ComparableTypesDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ComparableTypes).WithLocation(0);
        private static readonly DiagnosticResult ComparableOnObjectDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ComparableOnObject).WithLocation(0);

        [Test]
        public async Task AnalyzeWhenIncompatibleTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "Assert.That(1, Is.LessThan({|#0:\"1\"|}));");

            await Verify.VerifyAnalyzerAsync(testCode, ComparableTypesDiagnostic);
        }

        [Test]
        public async Task AnalyzeWhenObjectTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                            object o = 0;
                            Assert.That(o, Is.LessThan({|#0:1|}));

                """);

            await Verify.VerifyAnalyzerAsync(testCode, ComparableOnObjectDiagnostic);
        }

        [Test]
        public async Task NoDiagnosticsWhenIComparableTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                            IComparable smallValue = 0;
                            IComparable bigValue = 9;
                            Assert.That(smallValue, Is.LessThan(bigValue));

                """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticsWhenGenericIComparableTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                            IComparable<int> smallValue = 0;
                            int bigValue = 9;
                            Assert.That(smallValue, Is.LessThan(bigValue));

                """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenNonComparableTypesProvidedWithLambdaActualValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class A { }

                public class Tests
                {
                    [Test]
                    public void AnalyzeWhenNonComparableTypesProvidedWithLambdaActualValue()
                    {
                        var expected = new A();
                        Assert.That(() => new A(), Is.GreaterThanOrEqualTo({|#0:expected|}));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, ComparableTypesDiagnostic);
        }

        [Test]
        public async Task AnalyzeWhenIncompatibleTypesProvidedWithLocalFunctionActualValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class A { }

                public class Tests
                {
                    [Test]
                    public void AnalyzeWhenIncompatibleTypesProvidedWithLocalFunctionActualValue()
                    {
                        A actual() => new A();
                        var expected = new A();
                        Assert.That(actual, Is.Not.GreaterThan({|#0:expected|}));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, ComparableTypesDiagnostic);
        }

        [Test]
        public async Task AnalyzeWhenIncompatibleTypesProvidedWithFuncActualValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class A { }

                public class Tests
                {
                    [Test]
                    public void AnalyzeWhenIncompatibleTypesProvidedWithNegatedAssert()
                    {
                        Func<A> actual = () => new A();
                        var expected = new A();
                        Assert.That(actual, Is.LessThanOrEqualTo({|#0:expected|}));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, ComparableTypesDiagnostic);
        }

        [Test]
        public async Task AnalyzeWhenIncompatibleTypesProvidedWithTaskActualValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class A { }

                public class Tests
                {
                    [Test]
                    public void AnalyzeWhenIncompatibleTypesProvidedWithNegatedAssert()
                    {
                        var actual = Task.FromResult(new A());
                        var expected = new A();
                        Assert.That(actual, Is.GreaterThan({|#0:expected|}));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, ComparableTypesDiagnostic);
        }

        [Test]
        public async Task NoDiagnosticForNumericTypes(
            [ValueSource(nameof(NumericTypes))] string actualType,
            [ValueSource(nameof(NumericTypes))] string expectedType)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        {{actualType}} actual = 1;
                        {{expectedType}} expected = 1;
                        Assert.That(actual, Is.LessThanOrEqualTo(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenSameEnumsProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenDifferentEnumsProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                enum EnumOne { A, B, C }
                enum EnumTwo { A, B, C }

                class TestClass
                {
                    [Test]
                    public void EnumTest()
                    {
                        var actual = EnumOne.B;
                        var expected = EnumTwo.B;
                        Assert.That(actual, Is.LessThan({|#0:expected|}));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, ComparableTypesDiagnostic);
        }

        [Test]
        public async Task NoDiagnosticWhenActualHasIComparableOfExpected()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenExpectedHasIComparableOfActual()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticForGenericWithIComparableConstraint()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticForGenericWithIComparableTConstraint()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenCustomComparerProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
                }
            """, "using System.Collections;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenExpectedIsMatchingNullableType()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        int actual = 5;
                        int? expected = 5;
                        Assert.That(actual, Is.GreaterThan(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenActualIsNumericNullableType()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        int? actual = 5;
                        double expected = 5.0;
                        Assert.That(actual, Is.GreaterThan(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenActualIsDelegate()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        int actual = 6;
                        double expected = 5.0;
                        Assert.That(() => actual, Is.GreaterThan(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenMoreThanOneConstraintExpressionIsUsed()
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That(5, Is.GreaterThan(4) & Is.LessThan({|#0:"6"|}));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, ComparableTypesDiagnostic);
        }

        [Test]
        public async Task AnalyzeWhenFirstConstraintExpressionUsesComparer()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    class AsInt32Comparer : IComparer
                    {
                        public int Compare(object? x, object? y) => Convert.ToInt32(x).CompareTo(Convert.ToInt32(y));
                    }

                    [Test]
                    public void Test()
                    {
                        Assert.That(5, Is.GreaterThan("4").Using(new AsInt32Comparer()) & Is.LessThan({|#0:"6"|}));
                    }
            """, "using System.Collections;");

            await Verify.VerifyAnalyzerAsync(testCode, ComparableTypesDiagnostic);
        }

        [Test]
        public async Task AnalyzeWhenDifferentComparableTypesProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
                        Assert.That(actual, Is.LessThanOrEqualTo({|#0:expected|}));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, ComparableTypesDiagnostic);
        }

        [Test]
        public async Task AnalyzeWhenTypeImplementsComparableToOtherTypeProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
                        Assert.That(actual, Is.LessThanOrEqualTo({|#0:implicitConversion|}));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, ComparableTypesDiagnostic);
        }

        [Test]
        public async Task AnalyzeWhenDifferentNonGenericComparableTypesProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
                        Assert.That(actual, Is.LessThanOrEqualTo({|#0:expected|}));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, ComparableTypesDiagnostic);
        }

        [Test]
        public async Task AnalyzeWhenSameNonGenericComparableTypesProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenTypesHaveCompareToClassMethod()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

#if NUNIT5
        [Test]
        public void NoDiagnosticWhenTupleAndValueTupleAreCompared()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var tuple = Tuple.Create(1, 2);
                var valueTuple = (3, 4);
                Assert.That(tuple, Is.LessThan(valueTuple));
                Assert.That(valueTuple, Is.GreaterThan(tuple));");

            RoslynAssert.Valid(analyzer, testCode);
        }
#else
        [Test]
        public void AnalyzeWhenTupleAndValueTupleAreCompared()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var tuple = Tuple.Create(1, 2);
                var valueTuple = (3, 4);
                Assert.That(tuple, Is.LessThan(↓valueTuple));
                Assert.That(valueTuple, Is.GreaterThan(↓tuple));");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }
#endif
    }
}
