using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.UpdateStringFormatToInterpolatableString.UpdateStringFormatToInterpolatableStringAnalyzer>;

namespace NUnit.Analyzers.Tests.UpdateStringFormatToInterpolatableString
{
    [TestFixture]
    public sealed class UpdateStringFormatToInterpolatableStringAnalyzerTests
    {
        private readonly DiagnosticResult diagnostic = Verify.Diagnostic(AnalyzerIdentifiers.UpdateStringFormatToInterpolatableString);

        private static readonly string[] AssertAndAssume =
        [
            NUnitFrameworkConstants.NameOfAssert,
            NUnitFrameworkConstants.NameOfAssume,
        ];

        private static IEnumerable<string> NoArgumentsAsserts { get; } =
        [
            NUnitFrameworkConstants.NameOfAssertPass,
            NUnitFrameworkConstants.NameOfAssertFail,
            NUnitFrameworkConstants.NameOfAssertIgnore,
            NUnitFrameworkConstants.NameOfAssertInconclusive,
        ];

        private static IEnumerable<string> OneArgumentsAsserts { get; } = NoArgumentsAsserts.Concat([NUnitFrameworkConstants.NameOfAssertWarn]);

        [TestCaseSource(nameof(NoArgumentsAsserts))]
        public async Task AnalyzeWhenNoArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        Assert.{{method}}();

            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCaseSource(nameof(OneArgumentsAsserts))]
        public async Task AnalyzeWhenOnlyMessageArgumentIsUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        Assert.{{method}}("Message");

            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

#if !NUNIT4
        [TestCaseSource(nameof(OneArgumentsAsserts))]
        public async Task AnalyzeWhenFormatAndArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        {|#0:Assert.{{method}}("Method: {0}", "{{method}}")|};

            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(OneArgumentsAsserts))]
        public async Task AnalyzeWhenFormatAndArrayArgumentsAreUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var values = new object[] { "{{method}}" };
                        {|#0:Assert.{{method}}("Method: {0}", values)|};

            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }
#endif

        [TestCaseSource(nameof(OneArgumentsAsserts))]
        public async Task AnalyzeWhenFormattableStringIsUsed(string method)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        Assert.{{method}}($"Method: {"method"}");

            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public async Task AnalyzeAssertOrAssumeBoolWhenNoArgumentsAreUsed(string assertOrAssume)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        {{assertOrAssume}}.That(true);

            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public async Task AnalyzeAssertOrAssumeBoolWhenOnlyMessageArgumentIsUsed(string assertOrAssume)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        {{assertOrAssume}}.That(false, "Message");

            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public async Task AnalyzeAssertOrAssumeBoolWhenFormatAndArgumentsAreUsed(string assertOrAssume)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        {|#0:{{assertOrAssume}}.That(false, "Method: {0}", false.ToString())|};

            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public async Task AnalyzeAssertOrAssumeBoolWhenFormattableStringIsUsed(string assertOrAssume)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        {{assertOrAssume}}.That(false, $"Method: {false}");

            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public async Task AnalyzeAssertOrAssumeThatWhenNoArgumentsAreUsed(string assertOrAssume)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        double pi = 3.1415;
                        {{assertOrAssume}}.That(pi, Is.EqualTo(3.1415).Within(0.0001));

            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public async Task AnalyzeAssertOrAssumeThatWhenOnlyMessageArgumentIsUsed(string assertOrAssume)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        double pi = 3.1415;
                        {{assertOrAssume}}.That(pi, Is.EqualTo(3.1415).Within(0.0001), "Message");

            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCaseSource(nameof(AssertAndAssume))]
        public async Task AnalyzeAssertOrAssumeThatWhenFormatAndStringArgumentsAreUsed(string assertOrAssume)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase("NUnit 4.0", "NUnit 3.14")]
                    public void {{assertOrAssume}}Something(string actual, string expected)
                    {
                        {|#0:{{assertOrAssume}}.That(actual, Is.EqualTo(expected).IgnoreCase, "Expected '{0}', but got: {1}", expected, actual)|};
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }

#if !NUNIT4
        [TestCaseSource(nameof(AssertAndAssume))]
        public async Task AnalyzeAssertOrAssumeThatWhenFormatAndArgumentsAreUsed(string assertOrAssume)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        double pi = 3.1415;
                        {|#0:{{assertOrAssume}}.That(pi, Is.EqualTo(3.1415).Within(0.0001), "Method: {0}", pi)|};

            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0));
        }
#endif

        [TestCaseSource(nameof(AssertAndAssume))]
        public async Task AnalyzeAssertOrAssumeThatWhenFormatStringIsUsed(string assertOrAssume)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        double pi = 3.1415;
                        {{assertOrAssume}}.That(pi, Is.EqualTo(3.1415).Within(0.0001), $"Method: {pi}");

            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeAssertDoesNotThrowWhenFormatAndArgumentsAreUsed()
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        Assert.DoesNotThrow(() => SomeOperation(), "Expected no exception to be thrown by {0}", nameof(SomeOperation));

                        static void SomeOperation()
                        {
                        }

            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeAssertThrowsWhenFormatAndArgumentsAreUsed()
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        Assert.Throws<InvalidOperationException>(() => SomeOperation(),
                            "Expected an InvalidOperationException exception to be thrown by {0}", nameof(SomeOperation));

                        static void SomeOperation()
                        {
                        }

            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

#if NUNIT4 && NET6_0_OR_GREATER
        [Test]
        public async Task AnalyzeAssertRecognizesPassingThroughCallerArgumentExpression()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public static void AssertThat(
                        Func<object> valueProvider,
                        IConstraint expression,
                        NUnitString message,
                        [CallerArgumentExpression("valueProvider")] string actualExpression = "",
                        [CallerArgumentExpression("expression")] string constraintExpression = ""
                        )
                    {
                        Assert.That(
                            valueProvider,
                            new DelayedConstraint(expression, 3000, 300),
                            message, actualExpression, constraintExpression);
                    }

            """, "using System.Runtime.CompilerServices;using NUnit.Framework.Constraints;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }
#endif
    }
}
