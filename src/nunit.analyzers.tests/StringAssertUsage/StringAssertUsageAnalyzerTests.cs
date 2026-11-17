using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.StringAssertUsage;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.StringAssertUsage.StringAssertUsageAnalyzer>;

namespace NUnit.Analyzers.Tests.StringAssertUsage
{
    [TestFixture]
    internal class StringAssertUsageAnalyzerTests
    {
        private readonly DiagnosticResult diagnostic = Verify.Diagnostic(AnalyzerIdentifiers.StringAssertUsage);

        private static IEnumerable<string> StringAsserts => StringAssertUsageAnalyzer.StringAssertToConstraint.Keys;

        [TestCaseSource(nameof(StringAsserts))]
        public async Task AnalyzeWhenNoArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        {|#0:StringAssert.{{method}}("expected", "actual")|};

            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(StringAsserts))]
        public async Task AnalyzeWhenOnlyMessageArgumentIsUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        {|#0:StringAssert.{{method}}("expected", "actual", "message")|};

            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(StringAsserts))]
        public async Task AnalyzeWhenFormatAndArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        {|#0:StringAssert.{{method}}("expected", "actual", "Because of {0}", "message")|};

            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }
    }
}
