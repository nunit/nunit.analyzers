using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.MissingProperty.MissingPropertyAnalyzer>;

namespace NUnit.Analyzers.Tests.MissingProperty
{
    public class MissingPropertyAnalyzerTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.MissingProperty);

        [Test]
        public async Task AnalyzeWhenHasCountIsUsedForIEnumerable()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new [] {1, 2, 3}.Where(i => i > 1);
                        Assert.That(actual, {|#0:Has.Count|}.EqualTo(2));
            """,
                additionalUsings: "using System.Linq;");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("IEnumerable<int>", "Count"));
        }

        [Test]
        public async Task AnalyzeWhenHasLengthIsUsedForList()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new System.Collections.Generic.List<int> {1, 2, 3};
                        Assert.That(actual, {|#0:Has.Length|}.EqualTo(2));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("List<int>", "Length"));
        }

        [Test]
        public async Task AnalyzeWhenHasMessageIsUsedForString()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "Can we consider string as message?...";
                        Assert.That(actual, {|#0:Has.Message|}.EqualTo(2),
                            "Nope, we can't!");
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("string", "Message"));
        }

        [Test]
        public async Task AnalyzeWhenActualHasNoInnerException()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = 12345;
                        Assert.That(actual, {|#0:Has.InnerException|}.TypeOf<InvalidCastException>());
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("int", "InnerException"));
        }

        [Test]
        public async Task AnalyzeWhenPropertyConstraintIsUsedWhenNoSuchInActual()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = 12345;
                        Assert.That(actual, {|#0:Has.Property("Whatever")|}.EqualTo(1));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("int", "Whatever"));
        }

        [Test]
        public async Task AnalyzeWhenPropertyConstraintIsUsedWhenNoSuchInDelegateActual()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        int getActual() => 12345;
                        Assert.That(getActual, {|#0:Has.Property("Whatever")|}.EqualTo(1));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("int", "Whatever"));
        }

        [Test]
        public async Task ValidWhenHasCountIsUsedForList()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new [] {1, 2, 3}.Where(i => i > 1).ToList();
                        Assert.That(actual, Has.Count.EqualTo(2));
            """,
                additionalUsings: "using System.Linq;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenHasCountIsUsedForIList()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        IList<int> actual = new [] {1, 2, 3}.Where(i => i > 1).ToList();
                        Assert.That(actual, Has.Count.EqualTo(2));
            """,
                additionalUsings: "using System.Linq; using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenHasCountIsUsedForIDictionary()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new Dictionary<int, int>() { [1] = 1, [2] = 2 };
                        Assert.That(actual, Has.Count.EqualTo(2));
            """,
                additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenHasCountIsUsedForGenericConstraintOnICollection()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new [] { 1, 2 };

                        AssertCollectionOfIntCount(actual);

                        static void AssertCollectionOfIntCount<T>(T instance)
                            where T : ICollection<int>
                        {
                            Assert.That(instance, Has.Count.EqualTo(2));
                        }
            """,
                additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenHasLengthIsUsedForArray()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new[] {1, 2, 3};
                        Assert.That(actual, Has.Length.EqualTo(3));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenHasLengthIsUsedForString()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "ABCDefgh";
                        Assert.That(actual, Has.Length.EqualTo(8));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenHasMessageIsUsedForException()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new Exception("SomeMessage");
                        Assert.That(actual, Has.Message.EqualTo("SomeMessage"));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenHasInnerExceptionIsUsedForException()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var innerException = new Exception("inner");
                        var actual = new Exception("SomeMessage", innerException);
                        Assert.That(actual, Has.InnerException.Not.Null);
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenPropertyFromConstraintIsPresentInActual()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = DateTime.UtcNow;
                        Assert.That(actual, Has.Property("Ticks").GreaterThan(0));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenPropertyConstraintIsUsedForDelegateAndPropertyIsPresentInReturnType()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        string getActual() => "12345";
                        Assert.That(getActual, Has.Property("Length").EqualTo(5));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenPropertyIsUsedForVerificationOfMissingProperty()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        string actual = "321";
                        Assert.That(actual, Has.No.Property("Whatever"));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenCountIsUsedForAbsenceVerification()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        int actual = 321;
                        Assert.That(actual, Has.No.Count);
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenPropertyChainIsUsed()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new { Foo = new { Bar = "Baz" } };
                        Assert.That(actual, Has.Property("Foo").With.Property("Bar").EqualTo("Baz"));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenMultiplePartsCombinedWithCollectionOperatorPrefix()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new[]
                        {
                            new
                            {
                                Foo = "Fuzz",
                                Bar = "Buzz"
                            }
                        };
                        Assert.That(actual, Has.Some.With.Property("Foo").And.Property("Bar"));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenUsedWithThrowsConstraint()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        string getActual() => throw new System.Exception("Oops");
                        Assert.That(getActual, Throws.Exception.With.Message.EqualTo("Oops"));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
