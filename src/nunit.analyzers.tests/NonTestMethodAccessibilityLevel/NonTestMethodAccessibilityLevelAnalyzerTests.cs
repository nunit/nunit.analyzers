using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.NonTestMethodAccessibilityLevel.NonTestMethodAccessibilityLevelAnalyzer>;

namespace NUnit.Analyzers.Tests.NonTestMethodAccessibilityLevel
{
    public class NonTestMethodAccessibilityLevelAnalyzerTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.NonTestMethodIsPublic);

        private static IEnumerable<string> TestMethodRelatedAttributes =>
        [
            "Test",
            "OneTimeSetUp",
            "OneTimeTearDown",
            "SetUp",
            "TearDown"
        ];

        private static IEnumerable<string> NonPrivateModifiers =>
        [
            "public",
            "internal",
            "protected internal",

            "static public",
            "static internal",
            "static protected internal",
        ];

        [Test]
        public async Task AnalyzeWhenNonTestClassHasPublicMethod()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void AssertMethod() { }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenNonTestMethodIsNotPublic()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void TestMethod() { AssertMethod(); }
                    private void AssertMethod() { }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenNonTestMethodIsProtected()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void TestMethod() { AssertMethod(); }
                    protected virtual void AssertMethod() { }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCaseSource(nameof(TestMethodRelatedAttributes))]
        public async Task AnalyzeWhenAuxiliaryTestMethodIsPublic(string attribute)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void TestMethod() { }
                    [{{attribute}}]
                    public void AuxiliaryMethod() { }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCaseSource(nameof(NonPrivateModifiers))]
        public async Task AnalyzeWhenNonTestMethodIsPublicOrInternal(string modifiers)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase(1)]
                    public void TestMethod(int i) { AssertMethod(); }
                    {{modifiers}} void {|#0:AssertMethod|}() { }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(TestMethodRelatedAttributes))]
        public async Task AnalyzeWhenTestMethodIsPublicAndOverridden(string attribute)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public class BaseTestClass
                {
                    [{{attribute}}]
                    public virtual void TestMethod() { }
                }

                public sealed class DerivedTestClass : BaseTestClass
                {
                    public override void TestMethod() { }

                    [Test]
                    public void OtherTestMethod() { }
                }

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeConstructor()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class TestClass
                {
                    public TestClass() {}

                    [Test]
                    public void TestMethod() { }
                }

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenMethodIsExplicitDispose()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class TestClass : IDisposable
                {
                    [Test]
                    public void TestMethod() { }

                    void IDisposable.Dispose() { }
                }

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenMethodIsDispose()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class TestClass : IDisposable
                {
                    [Test]
                    public void TestMethod() { }

                    public void Dispose() { }
                }

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenMethodIsDisposeOverride()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
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

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenMethodIsExplicitDisposeAsync()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class TestClass : IAsyncDisposable
                {
                    [Test]
                    public void TestMethod() { }

                    ValueTask IAsyncDisposable.DisposeAsync() => default(ValueTask);
                }

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenMethodIsDisposeAsync()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class TestClass : IAsyncDisposable
                {
                    [Test]
                    public void TestMethod() { }

                    public ValueTask DisposeAsync() => default(ValueTask);
                }

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenMethodIsDisposeAsyncOverride()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class BaseClass : IAsyncDisposable
                {
                    public virtual ValueTask DisposeAsync() => default(ValueTask);
                }

                public sealed class DerivedClass : BaseClass
                {
                    [Test]
                    public void TestMethod() { }

                    public override ValueTask DisposeAsync() => default(ValueTask);
                }

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenMethodIsBaseClassOverride()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class MyTestClass
                {
                    [Test]
                    public void TestMethod() { }

                    public override string ToString() => "My Test Class";
                }

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenMethodIsCompilerGenerated()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed record MyTestClass
                {
                    [Test]
                    public void TestMethod() { }
                }

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
