using Gu.Roslyn.Asserts;
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
        [TestCase("ClassicAssert.NotNull(string.Empty)")]
        [TestCase("ClassicAssert.IsNull(s)")]
        [TestCase("ClassicAssert.Null(s)")]
        [TestCase("ClassicAssert.That(s, Is.Null)")]
        public void NoValidAssert(string assert)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                [TestCase("""")]
                public void Test(string? s)
                {{
                    {assert};
                    Assert.That(↓s.Length, Is.GreaterThan(0));
                }}
            ");

            RoslynAssert.NotSuppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"]),
                testCode);
        }

        [TestCase("ClassicAssert.NotNull(s)")]
        [TestCase("ClassicAssert.IsNotNull(s)")]
        [TestCase("Assert.That(s, Is.Not.Null)")]
        [TestCase("Assume.That(s, Is.Not.Null)")]
        public void WithLocalValidAssert(string assert)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                [TestCase("""")]
                public void Test(string? s)
                {{
                    {assert};
                    Assert.That(↓s.Length, Is.GreaterThan(0));
                }}
            ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"]),
                testCode);
        }

        [TestCase("ClassicAssert.NotNull(s)")]
        [TestCase("ClassicAssert.IsNotNull(s)")]
        [TestCase("Assert.That(s, Is.Not.Null)")]
        [TestCase("Assume.That(s, Is.Not.Null)")]
        public void WithFieldValidAssert(string assert)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                private string? s;
                [Test]
                public void Test()
                {{
                    {assert};
                    Assert.That(↓s.Length, Is.GreaterThan(0));
                }}

                public void SetS(string? v) => s = v;
            ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"]),
                testCode);
        }

        [Test]
        public void ReturnValue()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                [TestCase("""")]
                public void Test(string? s)
                {
                    string result = DoSomething(s);
                }

                private static string DoSomething(string? s)
                {
                    ClassicAssert.NotNull(s);
                    return ↓s;
                }
            ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8603"]),
                testCode);
        }

        [Test]
        public void Parameter()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                [TestCase("""")]
                public void Test(string? s)
                {{
                    ClassicAssert.NotNull(s);
                    DoSomething(↓s);
                }}

                private static void DoSomething(string s)
                {{
                    _ = s;
                }}
            ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8604"]),
                testCode);
        }

        [Test]
        public void NullableCast()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                [Test]
                public void Test()
                {{
                    int? possibleNull = GetNext();
                    ClassicAssert.NotNull(possibleNull);
                    int i = ↓(int)possibleNull;
                    AssertOne(i);
                }}

                private static int? GetNext() => 1;

                private static void AssertOne(int i) => Assert.That(i, Is.EqualTo(1));
            ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8629"]),
                testCode);
        }

        [Test]
        public void NullableReferenceCastAssignmentInDeclarationInitializer()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                [Test]
                public void Test()
                {
                    string? possibleNull = GetNext();
                    Assert.That(possibleNull, Is.Not.Null);
                    string s = (string)possibleNull;
                    AssertEmpty(s);
                }

                private static string? GetNext() => string.Empty;

                private static void AssertEmpty(string s) => Assert.That(s, Is.EqualTo(string.Empty));
            ");

            RoslynAssert.Suppressed(suppressor,
                new[]
                {
                    ExpectedDiagnostic.Create("CS8600", 24, 31),
                    ExpectedDiagnostic.Create("CS8604", 25, 32),
                },
                testCode);
        }

        [Test]
        public void NullableReferenceCastAssignmentToLocal()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                [Test]
                public void Test()
                {
                    string s;
                    string? possibleNull = GetNext();
                    Assert.That(possibleNull, Is.Not.Null);
                    s = (string)possibleNull;
                    AssertEmpty(s);
                }

                private static string? GetNext() => string.Empty;

                private static void AssertEmpty(string s) => Assert.That(s, Is.EqualTo(string.Empty));
            ");

            RoslynAssert.Suppressed(suppressor,
                new[]
                {
                    ExpectedDiagnostic.Create("CS8600", 25, 24),
                    ExpectedDiagnostic.Create("CS8604", 26, 32),
                },
                testCode);
        }

        [Test]
        public void NullableReferenceCastArgument()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                [Test]
                public void Test()
                {
                    string? possibleNull = GetNext();
                    Assert.That(possibleNull, Is.Not.Null);
                    AssertEmpty((string)possibleNull);
                }

                private static string? GetNext() => string.Empty;

                private static void AssertEmpty(string s) => Assert.That(s, Is.EqualTo(string.Empty));
            ");

            RoslynAssert.Suppressed(suppressor,
                new[]
                {
                    ExpectedDiagnostic.Create("CS8600", 24, 32),
                    ExpectedDiagnostic.Create("CS8604", 24, 32),
                },
                testCode);
        }

        [Test]
        public void WithReassignedAfterAssert()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                [TestCase("""")]
                public void Test(string? s)
                {{
                    ClassicAssert.NotNull(s);
                    s = null;
                    Assert.That(↓s.Length, Is.GreaterThan(0));
                }}
            ");

            RoslynAssert.NotSuppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"]),
                testCode);
        }

        [Test]
        public void WithReassignedFieldAfterAssert()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                private string? s;
                [Test]
                public void Test()
                {{
                    ClassicAssert.NotNull(this.s);
                    this.s = null;
                    Assert.That(↓this.s.Length, Is.GreaterThan(0));
                }}
            ");

            RoslynAssert.NotSuppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"]),
                testCode);
        }

        [Test]
        public void WithPropertyExpression()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                [Test]
                public void Test([Values] bool create)
                {{
                    A? a = GetA(true);
                    Assert.That(a, Is.Not.Null);
                    ↓a.B = GetB(create);
                    Assert.That(a.B, Is.Not.Null);
                    ↓a.B.Text = ""?"";
                }}

                {ABDefinition}
            ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"]),
                testCode);
        }

        [Test]
        public void WithComplexExpression()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                [Test]
                public void Test([Values] bool create)
                {{
                    A? a = GetA(create);
                    Assert.That(a?.B?.Text, Is.Not.Null);
                    Assert.That(↓a.B.Text.Length, Is.GreaterThan(0));
                }}

                {ABDefinition}
            ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"]),
                testCode);
        }

        [Test]
        public void WithComplexReassignAfterAssert()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                [Test]
                public void Test([Values] bool create)
                {{
                    A a = new A {{ B = new B {{ Text = ""."" }} }};
                    Assert.That(a.B.Text, Is.Not.Null);
                    a.B = new B();
                    Assert.That(↓a.B.Text.Length, Is.GreaterThan(0));
                }}

                {ABDefinition}
            ");

            RoslynAssert.NotSuppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"]),
                testCode);
        }

        [Test]
        public void InsideAssertMultiple()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                [TestCase("""")]
                public void Test(string? s)
                {{
                    Assert.Multiple(() =>
                    {{
                        ClassicAssert.NotNull(s);
                        Assert.That(↓s.Length, Is.GreaterThan(0));
                    }});
                }}
            ");

            RoslynAssert.NotSuppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"]),
                testCode);
        }

#if NUNIT4
        [Test]
        public void InsideAssertMultipleAsync()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                [TestCase("""")]
                public async Task Test(string? s)
                {{
                    await Assert.MultipleAsync(async () =>
                    {{
                        await Task.Yield();
                        ClassicAssert.NotNull(s);
                        Assert.That(↓s.Length, Is.GreaterThan(0));
                    }});
                }}
            ");

            RoslynAssert.NotSuppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"]),
                testCode);
        }
#endif

        [TestCase("ClassicAssert.True(nullable.HasValue)")]
        [TestCase("ClassicAssert.IsTrue(nullable.HasValue)")]
        [TestCase("Assert.That(nullable.HasValue, \"Ensure Value is set\")")]
        [TestCase("Assert.That(nullable.HasValue)")]
        [TestCase("Assert.That(nullable.HasValue, Is.True)")]
        [TestCase("Assert.That(nullable, Is.Not.Null)")]
        [TestCase("Assume.That(nullable, Is.Not.Null)")]
        public void NullableWithValidAssert(string assert)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                [TestCase(42)]
                public void Test(int? nullable)
                {{
                    {assert};
                    Assert.That(↓nullable.Value, Is.EqualTo(42));
                }}
            ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8629"]),
                testCode);
        }

        [TestCase("ClassicAssert.False(nullable.HasValue)")]
        [TestCase("ClassicAssert.IsFalse(nullable.HasValue)")]
        [TestCase("Assert.That(!nullable.HasValue)")]
        [TestCase("Assert.That(nullable.HasValue, Is.False)")]
        [TestCase("Assert.That(nullable, Is.Null)")]
        public void NullableWithInvalidAssert(string assert)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                [TestCase(42)]
                public void Test(int? nullable)
                {{
                    {assert};
                    Assert.That(↓nullable.Value, Is.EqualTo(42));
                }}
            ");

            RoslynAssert.NotSuppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8629"]),
                testCode);
        }

        [Test]
        public void WithIndexer()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                [Test]
                public void Test()
                {{
                    string? directory = GetDirectoryName(""C:/Temp"");

                    Assert.That(directory, Is.Not.Null);
                    Assert.That(↓directory[0], Is.EqualTo('T'));
                }}

                // System.IO.Path.GetDirectoryName is not annotated in the libraries we are referencing.
                private static string? GetDirectoryName(string path) => System.IO.Path.GetDirectoryName(path);
            ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"]),
                testCode);
        }

        [TestCase("var", "Throws")]
        [TestCase("Exception?", "Catch")]
        public void ThrowsLocalDeclaration(string type, string assert)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                [Test]
                public void Test()
                {{
                    {type} ex = Assert.{assert}<Exception>(() => throw new InvalidOperationException());
                    string m = ↓ex.Message;
                }}
            ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"]),
                testCode);
        }

        [TestCase("var", "CatchAsync")]
        [TestCase("Exception?", "ThrowsAsync")]
        public void ThrowsAsyncLocalDeclaration(string type, string assert)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                [Test]
                public void Test()
                {{
                    {type} ex = Assert.{assert}<Exception>(() => Task.Delay(0));
                    string m = ↓ex.Message;
                }}
            ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"]),
                testCode);
        }

        [TestCase("var")]
        [TestCase("Exception?")]
        public void ThrowsLocalDeclarationInsideAssertMultiple(string type)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                [Test]
                public void Test()
                {{
                    Assert.Multiple(() =>
                    {{
                        {type} ex = Assert.Throws<Exception>(() => throw new InvalidOperationException());
                        string m = ↓ex.Message;
                    }});
                }}
            ");

            RoslynAssert.NotSuppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"]),
                testCode);
        }

        [Test]
        public void ThrowsLocalAssignment()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                [Test]
                public void Test()
                {
                    Exception ex;
                    ex = ↓Assert.Throws<Exception>(() => throw new InvalidOperationException());
                }
            ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8600"]),
                testCode);
        }

        [Test]
        public void ThrowsPropertyAssignment()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                private Exception Ex { get; set; } = new NotImplementedException();

                [Test]
                public void Test()
                {
                    Ex = ↓Assert.Throws<Exception>(() => throw new InvalidOperationException());
                }
            ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8601"]),
                testCode);
        }

        [Test]
        public void ThrowsPassedAsArgument()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                private void ShowException(Exception ex) => Console.WriteLine(ex.Message);

                [Test]
                public void Test()
                {
                    ShowException(↓Assert.Throws<Exception>(() => throw new InvalidOperationException()));
                }
            ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8604"]),
                testCode);
        }

        [Test]
        public void ThrowAssignedOutsideAssertMultipleUsedInside()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                [Test]
                public void Test()
                {
                    var e = Assert.Throws<Exception>(() => throw new Exception(""Test""));
                    Assert.Multiple(() =>
                    {
                        Assert.That(↓e.Message, Is.EqualTo(""Test""));
                        Assert.That(e.InnerException, Is.Null);
                        Assert.That(e.StackTrace, Is.Not.Empty);
                    });
                }");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"]),
                testCode);
        }

        [Test]
        public void VariableAssertedOutsideAssertMultipleUsedInside()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                [Test]
                public void Test()
                {
                    var e = GetPossibleException();
                    Assert.That(e, Is.Not.Null);
                    Assert.Multiple(() =>
                    {
                        Assert.That(↓e.Message, Is.EqualTo(""Test""));
                        Assert.That(e.InnerException, Is.Null);
                        Assert.That(e.StackTrace, Is.Not.Empty);
                    });
                }

                private Exception? GetPossibleException() => new Exception();
                ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"]),
                testCode);
        }

        [Test]
        public void VariableAssignedOutsideAssertMultipleUsedInside()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                [Test]
                public void Test()
                {
                    var e = GetPossibleException();
                    Assert.Multiple(() =>
                    {
                        Assert.That(↓e.Message, Is.EqualTo(""Test""));
                        Assert.That(e.InnerException, Is.Null);
                        Assert.That(e.StackTrace, Is.Not.Empty);
                    });
                }

                private Exception? GetPossibleException() => new Exception();
                ");

            RoslynAssert.NotSuppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"]),
                testCode);
        }

        [Test]
        public void VariableAssignedUsedInsideLambda()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                [Test]
                public void Test()
                {
                    var e = GetPossibleException();
                    Assert.That(() => ↓e.Message, Is.EqualTo(""Test""));
                }

                private Exception? GetPossibleException() => new Exception();
                ");

            RoslynAssert.NotSuppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"]),
                testCode);
        }

        [Test]
        public void VariableAssertedUsedInsideLambda()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                [Test]
                public void Test()
                {
                    var e = GetPossibleException();
                    Assert.That(e, Is.Not.Null);
                    Assert.That(() => ↓e.Message, Is.EqualTo(""Test""));
                }

                private Exception? GetPossibleException() => new Exception();
                ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"]),
                testCode);
        }

        [Test]
        public void NestedStatements()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                [Test]
                public void Test()
                {
                    Assert.Multiple(() =>
                    {
                        Assert.DoesNotThrow(() =>
                        {
                            var e = Assert.Throws<Exception>(() => new Exception(""Test""));
                            if (↓e.InnerException is not null)
                            {
                                Assert.That(e.InnerException.Message, Is.EqualTo(""Test""));
                            }
                            else
                            {
                                Assert.That(e.Message, Is.EqualTo(""Test""));
                            }
                        });
                    });
                }");

            RoslynAssert.NotSuppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"]),
                testCode);
        }

        [Test]
        public void TestIssue436()
        {
            // Test is changed from actual issue by replacing the .Select with an [1].
            // The original code would not give null reference issues on the .Select for the .NET Framework
            // because System.Linq is not annotated and therefore the compiler doesn't know null is not allowed.
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                [TestCase(null)]
                public void HasCountShouldNotAffectNullabilitySuppression(List<int>? maybeNull)
                {
                    Assert.That(maybeNull, Is.Not.Null);
                    Assert.Multiple(() =>
                    {
                        Assert.That(maybeNull, Has.Count.EqualTo(2));
                        Assert.That(↓maybeNull[1], Is.EqualTo(1));
                    });
                }
            ", @"using System.Collections.Generic;");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"]),
                testCode);
        }

        [TestCase("string fatal = line2 ?? line1;")]
        [TestCase("string fatal = line2 is not null ? line2 : line1;")]
        [TestCase("string fatal = line2 is null ? line1 : line2;")]
        [TestCase("string fatal = line2 != null ? line2 : line1;")]
        [TestCase("string fatal = line2 == null ? line1 : line2;")]
        [TestCase("string fatal; if (line2 is not null) fatal = line2; else fatal = line1;")]
        [TestCase("string fatal; if (line2 is not null) { fatal = line2; } else { fatal = line1; }")]
        [TestCase("string fatal; if (line2 != null) fatal = line2; else fatal = line1;")]
        [TestCase("string fatal; if (null != line2) { fatal = line2; } else { fatal = line1; }")]
        [TestCase("string fatal; if (line2 is null) fatal = line1; else fatal = line2;")]
        [TestCase("string fatal; if (line2 is null) { fatal = line1; } else { fatal = line2; }")]
        [TestCase("string fatal; if (null == line2) fatal = line1; else fatal = line2;")]
        [TestCase("string fatal; if (line2 == null) { fatal = line1; } else { fatal = line2; }")]
        public void TestIssue503SimpleIdentifier(string expression)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
                [Test]
                public void ConditionalExpressionDetectsNotNull()
                {{
                    string? line1 = ReadLine();
                    string? line2 = ReadLine();
                    Assert.That(line1, Is.Not.Null);
                    {expression}
                }}

                private string? ReadLine() => null;
            ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8600"]),
                testCode);
        }

        [TestCase("string fatal = lines.Line2 ?? lines.Line1;")]
        [TestCase("string fatal = lines.Line2 is not null ? lines.Line2 : lines.Line1;")]
        [TestCase("string fatal = lines.Line2 is null ? lines.Line1 : lines.Line2;")]
        [TestCase("string fatal = lines.Line2 != null ? lines.Line2 : lines.Line1;")]
        [TestCase("string fatal = lines.Line2 == null ? lines.Line1 : lines.Line2;")]
        [TestCase("string fatal = null != lines.Line2 ? lines.Line2 : lines.Line1;")]
        [TestCase("string fatal = null == lines.Line2 ? lines.Line1 : lines.Line2;")]
        public void TestIssue503Properties(string expression)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
                [Test]
                public void ConditionalExpressionDetectsNotNull()
                {{
                    var lines = new Lines(ReadLine(), ReadLine());
                    Assert.That(lines.Line1, Is.Not.Null);
                    {expression}
                }}

                private string? ReadLine() => null;

                private sealed class Lines
                {{
                    public Lines(string? line1, string? line2)
                    {{
                        Line1 = line1;
                        Line2 = line2;
                    }}

                    public string? Line1 {{ get; }}
                    public string? Line2 {{ get; }}
                }}
            ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8600"]),
                testCode);
        }

        [TestCase("class", "CS8634")]
        [TestCase("notnull", "CS8714")]
        public void TestIssue462Suppressed(string constraint, string diagnostic)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
                [Test]
                public void Test()
                {{
                    object? possibleNull = GetNext();
                    object? assertedNotNull = possibleNull;
                    ClassicAssert.NotNull(assertedNotNull);
                    ↓DoNothing(assertedNotNull);
                }}

                private static object? GetNext() => default;

                private static void DoNothing<T>(T s)
                    where T : {constraint}
                {{ }}
            ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors[diagnostic]),
                testCode);
        }

        [TestCase("class", "CS8634")]
        [TestCase("notnull", "CS8714")]
        public void TestIssue462AlsoSuppressed(string constraint, string diagnostic)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
                [Test]
                public void Test()
                {{
                    object? possibleNull = GetNext();
                    object? assertedNotNull = GetNext();
                    ClassicAssert.NotNull(assertedNotNull);
                    ↓DoNothing(assertedNotNull, possibleNull);
                }}

                private static object? GetNext() => default;

                private static void DoNothing<T1, T2>(T1 p1, T2 p2)
                    where T1 : {constraint}
                    where T2 : class?
                {{ }}
            ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors[diagnostic]),
                testCode);
        }

        [TestCase("class", "CS8634")]
        [TestCase("notnull", "CS8714")]
        public void TestIssue462SuppressesMultiple(string constraint, string diagnostic)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
                [Test]
                public void Test()
                {{
                    object? possibleNull = GetNext();
                    object? assertedNotNull = GetNext();
                    ClassicAssert.NotNull(assertedNotNull);
                    ↓DoNothing(assertedNotNull, assertedNotNull);
                }}

                private static object? GetNext() => default;

                private static void DoNothing<T1, T2>(T1 p1, T2 p2)
                    where T1 : {constraint}
                    where T2 : {constraint}
                {{ }}
            ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors[diagnostic]),
                testCode);
        }

        [TestCase("class", "CS8634")]
        [TestCase("notnull", "CS8714")]
        public void TestIssue462SuppressesParams(string constraint, string diagnostic)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
                [Test]
                public void Test()
                {{
                    object? possibleNull = GetNext();
                    object? assertedNotNull = GetNext();
                    ClassicAssert.NotNull(assertedNotNull);
                    ↓DoNothing(assertedNotNull, assertedNotNull);
                }}

                private static object? GetNext() => default;

                private static void DoNothing<T>(params T[] p)
                    where T : {constraint}
                {{ }}
            ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors[diagnostic]),
                testCode);
        }

        [TestCase("class", "CS8634")]
        [TestCase("notnull", "CS8714")]
        public void TestIssue462NotSuppressed(string constraint, string diagnostic)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
                [Test]
                public void Test()
                {{
                    object? possibleNull = GetNext();
                    object? assertedNotNull = possibleNull;
                    ClassicAssert.NotNull(assertedNotNull);
                    ↓DoNothing(possibleNull);
                }}

                private static object? GetNext() => default;

                private static void DoNothing<T>(T s)
                    where T : {constraint}
                {{ }}
            ");

            // The Analyzer doesn't do flow control and therefore doesn't know that
            // possibleNull is the same as assertedNotNull and hence also not null.
            RoslynAssert.NotSuppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors[diagnostic]),
                testCode);
        }

        [TestCase("class", "CS8634")]
        [TestCase("notnull", "CS8714")]
        public void TestIssue462NotSuppressesParams(string constraint, string diagnostic)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
                [Test]
                public void Test()
                {{
                    object? possibleNull = GetNext();
                    object? assertedNotNull = GetNext();
                    ClassicAssert.NotNull(assertedNotNull);
                    ↓DoNothing(assertedNotNull, possibleNull);
                }}

                private static object? GetNext() => default;

                private static void DoNothing<T>(params T[] p)
                    where T : {constraint}
                {{ }}
            ");

            RoslynAssert.NotSuppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors[diagnostic]),
                testCode);
        }

        [Test]
        public void TestIssue587SuppressedInsideAssertMultiple()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                [Test]
                public void Test()
                {
                    Extra? oldExtra = GetResult();
                    Extra? extra = GetResult();

                    Assert.Multiple(() =>
                    {
                        Assert.That(oldExtra, Is.SameAs(extra));
                        Assert.That(extra, Is.Not.Null);
                    });
                    Assert.Multiple(() =>
                    {
                        Assert.That(↓extra.Value, Is.EqualTo(8));
                        Assert.That(extra.Info, Is.EqualTo(""Hi""));
                    });
                }

                private static Extra? GetResult() => new("".NET"", 8);

                private sealed class Extra
                {
                    public Extra(string info, int value)
                    {
                        Info = info;
                        Value = value;
                    }

                    public string Info { get; }
                    public int Value { get; }
                }
            ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8602"]),
                testCode);
        }

        [Test]
        public void TestNullSuppressionOperator()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                [TestCase(default(string))]
                public void Test(string ?possibleNullString)
                {
                    HasString str = new(possibleNullString);

                    string nonNullString = GetStringSuppress(str);
                    Assert.That(nonNullString, Is.Not.Null);
                }

                private static string GetStringSuppress(HasString? str) // argument is nullable
                {
                    Assert.That(str!.Inner, Is.Not.Null);
                    return str.Inner; // warning: possible null reference return
                }

                private sealed class HasString
                {
                    public HasString(string? inner)
                    {                   
                        Inner = inner;
                    }

                    public string? Inner { get; }
                }
            ");

            RoslynAssert.Suppressed(suppressor,
                ExpectedDiagnostic.Create(DereferencePossiblyNullReferenceSuppressor.SuppressionDescriptors["CS8603"]),
                testCode);
        }
    }
}
