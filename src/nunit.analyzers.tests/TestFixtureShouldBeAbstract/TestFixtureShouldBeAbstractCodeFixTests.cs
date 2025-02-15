using System.Collections.Generic;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TestFixtureShouldBeAbstract;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.TestFixtureShouldBeAbstract
{
    public class TestFixtureShouldBeAbstractCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new TestFixtureShouldBeAbstractAnalyzer();
        private static readonly CodeFixProvider fix = new TestFixtureShouldBeAbstractCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.BaseTestFixtureIsNotAbstract);

        private static readonly IEnumerable<string> testMethodRelatedAttributes =
        [
            "OneTimeSetUp",
            "OneTimeTearDown",
            "SetUp",
            "TearDown",
            "Test",
        ];

        [TestCaseSource(nameof(testMethodRelatedAttributes))]
        public void FixWhenBaseFixtureIsNotAbstract(string attribute)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    // Base Fixture
    public class ↓BaseFixture
    {{
        [{attribute}]
        public void BaseFixtureMethod() {{ }}
    }}

    public class DerivedFixture : BaseFixture
    {{
        [Test]
        public void DerivedFixtureMethod() {{ }}
    }}");

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    // Base Fixture
    public abstract class BaseFixture
    {{
        [{attribute}]
        public void BaseFixtureMethod() {{ }}
    }}

    public class DerivedFixture : BaseFixture
    {{
        [Test]
        public void DerivedFixtureMethod() {{ }}
    }}");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, testCode, fixedCode);
        }
    }
}
