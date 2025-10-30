using System.Globalization;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.EqualToIncompatibleTypes;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.EqualToIncompatibleTypes
{
    public class EqualToIncompatibleTypesAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new EqualToIncompatibleTypesAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.EqualToIncompatibleTypes);

        private static readonly string[] NumericTypes = new[]
        {
            "decimal", "double", "float", "int", "uint", "long", "ulong", "short", "ushort"
        };

        [Test]
        public void AnalyzeWhenIncompatibleTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "Assert.That(1, Is.EqualTo(↓\"1\"));");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeClassicWhenIncompatibleTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "ClassicAssert.AreEqual(↓\"1\", 1);");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenCollectionsWithIncompatibleValueTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new List<string> { ""1"" };
                var expected = new [] { 1.0 };
                Assert.That(actual, Is.EqualTo(↓expected));",
                additionalUsings: "using System.Collections.Generic;");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenDictionariesWithIncompatibleValueTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new Dictionary<string, string> { [""a""] = ""1"" };
                var expected = new Dictionary<string, double> { [""a""] = 1.0 };
                Assert.That(actual, Is.EqualTo(↓expected));",
                additionalUsings: "using System.Collections.Generic;");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenDictionariesWithDifferentNumericKeyTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new Dictionary<int, string> { [1] = ""1"" };
                var expected = new Dictionary<double, string> { [1.0] = ""1"" };
                Assert.That(actual, Is.EqualTo(↓expected));",
                additionalUsings: "using System.Collections.Generic;");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenTuplesWithIncompatibleElementTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = Tuple.Create(""1"", ""2"");
                var expected = Tuple.Create(""1"", 2);
                Assert.That(actual, Is.EqualTo(↓expected));");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenValueTuplesWithIncompatibleElementTypesProvided()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = (""1"", ""2"");
                var expected = (""1"", 2);
                Assert.That(actual, Is.EqualTo(↓expected));");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
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
                        Assert.That(actual, Is.EqualTo(↓expected));
                    }
                }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenDifferentNullableEnumsProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
                enum EnumOne { A, B, C }
                enum EnumTwo { A, B, C }

                class TestClass
                {
                    [Test]
                    public void EnumTest()
                    {
                        EnumOne? actual = EnumOne.B;
                        EnumTwo expected = EnumTwo.B;
                        Assert.That(actual, Is.EqualTo(↓expected));
                    }
                }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIncompatibleTypesWithCyclicTypesProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
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
                        Assert.That(actual, Is.EqualTo(↓expected));
                    }
                }",
                additionalUsings: @"using System.Collections;
using System.Collections.Generic;");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIncompatibleTypesProvidedWithCombinedConstraints()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class A { }
    class B { }

    public class Tests
    {
        [Test]
        public void AnalyzeWhenIncompatibleTypesProvided()
        {
            var actual = new A();
            var expected = new B();
            Assert.That(actual, Is.Not.Null & Is.EqualTo(↓expected));
        }
    }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIncompatibleTypesProvidedWithModifier()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
            var actual = 1;
            var expected = ""A"";
            Assert.That(actual, Is.EqualTo(↓expected).IgnoreCase);");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIncompatibleTypesProvidedWithMessage()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
            var actual = 1;
            var expected = ""A"";
            Assert.That(actual, Is.EqualTo(↓expected), ""Assertion Message"");");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIncompatibleTypesProvidedWithNegatedAssert()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class A { }
    class B { }

    public class Tests
    {
        [Test]
        public void AnalyzeWhenIncompatibleTypesProvidedWithNegatedAssert()
        {
            var actual = new A();
            var expected = new B();
            Assert.That(actual, Is.Not.EqualTo(↓expected));
        }
    }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeClassicWhenIncompatibleTypesProvidedWithNegatedAssert()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class A { }
    class B { }

    public class Tests
    {
        [Test]
        public void AnalyzeWhenIncompatibleTypesProvidedWithNegatedAssert()
        {
            var actual = new A();
            var expected = new B();
            ClassicAssert.AreNotEqual(↓expected, actual);
        }
    }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIncompatibleTypesProvidedWithLambdaActualValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class A { }
    class B { }

    public class Tests
    {
        [Test]
        public void AnalyzeWhenIncompatibleTypesProvidedWithNegatedAssert()
        {
            var expected = new B();
            Assert.That(() => new A(), Is.Not.EqualTo(↓expected));
        }
    }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIncompatibleTypesProvidedWithLocalFunctionActualValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class A { }
    class B { }

    public class Tests
    {
        [Test]
        public void AnalyzeWhenIncompatibleTypesProvidedWithNegatedAssert()
        {
            A actual() => new A();
            var expected = new B();
            Assert.That(actual, Is.Not.EqualTo(↓expected));
        }
    }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIncompatibleTypesProvidedWithFuncActualValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class A { }
    class B { }

    public class Tests
    {
        [Test]
        public void AnalyzeWhenIncompatibleTypesProvidedWithNegatedAssert()
        {
            Func<A> actual = () => new A();
            var expected = new B();
            Assert.That(actual, Is.Not.EqualTo(↓expected));
        }
    }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIncompatibleTypesProvidedWithTaskActualValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class A { }
    class B { }

    public class Tests
    {
        [Test]
        public void AnalyzeWhenIncompatibleTypesProvidedWithNegatedAssert()
        {
            var actual = Task.FromResult(new A());
            var expected = new B();
            Assert.That(actual, Is.Not.EqualTo(↓expected));
        }
    }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenActualAndExpectedTypesAreSameWithTaskActualValue()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = Task.FromResult("""");
                var expected = """";
                Assert.That(actual, Is.EqualTo(↓expected));");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenActualIsIncompatibleNullableType()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
            int? actual = 5;
            string expected = ""5"";
            Assert.That(actual, Is.EqualTo(↓expected));");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void NoDiagnosticWhenActualAndExpectedTypesAreSame()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = """";
                var expected = """";
                Assert.That(actual, Is.EqualTo(expected));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenActualAndExpectedTypesAreSameWithNegatedAssert()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = """";
                var expected = """";
                Assert.That(actual, Is.Not.EqualTo(expected));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenActualAndExpectedTypesAreSameWithLambdaActualValue()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var expected = """";
                Assert.That(() => """", Is.EqualTo(expected));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenActualAndExpectedTypesAreSameWithFuncActualValue()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Func<string> actual = () => """";
                var expected = """";
                Assert.That(actual, Is.EqualTo(↓expected));");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void NoDiagnosticWhenActualAndExpectedTypesAreSameWithLocalFunctionActualValue()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                string actual() => """";
                var expected = """";
                Assert.That(actual, Is.EqualTo(expected));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenExpectedTypeInheritsActual()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
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
    }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenActualTypeInheritsExpected()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
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
    }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticForNumericTypes(
            [ValueSource(nameof(NumericTypes))] string actualType,
            [ValueSource(nameof(NumericTypes))] string expectedType)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                {actualType} actual = 1;
                {expectedType} expected = 1;
                Assert.That(actual, Is.EqualTo(expected));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticForCharAndInt()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = 'a';
                var expected = (int)actual;
                Assert.That(actual, Is.EqualTo(expected));");

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
                        var expected = EnumOne.B;
                        Assert.That(actual, Is.EqualTo(expected));
                    }
                }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenSameNullableEnumsProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
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
                }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticForStreams()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new System.IO.FileStream("""", System.IO.FileMode.Open);;
                var expected = new System.IO.MemoryStream();
                Assert.That(actual, Is.EqualTo(expected));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticForDifferentCollectionsWithSameType()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new List<string> { ""a"" };
                var expected = new [] { ""a"" };
                Assert.That(actual, Is.EqualTo(expected));",
                additionalUsings: "using System.Collections.Generic;");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticForDifferentCollectionsWithNumerics()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new List<int> { 1 };
                var expected = new [] { 1.0 };
                Assert.That(actual, Is.EqualTo(expected));",
                additionalUsings: "using System.Collections.Generic;");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticForDictionariesOfNumerics()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new Dictionary<string, int> { [""a""] = 1 };
                var expected = new Dictionary<string, double> { [""a""] = 1.0 };
                Assert.That(actual, Is.EqualTo(expected));",
                additionalUsings: "using System.Collections.Generic;");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticForKeyValuePairsOfNumerics()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new KeyValuePair<string, int>(""a"", 1);
                var expected = new KeyValuePair<string, double>(""a"", 1.0);
                Assert.That(actual, Is.EqualTo(expected));",
                additionalUsings: "using System.Collections.Generic;");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticForTuplesWithCompatibleElementTypes()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = Tuple.Create(""1"", 2.0);
                var expected = Tuple.Create(""1"", 2);
                Assert.That(actual, Is.EqualTo(expected));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticForValueTuplesWithCompatibleElementTypes()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = (""1"", 2.0);
                var expected = (""1"", 2);
                Assert.That(actual, Is.EqualTo(expected));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenActualHasIEquatableOfExpected()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
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
                }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenExpectedHasIEquatableOfActual()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
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
                }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenExpectedHasIEquatableOfActualAndIsIEnumerable()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
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
                        var actual = ""123"";
                        var expected = new B();
                        Assert.That(actual, Is.EqualTo(expected));
                    }
                }",
                additionalUsings: "using System.Collections;" +
                    "using System.Collections.Generic;");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenActualIsDynamic()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                dynamic actual = 1;
                var expected = """";
                Assert.That(actual, Is.EqualTo(expected));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenExpectedIsDynamic()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = """";
                dynamic expected = 2;
                Assert.That(actual, Is.EqualTo(expected));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenCustomComparerProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
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
    }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticForOtherConstraints()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
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
    }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenUsedWithPropertyConstraint()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
            var actual = ""abc"";
            Assert.That(actual, Has.Property(""Length"").EqualTo(3));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenActualIsMatchingNullableType()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
            int? actual = 5;
            int expected = 5;
            Assert.That(actual, Is.EqualTo(expected));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenExpectedIsMatchingNullableType()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
            int actual = 5;
            int? expected = 5;
            Assert.That(actual, Is.EqualTo(expected));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenActualIsNumericNullableType()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
            int? actual = 5;
            double expected = 5.0;
            Assert.That(actual, Is.EqualTo(expected));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenConditionalPrefixPresent()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
            bool shouldBePresent = true;
            Assert.That(new[] { 1, 2, 3 }, (shouldBePresent ? Has.Some : Has.None).EqualTo(2));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenVariableIsPartOfConstraint()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
            bool shouldBePresent = true;
            var constraintModifier = (shouldBePresent ? Has.Some : Has.None);
            Assert.That(new[] { 1, 2, 3 }, constraintModifier.EqualTo(2));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenObjectComparedToInt()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                object actual = 3;
                Assert.That(actual, Is.EqualTo(3));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenExpectedIsErrorType()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = 3;
                var expected = HaveNoIdea();
                Assert.That(actual, Is.EqualTo(expected));");

            RoslynAssert.NoAnalyzerDiagnostics(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenActualIsErrorType()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = HaveNoIdea();
                var expected = 3;
                Assert.That(actual, Is.EqualTo(expected));");

            RoslynAssert.NoAnalyzerDiagnostics(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenActualIsErrorTypeArray()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new[] { HaveNoIdea() };
                var expected = new[] { 3 };
                Assert.That(actual, Is.EqualTo(expected));");

            RoslynAssert.NoAnalyzerDiagnostics(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenExpectedIsErrorTypeArray()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new[] { 3 };
                var expected = HaveNoIdea();
                Assert.That(actual, Is.EqualTo(new[] { expected }));");

            RoslynAssert.NoAnalyzerDiagnostics(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenCombinedConstraintAndUseAllOperator()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new[] { 1,2,2,1 };
                Assert.That(actual, Has.All.EqualTo(1).Or.EqualTo(2));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenComparingDelegate()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Action x = () => {};
                var y = x;
                Assert.That(y, Is.EqualTo(x));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenComparingTask()
        {
            var testCode = TestUtility.WrapInAsyncTestMethod(@"
                Task wait = Task.CompletedTask;
                Assert.That(await Task.WhenAny(wait).ConfigureAwait(false), Is.EqualTo(wait));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenComparingTask2()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Task task1 = Task.CompletedTask;
                Task task2 = Task.CompletedTask;
                Assert.That(task1, Is.SameAs(task2));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenMoreThanOneConstraintExpressionIsUsed()
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                Assert.That(5, Is.Not.EqualTo(4) & Is.Not.EqualTo(↓""6""));");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [TestCase("InstanceOf<{0}>()")]
        [TestCase("InstanceOf(typeof({0}))")]
        [TestCase("InstanceOf(exception.GetType())")]
        [TestCase("TypeOf<{0}>()")]
        [TestCase("TypeOf(typeof({0}))")]
        [TestCase("{0}")]
        [TestCase("Exception")]
        public void AnalyzeWhenThrowsWithTypeConstraintIsUsed(string constraintSpecification)
        {
            const string exceptionType = "InvalidOperationException";
            string constraint = string.Format(CultureInfo.InvariantCulture, constraintSpecification, exceptionType);

            var testCode = TestUtility.WrapInTestMethod($@"
                var exception = new {exceptionType}();
                void ThrowingMethod() => throw exception;

                Assert.That(() => ThrowingMethod(), Throws.{constraint}.And.EqualTo(exception));
            ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase("InstanceOf<ArgumentException>()")]
        [TestCase("InstanceOf(typeof(ArgumentNullException))")]
        [TestCase("TargetInvocationException")]
        [TestCase("ArgumentNullException")]
        [TestCase("ArgumentException")]
        public void AnalyzeWhenThrowsWithTypeConstraintIsUsedWithDifferentType(string constraintSpecification)
        {
            const string exceptionType = "InvalidOperationException";
            string constraint = string.Format(CultureInfo.InvariantCulture, constraintSpecification, exceptionType);

            var testCode = TestUtility.WrapInTestMethod($@"
                var exception = new {exceptionType}();
                void ThrowingMethod() => throw exception;

                Assert.That(() => ThrowingMethod(), Throws.{constraint}.And.EqualTo(↓exception));
            ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenComparingUriWithString()
        {
            // Uri.Equals(object? comparand) has code that checks if comparand is a string.
            // There is no separate overload for Uri.Equals(string?) to check for.
            var testCode = TestUtility.WrapInTestMethod(@"
                const string testString = ""test"";
                Uri testUri = new Uri(""test"", UriKind.Relative);

                Assert.That(testUri, Is.EqualTo(testString));
            ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenComparingStringWithUri()
        {
            // string.Equals(Uri) always fails
            var testCode = TestUtility.WrapInTestMethod(@"
                const string testString = ""test"";
                Uri testUri = new Uri(""test"", UriKind.Relative);

                Assert.That(testString, Is.EqualTo(↓testUri));
            ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void NoDiagnosticWhenComparingInstanceWithValueCastToInstanceType()
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

            RoslynAssert.Valid(analyzer, testCode);
        }

#if NUNIT4
        [Test]
        public void AnalyzeWhenUsingPropertiesComparerOverloadWithoutArgument()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                [Test]
                public void TestMethod()
                {
                    var record1 = new Record("Test");
                    var record2 = new AnotherRecord("Test");

                    Assert.That(record1, Is.EqualTo(↓record2).UsingPropertiesComparer());
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

            RoslynAssert.Valid(analyzer, testCode);
        }
#endif

        [TestCase("1.1f")]
        [TestCase("2.2f")]
        public void AnalyzeIsNotNaNOnValidTypes(string actual)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                var actual = {actual};
                Assert.That(actual, Is.Not.NaN);
            ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase("1.1f")]
        [TestCase("2.2f")]
        public void AnalyzeIsNaNOnValidTypes(string actual)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                var actual = {actual};
                Assert.That(actual, Is.NaN);
            ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase("3.3m")]
        [TestCase("4")]
        [TestCase("string.Empty")]
        public void AnalyzeIsNotNaNOnUnsupportedTypes(string actual)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                var actual = {actual};
                Assert.That(actual, Is.Not.NaN);
            ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [TestCase("3.3m")]
        [TestCase("4")]
        [TestCase("string.Empty")]
        public void AnalyzeIsNaNOnUnsupportedTypes(string actual)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                var actual = {actual};
                Assert.That(actual, Is.Positive.And.↓Not.NaN);
            ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeOnNullableDouble()
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                double? actual = Math.PI;
                Assert.That(actual, Is.Not.NaN);
            ");

            RoslynAssert.Valid(analyzer, testCode);
        }
    }
}
