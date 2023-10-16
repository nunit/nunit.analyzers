using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.SameAsIncompatibleTypes;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.SameAsIncompatibleTypes
{
    public class SameAsIncompatibleTypesAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new SameAsIncompatibleTypesAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.SameAsIncompatibleTypes);

        [Test]
        public void AnalyzeWhenIncompatibleTypesProvided()
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
            Assert.That(actual, Is.SameAs(↓expected));
        }
    }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeClassicWhenIncompatibleTypesProvided()
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
            ClassicAssert.AreSame(↓expected, actual);
        }
    }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIncompatibleTypesProvidedWithImplicitConversion()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class Tests
    {
        [Test]
        public void AnalyzeWhenIncompatibleTypesProvided()
        {
            var actual = 1;
            var expected = 1.5;
            Assert.That(actual, Is.SameAs(↓expected));
        }
    }");

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
            Assert.That(actual, Is.Not.Null & Is.SameAs(↓expected));
        }
    }");

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
            Assert.That(actual, Is.Not.SameAs(↓expected));
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
            ClassicAssert.AreNotSame(↓expected, actual);
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
            Assert.That(() => new A(), Is.Not.SameAs(↓expected));
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
            Assert.That(actual, Is.Not.SameAs(↓expected));
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
            Assert.That(actual, Is.Not.SameAs(↓expected));
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
            Assert.That(actual, Is.Not.SameAs(↓expected));
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
                Assert.That(actual, Is.SameAs(↓expected));");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void NoDiagnosticWhenActualAndExpectedTypesAreSame()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = """";
                var expected = """";
                Assert.That(actual, Is.SameAs(expected));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticClassicWhenActualAndExpectedTypesAreSame()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = """";
                var expected = """";
                ClassicAssert.AreSame(expected, actual);");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenActualAndExpectedTypesAreSameWithNegatedAssert()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = """";
                var expected = """";
                Assert.That(actual, Is.Not.SameAs(expected));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticClassicWhenActualAndExpectedTypesAreSameWithNegatedAssert()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = """";
                var expected = """";
                ClassicAssert.AreNotSame(expected, actual);");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenActualAndExpectedTypesAreSameWithLambdaActualValue()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var expected = """";
                Assert.That(() => """", Is.SameAs(expected));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenActualAndExpectedTypesAreSameWithFuncActualValue()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Func<string> actual = () => """";
                var expected = """";
                Assert.That(actual, Is.SameAs(↓expected));");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void NoDiagnosticWhenActualAndExpectedTypesAreSameWithLocalFunctionActualValue()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                string actual() => """";
                var expected = """";
                Assert.That(actual, Is.SameAs(expected));");

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
        public void NoDiagnosticWhenExpectedTypeInheritsActual()
        {
            var actual = new A();
            var expected = new B();
            Assert.That(actual, Is.SameAs(expected));
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
            Assert.That(actual, Is.SameAs(expected));
        }
    }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenUsedWithNoneOperator()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "Assert.That(new string[0], Has.None.SameAs(string.Empty));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenActualIsDynamic()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                dynamic actual = 1;
                var expected = """";
                Assert.That(actual, Is.SameAs(expected));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticWhenExpectedIsDynamic()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = """";
                dynamic expected = 2;
                Assert.That(actual, Is.SameAs(expected));");

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
            Assert.That(actual, Is.EqualTo(expected));
        }
    }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenMoreThanOneConstraintExpressionIsUsed()
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
            var expected1 = new A();
            var expected2 = new B();
            Assert.That(actual, Is.Not.SameAs(expected1) & Is.Not.SameAs(↓expected2));
        }
    }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }
    }
}
