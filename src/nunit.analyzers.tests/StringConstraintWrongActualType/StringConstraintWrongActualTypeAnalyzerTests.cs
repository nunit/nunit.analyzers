using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.StringConstraintWrongActualType;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.StringConstraintWrongActualType
{
    public class StringConstraintWrongActualTypeAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new StringConstraintWrongActualTypeAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.StringConstraintWrongActualType);

        private static readonly string[] StringConstraints = new[]
        {
            "Does.StartWith(\"12\")",
            "Does.EndWith(\"34\")",
            "Does.Match(@\"\\d+\")",
            "Does.Not.StartWith(\"12\")",
            "Does.Not.EndWith(\"34\")",
            "Does.Not.Match(@\"\\d+\")",
            "Contains.Substring(\"23\")",
            "new NUnit.Framework.Constraints.EmptyStringConstraint()",
            @"Is.SamePath(@""C:\repos\nunit.analyzers"")",
            @"Is.SamePathOrUnder(@""C:\repos"")",
            @"Is.SubPathOf(@""C:\"")"
        };

        [Test]
        public void AnalyzeWhenNonStringValueProvided([ValueSource(nameof(StringConstraints))] string stringConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var actual = 1234;
                Assert.That(actual, ↓{stringConstraint});");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenStringTaskValueProvided([ValueSource(nameof(StringConstraints))] string stringConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var actual = Task.FromResult(""1234"");
                Assert.That(actual, ↓{stringConstraint});");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void ValidWhenStringValueProvided([ValueSource(nameof(StringConstraints))] string stringConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var actual = ""1234"";
                Assert.That(actual, {stringConstraint});");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenStringDelegateProvided([ValueSource(nameof(StringConstraints))] string stringConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var actual = ""1234"";
                Assert.That(() => actual, {stringConstraint});");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenStringTaskDelegateProvided([ValueSource(nameof(StringConstraints))] string stringConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var actual = Task.FromResult(""1234"");
                Assert.That(() => actual, {stringConstraint});");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenValueConvertedToStringProvided([ValueSource(nameof(StringConstraints))] string stringConstraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var actual = 1234;
                Assert.That(actual.ToString(), {stringConstraint});");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenAllOperatorUsed()
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var actual = new[] {{""11"", ""12"", ""13""}};
                Assert.That(actual, Has.All.StartsWith(""1""));");

            AnalyzerAssert.Valid(analyzer, testCode);
        }
    }
}
