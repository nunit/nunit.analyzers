using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.DiagnosticSuppressors;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.DiagnosticSuppressors
{
    public class DereferencePossiblyNullReferenceSuppressorTests
    {
        private const string ABDefinition = @"
            private static A? GetA(bool create) => create ? new A() : default(A);

            private static B? GetB(bool create) => create ? new B() : default(B);

            private static A? GetAB(string? text) => new A { B = new B { Text = text } };
                
            private class A
            {
                public B? B { get; set; }
            }

            private class B
            {
                public string? Text { get; set; }

                public void Clear() => this.Text = null;

                [System.Diagnostics.Contracts.Pure]
                public string SafeGetText() => this.Text ?? string.Empty;
            }
        ";

        private static readonly DiagnosticSuppressor suppressor = new DereferencePossiblyNullReferenceSuppressor();

        [TestCase("")]
        [TestCase("Assert.NotNull(string.Empty)")]
        [TestCase("Assert.IsNull(s)")]
        [TestCase("Assert.Null(s)")]
        [TestCase("Assert.That(s, Is.Null)")]
        public async Task NoValidAssert(string assert)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                #nullable enable
                [TestCase("""")]
                public void Test(string? s)
                {{
                    {assert};
                    Assert.That(s.Length, Is.GreaterThan(0));
                }}
            ");

            await DiagnosticsSuppressorAnalyzer.EnsureNotSuppressed(suppressor, testCode)
                                               .ConfigureAwait(false);
        }

        [TestCase("Assert.NotNull(s)")]
        [TestCase("Assert.IsNotNull(s)")]
        [TestCase("Assert.That(s, Is.Not.Null)")]
        public async Task WithLocalValidAssert(string assert)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                #nullable enable
                [TestCase("""")]
                public void Test(string? s)
                {{
                    {assert};
                    Assert.That(s.Length, Is.GreaterThan(0));
                }}
            ");

            await DiagnosticsSuppressorAnalyzer.EnsureSuppressed(suppressor,
                DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"], testCode)
                .ConfigureAwait(false);
        }

        [TestCase("Assert.NotNull(s)")]
        [TestCase("Assert.IsNotNull(s)")]
        [TestCase("Assert.That(s, Is.Not.Null)")]
        public async Task WithFieldValidAssert(string assert)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                #nullable enable
                private string? s;
                [Test]
                public void Test()
                {{
                    {assert};
                    Assert.That(s.Length, Is.GreaterThan(0));
                }}

                public void SetS(string? v) => s = v;
            ");

            await DiagnosticsSuppressorAnalyzer.EnsureSuppressed(suppressor,
                DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"], testCode)
                .ConfigureAwait(false);
        }

        [Test]
        public async Task ReturnValue()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                #nullable enable
                [TestCase("""")]
                public void Test(string? s)
                {
                    string result = DoSomething(s);
                }

                private static string DoSomething(string? s)
                {
                    Assert.NotNull(s);
                    return s;
                }
            ");

            await DiagnosticsSuppressorAnalyzer.EnsureSuppressed(suppressor,
                DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8603"], testCode)
                .ConfigureAwait(false);
        }

        [Test]
        public async Task Parameter()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                #nullable enable
                [TestCase("""")]
                public void Test(string? s)
                {
                    Assert.NotNull(s);
                    DoSomething(s);
                }

                private static void DoSomething(string s)
                {
                    _ = s;
                }
            ");

            await DiagnosticsSuppressorAnalyzer.EnsureSuppressed(suppressor,
                DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8604"], testCode)
                .ConfigureAwait(false);
        }

        [Test]
        public async Task NullableCast()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                #nullable enable
                [Test]
                public void Test()
                {
                    int? possibleNull = GetNext();
                    Assert.NotNull(possibleNull);
                    int i = (int)possibleNull;
                    Assert.That(i, Is.EqualTo(1));
                }

                private static int? GetNext() => 1;
            ");

            await DiagnosticsSuppressorAnalyzer.EnsureSuppressed(suppressor,
                DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8629"], testCode)
                .ConfigureAwait(false);
        }

        [Test]
        public async Task WithReassignedAfterAssert()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                #nullable enable
                [TestCase("""")]
                public void Test(string? s)
                {
                    Assert.NotNull(s);
                    s = null;
                    Assert.That(s.Length, Is.GreaterThan(0));
                }
            ");

            await DiagnosticsSuppressorAnalyzer.EnsureNotSuppressed(suppressor, testCode)
                                               .ConfigureAwait(false);
        }

        [Test]
        public async Task WithReassignedFieldAfterAssert()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                #nullable enable
                private string? s;
                [Test]
                public void Test()
                {
                    Assert.NotNull(this.s);
                    this.s = null;
                    Assert.That(this.s.Length, Is.GreaterThan(0));
                }
            ");

            await DiagnosticsSuppressorAnalyzer.EnsureNotSuppressed(suppressor, testCode)
                                               .ConfigureAwait(false);
        }

        [Test]
        public async Task WithPropertyExpression()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                #nullable enable
                [Test]
                public void Test([Values] bool create)
                {{
                    A? a = GetA(true);
                    Assert.That(a, Is.Not.Null);
                    a.B = GetB(create);
                    Assert.That(a.B, Is.Not.Null);
                    a.B.Text = ""?"";
                }}

                {ABDefinition}
            ");

            await DiagnosticsSuppressorAnalyzer.EnsureSuppressed(suppressor,
                DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"], testCode)
                .ConfigureAwait(false);
        }

        [Test]
        public async Task WithComplexExpression()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                #nullable enable
                [Test]
                public void Test([Values] bool create)
                {{
                    A? a = GetA(create);
                    Assert.That(a?.B?.Text, Is.Not.Null);
                    Assert.That(a.B.Text.Length, Is.GreaterThan(0));
                }}

                {ABDefinition}
            ");

            await DiagnosticsSuppressorAnalyzer.EnsureSuppressed(suppressor,
                DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"], testCode)
                .ConfigureAwait(false);
        }

        [Test]
        public async Task WithComplexReassignAfterAssert()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                #nullable enable
                [Test]
                public void Test([Values] bool create)
                {{
                    A a = new A {{ B = new B {{ Text = ""."" }} }};
                    Assert.That(a.B.Text, Is.Not.Null);
                    a.B = new B();
                    Assert.That(a.B.Text.Length, Is.GreaterThan(0));
                }}

                {ABDefinition}
            ");

            await DiagnosticsSuppressorAnalyzer.EnsureNotSuppressed(suppressor, testCode)
                                               .ConfigureAwait(false);
        }

        [Test]
        public async Task InsideAssertMultiple()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                #nullable enable
                [TestCase("""")]
                public void Test(string? s)
                {
                    Assert.Multiple(() =>
                    {
                        Assert.NotNull(s);
                        Assert.That(s.Length, Is.GreaterThan(0));
                    });
                }
            ");

            await DiagnosticsSuppressorAnalyzer.EnsureNotSuppressed(suppressor, testCode)
                                               .ConfigureAwait(false);
        }

        [Test]
        public async Task WithIndexer()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                #nullable enable
                [Test]
                public void Test()
                {{
                    string? directory = GetDirectoryName(""C:/Temp"");

                    Assert.That(directory, Is.Not.Null);
                    Assert.That(directory[0], Is.EqualTo('T'));
                }}

                // System.IO.Path.GetDirectoryName is not annotated in the libraries we are referencing.
                private static string? GetDirectoryName(string path) => System.IO.Path.GetDirectoryName(path);
            ");

            await DiagnosticsSuppressorAnalyzer.EnsureSuppressed(suppressor,
                DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"], testCode)
                .ConfigureAwait(false);
        }
    }
}
