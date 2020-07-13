using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.NullConstraintUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.NullConstraintUsage
{
    public class NullConstraintUsageAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new NullConstraintUsageAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.NullConstraintUsage);

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public void AnalyzeWhenActualIsNonNullableValueType(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var actual = default(int);
                Assert.That(actual, ↓{constraint});");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public void AnalyzeWhenActualIsNonNullableValueTypeDelegate(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var actual = default(int);
                Assert.That(() => actual, ↓{constraint});");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public void ValidWhenActualIsNullableValueType(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                bool? actual = false;
                Assert.That(actual, {constraint});");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public void ValidWhenActualIsReferenceType(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var actual = new string[] {{ ""TestString"" }};
                Assert.That(actual, {constraint});");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public void ValidWhenActualIsReferenceTypeDelegate(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var actual = new string[] {{ ""TestString"" }};
                Assert.That(() => actual, {constraint});");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenUsedWithPropertyOperator()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public struct TestStruct
                {
                    public string RefTypeProp { get; set; }
                }
                
                [Test]
                public void TestMethod()
                {
                    var actual = new TestStruct();
                    Assert.That(actual, Has.Property(""RefTypeProp"").Null);
                }");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public void ValidWhenActualIsTask(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var task = Task.CompletedTask;
                Assert.That(task, {constraint});");

            AnalyzerAssert.Valid(analyzer, testCode);
        }
    }
}
