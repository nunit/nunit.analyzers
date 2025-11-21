using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TypeOfConstraint;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.TypeOfConstraint
{
    public class TypeOfCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new TypeOfAnalyzer();
        private static readonly CodeFixProvider fix = null!; // TODO
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.InstanceOf);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.InstanceOf }));
        }

        [Test]
        public void VerifyFix()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                Assert.That(0.GetType(), Is.EqualTo(typeof(int)));");
            var fixedCode = TestUtility.WrapInTestMethod(@"
                Assert.That(0, Is.InstanceOf<int>());");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, testCode, fixedCode);
        }
    }
}
