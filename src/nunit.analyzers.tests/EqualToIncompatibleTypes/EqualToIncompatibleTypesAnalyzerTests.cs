using System.Globalization;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.EqualToIncompatibleTypes.EqualToIncompatibleTypesAnalyzer>;

namespace NUnit.Analyzers.Tests.EqualToIncompatibleTypes
{
    public class EqualToIncompatibleTypesAnalyzerTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.EqualToIncompatibleTypes);

        private static readonly string[] NumericTypes =
        [
            "decimal", "double", "float", "int", "uint", "long", "ulong", "short", "ushort"
        ];

        [Test]
        public async Task AnalyzeWhenIncompatibleTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "Assert.That(1, Is.EqualTo({|#0:\"1\"|}));");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeClassicWhenIncompatibleTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "ClassicAssert.AreEqual({|#0:\"1\"|}, 1);");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenCollectionsWithIncompatibleValueTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new List<string> { "1" };
                        var expected = new [] { 1.0 };
                        Assert.That(actual, Is.EqualTo({|#0:expected|}));
            """,
                additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenDictionariesWithIncompatibleValueTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new Dictionary<string, string> { ["a"] = "1" };
                        var expected = new Dictionary<string, double> { ["a"] = 1.0 };
                        Assert.That(actual, Is.EqualTo({|#0:expected|}));
            """,
                additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenDictionariesWithDifferentNumericKeyTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new Dictionary<int, string> { [1] = "1" };
                        var expected = new Dictionary<double, string> { [1.0] = "1" };
                        Assert.That(actual, Is.EqualTo({|#0:expected|}));
            """,
                additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenTuplesWithIncompatibleElementTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = Tuple.Create("1", "2");
                        var expected = Tuple.Create("1", 2);
                        Assert.That(actual, Is.EqualTo({|#0:expected|}));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenValueTuplesWithIncompatibleElementTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = ("1", "2");
                        var expected = ("1", 2);
                        Assert.That(actual, Is.EqualTo({|#0:expected|}));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
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
                        Assert.That(actual, Is.EqualTo({|#0:expected|}));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenDifferentNullableEnumsProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                enum EnumOne { A, B, C }
                enum EnumTwo { A, B, C }

                class TestClass
                {
                    [Test]
                    public void EnumTest()
                    {
                        EnumOne? actual = EnumOne.B;
                        EnumTwo expected = EnumTwo.B;
                        Assert.That(actual, Is.EqualTo({|#0:expected|}));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenIncompatibleTypesWithCyclicTypesProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class A : IEnumerable<A>
                {
                    public IEnumerator<A> GetEnumerator() => null!;
                    IEnumerator IEnumerable.GetEnumerator() => null!;
                }

                class B : IEnumerable<B>
                {
                    public IEnumerator<B> GetEnumerator() => null!;
                    IEnumerator IEnumerable.GetEnumerator() => null!;
                }

                public class Tests
                {
                    [Test]
                    public void AnalyzeWhenIncompatibleTypesProvided()
                    {
                        var actual = new A();
                        var expected = new B();
                        Assert.That(actual, Is.EqualTo({|#0:expected|}));
                    }
                }
            """,
                additionalUsings: """
                    using System.Collections;
                    using System.Collections.Generic;
                """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenIncompatibleTypesProvidedWithCombinedConstraints()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class A { }
                class B { }

                public class Tests
                {
                    [Test]
                    public void AnalyzeWhenIncompatibleTypesProvided()
                    {
                        var actual = new A();
                        var expected = new B();
                        Assert.That(actual, Is.Not.Null & Is.EqualTo({|#0:expected|}));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenIncompatibleTypesProvidedWithModifier()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = 1;
                        var expected = "A";
                        Assert.That(actual, Is.EqualTo({|#0:expected|}).IgnoreCase);
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenIncompatibleTypesProvidedWithMessage()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = 1;
                        var expected = "A";
                        Assert.That(actual, Is.EqualTo({|#0:expected|}), "Assertion Message");
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenIncompatibleTypesProvidedWithNegatedAssert()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class A { }
                class B { }

                public class Tests
                {
                    [Test]
                    public void AnalyzeWhenIncompatibleTypesProvidedWithNegatedAssert()
                    {
                        var actual = new A();
                        var expected = new B();
                        Assert.That(actual, Is.Not.EqualTo({|#0:expected|}));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeClassicWhenIncompatibleTypesProvidedWithNegatedAssert()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class A { }
                class B { }

                public class Tests
                {
                    [Test]
                    public void AnalyzeWhenIncompatibleTypesProvidedWithNegatedAssert()
                    {
                        var actual = new A();
                        var expected = new B();
                        ClassicAssert.AreNotEqual({|#0:expected|}, actual);
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenIncompatibleTypesProvidedWithLambdaActualValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class A { }
                class B { }

                public class Tests
                {
                    [Test]
                    public void AnalyzeWhenIncompatibleTypesProvidedWithNegatedAssert()
                    {
                        var expected = new B();
                        Assert.That(() => new A(), Is.Not.EqualTo({|#0:expected|}));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenIncompatibleTypesProvidedWithLocalFunctionActualValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class A { }
                class B { }

                public class Tests
                {
                    [Test]
                    public void AnalyzeWhenIncompatibleTypesProvidedWithNegatedAssert()
                    {
                        A actual() => new A();
                        var expected = new B();
                        Assert.That(actual, Is.Not.EqualTo({|#0:expected|}));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenIncompatibleTypesProvidedWithFuncActualValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class A { }
                class B { }

                public class Tests
                {
                    [Test]
                    public void AnalyzeWhenIncompatibleTypesProvidedWithNegatedAssert()
                    {
                        Func<A> actual = () => new A();
                        var expected = new B();
                        Assert.That(actual, Is.Not.EqualTo({|#0:expected|}));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenIncompatibleTypesProvidedWithTaskActualValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class A { }
                class B { }

                public class Tests
                {
                    [Test]
                    public void AnalyzeWhenIncompatibleTypesProvidedWithNegatedAssert()
                    {
                        var actual = Task.FromResult(new A());
                        var expected = new B();
                        Assert.That(actual, Is.Not.EqualTo({|#0:expected|}));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenActualAndExpectedTypesAreSameWithTaskActualValue()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = Task.FromResult("");
                        var expected = "";
                        Assert.That(actual, Is.EqualTo({|#0:expected|}));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenActualIsIncompatibleNullableType()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        int? actual = 5;
                        string expected = "5";
                        Assert.That(actual, Is.EqualTo({|#0:expected|}));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task NoDiagnosticWhenActualAndExpectedTypesAreSame()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "";
                        var expected = "";
                        Assert.That(actual, Is.EqualTo(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenActualAndExpectedTypesAreSameWithNegatedAssert()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "";
                        var expected = "";
                        Assert.That(actual, Is.Not.EqualTo(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenActualAndExpectedTypesAreSameWithLambdaActualValue()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var expected = "";
                        Assert.That(() => "", Is.EqualTo(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenActualAndExpectedTypesAreSameWithFuncActualValue()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Func<string> actual = () => "";
                        var expected = "";
                        Assert.That(actual, Is.EqualTo(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenActualAndExpectedTypesAreSameWithLocalFunctionActualValue()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        string actual() => "";
                        var expected = "";
                        Assert.That(actual, Is.EqualTo(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenExpectedTypeInheritsActual()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class A { }
                class B : A { }

                public class Tests
                {
                    [Test]
                    public void TestMethod()
                    {
                        var actual = new A();
                        var expected = new B();
                        Assert.That(actual, Is.EqualTo(expected));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenActualTypeInheritsExpected()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class A : B { }
                class B { }

                public class Tests
                {
                    [Test]
                    public void NoDiagnosticWhenActualTypeInheritsExpected()
                    {
                        var actual = new A();
                        var expected = new B();
                        Assert.That(actual, Is.EqualTo(expected));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticForNumericTypes(
            [ValueSource(nameof(NumericTypes))] string actualType,
            [ValueSource(nameof(NumericTypes))] string expectedType)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        {{actualType}} actual = 1;
                        {{expectedType}} expected = 1;
                        Assert.That(actual, Is.EqualTo(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticForCharAndInt()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = 'a';
                        var expected = (int)actual;
                        Assert.That(actual, Is.EqualTo(expected));
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
                        var expected = EnumOne.B;
                        Assert.That(actual, Is.EqualTo(expected));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenSameNullableEnumsProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                enum EnumOne { A, B, C }

                class TestClass
                {
                    [Test]
                    public void EnumTest()
                    {
                        EnumOne? actual = EnumOne.B;
                        EnumOne expected = EnumOne.B;
                        Assert.That(actual, Is.EqualTo(expected));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticForStreams()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new System.IO.FileStream("", System.IO.FileMode.Open);;
                        var expected = new System.IO.MemoryStream();
                        Assert.That(actual, Is.EqualTo(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticForDifferentCollectionsWithSameType()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new List<string> { "a" };
                        var expected = new [] { "a" };
                        Assert.That(actual, Is.EqualTo(expected));
            """,
                additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticForDifferentCollectionsWithNumerics()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new List<int> { 1 };
                        var expected = new [] { 1.0 };
                        Assert.That(actual, Is.EqualTo(expected));
            """,
                additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticForDictionariesOfNumerics()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new Dictionary<string, int> { ["a"] = 1 };
                        var expected = new Dictionary<string, double> { ["a"] = 1.0 };
                        Assert.That(actual, Is.EqualTo(expected));
            """,
                additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticForKeyValuePairsOfNumerics()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new KeyValuePair<string, int>("a", 1);
                        var expected = new KeyValuePair<string, double>("a", 1.0);
                        Assert.That(actual, Is.EqualTo(expected));
            """,
                additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticForTuplesWithCompatibleElementTypes()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = Tuple.Create("1", 2.0);
                        var expected = Tuple.Create("1", 2);
                        Assert.That(actual, Is.EqualTo(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticForValueTuplesWithCompatibleElementTypes()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = ("1", 2.0);
                        var expected = ("1", 2);
                        Assert.That(actual, Is.EqualTo(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenActualHasIEquatableOfExpected()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class A : IEquatable<B>
                {
                    public bool Equals(B? other) => true;
                }

                class B { }

                public class Tests
                {
                    [Test]
                    public void TestMethod()
                    {
                        var actual = new A();
                        var expected = new B();
                        Assert.That(actual, Is.EqualTo(expected));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenExpectedHasIEquatableOfActual()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class A { }

                class B : IEquatable<A>
                {
                    public bool Equals(A? other) => true;
                }

                public class Tests
                {
                    [Test]
                    public void TestMethod()
                    {
                        var actual = new A();
                        var expected = new B();
                        Assert.That(actual, Is.EqualTo(expected));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenExpectedHasIEquatableOfActualAndIsIEnumerable()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class B : IEquatable<string>, IEnumerable<string>
                {
                    public bool Equals(string? other) => true;

                    public IEnumerator<string> GetEnumerator() => new List<string>().GetEnumerator();
                    IEnumerator IEnumerable.GetEnumerator() => new List<string>().GetEnumerator();
                }

                public class Tests
                {
                    [Test]
                    public void TestMethod()
                    {
                        var actual = "123";
                        var expected = new B();
                        Assert.That(actual, Is.EqualTo(expected));
                    }
                }
            """,
                additionalUsings: "using System.Collections;" +
                    "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenActualIsDynamic()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        dynamic actual = 1;
                        var expected = "";
                        Assert.That(actual, Is.EqualTo(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenExpectedIsDynamic()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "";
                        dynamic expected = 2;
                        Assert.That(actual, Is.EqualTo(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenCustomComparerProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class A { }
                class B { }

                public class Tests
                {
                    [Test]
                    public void TestMethod()
                    {
                        var actual = new A();
                        var expected = new B();
                        Assert.That(actual, Is.EqualTo(expected).Using<A, B>((a, b) => true));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticForOtherConstraints()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class A { }
                class B { }

                public class Tests
                {
                    [Test]
                    public void NoDiagnosticForOtherConstraints()
                    {
                        var actual = new A();
                        var expected = new B();
                        Assert.That(actual, Is.SameAs(expected));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenUsedWithPropertyConstraint()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual, Has.Property("Length").EqualTo(3));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenActualIsMatchingNullableType()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        int? actual = 5;
                        int expected = 5;
                        Assert.That(actual, Is.EqualTo(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenExpectedIsMatchingNullableType()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        int actual = 5;
                        int? expected = 5;
                        Assert.That(actual, Is.EqualTo(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenActualIsNumericNullableType()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        int? actual = 5;
                        double expected = 5.0;
                        Assert.That(actual, Is.EqualTo(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenConditionalPrefixPresent()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        bool shouldBePresent = true;
                        Assert.That(new[] { 1, 2, 3 }, (shouldBePresent ? Has.Some : Has.None).EqualTo(2));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenVariableIsPartOfConstraint()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        bool shouldBePresent = true;
                        var constraintModifier = (shouldBePresent ? Has.Some : Has.None);
                        Assert.That(new[] { 1, 2, 3 }, constraintModifier.EqualTo(2));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenObjectComparedToInt()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        object actual = 3;
                        Assert.That(actual, Is.EqualTo(3));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenExpectedIsErrorType()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = 3;
                        var expected = HaveNoIdea();
                        Assert.That(actual, Is.EqualTo(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
        }

        [Test]
        public async Task NoDiagnosticWhenActualIsErrorType()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = HaveNoIdea();
                        var expected = 3;
                        Assert.That(actual, Is.EqualTo(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
        }

        [Test]
        public async Task NoDiagnosticWhenActualIsErrorTypeArray()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new[] { HaveNoIdea() };
                        var expected = new[] { 3 };
                        Assert.That(actual, Is.EqualTo(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
        }

        [Test]
        public async Task NoDiagnosticWhenExpectedIsErrorTypeArray()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new[] { 3 };
                        var expected = HaveNoIdea();
                        Assert.That(actual, Is.EqualTo(new[] { expected }));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
        }

        [Test]
        public async Task NoDiagnosticWhenCombinedConstraintAndUseAllOperator()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new[] { 1,2,2,1 };
                        Assert.That(actual, Has.All.EqualTo(1).Or.EqualTo(2));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenComparingDelegate()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Action x = () => {};
                        var y = x;
                        Assert.That(y, Is.EqualTo(x));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenComparingTask()
        {
            var testCode = TestUtility.WrapInAsyncTestMethod("""
                        Task wait = Task.CompletedTask;
                        Assert.That(await Task.WhenAny(wait).ConfigureAwait(false), Is.EqualTo(wait));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenComparingTask2()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Task task1 = Task.CompletedTask;
                        Task task2 = Task.CompletedTask;
                        Assert.That(task1, Is.SameAs(task2));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenMoreThanOneConstraintExpressionIsUsed()
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That(5, Is.Not.EqualTo(4) & Is.Not.EqualTo({|#0:"6"|}));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [TestCase("InstanceOf<{0}>()")]
        [TestCase("InstanceOf(typeof({0}))")]
        [TestCase("InstanceOf(exception.GetType())")]
        [TestCase("TypeOf<{0}>()")]
        [TestCase("TypeOf(typeof({0}))")]
        [TestCase("{0}")]
        [TestCase("Exception")]
        public async Task AnalyzeWhenThrowsWithTypeConstraintIsUsed(string constraintSpecification)
        {
            const string exceptionType = "InvalidOperationException";
            string constraint = string.Format(CultureInfo.InvariantCulture, constraintSpecification, exceptionType);

            var testCode = TestUtility.WrapInTestMethod($$"""
                        var exception = new {{exceptionType}}();
                        void ThrowingMethod() => throw exception;

                        Assert.That(() => ThrowingMethod(), Throws.{{constraint}}.And.EqualTo(exception));

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("InstanceOf<ArgumentException>()")]
        [TestCase("InstanceOf(typeof(ArgumentNullException))")]
        [TestCase("TargetInvocationException")]
        [TestCase("ArgumentNullException")]
        [TestCase("ArgumentException")]
        public async Task AnalyzeWhenThrowsWithTypeConstraintIsUsedWithDifferentType(string constraintSpecification)
        {
            const string exceptionType = "InvalidOperationException";
            string constraint = string.Format(CultureInfo.InvariantCulture, constraintSpecification, exceptionType);

            var testCode = TestUtility.WrapInTestMethod($$"""
                        var exception = new {{exceptionType}}();
                        void ThrowingMethod() => throw exception;

                        Assert.That(() => ThrowingMethod(), Throws.{{constraint}}.And.EqualTo({|#0:exception|}));

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenComparingUriWithString()
        {
            // Uri.Equals(object? comparand) has code that checks if comparand is a string.
            // There is no separate overload for Uri.Equals(string?) to check for.
            var testCode = TestUtility.WrapInTestMethod("""
                        const string testString = "test";
                        Uri testUri = new Uri("test", UriKind.Relative);

                        Assert.That(testUri, Is.EqualTo(testString));

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenComparingStringWithUri()
        {
            // string.Equals(Uri) always fails
            var testCode = TestUtility.WrapInTestMethod("""
                        const string testString = "test";
                        Uri testUri = new Uri("test", UriKind.Relative);

                        Assert.That(testString, Is.EqualTo({|#0:testUri|}));

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task NoDiagnosticWhenComparingInstanceWithValueCastToInstanceType()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                [Test]
                public void TestMethod()
                {
                    MyLong l0 = new(0);

                    Assert.That(l0, Is.EqualTo((MyLong)0));
                }

                private readonly struct MyLong : IEquatable<MyLong>
                {
                    private readonly long _value;

                    public MyLong(long value) => _value = value;

                    public bool Equals(MyLong other) => _value == other._value;

                    public override bool Equals(object? obj) => obj is MyLong other && Equals(other);
                    public override int GetHashCode() => _value.GetHashCode();

                    public static implicit operator MyLong(long value) => new(value);
                    public static implicit operator long(MyLong value) => value._value;
                }
                """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

#if NUNIT4
        [Test]
        public async Task AnalyzeWhenUsingPropertiesComparerOverloadWithoutArgument()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                [Test]
                public void TestMethod()
                {
                    var record1 = new Record("Test");
                    var record2 = new AnotherRecord("Test");

                    Assert.That(record1, Is.EqualTo({|#0:record2|}).UsingPropertiesComparer());
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

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
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
                    var record1 = new Record("Test");
                    var record2 = new AnotherRecord("Test");

                    Assert.That(record1, Is.EqualTo(record2).UsingPropertiesComparer<AnotherRecord>(
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

        [TestCase("1.1f")]
        [TestCase("2.2f")]
        public async Task AnalyzeIsNotNaNOnValidTypes(string actual)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var actual = {{actual}};
                        Assert.That(actual, Is.Not.NaN);

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("1.1f")]
        [TestCase("2.2f")]
        public async Task AnalyzeIsNaNOnValidTypes(string actual)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var actual = {{actual}};
                        Assert.That(actual, Is.NaN);

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("3.3m")]
        [TestCase("4")]
        [TestCase("string.Empty")]
        public async Task AnalyzeIsNotNaNOnUnsupportedTypes(string actual)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var actual = {{actual}};
                        Assert.That(actual, {|#0:Is.Not.NaN|});

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [TestCase("3.3m")]
        [TestCase("4")]
        [TestCase("string.Empty")]
        public async Task AnalyzeIsNaNOnUnsupportedTypes(string actual)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var actual = {{actual}};
                        Assert.That(actual, Is.Positive.Or.{|#0:NaN|});

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeOnNullableDouble()
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        double? actual = Math.PI;
                        Assert.That(actual, Is.Not.NaN);
                        Assert.That(actual, Is.EqualTo(Math.PI));

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeOnFuncReturningDouble()
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        Func<double> actual = () => Math.PI;
                        Assert.That(actual, Is.Not.NaN);
                        Assert.That(actual, Is.EqualTo(Math.PI));

            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeOnAsyncFuncReturningDouble()
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        Func<Task<double>> actual = () => Task.FromResult(Math.PI);
                        Assert.That(actual, Is.Not.NaN);
                        Assert.That(actual, Is.EqualTo(Math.PI));

            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
