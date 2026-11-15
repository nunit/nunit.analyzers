using System.Globalization;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.ConstraintUsage;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.ConstraintUsage.EqualConstraintUsageAnalyzer>;

namespace NUnit.Analyzers.Tests.ConstraintsUsage
{
    public class EqualConstraintUsageAnalyzerTests
    {
        private static readonly DiagnosticResult isEqualToDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.EqualConstraintUsage).WithMessage(string.Format(CultureInfo.InvariantCulture, EqualConstraintUsageConstants.Message, "Is.EqualTo"));
        private static readonly DiagnosticResult isNotEqualToDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.EqualConstraintUsage).WithMessage(string.Format(CultureInfo.InvariantCulture, EqualConstraintUsageConstants.Message, "Is.Not.EqualTo"));

        [Test]
        public async Task AnalyzeWhenEqualsOperatorUsed()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:actual == "abc"|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, isEqualToDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenNotEqualsOperatorUsed()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:actual != "bcd"|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, isNotEqualToDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenIsOperatorUsed()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:actual is "abc"|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, isEqualToDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenIsNotOperatorUsed()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:actual is not "bcd"|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, isNotEqualToDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenEqualsInstanceMethodUsed()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:actual.Equals("abc")|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, isEqualToDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenNegatedEqualsInstanceMethodUsed()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:!actual.Equals("bcd")|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, isNotEqualToDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenEqualsStaticMethodUsed()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:Equals(actual,"abc")|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, isEqualToDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenNegatedEqualsStaticMethodUsed()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:!Equals(actual,"abc")|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, isNotEqualToDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenEqualsMethodUsedWithIsTrue()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:actual.Equals("abc")|}, Is.True);
            """);

            await Verify.VerifyAnalyzerAsync(testCode, isEqualToDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenEqualsMethodUsedWithIsFalse()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:actual.Equals("abc")|}, Is.False);
            """);

            await Verify.VerifyAnalyzerAsync(testCode, isNotEqualToDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenEqualsMethodUsedWithAssertTrue()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        ClassicAssert.True({|#0:actual.Equals("abc")|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, isEqualToDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenEqualsMethodUsedWithAssertFalse()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        ClassicAssert.False({|#0:actual.Equals("bcd")|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, isNotEqualToDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenEqualsMethodUsedWithAssertIsTrue()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        ClassicAssert.IsTrue({|#0:actual.Equals("abc")|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, isEqualToDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenEqualsMethodUsedWithAssertIsFalse()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        ClassicAssert.IsFalse({|#0:actual.Equals("bcd")|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, isNotEqualToDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeDoubleNegationAsPositive()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        ClassicAssert.IsFalse({|#0:!actual.Equals("bcd")|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, isEqualToDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenAssertThatIsUsedWithMessage()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:actual == "abc"|}, "Assertion message");
            """);

            await Verify.VerifyAnalyzerAsync(testCode, isEqualToDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenEqualsMethodUsedWithIsTrueWithMessage()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That({|#0:actual.Equals("abc")|}, Is.True, "Assertion message");
            """);

            await Verify.VerifyAnalyzerAsync(testCode, isEqualToDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenEqualsMethodUsedWithAssertTrueWithMessage()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        ClassicAssert.True({|#0:actual.Equals("abc")|}, "Assertion message");
            """);

            await Verify.VerifyAnalyzerAsync(testCode, isEqualToDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenEqualsMethodUsedWithAssertFalseWithMessage()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        ClassicAssert.False({|#0:actual.Equals("bcd")|}, "Assertion message");
            """);

            await Verify.VerifyAnalyzerAsync(testCode, isNotEqualToDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task ValidWhenIsEqualToConstraintUsed()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual, Is.EqualTo("bcd"));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenIsNotEqualToConstraintUsed()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual, Is.Not.EqualTo("bcd"));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidOtherMethodUsed()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "abc";
                        Assert.That(actual.Contains("bc"));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("==")]
        [TestCase("!=")]
        public async Task UseEqualsOperatorOnRefStruct(string operatorToken)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var span1 = new[] { 1 }.AsSpan();
                        var span2 = new[] { 1 }.AsSpan();
                        Assert.That(span1 {{operatorToken}} span2, Is.True);
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task UseEqualsMethodWithRefStructArgument()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class TestClass
                {
                    [Test]
                    public void TestMethod()
                    {
                        var span1 = new[] { 1 }.AsSpan();
                        var span2 = new[] { 1 }.AsSpan();
                        Assert.That(span1.Equals(span2), Is.True, "Span comparison");
                    }
                }

                internal static class SpanExtension
                {
                    public static bool Equals<T>(this Span<T> left, Span<T> right) => left == right;
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task UseEqualsMethodWithRefStructInstance()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class TestClass
                {
                    [Test]
                    public void TestMethod()
                    {
                        ReadOnlySpan<char> actual = GetPart();
                        Assert.That(actual.Equals("Text"), Is.True);
                    }

                    ReadOnlySpan<char> GetPart() => "Some Text".AsSpan(4);
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task EqualsMethodButClassDoesNotImplementIEqualityInterface()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class TestClass
                {
                    [Test]
                    public void TestMethod()
                    {
                        Foo expected = new Foo();
                        Foo actual = new Foo();
                        Assert.That(actual.Equals(expected), Is.True);
                    }

                    private sealed class Foo
                    {
                        public bool Equals(Foo other) => true;
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task EqualsMethodAndClassDoesImplementIEqualityInterface()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class TestClass
                {
                    [Test]
                    public void TestMethod()
                    {
                        Foo expected = new Foo();
                        Foo actual = new Foo();
                        Assert.That({|#0:actual.Equals(expected)|}, Is.True);
                    }

                    private sealed class Foo : IEquatable<Foo>
                    {
                        public bool Equals(Foo? other) => true;
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, isEqualToDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task EqualsMethodOnObject()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class TestClass
                {
                    [Test]
                    public void TestMethod()
                    {
                        object expected = new Foo();
                        Foo actual = new Foo();
                        Assert.That({|#0:actual.Equals(expected)|}, Is.True);
                    }

                    private sealed class Foo : IEquatable<Foo>
                    {
                        public bool Equals(Foo? other) => true;
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, isEqualToDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task EqualsMethodButClassDoesNotImplementIEqualityInterfaceForOtherClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class TestClass
                {
                    [Test]
                    public void FooIsMyFriend()
                    {
                        Friend expected = new Friend();
                        Foo actual = new Foo();
                        Assert.That(actual.Equals(expected), Is.True);
                    }

                    private sealed class Foo : IEquatable<Foo>
                    {
                        public bool Equals(Foo? other) => true;
                        public bool Equals(Friend? other) => true;
                    }

                    private sealed class Friend : IEquatable<Friend>
                    {
                        public bool Equals(Friend? other) => true;
                        public bool Equals(Foo? other) => true;
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
