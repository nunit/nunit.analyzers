using Gu.Roslyn.Asserts;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.RangeUsage;
using NUnit.Framework;

using TestCaseType = (string Range, string ParameterType);

namespace NUnit.Analyzers.Tests.RangeUsage
{
    public class RangeUsageAnalyzerTests
    {
        private readonly RangeUsageAnalyzer analyzer = new RangeUsageAnalyzer();

        private static readonly TestCaseType[] CorrectUsages =
        [

            // Integer ranges
            ("Range(1, 1)", "int"),
            ("Range(1, 3)", "int"),
            ("RangeAttribute(1, 3)", "int"),
            ("NUnit.Framework.Range(1, 3)", "int"),
            ("Range(3, 1)", "int"),
            ("Range(to: 3, from: 1)", "int"),
            ("Range(1, 3, 1)", "int"),
            ("Range(3, 1, -1)", "int"),
            ("Range(3 - 3, 1 + 3, 1 + 1)", "int"),

            // Unsigned integer ranges
            ("Range(1U, 1U)", "uint"),
            ("Range(1U, 3U)", "uint"),
            ("Range(1U, 3U, 1U)", "uint"),
            ("Range(1U, 1U + 2U, step: 1U)", "uint"),

            // Long ranges
            ("Range(11L, 11L)", "long"),
            ("Range(11L, 33L)", "long"),
            ("Range(33L, 11L)", "long"),
            ("Range(11L, 33L, 11L)", "long"),
            ("Range(-33L, -11L, 11L)", "long"),
            ("Range(33L, 11L, -11L)", "long"),

            // Unsigned long ranges
            ("Range(11UL, 11UL)", "ulong"),
            ("Range(11UL, 33UL)", "ulong"),
            ("Range(11UL, 33UL, 11UL)", "ulong"),

            // Single ranges
            ("Range(1.0f, 1.0f, 0.1f)", "float"),
            ("Range(1.0f, 3.0f, 0.1f)", "float"),
            ("Range(3.0f, 1.0f, -0.1f)", "float"),

            // Double ranges
            ("Range(1.1, 1.1, 0.1)", "double"),
            ("Range(1.1, 3.3, 0.1)", "double"),
            ("Range(3.3, 1.1, -0.1)", "double"),
            ("Range(3.3, step: -0.1, to: 1.1)", "double"),
        ];

        private static readonly TestCaseType[] ZeroStepUsages =
        [

            // Integer ranges
            ("Range(1, 3, 0)", "int"),
            ("Range(3, 1, -0)", "int"),

            // Unsigned integer ranges
            ("Range(1U, 3U, 0U)", "uint"),

            // Long ranges
            ("Range(11L, 33L, 0L)", "long"),
            ("Range(33L, 11L, -0L)", "long"),

            // Unsigned long ranges
            ("Range(11UL, 33UL, 0UL)", "ulong"),

            // Single ranges
            ("Range(1.0f, 3.0f, 0.0f)", "float"),
            ("Range(3.0f, 1.0f, -0.0f)", "float"),

            // Double ranges
            ("Range(1.1, 3.3, 0.0)", "double"),
            ("Range(3.3, 1.1, -0.0)", "double"),
        ];

        private static readonly TestCaseType[] FromGreaterThanToWithPositiveStepUsages =
        [

            // Integer ranges
            ("Range(3, 1, 1)", "int"),

            // Unsigned integer ranges
            ("Range(3U, 1U)", "uint"),
            ("Range(3U, 1U, 1U)", "uint"),

            // Long ranges
            ("Range(33L, 11L, 11L)", "long"),

            // Unsigned long ranges
            ("Range(33UL, 11UL)", "ulong"),
            ("Range(33UL, 11UL, 11UL)", "ulong"),

            // Single ranges
            ("Range(3.0f, 1.0f, 0.1f)", "float"),

            // Double ranges
            ("Range(3.3, 1.1, 0.1)", "double"),
        ];

        private static readonly TestCaseType[] FromLessThanToWithNegativeStepUsages =
        [

            // Integer ranges
            ("Range(1, 3, -1)", "int"),

            // Unsigned integer ranges

            // Long ranges
            ("Range(11L, 33L, -11L)", "long"),

            // Unsigned long ranges

            // Single ranges
            ("Range(1.0f, 3.0f, -0.1f)", "float"),

            // Double ranges
            ("Range(1.1, 3.3, -0.1)", "double"),
        ];

        [Test]
        public void VerifySupportedDiagnostics()
        {
            var diagnostics = this.analyzer.SupportedDiagnostics;

            Assert.That(diagnostics, Has.Length.EqualTo(4));
        }

        [Test]
        public void AnalyzeWhenAttributeIsNotInNUnit()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public sealed class AnalyzeWhenAttributeIsNotInNUnit
            {
                [Test]
                public void ATest([Range(3, 1, 1)] bool value) { }

                private sealed class RangeAttribute : Attribute
                {
                    public RangeAttribute(int count, int start, int step) { }
                }
            }
            """);
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [TestCaseSource(nameof(CorrectUsages))]
        public void AnalyzeWhenArgumentsAreCorrect(TestCaseType testCase)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [Test]
                public void ATest([{{testCase.Range}}] {{testCase.ParameterType}} value) { }
            """);
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenUsingClassConstants()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public sealed class AnalyzeWhenUsingClassConstants
            {
                private const int Increment = 1;
                private const int Multiplier = 2;
                private const int Step = Increment * Multiplier;
                private const int NumberOfIterations = 5;

                private const int From = 3;
                private const int To = From + NumberOfIterations * Step;

                [Test]
                public void ATest([Range(From, To, Step)] int value) { }
            }
            """);
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        // Types taken from NUnit's ParamAttributeTypeConversions
        [TestCase("Range(0, 10)", "short")]
        [TestCase("Range(0, 10)", "byte")]
        [TestCase("Range(0, 10)", "sbyte")]
        [TestCase("Range(0, 10)", "long")]
        [TestCase("Range(0, 10)", "double")]
        [TestCase("Range(0, 10)", "decimal")]
        [TestCase("Range(0.0, 10.0, 1.0)", "decimal")]
        public void AnalyzeWhenMismatchedTypeCanBeConverted(string range, string type)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [Test]
                public void ATest([{{range}}] {{type}} value) { }
            """);
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [TestCase("Range(0, 10)", "float")]
        [TestCase("Range(0L, 10L)", "float")]
        [TestCase("Range(0L, 10L)", "int")]
        [TestCase("Range(0UL, 10UL)", "int")]
        [TestCase("Range(0.0f, 1.0f, 0.5f)", "int")]
        [TestCase("Range(0.0f, 1.0f, 0.5f)", "double")]
        [TestCase("Range(0.0, 1.0, 0.5)", "int")]
        [TestCase("Range(0.0, 1.0, 0.5)", "float")]
        public void AnalyzeWhenMismatchedTypesAreNotCompatible(string range, string type)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [Test]
                public void ATest([↓{{range}}] {{type}} value) { }
            """);
            RoslynAssert.Diagnostics(this.analyzer, ExpectedDiagnostic.Create(AnalyzerIdentifiers.AttributeValueMismatchedParameterType), testCode);
        }

        [TestCaseSource(nameof(ZeroStepUsages))]
        public void AnalyzeWhenStepIsZero(TestCaseType testCase)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [Test]
                public void ATest([↓{{testCase.Range}}] {{testCase.ParameterType}} value) { }
            """);
            RoslynAssert.Diagnostics(this.analyzer, ExpectedDiagnostic.Create(AnalyzerIdentifiers.RangeUsesZeroStep), testCode);
        }

        [TestCaseSource(nameof(FromGreaterThanToWithPositiveStepUsages))]
        public void AnalyzeWhenFromGreaterThanToWithPositiveStep(TestCaseType testCase)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [Test]
                public void ATest([↓{{testCase.Range}}] {{testCase.ParameterType}} value) { }
            """);
            RoslynAssert.Diagnostics(this.analyzer, ExpectedDiagnostic.Create(AnalyzerIdentifiers.RangeInvalidIncrementing), testCode);
        }

        [TestCaseSource(nameof(FromLessThanToWithNegativeStepUsages))]
        public void AnalyzeWhenFromLessThanToWithNegativeStep(TestCaseType testCase)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [Test]
                public void ATest([↓{{testCase.Range}}] {{testCase.ParameterType}} value) { }
            """);
            RoslynAssert.Diagnostics(this.analyzer, ExpectedDiagnostic.Create(AnalyzerIdentifiers.RangeInvalidDecrementing), testCode);
        }
    }
}
