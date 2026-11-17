using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.ParallelizableUsage;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.ParallelizableUsage.ParallelizableUsageAnalyzer>;

namespace NUnit.Analyzers.Tests.ParallelizableUsage
{
    [TestFixture]
    public sealed class ParallelizableUsageAnalyzerTests
    {
        private static IEnumerable<ParallelScope> ParallelScopesExceptFixtures =>
            [ParallelScope.All, ParallelScope.Children, ParallelScope.Self];

        [Test]
        public void VerifySupportedDiagnostics()
        {
            var diagnostics = new ParallelizableUsageAnalyzer().SupportedDiagnostics;

            var expectedIdentifiers = new List<string>
            {
                AnalyzerIdentifiers.ParallelScopeSelfNoEffectOnAssemblyUsage,
                AnalyzerIdentifiers.ParallelScopeChildrenOnNonParameterizedTestMethodUsage,
                AnalyzerIdentifiers.ParallelScopeFixturesOnTestMethodUsage
            };

            Assert.That(diagnostics.Select(d => d.Id), Is.EquivalentTo(expectedIdentifiers));

            foreach (var diagnostic in diagnostics)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(diagnostic.Title.ToString(CultureInfo.InvariantCulture), Is.Not.Empty,
                        $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Title)}");
                    Assert.That(diagnostic.Category, Is.EqualTo(Categories.Structure),
                        $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Category)}");
                });
            }

            var diagnosticMessage = diagnostics.Select(_ => _.MessageFormat.ToString(CultureInfo.InvariantCulture)).ToImmutableArray();

            Assert.Multiple(() =>
            {
                Assert.That(diagnosticMessage, Contains.Item(ParallelizableUsageAnalyzerConstants.ParallelScopeSelfNoEffectOnAssemblyMessage),
                    $"{ParallelizableUsageAnalyzerConstants.ParallelScopeSelfNoEffectOnAssemblyMessage} is missing.");
                Assert.That(diagnosticMessage, Contains.Item(ParallelizableUsageAnalyzerConstants.ParallelScopeChildrenOnNonParameterizedTestMethodMessage),
                    $"{ParallelizableUsageAnalyzerConstants.ParallelScopeChildrenOnNonParameterizedTestMethodMessage} is missing.");
                Assert.That(diagnosticMessage, Contains.Item(ParallelizableUsageAnalyzerConstants.ParallelScopeFixturesOnTestMethodMessage),
                    $"{ParallelizableUsageAnalyzerConstants.ParallelScopeFixturesOnTestMethodMessage} is missing.");
            });
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
        public async Task AnalyzeWhenAssemblyAttributeIsNotParallelScopeSelf(ParallelScope parallelScope)
        {
            var enumValue = parallelScope.ToString();
            var testCode = $$"""
                using NUnit.Framework;
                [assembly: Parallelizable(ParallelScope.{{enumValue}})]
            """;
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenAssemblyAttributeIsExplicitlyParallelScopeSelf()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ParallelScopeSelfNoEffectOnAssemblyUsage).WithMessage(ParallelizableUsageAnalyzerConstants.ParallelScopeSelfNoEffectOnAssemblyMessage);

            var testCode = $$"""
                using NUnit.Framework;
                [assembly: {|#0:Parallelizable(ParallelScope.Self)|}]
            """;
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenAssemblyAttributeIsImplicitlyParallelScopeSelf()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ParallelScopeSelfNoEffectOnAssemblyUsage).WithMessage(ParallelizableUsageAnalyzerConstants.ParallelScopeSelfNoEffectOnAssemblyMessage);

            var testCode = $$"""
                using NUnit.Framework;
                [assembly: {|#0:Parallelizable()|}]
            """;
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Theory]
        public async Task AnalyzeWhenAttributeIsOnClass(ParallelScope parallelScope)
        {
            var enumValue = parallelScope.ToString();
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                [TestFixture]
                [Parallelizable(ParallelScope.{{enumValue}})]
                public sealed class AnalyzeWhenAttributeIsOnClass
                {
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenAttributeIsOnSimpleTestMethodParallelScopeSelf()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public sealed class AnalyzeWhenAttributeIsOnSimpleTestMethodParallelScopeSelf
                {
                    [Test]
                    [Parallelizable(ParallelScope.Self)]
                    public void Test()
                    {
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase(ParallelScope.All)]
        [TestCase(ParallelScope.Children)]
        public async Task AnalyzeWhenAttributeIsOnSimpleTestMethodContainsParallelScopeChildren(ParallelScope parallelScope)
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ParallelScopeChildrenOnNonParameterizedTestMethodUsage).WithMessage(ParallelizableUsageAnalyzerConstants.ParallelScopeChildrenOnNonParameterizedTestMethodMessage);

            var enumValue = parallelScope.ToString();
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                [TestFixture]
                public sealed class AnalyzeWhenAttributeIsOnSimpleTestMethodContainsParallelScopeChildren
                {
                    [Test]
                    [{|#0:Parallelizable(ParallelScope.{{enumValue}})|}]
                    public void Test()
                    {
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenAttributeIsOnSimpleTestMethodIsParallelScopeFixtures()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ParallelScopeFixturesOnTestMethodUsage).WithMessage(ParallelizableUsageAnalyzerConstants.ParallelScopeFixturesOnTestMethodMessage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public sealed class AnalyzeWhenAttributeIsOnSimpleTestMethodIsParallelScopeFixtures
                {
                    [Test]
                    [{|#0:Parallelizable(ParallelScope.Fixtures)|}]
                    public void Test()
                    {
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(ParallelScopesExceptFixtures))]
        public async Task AnalyzeWhenAttributeIsOnTestCaseTestMethodNotParallelScopeFixtures(ParallelScope parallelScope)
        {
            var enumValue = parallelScope.ToString();
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                [TestFixture]
                public sealed class AnalyzeWhenAttributeIsOnTestCaseTestMethodNotParallelScopeFixtures
                {
                    [TestCase(1)]
                    [Parallelizable(ParallelScope.{{enumValue}})]
                    public void Test(int data)
                    {
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenAttributeIsOnTestCaseTestMethodIsParallelScopeFixtures()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ParallelScopeFixturesOnTestMethodUsage).WithMessage(ParallelizableUsageAnalyzerConstants.ParallelScopeFixturesOnTestMethodMessage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public sealed class AnalyzeWhenAttributeIsOnTestCaseTestMethodIsParallelScopeFixtures
                {
                    [TestCase(1)]
                    [{|#0:Parallelizable(ParallelScope.Fixtures)|}]
                    public void Test(int data)
                    {
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(ParallelScopesExceptFixtures))]
        public async Task AnalyzeWhenAttributeIsOnParametricTestMethodNotParallelScopeFixtures(ParallelScope parallelScope)
        {
            var enumValue = parallelScope.ToString();
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                [TestFixture]
                public sealed class AnalyzeWhenAttributeIsOnParametricTestMethodNotParallelScopeFixtures
                {
                    [Test]
                    [Parallelizable(ParallelScope.{{enumValue}})]
                    public void Test([Values(1, 2, 3)] int data)
                    {
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenAttributeIsOnParametricTestMethodIsParallelScopeFixtures()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ParallelScopeFixturesOnTestMethodUsage).WithMessage(ParallelizableUsageAnalyzerConstants.ParallelScopeFixturesOnTestMethodMessage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public sealed class AnalyzeWhenAttributeIsOnParametricTestMethodIsParallelScopeFixtures
                {
                    [Test]
                    [{|#0:Parallelizable(ParallelScope.Fixtures)|}]
                    public void Test([Values(1, 2, 3)] int data)
                    {
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }
    }
}
