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

        private static readonly IEnumerable<string[]> testMethodRelatedAttributes =
        [
            ["", "OneTimeSetUp"],
            ["internal ", "OneTimeTearDown"],
            ["public ", "SetUp"],
            ["partial ", "TearDown"],
            ["internal partial ", "Test"],
        ];

        [TestCaseSource(nameof(testMethodRelatedAttributes))]
        public void FixWhenBaseFixtureIsNotAbstract(string modifiers, string attribute)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    // Base Fixture
    {modifiers}class ↓BaseFixture
    {{
        [{attribute}]
        public void BaseFixtureMethod() {{ }}
    }}

    {modifiers}class DerivedFixture : BaseFixture
    {{
        [Test]
        public void DerivedFixtureMethod() {{ }}
    }}");

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    // Base Fixture
    {modifiers}abstract class BaseFixture
    {{
        [{attribute}]
        public void BaseFixtureMethod() {{ }}
    }}

    {modifiers}class DerivedFixture : BaseFixture
    {{
        [Test]
        public void DerivedFixtureMethod() {{ }}
    }}");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, testCode, fixedCode);
        }
    }
}
