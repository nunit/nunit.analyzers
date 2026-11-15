using System.Globalization;
using System.Threading.Tasks;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.ConstraintUsage;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.ConstraintUsage.ComparisonConstraintUsageAnalyzer>;

namespace NUnit.Analyzers.Tests.ConstraintsUsage
{
    public sealed class ComparisonConstraintUsageAnalyzerTests
    {
        [TestCase(">=", "Is.GreaterThanOrEqualTo")]
        [TestCase(">", "Is.GreaterThan")]
        [TestCase("<=", "Is.LessThanOrEqualTo")]
        [TestCase("<", "Is.LessThan")]
        public async Task AnalyzeWhenComparisonOperatorUsed(string operatorToken, string message)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        int actual = 5;
                        Assert.That({|#0:actual {{operatorToken}} 9|});
            """);

            var diagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ComparisonConstraintUsage).WithMessage(string.Format(CultureInfo.InvariantCulture, ComparisonConstraintUsageConstants.Message, message));
            await Verify.VerifyAnalyzerAsync(testCode, diagnostic.WithLocation(0));
        }

        [TestCase(">=", "Is.GreaterThanOrEqualTo")]
        [TestCase(">", "Is.GreaterThan")]
        [TestCase("<=", "Is.LessThanOrEqualTo")]
        [TestCase("<", "Is.LessThan")]
        public async Task AnalyzeWhenComparisonOperatorUsedWithIsTrue(string operatorToken, string message)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        int actual = 5;
                        Assert.That({|#0:actual {{operatorToken}} 9|}, Is.True);
            """);

            var diagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ComparisonConstraintUsage).WithMessage(string.Format(CultureInfo.InvariantCulture, ComparisonConstraintUsageConstants.Message, message));
            await Verify.VerifyAnalyzerAsync(testCode, diagnostic.WithLocation(0));
        }

        [TestCase(">=", "Is.LessThan")]
        [TestCase(">", "Is.LessThanOrEqualTo")]
        [TestCase("<=", "Is.GreaterThan")]
        [TestCase("<", "Is.GreaterThanOrEqualTo")]
        public async Task AnalyzeWhenComparisonOperatorUsedWithIsFalse(string operatorToken, string message)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        int actual = 5;
                        Assert.That({|#0:actual {{operatorToken}} 9|}, Is.False);
            """);

            var diagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ComparisonConstraintUsage).WithMessage(string.Format(CultureInfo.InvariantCulture, ComparisonConstraintUsageConstants.Message, message));
            await Verify.VerifyAnalyzerAsync(testCode, diagnostic.WithLocation(0));
        }

        [TestCase("Is.GreaterThanOrEqualTo")]
        [TestCase("Is.GreaterThan")]
        [TestCase("Is.LessThanOrEqualTo")]
        [TestCase("Is.LessThan")]
        public async Task ValidWhenConstraintUsed(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        int actual = 5;
                        Assert.That(actual, {{constraint}}(9));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase(">=")]
        [TestCase(">")]
        [TestCase("<=")]
        [TestCase("<")]
        public async Task ValidOnRefStruct(string operatorToken)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                [TestFixture]
                public class TestClass
                {
                    [Test]
                    public void TestMethod()
                    {
                        ComparableSpan<char> span1 = "Hello".AsSpan();
                        ComparableSpan<char> span2 = "World".AsSpan();
                        Assert.That(span1 {{operatorToken}} span2);
                    }

                    private ref struct ComparableSpan<T>
                        where T : IComparable
                    {
                        private readonly ReadOnlySpan<T> span;

                        public ComparableSpan(ReadOnlySpan<T> span) => this.span = span;

                        public static implicit operator ReadOnlySpan<T>(ComparableSpan<T> c) => c.span;
                        public static implicit operator ComparableSpan<T>(ReadOnlySpan<T> c) => new(c);

                        public static bool operator <(ComparableSpan<T> left, ComparableSpan<T> right) => false;
                        public static bool operator <=(ComparableSpan<T> left, ComparableSpan<T> right) => false;

                        public static bool operator >(ComparableSpan<T> left, ComparableSpan<T> right) => true;
                        public static bool operator >=(ComparableSpan<T> left, ComparableSpan<T> right) => true;
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
