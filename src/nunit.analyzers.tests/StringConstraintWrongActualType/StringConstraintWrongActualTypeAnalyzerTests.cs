using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.StringConstraintWrongActualType.StringConstraintWrongActualTypeAnalyzer>;

namespace NUnit.Analyzers.Tests.StringConstraintWrongActualType
{
    public class StringConstraintWrongActualTypeAnalyzerTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.StringConstraintWrongActualType);

        private static readonly string[] StringConstraints =
        [
            "Does.StartWith(\"12\")",
            "Does.EndWith(\"34\")",
            "Does.Match(@\"\\d+\")",
            "Does.Not.StartWith(\"12\")",
            "Does.Not.EndWith(\"34\")",
            "Does.Not.Match(@\"\\d+\")",
            "Contains.Substring(\"23\")",
            "new NUnit.Framework.Constraints.EmptyStringConstraint()",
            """Is.SamePath(@"C:\repos\nunit.analyzers")""",
            """Is.SamePathOrUnder(@"C:\repos")""",
            """Is.SubPathOf(@"C:\")"""
        ];

        [Test]
        public async Task AnalyzeWhenNonStringValueProvided([ValueSource(nameof(StringConstraints))] string stringConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var actual = 1234;
                        Assert.That(actual, {|#0:{{stringConstraint}}|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments(ConstraintTypeName(stringConstraint), "int"));
        }

        [Test]
        public async Task AnalyzeWhenStringTaskValueProvided([ValueSource(nameof(StringConstraints))] string stringConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var actual = Task.FromResult("1234");
                        Assert.That(actual, {|#0:{{stringConstraint}}|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments(ConstraintTypeName(stringConstraint), "Task<string>"));
        }

        [Test]
        public async Task ValidWhenStringValueProvided([ValueSource(nameof(StringConstraints))] string stringConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var actual = "1234";
                        Assert.That(actual, {{stringConstraint}});
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenStringDelegateProvided([ValueSource(nameof(StringConstraints))] string stringConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var actual = "1234";
                        Assert.That(() => actual, {{stringConstraint}});
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenStringTaskDelegateProvided([ValueSource(nameof(StringConstraints))] string stringConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var actual = Task.FromResult("1234");
                        Assert.That(() => actual, {{stringConstraint}});
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenValueConvertedToStringProvided([ValueSource(nameof(StringConstraints))] string stringConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var actual = 1234;
                        Assert.That(actual.ToString(), {{stringConstraint}});
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenAllOperatorUsed()
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var actual = new[] {"11", "12", "13"};
                        Assert.That(actual, Has.All.StartsWith("1"));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        // The diagnostic message contains the name of the constraint type created by the constraint expression.
        private static string ConstraintTypeName(string stringConstraint)
        {
            string method = Regex.Match(stringConstraint, @"\.(\w+)\(").Groups[1].Value;
            return method switch
            {
                "StartWith" => "StartsWithConstraint",
                "EndWith" => "EndsWithConstraint",
                "Match" => "RegexConstraint",
                "Substring" => "SubstringConstraint",
                "SamePath" => "SamePathConstraint",
                "SamePathOrUnder" => "SamePathOrUnderConstraint",
                "SubPathOf" => "SubPathConstraint",
                _ => method,
            };
        }
    }
}
