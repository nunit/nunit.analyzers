using System.Collections.Immutable;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.SameAsOnValueTypes;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.SameAsOnValueTypes
{
    [TestFixture]
    public sealed class SameAsOnValueTypesCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new SameAsOnValueTypesAnalyzer();
        private static readonly CodeFixProvider fix = new SameAsOnValueTypesCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.SameAsOnValueTypes);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new SameAsOnValueTypesCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.SameAsOnValueTypes }));
        }

        [Test]
        public void VerifySameAsIntoEqualToFix()
        {
            var code = TestUtility.WrapInTestMethod(@"
                var expected = Guid.Empty;
                var actual = expected;

                Assert.That(actual, Is.SameAs(↓expected));
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@"
                var expected = Guid.Empty;
                var actual = expected;

                Assert.That(actual, Is.EqualTo(expected));
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.UseIsEqualToDescription);
        }

        [Test]
        public void VerifySameAsIntoEqualToFixWithMessage()
        {
            var code = TestUtility.WrapInTestMethod(@"
                var expected = Guid.Empty;

                Assert.That(() => expected, Is.Not.SameAs(↓expected), ""message"");
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@"
                var expected = Guid.Empty;

                Assert.That(() => expected, Is.Not.EqualTo(expected), ""message"");
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.UseIsEqualToDescription);
        }

        [Test]
        public void VerifySameAsIntoEqualToFixWithMessageAndParams()
        {
            var code = TestUtility.WrapInTestMethod(@"
                var expected = Guid.Empty;

                Assert.That(Task.FromResult(expected), Is.SameAs(↓expected), ""message"", Guid.Empty);
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@"
                var expected = Guid.Empty;

                Assert.That(Task.FromResult(expected), Is.EqualTo(expected), ""message"", Guid.Empty);
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.UseIsEqualToDescription);
        }

        [Test]
        public void VerifyAreSameIntoAreEqualFix()
        {
            var code = TestUtility.WrapInTestMethod(@"
                var expected = Guid.Empty;
                var actual = expected;

                Assert.AreSame(↓expected, actual);
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@"
                var expected = Guid.Empty;
                var actual = expected;

                Assert.AreEqual(expected, actual);
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.UseIsEqualToDescription);
        }

        [Test]
        public void VerifyAreNotSameIntoAreNotEqualFixWithMessage()
        {
            var code = TestUtility.WrapInTestMethod(@"
                var expected = Guid.Empty;

                Assert.AreNotSame(↓expected, Guid.NewGuid(), ""message"");
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@"
                var expected = Guid.Empty;

                Assert.AreNotEqual(expected, Guid.NewGuid(), ""message"");
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.UseIsEqualToDescription);
        }

        [Test]
        public void VerifyAreSameIntoAreEqualFixWithMessageAndParams()
        {
            var code = TestUtility.WrapInTestMethod(@"
                var expected = Guid.Empty;

                Assert.AreSame(↓expected, expected, ""message"", Guid.Empty);
            ");
            var fixedCode = TestUtility.WrapInTestMethod(@"
                var expected = Guid.Empty;

                Assert.AreEqual(expected, expected, ""message"", Guid.Empty);
            ");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.UseIsEqualToDescription);
        }

        [Test]
        public void AnalyzeWhenMoreThanOneConstraintExpressionIsUsed()
        {
            var code = TestUtility.WrapInTestMethod(
                @"Assert.That(""1"", Is.Not.SameAs(""1"") & Is.Not.SameAs(↓2));");

            var fixedCode = TestUtility.WrapInTestMethod(
                @"Assert.That(""1"", Is.Not.SameAs(""1"") & Is.Not.EqualTo(2));");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: CodeFixConstants.UseIsEqualToDescription);
        }
    }
}
