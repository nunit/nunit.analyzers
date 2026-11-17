using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.SameAsIncompatibleTypes.SameAsIncompatibleTypesAnalyzer>;

namespace NUnit.Analyzers.Tests.SameAsIncompatibleTypes
{
    public class SameAsIncompatibleTypesAnalyzerTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.SameAsIncompatibleTypes);

        [Test]
        public async Task AnalyzeWhenIncompatibleTypesProvided()
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
                        Assert.That(actual, Is.SameAs({|#0:expected|}));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeClassicWhenIncompatibleTypesProvided()
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
                        ClassicAssert.AreSame({|#0:expected|}, actual);
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

#if !NUNIT5 // NUnit 5 uses a class constraint hence these rules are not needed as the user code will not even compile.
        [Test]
        public async Task AnalyzeWhenIncompatibleTypesProvidedWithImplicitConversion()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class Tests
                {
                    [Test]
                    public void AnalyzeWhenIncompatibleTypesProvided()
                    {
                        var actual = 1;
                        var expected = 1.5;
                        Assert.That(actual, Is.SameAs({|#0:expected|}));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }
#endif

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
                        Assert.That(actual, Is.Not.Null & Is.SameAs({|#0:expected|}));
                    }
                }
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
                        Assert.That(actual, Is.Not.SameAs({|#0:expected|}));
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
                        ClassicAssert.AreNotSame({|#0:expected|}, actual);
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
                        Assert.That(() => new A(), Is.Not.SameAs({|#0:expected|}));
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
                        Assert.That(actual, Is.Not.SameAs({|#0:expected|}));
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
                        Assert.That(actual, Is.Not.SameAs({|#0:expected|}));
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
                        Assert.That(actual, Is.Not.SameAs({|#0:expected|}));
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
                        Assert.That(actual, Is.SameAs({|#0:expected|}));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task NoDiagnosticWhenActualAndExpectedTypesAreSame()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "";
                        var expected = "";
                        Assert.That(actual, Is.SameAs(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticClassicWhenActualAndExpectedTypesAreSame()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "";
                        var expected = "";
                        ClassicAssert.AreSame(expected, actual);
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenActualAndExpectedTypesAreSameWithNegatedAssert()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "";
                        var expected = "";
                        Assert.That(actual, Is.Not.SameAs(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticClassicWhenActualAndExpectedTypesAreSameWithNegatedAssert()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "";
                        var expected = "";
                        ClassicAssert.AreNotSame(expected, actual);
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenActualAndExpectedTypesAreSameWithLambdaActualValue()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var expected = "";
                        Assert.That(() => "", Is.SameAs(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenActualAndExpectedTypesAreSameWithFuncActualValue()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        Func<string> actual = () => "";
                        var expected = "";
                        Assert.That(actual, Is.SameAs(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenActualAndExpectedTypesAreSameWithLocalFunctionActualValue()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        string actual() => "";
                        var expected = "";
                        Assert.That(actual, Is.SameAs(expected));
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
                    public void NoDiagnosticWhenExpectedTypeInheritsActual()
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
                        Assert.That(actual, Is.SameAs(expected));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenUsedWithNoneOperator()
        {
            var testCode = TestUtility.WrapInTestMethod(
                "Assert.That(new string[0], Has.None.SameAs(string.Empty));");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenActualIsDynamic()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        dynamic actual = 1;
                        var expected = "";
                        Assert.That(actual, Is.SameAs(expected));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticWhenExpectedIsDynamic()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "";
                        dynamic expected = 2;
                        Assert.That(actual, Is.SameAs(expected));
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
                        Assert.That(actual, Is.EqualTo(expected));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenMoreThanOneConstraintExpressionIsUsed()
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
                        var expected1 = new A();
                        var expected2 = new B();
                        Assert.That(actual, Is.Not.SameAs(expected1) & Is.Not.SameAs({|#0:expected2|}));
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }
    }
}
