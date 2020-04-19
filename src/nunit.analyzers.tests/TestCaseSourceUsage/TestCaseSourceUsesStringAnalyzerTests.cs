using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TestCaseSourceUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.TestCaseSourceUsage
{
    [TestFixture]
    public sealed class TestCaseSourceUsesStringAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new TestCaseSourceUsesStringAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.TestCaseSourceStringUsage);
        private static readonly CodeFixProvider fix = new UseNameofFix();

        [Test]
        public void AnalyzeWhenNameOf()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenNameOf
    {
        string Tests;

        [TestCaseSource(nameof(Tests))]
        public void Test()
        {
        }
    }");
            AnalyzerAssert.Valid<TestCaseSourceUsesStringAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenTypeOf()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenTypeOf
    {
        [TestCaseSource(typeof(MyTests))]
        public void Test()
        {
        }
    }

    class MyTests
    {
    }");
            AnalyzerAssert.Valid<TestCaseSourceUsesStringAnalyzer>(testCode);
        }

        [Test]
        public void NoWarningWhenStringLiteralMissingMember()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class NoWarningWhenStringLiteralMissingMember
    {
        [TestCaseSource(""Missing"")]
        public void Test()
        {
        }
    }");
            var descriptor = new DiagnosticDescriptor(AnalyzerIdentifiers.TestCaseSourceStringUsage, string.Empty, string.Empty, string.Empty, DiagnosticSeverity.Warning, true);
            AnalyzerAssert.Valid(analyzer, descriptor, testCode);
        }

        [TestCase("private static readonly TestCaseData[] TestCases = new TestCaseData[0];")]
        [TestCase("private static TestCaseData[] TestCases => new TestCaseData[0];")]
        [TestCase("private static TestCaseData[] TestCases() => new TestCaseData[0];")]
        public void FixWhenStringLiteral(string testCaseMember)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenStringConstant
    {
        private static readonly TestCaseData[] TestCases = new TestCaseData[0];

        [TestCaseSource(↓""TestCases"")]
        public void Test()
        {
        }
    }").AssertReplace("private static readonly TestCaseData[] TestCases = new TestCaseData[0];", testCaseMember);

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenStringConstant
    {
        private static readonly TestCaseData[] TestCases = new TestCaseData[0];

        [TestCaseSource(nameof(TestCases))]
        public void Test()
        {
        }
    }").AssertReplace("private static readonly TestCaseData[] TestCases = new TestCaseData[0];", testCaseMember);

            var message = "Consider using nameof(TestCases) instead of \"TestCases\".";
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic.WithMessage(message), testCode, fixedCode);
        }

        [Test]
        public void FixWhenMultipleUnrelatedAttributes()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    [TestFixture]
    public class AnalyzeWhenMultipleUnrelatedAttributes
    {
        private static readonly TestCaseData[] TestCases = new TestCaseData[0];

        [Test]
        public void UnrelatedTest()
        {
        }

        [TestCaseSource(↓""TestCases"")]
        public void Test()
        {
        }
    }");

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    [TestFixture]
    public class AnalyzeWhenMultipleUnrelatedAttributes
    {
        private static readonly TestCaseData[] TestCases = new TestCaseData[0];

        [Test]
        public void UnrelatedTest()
        {
        }

        [TestCaseSource(nameof(TestCases))]
        public void Test()
        {
        }
    }");

            var message = "Consider using nameof(TestCases) instead of \"TestCases\".";
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic.WithMessage(message), testCode, fixedCode);
        }
    }
}
