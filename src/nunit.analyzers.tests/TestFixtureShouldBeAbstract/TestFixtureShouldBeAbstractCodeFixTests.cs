using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.TestFixtureShouldBeAbstract.TestFixtureShouldBeAbstractAnalyzer, NUnit.Analyzers.TestFixtureShouldBeAbstract.TestFixtureShouldBeAbstractCodeFix>;

namespace NUnit.Analyzers.Tests.TestFixtureShouldBeAbstract
{
    public class TestFixtureShouldBeAbstractCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.BaseTestFixtureIsNotAbstract);

        private static readonly IEnumerable<string[]> testMethodRelatedAttributes =
        [
            ["", "OneTimeSetUp"],
            ["internal ", "OneTimeTearDown"],
            ["public ", "SetUp"],
            ["partial ", "TearDown"],
            ["internal partial ", "Test"],
        ];

        [TestCaseSource(nameof(testMethodRelatedAttributes))]
        public async Task FixWhenBaseFixtureIsNotAbstract(string modifiers, string attribute)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                // Base Fixture
                {{modifiers}}class {|#0:BaseFixture|}
                {
                    [{{attribute}}]
                    public void BaseFixtureMethod() { }
                }

                {{modifiers}}class DerivedFixture : BaseFixture
                {
                    [Test]
                    public void DerivedFixtureMethod() { }
                }
            """);

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                // Base Fixture
                {{modifiers}}abstract class BaseFixture
                {
                    [{{attribute}}]
                    public void BaseFixtureMethod() { }
                }

                {{modifiers}}class DerivedFixture : BaseFixture
                {
                    [Test]
                    public void DerivedFixtureMethod() { }
                }
            """);

            await Verify.VerifyCodeFixAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("BaseFixture"), fixedCode, fixTitle: null, test =>
            {
                // The diagnostic is reported on the base class, not on the derived class that causes it.
                test.CodeFixTestBehaviors |= CodeFixTestBehaviors.SkipLocalDiagnosticCheck;

                int partialIndex = modifiers.IndexOf("partial", StringComparison.Ordinal);
                if (partialIndex >= 0)
                {
                    // BUG: The code fix inserts 'abstract' after 'partial', which does not compile.
                    test.FixedState.ExpectedDiagnostics.Add(
                        DiagnosticResult.CompilerError("CS0267").WithSpan(17, 5 + partialIndex, 17, 12 + partialIndex));
                }
            });
        }
    }
}
