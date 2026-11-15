using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.TestFixtureShouldBeAbstract.TestFixtureShouldBeAbstractAnalyzer>;

namespace NUnit.Analyzers.Tests.TestFixtureShouldBeAbstract
{
    public class TestFixtureShouldBeAbstractAnalyzerTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.BaseTestFixtureIsNotAbstract);

        private static readonly IEnumerable<string> testMethodRelatedAttributes =
        [
            "OneTimeSetUp",
            "OneTimeTearDown",
            "SetUp",
            "TearDown",
            "Test",
        ];

        [TestCaseSource(nameof(testMethodRelatedAttributes))]
        public async Task AnalyzeWhenBaseFixtureIsAbstract(string attribute)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public abstract class BaseFixture
                {
                    [{{attribute}}]
                    public void BaseFixtureMethod() { }
                }

                public class DerivedFixture : BaseFixture
                {
                    [Test]
                    public void DerivedFixtureMethod() { }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCaseSource(nameof(testMethodRelatedAttributes))]
        public async Task AnalyzeWhenBaseFixtureIsNotAbstract(string attribute)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public class {|#0:BaseFixture|}
                {
                    [{{attribute}}]
                    public void BaseFixtureMethod() { }
                }

                public class DerivedFixture : BaseFixture
                {
                    [Test]
                    public void DerivedFixtureMethod() { }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("BaseFixture"));
        }

        [Test]
        public async Task AnalyzeWhenGenericFixtureIsAbstract()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public abstract class BaseFixture<T>
                {
                    [Test]
                    public void BaseFixtureMethod() { }
                }

                public class IntFixture : BaseFixture<int>
                {
                }

                public class DoubleFixture : BaseFixture<double>
                {
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenGenericFixtureIsNotAbstract()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class {|#0:BaseFixture|}<T>
                {
                    [Test]
                    public void BaseFixtureMethod() { }
                }

                public class IntFixture : BaseFixture<int>
                {
                }

                public class DoubleFixture : BaseFixture<double>
                {
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("BaseFixture"));
        }
    }
}
