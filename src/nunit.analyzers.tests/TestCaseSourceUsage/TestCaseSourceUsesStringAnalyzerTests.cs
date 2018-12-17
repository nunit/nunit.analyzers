using System;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TestCaseSourceUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.TestCaseSourceUsage
{
    [TestFixture]
    public sealed class TestCaseSourceUsesStringAnalyzerTests
    {
        private DiagnosticAnalyzer analyzer = new TestCaseSourceUsesStringAnalyzer();

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
        public void AnalyzeWhenStringConstant()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestCaseSourceStringUsage,
                String.Format(TestCaseSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantMessage, "Tests"));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenStringConstant
    {
        [↓TestCaseSource(""Tests"")]
        public void Test()
        {
        }
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenMultipleUnrelatedAttributes()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestCaseSourceStringUsage,
                String.Format(TestCaseSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantMessage, "StringConstant"));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    [TestFixture]
    public class AnalyzeWhenMultipleUnrelatedAttributes
    {
        [Test]
        public void UnrelatedTest()
        {
        }

        [↓TestCaseSource(""StringConstant"")]
        public void Test()
        {
        }
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }
    }
}
