using System.Collections.Immutable;
using System.Linq;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class ClassicModelAssertUsageAnalyzerTests
    {
        private readonly DiagnosticAnalyzer analyzer = new ClassicModelAssertUsageAnalyzer();

        [Test]
        public void VerifySupportedDiagnostics()
        {
            var analyzer = new ClassicModelAssertUsageAnalyzer();
            var diagnostics = analyzer.SupportedDiagnostics;

            Assert.That(diagnostics.Length, Is.EqualTo(7), nameof(DiagnosticAnalyzer.SupportedDiagnostics));

            foreach (var diagnostic in diagnostics)
            {
                Assert.That(diagnostic.Title.ToString(), Is.Not.Empty,
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Title)}");
                Assert.That(diagnostic.MessageFormat.ToString(), Is.Not.Empty,
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.MessageFormat)}");
                Assert.That(diagnostic.Category, Is.EqualTo(Categories.Assertion),
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Category)}");
                Assert.That(diagnostic.DefaultSeverity, Is.EqualTo(DiagnosticSeverity.Warning),
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.DefaultSeverity)}");
            }

            var diagnosticIds = diagnostics.Select(_ => _.Id).ToImmutableArray();

            Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.AreEqualUsage),
                $"{AnalyzerIdentifiers.AreEqualUsage} is missing.");
            Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.AreNotEqualUsage),
                $"{AnalyzerIdentifiers.AreNotEqualUsage} is missing.");
            Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.FalseUsage),
                $"{AnalyzerIdentifiers.FalseUsage} is missing.");
            Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.IsFalseUsage),
                $"{AnalyzerIdentifiers.IsFalseUsage} is missing.");
            Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.IsTrueUsage),
                $"{AnalyzerIdentifiers.IsTrueUsage} is missing.");
            Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.TrueUsage),
                $"{AnalyzerIdentifiers.TrueUsage} is missing.");
            Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.AreSameUsage),
                $"{AnalyzerIdentifiers.AreSameUsage} is missing.");
        }

        [Test]
        public void AnalyzeWhenThatIsUsed()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenThatIsUsed
    {
        public void Test()
        {
            Assert.That(true, Is.True);
        }
    }");
            AnalyzerAssert.Valid<ClassicModelAssertUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenInvocationIsNotFromAssert()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenInvocationIsNotFromAssert
    {
        public void Test()
        {
            Assert.AreEqual(3, 4);
        }

        private static class Assert
        {
            public static bool AreEqual(int a, int b) => false;
        }
    }");
            AnalyzerAssert.Valid<ClassicModelAssertUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenIsTrueIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.IsTrueUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenIsTrueIsUsed
    {
        public void Test()
        {
            ↓Assert.IsTrue(true);
        }
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenTrueIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.TrueUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenTrueIsUsed
    {
        public void Test()
        {
            ↓Assert.True(true);
        }
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIsFalseIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.IsFalseUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenIsFalseIsUsed
    {
        public void Test()
        {
            ↓Assert.IsFalse(false);
        }
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenFalseIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.FalseUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenFalseIsUsed
    {
        public void Test()
        {
            ↓Assert.False(false);
        }
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenAreEqualIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.AreEqualUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenAreEqualIsUsed
    {
        public void Test()
        {
            ↓Assert.AreEqual(2, 2);
        }
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenAreEqualIsUsedWithTolerance()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.AreEqualUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenAreEqualIsUsedWithTolerance
    {
        public void Test()
        {
            ↓Assert.AreEqual(2d, 2d, 0.0000001d);
        }
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenAreNotEqualIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.AreNotEqualUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenAreNotEqualIsUsed
    {
        public void Test()
        {
            ↓Assert.AreNotEqual(2, 3);
        }
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenAreSameIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.AreSameUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenAreSameIsUsed
    {
        public void Test()
        {
            ↓Assert.AreSame(2, 3);
        }
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenInvocationIsNotWithinAMethod()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenInvocationIsNotWithinAMethod
    {
        private static readonly string fullTypeName = typeof(string).GetType().FullName;
    }");
            AnalyzerAssert.Valid<ClassicModelAssertUsageAnalyzer>(testCode);
        }
    }
}
