using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.WithinUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.WithinUsage
{
    public class WithinUsageCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new WithinUsageAnalyzer();
        private static readonly CodeFixProvider fix = new WithinUsageCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.WithinIncompatibleTypes);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.WithinIncompatibleTypes }));
        }

        [TestCase(NUnitFrameworkConstants.NameOfIsEqualTo)]
        [TestCase(NUnitFrameworkConstants.NameOfIsLessThan)]
        [TestCase(NUnitFrameworkConstants.NameOfIsLessThanOrEqualTo)]
        [TestCase(NUnitFrameworkConstants.NameOfIsGreaterThan)]
        [TestCase(NUnitFrameworkConstants.NameOfIsGreaterThanOrEqualTo)]
        public void FixesWithinUsageOnConstraintWithIncompatibleExpectedType(string constraintName)
        {
            var code = TestUtility.WrapInTestMethod(
                $@"Assert.That(""1"", Is.{constraintName}(""1"").↓Within(1));");

            var fixedCode = TestUtility.WrapInTestMethod(
                $@"Assert.That(""1"", Is.{constraintName}(""1""));");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode,
                fixTitle: WithinUsageCodeFix.RemoveWithinDescription);
        }

        [TestCase(NUnitFrameworkConstants.NameOfIsEqualTo)]
        [TestCase(NUnitFrameworkConstants.NameOfIsLessThan)]
        [TestCase(NUnitFrameworkConstants.NameOfIsLessThanOrEqualTo)]
        [TestCase(NUnitFrameworkConstants.NameOfIsGreaterThan)]
        [TestCase(NUnitFrameworkConstants.NameOfIsGreaterThanOrEqualTo)]
        public void FixesWithinUsageOnInversedConstraintWithIncompatibleExpectedType(string constraintName)
        {
            var code = TestUtility.WrapInTestMethod(
                $@"Assert.That(""1"", Is.Not.{constraintName}(""1"").↓Within(1));");

            var fixedCode = TestUtility.WrapInTestMethod(
                $@"Assert.That(""1"", Is.Not.{constraintName}(""1""));");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode,
                fixTitle: WithinUsageCodeFix.RemoveWithinDescription);
        }

        [Test]
        public void FixesWithinUsageOnConstraintWithIncompatibleExpectedTypeWithMessage()
        {
            var code = TestUtility.WrapInTestMethod(
                @"Assert.That(""1"", Is.EqualTo(""1"").↓Within(1), ""message"");");

            var fixedCode = TestUtility.WrapInTestMethod(
                @"Assert.That(""1"", Is.EqualTo(""1""), ""message"");");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode,
                fixTitle: WithinUsageCodeFix.RemoveWithinDescription);
        }

        [Test]
        public void FixesWithinUsageOnConstraintWithIncompatibleExpectedTypeWithMessageAndParam()
        {
            var code = TestUtility.WrapInTestMethod(
                @"Assert.That(""1"", Is.EqualTo(""1"").↓Within(1), ""message"");");

            var fixedCode = TestUtility.WrapInTestMethod(
                @"Assert.That(""1"", Is.EqualTo(""1""), ""message"");");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode,
                fixTitle: WithinUsageCodeFix.RemoveWithinDescription);
        }
    }
}
