using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.ParallelizableUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ParallelizableUsage
{
    [TestFixture]
    public sealed class ParallelizableUsageAnalyzerTests
    {
        private readonly DiagnosticAnalyzer analyzer = new ParallelizableUsageAnalyzer();

        private static IEnumerable<ParallelScope> ParallelScopesExceptFixtures =>
            new ParallelScope[] { ParallelScope.All, ParallelScope.Children, ParallelScope.Self };

        [Test]
        public void VerifySupportedDiagnostics()
        {
            var diagnostics = this.analyzer.SupportedDiagnostics;

            var expectedIdentifiers = new List<string>
            {
                AnalyzerIdentifiers.ParallelScopeSelfNoEffectOnAssemblyUsage,
                AnalyzerIdentifiers.ParallelScopeChildrenOnNonParameterizedTestMethodUsage,
                AnalyzerIdentifiers.ParallelScopeFixturesOnTestMethodUsage
            };
            CollectionAssert.AreEquivalent(expectedIdentifiers, diagnostics.Select(d => d.Id));

            foreach (var diagnostic in diagnostics)
            {
                Assert.That(diagnostic.Title.ToString(), Is.Not.Empty,
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Title)}");
                Assert.That(diagnostic.Category, Is.EqualTo(Categories.Structure),
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Category)}");
            }

            var diagnosticMessage = diagnostics.Select(_ => _.MessageFormat.ToString()).ToImmutableArray();

            Assert.That(diagnosticMessage, Contains.Item(ParallelizableUsageAnalyzerConstants.ParallelScopeSelfNoEffectOnAssemblyMessage),
                $"{ParallelizableUsageAnalyzerConstants.ParallelScopeSelfNoEffectOnAssemblyMessage} is missing.");
            Assert.That(diagnosticMessage, Contains.Item(ParallelizableUsageAnalyzerConstants.ParallelScopeChildrenOnNonParameterizedTestMethodMessage),
                $"{ParallelizableUsageAnalyzerConstants.ParallelScopeChildrenOnNonParameterizedTestMethodMessage} is missing.");
            Assert.That(diagnosticMessage, Contains.Item(ParallelizableUsageAnalyzerConstants.ParallelScopeFixturesOnTestMethodMessage),
                $"{ParallelizableUsageAnalyzerConstants.ParallelScopeFixturesOnTestMethodMessage} is missing.");
        }

        [TestCase(ParallelizableUsageAnalyzerConstants.ParallelScope.Self, ParallelScope.Self)]
        [TestCase(ParallelizableUsageAnalyzerConstants.ParallelScope.Children, ParallelScope.Children)]
        [TestCase(ParallelizableUsageAnalyzerConstants.ParallelScope.Fixtures, ParallelScope.Fixtures)]
        public void ConstantMatchesValueInNUnit(int enumValue, ParallelScope parallelScope)
        {
            Assert.That(enumValue, Is.EqualTo((int)parallelScope));
        }

        [TestCase(ParallelScope.All)]
        [TestCase(ParallelScope.Children)]
        [TestCase(ParallelScope.Fixtures)]
        public void AnalyzeWhenAssemblyAttributeIsNotParallelScopeSelf(ParallelScope parallelScope)
        {
            var enumValue = parallelScope.ToString();
            var testCode = $@"
using NUnit.Framework;
[assembly: Parallelizable(ParallelScope.{enumValue})]";
            AnalyzerAssert.Valid<ParallelizableUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenAssemblyAttributeIsExplicitlyParallelScopeSelf()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.ParallelScopeSelfNoEffectOnAssemblyUsage,
                ParallelizableUsageAnalyzerConstants.ParallelScopeSelfNoEffectOnAssemblyMessage);

            var testCode = $@"
using NUnit.Framework;
[assembly: ↓Parallelizable(ParallelScope.Self)]";
            AnalyzerAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenAssemblyAttributeIsImplicitlyParallelScopeSelf()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.ParallelScopeSelfNoEffectOnAssemblyUsage,
                ParallelizableUsageAnalyzerConstants.ParallelScopeSelfNoEffectOnAssemblyMessage);

            var testCode = $@"
using NUnit.Framework;
[assembly: ↓Parallelizable()]";
            AnalyzerAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Theory]
        public void AnalyzeWhenAttributeIsOnClass(ParallelScope parallelScope)
        {
            var enumValue = parallelScope.ToString();
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    [TestFixture]
    [Parallelizable(ParallelScope.{enumValue})]
    public sealed class AnalyzeWhenAttributeIsOnClass
    {{
    }}");
            AnalyzerAssert.Valid<ParallelizableUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenAttributeIsOnSimpleTestMethodParallelScopeSelf()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    [TestFixture]
    public sealed class AnalyzeWhenAttributeIsOnSimpleTestMethodParallelScopeSelf
    {
        [Test]
        [Parallelizable(ParallelScope.Self)]
        public void Test()
        {
        }
    }");
            AnalyzerAssert.Valid<ParallelizableUsageAnalyzer>(testCode);
        }

        [TestCase(ParallelScope.All)]
        [TestCase(ParallelScope.Children)]
        public void AnalyzeWhenAttributeIsOnSimpleTestMethodContainsParallelScopeChildren(ParallelScope parallelScope)
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.ParallelScopeChildrenOnNonParameterizedTestMethodUsage,
                ParallelizableUsageAnalyzerConstants.ParallelScopeChildrenOnNonParameterizedTestMethodMessage);

            var enumValue = parallelScope.ToString();
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    [TestFixture]
    public sealed class AnalyzeWhenAttributeIsOnSimpleTestMethodContainsParallelScopeChildren
    {{
        [Test]
        [↓Parallelizable(ParallelScope.{enumValue})]
        public void Test()
        {{
        }}
    }}");
            AnalyzerAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenAttributeIsOnSimpleTestMethodIsParallelScopeFixtures()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.ParallelScopeFixturesOnTestMethodUsage,
                ParallelizableUsageAnalyzerConstants.ParallelScopeFixturesOnTestMethodMessage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    [TestFixture]
    public sealed class AnalyzeWhenAttributeIsOnSimpleTestMethodIsParallelScopeFixtures
    {
        [Test]
        [↓Parallelizable(ParallelScope.Fixtures)]
        public void Test()
        {
        }
    }");
            AnalyzerAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [TestCaseSource(nameof(ParallelScopesExceptFixtures))]
        public void AnalyzeWhenAttributeIsOnTestCaseTestMethodNotParallelScopeFixtures(ParallelScope parallelScope)
        {
            var enumValue = parallelScope.ToString();
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    [TestFixture]
    public sealed class AnalyzeWhenAttributeIsOnTestCaseTestMethodNotParallelScopeFixtures
    {{
        [TestCase(1)]
        [Parallelizable(ParallelScope.{enumValue})]
        public void Test(int data)
        {{
        }}
    }}");
            AnalyzerAssert.Valid<ParallelizableUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenAttributeIsOnTestCaseTestMethodIsParallelScopeFixtures()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.ParallelScopeFixturesOnTestMethodUsage,
                ParallelizableUsageAnalyzerConstants.ParallelScopeFixturesOnTestMethodMessage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    [TestFixture]
    public sealed class AnalyzeWhenAttributeIsOnTestCaseTestMethodIsParallelScopeFixtures
    {
        [TestCase(1)]
        [↓Parallelizable(ParallelScope.Fixtures)]
        public void Test(int data)
        {
        }
    }");
            AnalyzerAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [TestCaseSource(nameof(ParallelScopesExceptFixtures))]
        public void AnalyzeWhenAttributeIsOnParametricTestMethodNotParallelScopeFixtures(ParallelScope parallelScope)
        {
            var enumValue = parallelScope.ToString();
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    [TestFixture]
    public sealed class AnalyzeWhenAttributeIsOnParametricTestMethodNotParallelScopeFixtures
    {{
        [Test]
        [Parallelizable(ParallelScope.{enumValue})]
        public void Test([Values(1, 2, 3)] int data)
        {{
        }}
    }}");
            AnalyzerAssert.Valid<ParallelizableUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenAttributeIsOnParametricTestMethodIsParallelScopeFixtures()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.ParallelScopeFixturesOnTestMethodUsage,
                ParallelizableUsageAnalyzerConstants.ParallelScopeFixturesOnTestMethodMessage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    [TestFixture]
    public sealed class AnalyzeWhenAttributeIsOnParametricTestMethodIsParallelScopeFixtures
    {
        [Test]
        [↓Parallelizable(ParallelScope.Fixtures)]
        public void Test([Values(1, 2, 3)] int data)
        {
        }
    }");
            AnalyzerAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }
    }
}
