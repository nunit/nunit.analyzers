using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.MissingProperty;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.MissingProperty
{
    public class MissingPropertyAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new MissingPropertyAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.MissingProperty);

        [Test]
        public void AnalyzeWhenHasCountIsUsedForIEnumerable()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new [] {1, 2, 3}.Where(i => i > 1);
                Assert.That(actual, ↓Has.Count.EqualTo(2));",
                additionalUsings: "using System.Linq;");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenHasLengthIsUsedForList()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new System.Collections.Generic.List<int> {1, 2, 3};
                Assert.That(actual, ↓Has.Length.EqualTo(2));");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenHasMessageIsUsedForString()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""Can we consider string as message?..."";
                Assert.That(actual, ↓Has.Message.EqualTo(2),
                    ""Nope, we can't!"");");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenActualHasNoInnerException()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = 12345;
                Assert.That(actual, ↓Has.InnerException.TypeOf<InvalidCastException>());");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenPropertyConstraintIsUsedWhenNoSuchInActual()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = 12345;
                Assert.That(actual, ↓Has.Property(""Whatever"").EqualTo(1));");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenPropertyConstraintIsUsedWhenNoSuchInDelegateActual()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                int getActual() => 12345;
                Assert.That(getActual, ↓Has.Property(""Whatever"").EqualTo(1));");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void ValidWhenHasCountIsUsedForList()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new [] {1, 2, 3}.Where(i => i > 1).ToList();
                Assert.That(actual, Has.Count.EqualTo(2));",
                additionalUsings: "using System.Linq;");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenHasCountIsUsedForIList()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                IList<int> actual = new [] {1, 2, 3}.Where(i => i > 1).ToList();
                Assert.That(actual, Has.Count.EqualTo(2));",
                additionalUsings: "using System.Linq; using System.Collections.Generic;");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenHasLengthIsUsedForArray()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new[] {1, 2, 3};
                Assert.That(actual, Has.Length.EqualTo(3));");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenHasLengthIsUsedForString()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""ABCDefgh"";
                Assert.That(actual, Has.Length.EqualTo(8));");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenHasMessageIsUsedForException()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new Exception(""SomeMessage"");
                Assert.That(actual, Has.Message.EqualTo(""SomeMessage""));");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenHasInnerExceptionIsUsedForException()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var innerException = new Exception(""inner"");
                var actual = new Exception(""SomeMessage"", innerException);
                Assert.That(actual, Has.InnerException.Not.Null);");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenPropertyFromConstraintIsPresentInActual()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = DateTime.UtcNow;
                Assert.That(actual, Has.Property(""Ticks"").GreaterThan(0));");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenPropertyConstraintIsUsedForDelegateAndPropertyIsPresentInReturnType()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                string getActual() => ""12345"";
                Assert.That(getActual, Has.Property(""Length"").EqualTo(5));");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenPropertyIsUsedForVerificationOfMissingProperty()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                string actual = ""321"";
                Assert.That(actual, Has.No.Property(""Whatever""));");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenCountIsUsedForAbsenceVerification()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                int actual = 321;
                Assert.That(actual, Has.No.Count);");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenPropertyChainIsUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new { Foo = new { Bar = ""Baz"" } };
                Assert.That(actual, Has.Property(""Foo"").With.Property(""Bar"").EqualTo(""Baz""));");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenMultiplePartsCombinedWithCollectionOperatorPrefix()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new[]
                {
                    new
                    {
                        Foo = ""Fuzz"",
                        Bar = ""Buzz""
                    }
                };
                Assert.That(actual, Has.Some.With.Property(""Foo"").And.Property(""Bar""));");

            AnalyzerAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenUsedWithThrowsConstraint()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                string getActual() => throw new System.Exception(""Oops"");
                Assert.That(getActual, Throws.Exception.With.Message.EqualTo(""Oops""));");

            AnalyzerAssert.Valid(analyzer, testCode);
        }
    }
}
