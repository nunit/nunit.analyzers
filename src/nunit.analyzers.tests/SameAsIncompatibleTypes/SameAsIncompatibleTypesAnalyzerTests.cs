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
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    class A {{ }}
    class B {{ }}

    public class Tests
    {{
        [Test]
        public void AnalyzeWhenIncompatibleTypesProvided()
        {{
            var actual = new A();
            var expected = new B();
            Assert.That(actual, Is.SameAs(↓expected));
        }}
    }}");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIncompatibleTypesProvided_WithImplicitConversion()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public class Tests
    {{
        [Test]
        public void AnalyzeWhenIncompatibleTypesProvided()
        {{
            var actual = 1;
            var expected = 1.5;
            Assert.That(actual, Is.SameAs(↓expected));
        }}
    }}");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIncompatibleTypesProvided_WithCombinedConstraints()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    class A {{ }}
    class B {{ }}

    public class Tests
    {{
        [Test]
        public void AnalyzeWhenIncompatibleTypesProvided()
        {{
            var actual = new A();
            var expected = new B();
            Assert.That(actual, Is.Not.Null & Is.SameAs(↓expected));
        }}
    }}");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void NoDiagnoticWhenActualAndExpectedTypesAreSame()
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var actual = """";
                var expected = """";
                Assert.That(actual, Is.SameAs(expected));");

            AnalyzerAssert.NoAnalyzerDiagnostics(analyzer, testCode);
        }

        [Test]
        public void NoDiagnoticWhenExpectedTypeInheritsActual()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    class A {{ }}
    class B : A {{ }}

    public class Tests
    {{
        [Test]
        public void AnalyzeWhenIncompatibleTypesProvided()
        {{
            var actual = new A();
            var expected = new B();
            Assert.That(actual, Is.SameAs(expected));
        }}
    }}");

            AnalyzerAssert.NoAnalyzerDiagnostics(analyzer, testCode);
        }

        [Test]
        public void NoDiagnoticWhenActualTypeInheritsExpected()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    class A : B {{ }}
    class B {{ }}

    public class Tests
    {{
        [Test]
        public void AnalyzeWhenIncompatibleTypesProvided()
        {{
            var actual = new A();
            var expected = new B();
            Assert.That(actual, Is.SameAs(expected));
        }}
    }}");

            AnalyzerAssert.NoAnalyzerDiagnostics(analyzer, testCode);
        }

        [Test]
        public void NoDiagnoticWhenActualIsDynamic()
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                dynamic actual = 1;
                var expected = """";
                Assert.That(actual, Is.SameAs(expected));");

            AnalyzerAssert.NoAnalyzerDiagnostics(analyzer, testCode);
        }

        [Test]
        public void NoDiagnoticWhenExpectedIsDynamic()
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var actual = """";
                dynamic expected = 2;
                Assert.That(actual, Is.SameAs(expected));");

            AnalyzerAssert.NoAnalyzerDiagnostics(analyzer, testCode);
        }

        [Test]
        public void NoDiagnoticForOtherConstraints()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    class A {{ }}
    class B {{ }}

    public class Tests
    {{
        [Test]
        public void NoDiagnoticForOtherConstraints()
        {{
            var actual = new A();
            var expected = new B();
            Assert.That(actual, Is.EqualTo(expected));
        }}
    }}");

            AnalyzerAssert.NoAnalyzerDiagnostics(analyzer, testCode);
        }
    }
}
