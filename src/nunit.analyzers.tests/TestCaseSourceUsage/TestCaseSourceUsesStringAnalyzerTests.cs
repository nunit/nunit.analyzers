using Gu.Roslyn.Asserts;
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
        private static readonly DiagnosticAnalyzer Analyzer = new TestCaseSourceUsesStringAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.TestCaseSourceStringUsage);
        private static readonly CodeFixProvider Fix = new UseNameofFix();

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
        public void FixWhenStringLiteral()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenStringConstant
    {
        private static readonly TestCaseData[] TestCases = new TestCaseData[0];

        [TestCaseSource(↓""TestCases"")]
        public void Test()
        {
        }
    }");

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenStringConstant
    {
        private static readonly TestCaseData[] TestCases = new TestCaseData[0];

        [TestCaseSource(nameof(TestCases))]
        public void Test()
        {
        }
    }");
            var message = "Consider using nameof(TestCases) instead of \"TestCases\"";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), testCode, fixedCode, allowCompilationErrors: AllowCompilationErrors.Yes);
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

            var message = "Consider using nameof(TestCases) instead of \"TestCases\"";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage(message), testCode, fixedCode, allowCompilationErrors: AllowCompilationErrors.Yes);
        }
    }
}
