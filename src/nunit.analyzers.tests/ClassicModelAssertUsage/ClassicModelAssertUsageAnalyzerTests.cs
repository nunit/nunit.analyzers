using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.ClassicModelAssertUsage.ClassicModelAssertUsageAnalyzer>;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class ClassicModelAssertUsageAnalyzerTests
    {
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
                Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.IsAssignableFromUsage),
                    $"{AnalyzerIdentifiers.NegativeUsage} is missing.");
                Assert.That(diagnosticIds, Contains.Item(AnalyzerIdentifiers.IsNotAssignableFromUsage),
                    $"{AnalyzerIdentifiers.NegativeUsage} is missing.");
            });
        }

        [Test]
        public async Task AnalyzeWhenThatIsUsed()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenThatIsUsed
                {
                    public void Test()
                    {
                        Assert.That(true, Is.True);
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenInvocationIsNotFromAssert()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenIsTrueIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.IsTrueUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenIsTrueIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.IsTrue(true)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenTrueIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TrueUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenTrueIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.True(true)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenIsFalseIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.IsFalseUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenIsFalseIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.IsFalse(false)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenFalseIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.FalseUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenFalseIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.False(false)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenAreEqualIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.AreEqualUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenAreEqualIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.AreEqual(2, 2)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenAreEqualIsUsedWithTolerance()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.AreEqualUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenAreEqualIsUsedWithTolerance
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.AreEqual(2d, 2d, 0.0000001d)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenAreNotEqualIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.AreNotEqualUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenAreNotEqualIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.AreNotEqual(2, 3)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenAreSameIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.AreSameUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenAreSameIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.AreSame(2, 3)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenIsNullIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.IsNullUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenIsNullIsUsed
                {
                    public void Test()
                    {
                        object? obj = null;
                        {|#0:ClassicAssert.IsNull(obj)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenNullIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.NullUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenNullIsUsed
                {
                    public void Test()
                    {
                        object? obj = null;
                        {|#0:ClassicAssert.Null(obj)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenIsNotNullIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.IsNotNullUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenIsNotNullIsUsed
                {
                    public void Test()
                    {
                        object? obj = null;
                        {|#0:ClassicAssert.IsNotNull(obj)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenNotNullIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.NotNullUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenNotNullIsUsed
                {
                    public void Test()
                    {
                        object? obj = null;
                        {|#0:ClassicAssert.NotNull(obj)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenInvocationIsNotWithinAMethod()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenInvocationIsNotWithinAMethod
                {
                    private static readonly string fullTypeName = typeof(string).GetType().FullName!;
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenGreaterIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.GreaterUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenGreaterIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.Greater(2, 3)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenGreaterOrEqualIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.GreaterOrEqualUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenGreaterOrEqualIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.GreaterOrEqual(2, 3)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenLessIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.LessUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenLessIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.Less(2, 3)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenLessOrEqualIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.LessOrEqualUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenLessOrEqualIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.LessOrEqual(2, 3)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenAreNotSameIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.AreNotSameUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenAreNotSameIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.AreNotSame(2, 3)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenZeroIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ZeroUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenZeroIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.Zero(2)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenNotZeroIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.NotZeroUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenNotZeroIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.NotZero(2)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenIsNaNIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.IsNaNUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenIsNaNIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.IsNaN(2)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenIsEmptyIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.IsEmptyUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenIsEmptyIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.IsEmpty(Array.Empty<object>())|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenIsNotEmptyIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.IsNotEmptyUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenIsNotEmptyIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.IsNotEmpty(Array.Empty<object>())|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenContainsIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ContainsUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenContainsIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.Contains(this, Array.Empty<object>())|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenIsInstanceOfIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.IsInstanceOfUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenIsInstanceOfIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.IsInstanceOf(typeof(int), 2)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenGenericIsInstanceOfIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.IsInstanceOfUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenGenericIsInstanceOfIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.IsInstanceOf<int>(2)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenIsNotInstanceOfIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.IsNotInstanceOfUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenIsNotInstanceOfIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.IsNotInstanceOf(typeof(int), 2)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenGenericIsNotInstanceOfIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.IsNotInstanceOfUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenGenericIsNotInstanceOfIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.IsNotInstanceOf<int>(2)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenPositiveIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.PositiveUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenPositiveIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.Positive(2)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenNegativeIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.NegativeUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenNegativeIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.Negative(2)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenIsAssignableFromIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.IsAssignableFromUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenIsAssignableFromIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.IsAssignableFrom(typeof(int), 2)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenGenericIsAssignableFromIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.IsAssignableFromUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenGenericIsAssignableFromIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.IsAssignableFrom<int>(2)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenIsNotAssignableFromIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.IsNotAssignableFromUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenIsNotAssignableFromIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.IsNotAssignableFrom(typeof(int), 2)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenGenericIsNotAssignableFromIsUsed()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.IsNotAssignableFromUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenGenericIsNotAssignableFromIsUsed
                {
                    public void Test()
                    {
                        {|#0:ClassicAssert.IsNotAssignableFrom<int>(2)|};
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }
    }
}
