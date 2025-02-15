using System.Collections.Generic;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TestFixtureShouldBeAbstract;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.TestFixtureShouldBeAbstract
{
    public class TestFixtureShouldBeAbstractAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new TestFixtureShouldBeAbstractAnalyzer();
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
        public void AnalyzeWhenBaseFixtureIsAbstract(string attribute)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
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

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCaseSource(nameof(testMethodRelatedAttributes))]
        public void AnalyzeWhenBaseFixtureIsNotAbstract(string attribute)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
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

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenGenericFixtureIsAbstract()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
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
    }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenGenericFixtureIsNotAbstract()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class ↓BaseFixture<T>
    {
        [Test]
        public void BaseFixtureMethod() { }
    }

    public class IntFixture : BaseFixture<int>
    {
    }
    
    public class DoubleFixture : BaseFixture<double>
    {
    }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }
    }
}
