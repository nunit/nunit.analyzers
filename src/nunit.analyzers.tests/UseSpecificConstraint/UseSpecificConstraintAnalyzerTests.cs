using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.UseSpecificConstraint;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.UseSpecificConstraint
{
    public sealed class UseSpecificConstraintAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new UseSpecificConstraintAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.UseSpecificConstraint);

        private static readonly string[][] EqualToSpecificConstraint =
        [
            ["false", "False"],
#if NUNIT4
            ["default(bool)", "Default"],
#else
            ["default(bool)", "False"],
#endif
            ["true", "True"],

            ["(object?)null", "Null"],
            ["default(object)", "Null"],
            ["default(string)", "Null"],
#if NUNIT4
            ["default(int)", "Default"],
#endif
            ["(byte)0", "Zero"],
            ["(char)0", "Zero"],
            ["(short)0", "Zero"],
            ["(ushort)0", "Zero"],
            ["0", "Zero"],
            ["0.0", "Zero"],
            ["0D", "Zero"],
            ["0F", "Zero"],
            ["0L", "Zero"],
            ["0M", "Zero"],
            ["0U", "Zero"],
            ["0UL", "Zero"],
        ];

        [TestCaseSource(nameof(EqualToSpecificConstraint))]
        public void AnalyzeForSpecificConstraint(string literal, string constraint) => AnalyzeForEqualTo(literal, constraint);

#if NUNIT4
        /*
         *  Is.EqualTo(default) no longer compiles with NUnit 4.3
         *  As 'default' is untyped, the call is ambigous between the specificy typed overloads.
         *
            [Test]
            public void AnalyzeForIsDefault() => AnalyzeForEqualTo("default", "Default",
                Settings.Default.WithAllowedCompilerDiagnostics(AllowedCompilerDiagnostics.WarningsAndErrors));
          */

        [Test]
        public void AnalyzeForObjectAndSpecificTypeShouldNotSuggestIsDefault()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                    object o = 0;
                    Assert.That(o, Is.EqualTo(default(int)));
                """);

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase(0)]
        public void AnalyzeForDynamicAndSpecificTypeShouldNotSuggestIsDefault(dynamic d)
        {
            // The below works, but the system determines the call at runtime.
            // When analyzing IInvocation we never see the 'Assert.That'
            // There is no overload for 'dynamic', so the system determines the call
            // depending on the actual type stored in the dynamic variable at runtime.
            Assert.That(d, Is.EqualTo(default(int)));
            Assert.That(d, Is.Default);

            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                [TestCase(0)]
                public void TestMethod(dynamic d)
                {
                    Assert.That(d, Is.EqualTo(default(int)));
                }
                """);

            RoslynAssert.Valid(analyzer, testCode);
        }
#endif

        [Test]
        public void AnalyzeForStringShouldNotSuggestIsZero()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                [TestCase("Hello")]
                public void TestMethod(string s)
                {
                    Assert.That(s, Is.EqualTo("World!"));
                }
                """);

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeForNullableStructShouldNotSuggestIsDefault()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                class SomeType
                {
                    public Guid? Id { get; set; }
                }

                [Test]
                public void TestMethod()
                {
                    var obj = new SomeType { Id = Guid.NewGuid() };

                    Assert.That(obj.Id, Is.Not.EqualTo(default(Guid)));
                }
                """);

            RoslynAssert.Valid(analyzer, testCode);
        }

#if NUNIT4
        [TestCase("Exception? actual = null;", "default(Exception?)")]
        [TestCase("Exception? actual = null;", "default(Exception)")]
        [TestCase("Exception actual = null!;", "default(Exception?)")]
        [TestCase("Exception actual = null!;", "default(Exception)")]
        public void AnalyzeForReferenceTypeShouldSuggestIsDefaultEvenIfNullabilityDoesntMatch(string initialization, string literal)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                    {initialization}
                    Assert.That(actual, ↓Is.EqualTo({literal}));");

            RoslynAssert.Diagnostics(analyzer,
                expectedDiagnostic.WithMessage($"Replace 'Is.EqualTo({literal})' with 'Is.Default' constraint"),
                testCode);
        }
#endif

        private static void AnalyzeForEqualTo(string literal, string constraint)
        {
            AnalyzeForEqualTo("Is", string.Empty, literal, constraint);
            AnalyzeForEqualTo("Is", ".And.Not.Empty", literal, constraint);
            AnalyzeForEqualTo("Is.Not", string.Empty, literal, constraint);
            AnalyzeForEqualTo("Is.EqualTo(4).Or", string.Empty, literal, constraint);
        }

        private static void AnalyzeForEqualTo(string prefix, string suffix, string literal, string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod(@$"
                var actual = {literal};
                Assert.That(actual, ↓{prefix}.EqualTo({literal}){suffix});");

            RoslynAssert.Diagnostics(analyzer,
                expectedDiagnostic.WithMessage($"Replace 'Is.EqualTo({literal})' with 'Is.{constraint}' constraint"),
                testCode);
        }
    }
}
