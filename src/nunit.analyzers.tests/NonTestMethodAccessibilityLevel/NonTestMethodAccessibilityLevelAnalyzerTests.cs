using System.Collections.Generic;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.NonTestMethodAccessibilityLevel;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.NonTestMethodAccessibilityLevel
{
    public class NonTestMethodAccessibilityLevelAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new NonTestMethodAccessibilityLevelAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.NonTestMethodIsPublic);

        private static IEnumerable<string> TestMethodRelatedAttributes => new string[]
        {
            "Test",
            "OneTimeSetUp",
            "OneTimeTearDown",
            "SetUp",
            "TearDown"
        };

        private static IEnumerable<string> NonPrivateModifiers => new string[]
        {
            "public",
            "internal",
            "protected internal",

            "static public",
            "static internal",
            "static protected internal",
        };

        [Test]
        public void AnalyzeWhenNonTestClassHasPublicMethod()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void AssertMethod() {{ }}");
            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenNonTestMethodIsNotPublic()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [Test]
        public void TestMethod() {{ AssertMethod(); }}
        private void AssertMethod() {{ }}");
            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenNonTestMethodIsProtected()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [Test]
        public void TestMethod() {{ AssertMethod(); }}
        protected virtual void AssertMethod() {{ }}");
            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCaseSource(nameof(TestMethodRelatedAttributes))]
        public void AnalyzeWhenAuxiliaryTestMethodIsPublic(string attribute)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [Test]
        public void TestMethod() {{ }}
        [{attribute}]
        public void AuxiliaryMethod() {{ }}");
            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCaseSource(nameof(NonPrivateModifiers))]
        public void AnalyzeWhenNonTestMethodIsPublicOrInternal(string modifiers)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [TestCase(1)]
        public void TestMethod(int i) {{ AssertMethod(); }}
        {modifiers} void ↓AssertMethod() {{ }}");
            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [TestCaseSource(nameof(TestMethodRelatedAttributes))]
        public void AnalyzeWhenTestMethodIsPublicAndOverridden(string attribute)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
        public class BaseTestClass
        {{
            [{attribute}]
            public virtual void TestMethod() {{ }}
        }}

        public sealed class DerivedTestClass : BaseTestClass
        {{
            public override void TestMethod() {{ }}

            [Test]
            public void OtherTestMethod() {{ }}
        }}
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeConstructor()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
        public sealed class TestClass
        {
            public TestClass() {}

            [Test]
            public void TestMethod() { }
        }
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenMethodIsExplicitDispose()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
        public sealed class TestClass : IDisposable
        {
            [Test]
            public void TestMethod() { }

            void IDisposable.Dispose() { }
        }
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenMethodIsDispose()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
        public sealed class TestClass : IDisposable
        {
            [Test]
            public void TestMethod() { }

            public void Dispose() { }
        }
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenMethodIsDisposeOverride()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
        public class BaseClass : IDisposable
        {
            public virtual void Dispose() { }
        }

        public sealed class DerivedClass : BaseClass
        {
            [Test]
            public void TestMethod() { }

            public override void Dispose() { }
        }
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenMethodIsBaseClassOverride()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
        public sealed class MyTestClass
        {
            [Test]
            public void TestMethod() { }

            public override string ToString() => ""My Test Class"";
        }
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }
    }
}
