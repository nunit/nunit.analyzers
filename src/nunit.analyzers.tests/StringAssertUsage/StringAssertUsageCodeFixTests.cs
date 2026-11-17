using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.StringAssertUsage;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.StringAssertUsage.StringAssertUsageAnalyzer, NUnit.Analyzers.StringAssertUsage.StringAssertUsageCodeFix>;

namespace NUnit.Analyzers.Tests.StringAssertUsage
{
    [TestFixture]
    internal sealed class StringAssertUsageCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.StringAssertUsage);

        private static IEnumerable<string> StringAsserts => StringAssertUsageAnalyzer.StringAssertToConstraint.Keys;

        [TestCaseSource(nameof(StringAsserts))]
        public async Task AnalyzeWhenNoArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        {|#0:StringAssert.{{method}}("expected", "actual")|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That("actual", {{GetAdjustedConstraint(method)}});

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(StringAsserts))]
        public async Task AnalyzeWhenOnlyMessageArgumentIsUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        {|#0:StringAssert.{{method}}("expected", "actual", "message")|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That("actual", {{GetAdjustedConstraint(method)}}, "message");

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(StringAsserts))]
        public async Task AnalyzeWhenFormatAndArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        {|#0:StringAssert.{{method}}("expected", "actual", "Because of {0}", "message")|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That("actual", {{GetAdjustedConstraint(method)}}, $"Because of {"message"}");

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(StringAsserts))]
        public async Task AnalyzeWhenFormatAndArgsArrayAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        object[] args = { "message" };
                        {|#0:StringAssert.{{method}}("expected", "actual", "Because of {0}", args)|};

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        object[] args = { "message" };
                        Assert.That("actual", {{GetAdjustedConstraint(method)}}, () => string.Format("Because of {0}", args));

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(StringAsserts))]
        public async Task AnalyzeWhenFormatVariableAndArgumentsAreUsed(string method)
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        {|#0:StringAssert.{{method}}("expected", "actual", GetLocalizedFormatSpecification(), "message")|};
                        static string GetLocalizedFormatSpecification() => "Because of {0}";

            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That("actual", {{GetAdjustedConstraint(method)}}, () => string.Format(GetLocalizedFormatSpecification(), "message"));
                        static string GetLocalizedFormatSpecification() => "Because of {0}";

            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [TestCaseSource(nameof(StringAsserts))]
        public async Task AnalyzeWhenFormatAndArgumentsAreUsedOutOfOrder(string method)
        {
            var firstParameterName = StringAssertUsageCodeFix.StringAssertToExpectedParameterName[method];
            var code = TestUtility.WrapInTestMethod($$"""
                        {|#0:StringAssert.{{method}}(args: new[] { "first", "second" }, message: "{0}, {1}", actual: "actual", {{firstParameterName}}: "expected")|};
            """);
            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That("actual", {{GetAdjustedConstraint(method)}}, $"{"first"}, {"second"}");
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        private static string GetAdjustedConstraint(string method) =>
            StringAssertUsageAnalyzer.StringAssertToConstraint[method]
                .Replace("expected", "\"expected\"");
    }
}
