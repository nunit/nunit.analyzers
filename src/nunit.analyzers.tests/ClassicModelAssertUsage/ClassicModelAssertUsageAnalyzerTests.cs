using System.Collections.Immutable;
using System.Globalization;
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

            Assert.That(diagnostics, Has.Length.EqualTo(28), nameof(DiagnosticAnalyzer.SupportedDiagnostics));

            foreach (var diagnostic in diagnostics)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(diagnostic.Title.ToString(CultureInfo.InvariantCulture), Is.Not.Empty,
                        $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Title)}");
                    Assert.That(diagnostic.MessageFormat.ToString(CultureInfo.InvariantCulture), Is.Not.Empty,
                        $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.MessageFormat)}");
                    Assert.That(diagnostic.Category, Is.EqualTo(Categories.Assertion),
                        $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Category)}");
                    Assert.That(diagnostic.DefaultSeverity, Is.AnyOf(DiagnosticSeverity.Warning, DiagnosticSeverity.Info),
                        $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.DefaultSeverity)}");
                });
            }

            var diagnosticIds = diagnostics.Select(_ => _.Id).ToImmutableArray();

            Assert.Multiple(() =>
            {
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
                Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.NullUsage),
                    $"{AnalyzerIdentifiers.NullUsage} is missing.");
                Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.IsNullUsage),
                    $"{AnalyzerIdentifiers.IsNullUsage} is missing.");
                Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.NotNullUsage),
                    $"{AnalyzerIdentifiers.NotNullUsage} is missing.");
                Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.IsNotNullUsage),
                    $"{AnalyzerIdentifiers.IsNotNullUsage} is missing.");
                Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.GreaterUsage),
                    $"{AnalyzerIdentifiers.GreaterUsage} is missing.");
                Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.GreaterOrEqualUsage),
                    $"{AnalyzerIdentifiers.GreaterOrEqualUsage} is missing.");
                Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.LessUsage),
                    $"{AnalyzerIdentifiers.LessUsage} is missing.");
                Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.LessOrEqualUsage),
                    $"{AnalyzerIdentifiers.LessOrEqualUsage} is missing.");
                Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.AreNotSameUsage),
                    $"{AnalyzerIdentifiers.AreNotSameUsage} is missing.");
                Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.ZeroUsage),
                    $"{AnalyzerIdentifiers.ZeroUsage} is missing.");
                Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.NotZeroUsage),
                    $"{AnalyzerIdentifiers.NotZeroUsage} is missing.");
                Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.IsNaNUsage),
                    $"{AnalyzerIdentifiers.IsNaNUsage} is missing.");
                Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.IsEmptyUsage),
                    $"{AnalyzerIdentifiers.IsEmptyUsage} is missing.");
                Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.IsNotEmptyUsage),
                    $"{AnalyzerIdentifiers.IsNotEmptyUsage} is missing.");
                Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.ContainsUsage),
                    $"{AnalyzerIdentifiers.ContainsUsage} is missing.");
                Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.IsInstanceOfUsage),
                    $"{AnalyzerIdentifiers.IsInstanceOfUsage} is missing.");
                Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.IsNotInstanceOfUsage),
                    $"{AnalyzerIdentifiers.IsNotInstanceOfUsage} is missing.");
                Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.PositiveUsage),
                    $"{AnalyzerIdentifiers.PositiveUsage} is missing.");
                Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.NegativeUsage),
                    $"{AnalyzerIdentifiers.NegativeUsage} is missing.");
            });
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
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenInvocationIsNotFromAssert()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenInvocationIsNotFromAssert
    {
        public void Test()
        {
            ClassicAssert.AreEqual(3, 4);
        }

        private static class ClassicAssert
        {
            public static bool AreEqual(int a, int b) => false;
        }
    }");
            RoslynAssert.Valid(this.analyzer, testCode);
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
            ↓ClassicAssert.IsTrue(true);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
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
            ↓ClassicAssert.True(true);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
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
            ↓ClassicAssert.IsFalse(false);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
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
            ↓ClassicAssert.False(false);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
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
            ↓ClassicAssert.AreEqual(2, 2);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
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
            ↓ClassicAssert.AreEqual(2d, 2d, 0.0000001d);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
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
            ↓ClassicAssert.AreNotEqual(2, 3);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
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
            ↓ClassicAssert.AreSame(2, 3);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIsNullIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.IsNullUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenIsNullIsUsed
    {
        public void Test()
        {
            object? obj = null;
            ↓ClassicAssert.IsNull(obj);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenNullIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.NullUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenNullIsUsed
    {
        public void Test()
        {
            object? obj = null;
            ↓ClassicAssert.Null(obj);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIsNotNullIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.IsNotNullUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenIsNotNullIsUsed
    {
        public void Test()
        {
            object? obj = null;
            ↓ClassicAssert.IsNotNull(obj);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenNotNullIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.NotNullUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenNotNullIsUsed
    {
        public void Test()
        {
            object? obj = null;
            ↓ClassicAssert.NotNull(obj);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenInvocationIsNotWithinAMethod()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenInvocationIsNotWithinAMethod
    {
        private static readonly string fullTypeName = typeof(string).GetType().FullName!;
    }");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenGreaterIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.GreaterUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenGreaterIsUsed
    {
        public void Test()
        {
            ↓ClassicAssert.Greater(2, 3);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenGreaterOrEqualIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.GreaterOrEqualUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenGreaterOrEqualIsUsed
    {
        public void Test()
        {
            ↓ClassicAssert.GreaterOrEqual(2, 3);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenLessIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.LessUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenLessIsUsed
    {
        public void Test()
        {
            ↓ClassicAssert.Less(2, 3);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenLessOrEqualIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.LessOrEqualUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenLessOrEqualIsUsed
    {
        public void Test()
        {
            ↓ClassicAssert.LessOrEqual(2, 3);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenAreNotSameIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.AreNotSameUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenAreNotSameIsUsed
    {
        public void Test()
        {
            ↓ClassicAssert.AreNotSame(2, 3);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenZeroIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.ZeroUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenZeroIsUsed
    {
        public void Test()
        {
            ↓ClassicAssert.Zero(2);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenNotZeroIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.NotZeroUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenNotZeroIsUsed
    {
        public void Test()
        {
            ↓ClassicAssert.NotZero(2);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIsNaNIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.IsNaNUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenIsNaNIsUsed
    {
        public void Test()
        {
            ↓ClassicAssert.IsNaN(2);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIsEmptyIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.IsEmptyUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenIsEmptyIsUsed
    {
        public void Test()
        {
            ↓ClassicAssert.IsEmpty(Array.Empty<object>());
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIsNotEmptyIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.IsNotEmptyUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenIsNotEmptyIsUsed
    {
        public void Test()
        {
            ↓ClassicAssert.IsNotEmpty(Array.Empty<object>());
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenContainsIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.ContainsUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenContainsIsUsed
    {
        public void Test()
        {
            ↓ClassicAssert.Contains(this, Array.Empty<object>());
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIsInstanceOfIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.IsInstanceOfUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenIsInstanceOfIsUsed
    {
        public void Test()
        {
            ↓ClassicAssert.IsInstanceOf(typeof(int), 2);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenGenericIsInstanceOfIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.IsInstanceOfUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenGenericIsInstanceOfIsUsed
    {
        public void Test()
        {
            ↓ClassicAssert.IsInstanceOf<int>(2);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenIsNotInstanceOfIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.IsNotInstanceOfUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenIsNotInstanceOfIsUsed
    {
        public void Test()
        {
            ↓ClassicAssert.IsNotInstanceOf(typeof(int), 2);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenGenericIsNotInstanceOfIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.IsNotInstanceOfUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenGenericIsNotInstanceOfIsUsed
    {
        public void Test()
        {
            ↓ClassicAssert.IsNotInstanceOf<int>(2);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenPositiveIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.PositiveUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenPositiveIsUsed
    {
        public void Test()
        {
            ↓ClassicAssert.Positive(2);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenNegativeIsUsed()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.NegativeUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenNegativeIsUsed
    {
        public void Test()
        {
            ↓ClassicAssert.Negative(2);
        }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }
    }
}
