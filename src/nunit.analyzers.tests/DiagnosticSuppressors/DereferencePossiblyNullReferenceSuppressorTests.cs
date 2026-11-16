using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.DiagnosticSuppressors;
using NUnit.Analyzers.Tests.Verifiers;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.DiagnosticSuppressors
{
    [TestFixture]
    public sealed class DereferencePossiblyNullReferenceSuppressorTests
    {
        private const string ABDefinition = """
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

        """;

        private static readonly AssertMultiple[] Multiples =
        [
            new AssertMultiple("Multiple", "Assert.Multiple(() =>", ");"),
#if NUNIT4
            new AssertMultiple("EnterMultipleScope", "using (Assert.EnterMultipleScope())", ""),
#endif
        ];

        [TestCase("")]
        [TestCase("ClassicAssert.NotNull(string.Empty)")]
        [TestCase("ClassicAssert.IsNull(s)")]
        [TestCase("ClassicAssert.Null(s)")]
        [TestCase("ClassicAssert.That(s, Is.Null)")]
        public async Task NoValidAssert(string assert)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase("")]
                    public void Test(string? s)
                    {
                        {{assert}};
                        Assert.That({|#0:s|}.Length, Is.GreaterThan(0));
                    }

            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: false);
        }

        [TestCase("ClassicAssert.NotNull(s)")]
        [TestCase("ClassicAssert.IsNotNull(s)")]
        [TestCase("Assert.That(s, Is.Not.Null)")]
        [TestCase("Assume.That(s, Is.Not.Null)")]
        public async Task WithLocalValidAssert(string assert)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase("")]
                    public void Test(string? s)
                    {
                        {{assert}};
                        Assert.That({|#0:s|}.Length, Is.GreaterThan(0));
                    }

            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: true);
        }

        [TestCase("ClassicAssert.NotNull(s)")]
        [TestCase("ClassicAssert.IsNotNull(s)")]
        [TestCase("Assert.That(s, Is.Not.Null)")]
        [TestCase("Assume.That(s, Is.Not.Null)")]
        public async Task WithFieldValidAssert(string assert)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private string? s;
                    [Test]
                    public void Test()
                    {
                        {{assert}};
                        Assert.That({|#0:s|}.Length, Is.GreaterThan(0));
                    }

                    public void SetS(string? v) => s = v;

            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: true);
        }

        [Test]
        public async Task ReturnValue()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [TestCase("")]
                    public void Test(string? s)
                    {
                        string result = DoSomething(s);
                    }

                    private static string DoSomething(string? s)
                    {
                        ClassicAssert.NotNull(s);
                        return {|#0:s|};
                    }

            """);

            await VerifySuppressionAsync(testCode, "CS8603", isSuppressed: true);
        }

        [Test]
        public async Task Parameter()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase("")]
                    public void Test(string? s)
                    {
                        ClassicAssert.NotNull(s);
                        DoSomething({|#0:s|});
                    }

                    private static void DoSomething(string s)
                    {
                        _ = s;
                    }

            """);

            await VerifySuppressionAsync(testCode, "CS8604", isSuppressed: true);
        }

        [Test]
        public async Task NullableCast()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void Test()
                    {
                        int? possibleNull = GetNext();
                        ClassicAssert.NotNull(possibleNull);
                        int i = {|#0:(int)possibleNull|};
                        AssertOne(i);
                    }

                    private static int? GetNext() => 1;

                    private static void AssertOne(int i) => Assert.That(i, Is.EqualTo(1));

            """);

            await VerifySuppressionAsync(testCode, "CS8629", isSuppressed: true);
        }

        [Test]
        public async Task NullableReferenceCastAssignmentInDeclarationInitializer()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public void Test()
                    {
                        string? possibleNull = GetNext();
                        Assert.That(possibleNull, Is.Not.Null);
                        string s = {|#0:(string)possibleNull|};
                        AssertEmpty({|#1:s|});
                    }

                    private static string? GetNext() => string.Empty;

                    private static void AssertEmpty(string s) => Assert.That(s, Is.EqualTo(string.Empty));

            """);

            await VerifySuppressionAsync(testCode,
                DiagnosticResult.CompilerWarning("CS8600").WithLocation(0).WithIsSuppressed(true),
                DiagnosticResult.CompilerWarning("CS8604").WithLocation(1).WithIsSuppressed(true));
        }

        [Test]
        public async Task NullableReferenceCastAssignmentToLocal()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public void Test()
                    {
                        string s;
                        string? possibleNull = GetNext();
                        Assert.That(possibleNull, Is.Not.Null);
                        s = {|#0:(string)possibleNull|};
                        AssertEmpty({|#1:s|});
                    }

                    private static string? GetNext() => string.Empty;

                    private static void AssertEmpty(string s) => Assert.That(s, Is.EqualTo(string.Empty));

            """);

            await VerifySuppressionAsync(testCode,
                DiagnosticResult.CompilerWarning("CS8600").WithLocation(0).WithIsSuppressed(true),
                DiagnosticResult.CompilerWarning("CS8604").WithLocation(1).WithIsSuppressed(true));
        }

        [Test]
        public async Task NullableReferenceCastArgument()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public void Test()
                    {
                        string? possibleNull = GetNext();
                        Assert.That(possibleNull, Is.Not.Null);
                        AssertEmpty({|#0:(string)possibleNull|});
                    }

                    private static string? GetNext() => string.Empty;

                    private static void AssertEmpty(string s) => Assert.That(s, Is.EqualTo(string.Empty));
            """);

            await VerifySuppressionAsync(testCode,
                DiagnosticResult.CompilerWarning("CS8600").WithLocation(0).WithIsSuppressed(true),
                DiagnosticResult.CompilerWarning("CS8604").WithLocation(0).WithIsSuppressed(true));
        }

        [Test]
        public async Task WithReassignedAfterAssert()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase("")]
                    public void Test(string? s)
                    {
                        ClassicAssert.NotNull(s);
                        s = null;
                        Assert.That({|#0:s|}.Length, Is.GreaterThan(0));
                    }

            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: false);
        }

        [Test]
        public async Task WithReassignedFieldAfterAssert()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private string? s;
                    [Test]
                    public void Test()
                    {
                        ClassicAssert.NotNull(this.s);
                        this.s = null;
                        Assert.That({|#0:this.s|}.Length, Is.GreaterThan(0));
                    }

            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: false);
        }

        [Test]
        public async Task WithPropertyExpression()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void Test([Values] bool create)
                    {
                        A? a = GetA(true);
                        Assert.That(a, Is.Not.Null);
                        {|#0:a|}.B = GetB(create);
                        Assert.That(a.B, Is.Not.Null);
                        {|#1:a.B|}.Text = "?";
                    }

                    {{ABDefinition}}

            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: true);
        }

        [Test]
        public async Task WithComplexExpression()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void Test([Values] bool create)
                    {
                        A? a = GetA(create);
                        Assert.That(a?.B?.Text, Is.Not.Null);
                        Assert.That({|#0:{|#1:{|#2:a|}.B|}.Text|}.Length, Is.GreaterThan(0));
                    }

                    {{ABDefinition}}

            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: true);
        }

        [Test]
        public async Task WithComplexReassignAfterAssert()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void Test([Values] bool create)
                    {
                        A a = new A { B = new B { Text = "." } };
                        Assert.That(a.B.Text, Is.Not.Null);
                        a.B = new B();
                        Assert.That({|#0:a.B.Text|}.Length, Is.GreaterThan(0));
                    }

                    {{ABDefinition}}

            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: false);
        }

        [TestCaseSource(nameof(Multiples))]
        public async Task InsideAssertMultiple(AssertMultiple multiple)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [TestCase("")]
                public void Test(string? s)
                {
                    {{multiple.Start}}
                    {
                        ClassicAssert.NotNull(s);
                        Assert.That({|#0:s|}.Length, Is.GreaterThan(0));
                    }{{multiple.End}}
                }
            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: false);
        }

#if NUNIT4
        [Test]
        public async Task InsideAssertMultipleAsync()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase("")]
                    public async Task Test(string? s)
                    {
                        await Assert.MultipleAsync(async () =>
                        {
                            await Task.Yield();
                            ClassicAssert.NotNull(s);
                            Assert.That({|#0:s|}.Length, Is.GreaterThan(0));
                        });
                    }

            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: false);
        }

        [Test]
        public async Task InsideEnterMultipleScopeAsync()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase("")]
                    public async Task Test(string? s)
                    {
                        using (Assert.EnterMultipleScope())
                        {
                            await Task.Yield();
                            ClassicAssert.NotNull(s);
                            Assert.That({|#0:s|}.Length, Is.GreaterThan(0));
                        }
                    }

            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: false);
        }
#endif

        [TestCase("ClassicAssert.True(nullable.HasValue)")]
        [TestCase("ClassicAssert.IsTrue(nullable.HasValue)")]
        [TestCase("Assert.That(nullable.HasValue, \"Ensure Value is set\")")]
        [TestCase("Assert.That(nullable.HasValue)")]
        [TestCase("Assert.That(nullable.HasValue, Is.True)")]
        [TestCase("Assert.That(nullable, Is.Not.Null)")]
        [TestCase("Assume.That(nullable, Is.Not.Null)")]
        public async Task NullableWithValidAssert(string assert)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase(42)]
                    public void Test(int? nullable)
                    {
                        {{assert}};
                        Assert.That({|#0:nullable|}.Value, Is.EqualTo(42));
                    }

            """);

            await VerifySuppressionAsync(testCode, "CS8629", isSuppressed: true);
        }

        [TestCase("ClassicAssert.False(nullable.HasValue)")]
        [TestCase("ClassicAssert.IsFalse(nullable.HasValue)")]
        [TestCase("Assert.That(!nullable.HasValue)")]
        [TestCase("Assert.That(nullable.HasValue, Is.False)")]
        [TestCase("Assert.That(nullable, Is.Null)")]
        public async Task NullableWithInvalidAssert(string assert)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase(42)]
                    public void Test(int? nullable)
                    {
                        {{assert}};
                        Assert.That({|#0:nullable|}.Value, Is.EqualTo(42));
                    }

            """);

            await VerifySuppressionAsync(testCode, "CS8629", isSuppressed: false);
        }

        [Test]
        public async Task WithIndexer()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void Test()
                    {
                        string? directory = GetDirectoryName("C:/Temp");

                        Assert.That(directory, Is.Not.Null);
                        Assert.That({|#0:directory|}[0], Is.EqualTo('T'));
                    }

                    // System.IO.Path.GetDirectoryName is not annotated in the libraries we are referencing.
                    private static string? GetDirectoryName(string path) => System.IO.Path.GetDirectoryName(path);

            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: true);
        }

        [TestCase("var", "Throws")]
        [TestCase("Exception?", "Catch")]
        public async Task ThrowsLocalDeclaration(string type, string assert)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void Test()
                    {
                        {{type}} ex = Assert.{{assert}}<Exception>(() => throw new InvalidOperationException());
                        string m = {|#0:ex|}.Message;
                    }

            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: true);
        }

        [TestCase("var", "CatchAsync")]
        [TestCase("Exception?", "ThrowsAsync")]
        public async Task ThrowsAsyncLocalDeclaration(string type, string assert)
        {
#if NUNIT5
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public async Task Test()
                    {
                        {{type}} ex = await Assert.{{assert}}<Exception>(() => Task.Delay(0));
                        string m = {|#0:ex|}.Message;
                    }

            """);
#else
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void Test()
                    {
                        {{type}} ex = Assert.{{assert}}<Exception>(() => Task.Delay(0));
                        string m = {|#1:ex|}.Message;
                    }

            """);
#endif

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: true);
        }

        [Test]
        public async Task ThrowsLocalDeclarationInsideAssertMultiple(
            [Values("var", "Exception?")] string type,
            [ValueSource(nameof(Multiples))] AssertMultiple multiple)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [Test]
                public void Test()
                {
                    {{multiple.Start}}
                    {
                        {{type}} ex = Assert.Throws<Exception>(() => throw new InvalidOperationException());
                        string m = {|#0:ex|}.Message;
                    }{{multiple.End}}
                }
            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: false);
        }

        [Test]
        public async Task ThrowsLocalAssignment()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public void Test()
                    {
                        Exception ex;
                        ex = {|#0:Assert.Throws<Exception>(() => throw new InvalidOperationException())|};
                    }

            """);

            await VerifySuppressionAsync(testCode, "CS8600", isSuppressed: true);
        }

        [Test]
        public async Task ThrowsPropertyAssignment()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    private Exception Ex { get; set; } = new NotImplementedException();

                    [Test]
                    public void Test()
                    {
                        Ex = {|#0:Assert.Throws<Exception>(() => throw new InvalidOperationException())|};
                    }

            """);

            await VerifySuppressionAsync(testCode, "CS8601", isSuppressed: true);
        }

        [Test]
        public async Task ThrowsPassedAsArgument()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    private void ShowException(Exception ex) => Console.WriteLine(ex.Message);

                    [Test]
                    public void Test()
                    {
                        ShowException({|#0:Assert.Throws<Exception>(() => throw new InvalidOperationException())|});
                    }

            """);

            await VerifySuppressionAsync(testCode, "CS8604", isSuppressed: true);
        }

        [TestCaseSource(nameof(Multiples))]
        public async Task ThrowAssignedOutsideAssertMultipleUsedInside(AssertMultiple multiple)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [Test]
                public void Test()
                {
                    var e = Assert.Throws<Exception>(() => throw new Exception("Test"));
                    {{multiple.Start}}
                    {
                        Assert.That({|#0:e|}.Message, Is.EqualTo("Test"));
                        Assert.That(e.InnerException, Is.Null);
                        Assert.That(e.StackTrace, Is.Not.Empty);
                    }{{multiple.End}}
                }
            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: true);
        }

        [TestCaseSource(nameof(Multiples))]
        public async Task VariableAssertedOutsideAssertMultipleUsedInside(AssertMultiple multiple)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [Test]
                public void Test()
                {
                    var e = GetPossibleException();
                    Assert.That(e, Is.Not.Null);
                    {{multiple.Start}}
                    {
                        Assert.That({|#0:e|}.Message, Is.EqualTo("Test"));
                        Assert.That(e.InnerException, Is.Null);
                        Assert.That(e.StackTrace, Is.Not.Empty);
                    }{{multiple.End}}
                }

                private Exception? GetPossibleException() => new Exception();
            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: true);
        }

        [TestCaseSource(nameof(Multiples))]
        public async Task VariableAssertedOutsideAssertMultipleUsedAfter(AssertMultiple multiple)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [Test]
                public void Test()
                {
                    var e = GetPossibleException();
                    Assert.That(e, Is.Not.Null);
                    {{multiple.Start}}
                    {
                    }{{multiple.End}}

                    Assert.That({|#0:e|}.InnerException, Is.Null);
                }

                private Exception? GetPossibleException() => new Exception();
            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: true);
        }

        [TestCaseSource(nameof(Multiples))]
        public async Task VariableAssertedInsideAssertMultipleUsedAfter(AssertMultiple multiple)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [Test]
                public void Test()
                {
                    var e = GetPossibleException();
                    {{multiple.Start}}
                    {
                        Assert.That(e, Is.Not.Null);
                    }{{multiple.End}}

                    Assert.That({|#0:e|}.InnerException, Is.Null);
                }

                private Exception? GetPossibleException() => new Exception();
            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: true);
        }

        [TestCaseSource(nameof(Multiples))]
        public async Task VariableAssignedOutsideAssertMultipleUsedInside(AssertMultiple multiple)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [Test]
                public void Test()
                {
                    var e = GetPossibleException();
                    {{multiple.Start}}
                    {
                        Assert.That({|#0:e|}.Message, Is.EqualTo("Test"));
                        Assert.That(e.InnerException, Is.Null);
                        Assert.That(e.StackTrace, Is.Not.Empty);
                    }{{multiple.End}}
                }

                private Exception? GetPossibleException() => new Exception();
            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: false);
        }

#pragma warning disable SA1201 // Elements should appear in the correct order
        private static readonly TestCaseData[] UnsupportedConditionalStatementsTestCases =
#pragma warning restore SA1201 // Elements should appear in the correct order
        [
            new TestCaseData("if (DateTime.UtcNow.Hour < 12)").SetArgDisplayNames("if"),
            new TestCaseData("for (int i = 0; i < 5; i++)").SetArgDisplayNames("for"),
            new TestCaseData("int i = 0; while (i++ < 5)").SetArgDisplayNames("while"),
        ];

        [TestCaseSource(nameof(UnsupportedConditionalStatementsTestCases))]
        public async Task VariableAssertedInsideUnsupportedConditionalStatement(string statement)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [Test]
                public void Test()
                {
                    var e = GetPossibleException();
                    {{statement}}
                    {
                        Assert.That(e, Is.Not.Null);
                    }

                    Assert.That({|#0:e|}.Message, Is.EqualTo("Test"));
                }

                private Exception? GetPossibleException() => new Exception();
            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: false);
        }

        [Test]
        public async Task VariableAssertedInsideUnsupportedConditionalStatement()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [Test]
                public void Test()
                {
                    var e = GetPossibleException();
                    switch (DateTime.UtcNow.Hour)
                    {
                        case 1:
                        case 2:
                        case 3:
                            break;
                        default:
                            Assert.That(e, Is.Not.Null);
                            break;
                    }

                    Assert.That({|#0:e|}.Message, Is.EqualTo("Test"));
                }

                private Exception? GetPossibleException() => new Exception();
            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: false);
        }

        [Test]
        public async Task VariableAssertedInsideTryStatement()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [Test]
                public void Test()
                {
                    var e = GetPossibleException();
                    try
                    {
                        MethodThatMightThrowAnException();
                        Assert.That(e, Is.Not.Null);
                    }
                    catch
                    {
                        Assert.Fail("Should not throw");
                    }

                    Assert.That({|#0:e|}.Message, Is.EqualTo("Test"));
                }

                private Exception? GetPossibleException() => new Exception();

                private void MethodThatMightThrowAnException()
                {
                    if (DateTime.UtcNow.Hour < 6)
                    {
                        throw new Exception("Test");
                    }
                }
            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: false);
        }

        [Test]
        public async Task VariableAssertedInsideTryFinallyStatement()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [Test]
                public void Test()
                {
                    var e = GetPossibleException();
                    try
                    {
                        Assert.That(DateTime.UtcNow.Hour, Is.GreaterThan(6));
                    }
                    catch
                    {
                        Assert.Fail("Should not throw");
                    }
                    finally
                    {
                        Assert.That(e, Is.Not.Null);
                    }

                    Assert.That({|#0:e|}.Message, Is.EqualTo("Test"));
                }

                private Exception? GetPossibleException() => new Exception();
            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: true);
        }

        [Test]
        public async Task VariableAssertedInsideLockStatement()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [Test]
                public void Test()
                {
                    var e = GetPossibleException();
                    lock (this)
                    {
                        Assert.That(e, Is.Not.Null);
                    }

                    Assert.That({|#0:e|}.Message, Is.EqualTo("Test"));
                }

                private Exception? GetPossibleException() => new Exception();
            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: true);
        }

        [Test]
        public async Task VariableResetInsideLockStatement()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [Test]
                public void Test()
                {
                    var e = GetPossibleException();
                    Assert.That(e, Is.Not.Null);
                    lock (this)
                    {
                        e = null;
                    }

                    Assert.That({|#0:e|}.Message, Is.EqualTo("Test"));
                }

                private Exception? GetPossibleException() => new Exception();
            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: false);
        }

        [Test]
        public async Task VariableAssignedUsedInsideLambda()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public void Test()
                    {
                        var e = GetPossibleException();
                        Assert.That(() => {|#0:e|}.Message, Is.EqualTo("Test"));
                    }

                    private Exception? GetPossibleException() => new Exception();

            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: false);
        }

        [Test]
        public async Task VariableAssertedUsedInsideLambda()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public void Test()
                    {
                        var e = GetPossibleException();
                        Assert.That(e, Is.Not.Null);
                        Assert.That(() => {|#0:e|}.Message, Is.EqualTo("Test"));
                    }

                    private Exception? GetPossibleException() => new Exception();

            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: true);
        }

        [TestCaseSource(nameof(Multiples))]
        public async Task NestedStatements(AssertMultiple multiple)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [Test]
                public void Test()
                {
                    {{multiple.Start}}
                    {
                        Assert.DoesNotThrow(() =>
                        {
                            var e = Assert.Throws<Exception>(() => new Exception("Test"));
                            if ({|#0:e|}.InnerException is not null)
                            {
                                Assert.That(e.InnerException.Message, Is.EqualTo("Test"));
                            }
                            else
                            {
                                Assert.That(e.Message, Is.EqualTo("Test"));
                            }
                        });
                    }{{multiple.End}}
                }
            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: false);
        }

        [TestCaseSource(nameof(Multiples))]
        public async Task TestIssue436(AssertMultiple multiple)
        {
            // Test is changed from actual issue by replacing the .Select with an [1].
            // The original code would not give null reference issues on the .Select for the .NET Framework
            // because System.Linq is not annotated and therefore the compiler doesn't know null is not allowed.
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [TestCase(null)]
                public void HasCountShouldNotAffectNullabilitySuppression(List<int>? maybeNull)
                {
                    Assert.That(maybeNull, Is.Not.Null);
                    {{multiple.Start}}
                    {
                        Assert.That(maybeNull, Has.Count.EqualTo(2));
                        Assert.That({|#0:maybeNull|}[1], Is.EqualTo(1));
                    }{{multiple.End}}
                }
            """, """using System.Collections.Generic;""");

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: true);
        }

        [TestCase("string fatal = {|#0:line2 ?? line1|};")]
        [TestCase("string fatal = {|#1:line2 is not null ? line2 : line1|};")]
        [TestCase("string fatal = {|#2:line2 is null ? line1 : line2|};")]
        [TestCase("string fatal = {|#3:line2 != null ? line2 : line1|};")]
        [TestCase("string fatal = {|#4:line2 == null ? line1 : line2|};")]
        [TestCase("string fatal; if (line2 is not null) fatal = line2; else fatal = {|#5:line1|};")]
        [TestCase("string fatal; if (line2 is not null) { fatal = line2; } else { fatal = {|#6:line1|}; }")]
        [TestCase("string fatal; if (line2 != null) fatal = line2; else fatal = {|#7:line1|};")]
        [TestCase("string fatal; if (null != line2) { fatal = line2; } else { fatal = {|#8:line1|}; }")]
        [TestCase("string fatal; if (line2 is null) fatal = {|#9:line1|}; else fatal = line2;")]
        [TestCase("string fatal; if (line2 is null) { fatal = {|#10:line1|}; } else { fatal = line2; }")]
        [TestCase("string fatal; if (null == line2) fatal = {|#11:line1|}; else fatal = line2;")]
        [TestCase("string fatal; if (line2 == null) { fatal = {|#12:line1|}; } else { fatal = line2; }")]
        public async Task TestIssue503SimpleIdentifier(string expression)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void ConditionalExpressionDetectsNotNull()
                    {
                        string? line1 = ReadLine();
                        string? line2 = ReadLine();
                        Assert.That(line1, Is.Not.Null);
                        {{expression}}
                    }

                    private string? ReadLine() => null;

            """);

            await VerifySuppressionAsync(testCode, "CS8600", isSuppressed: true);
        }

        [TestCase("string fatal = {|#0:lines.Line2 ?? lines.Line1|};")]
        [TestCase("string fatal = {|#1:lines.Line2 is not null ? lines.Line2 : lines.Line1|};")]
        [TestCase("string fatal = {|#2:lines.Line2 is null ? lines.Line1 : lines.Line2|};")]
        [TestCase("string fatal = {|#3:lines.Line2 != null ? lines.Line2 : lines.Line1|};")]
        [TestCase("string fatal = {|#4:lines.Line2 == null ? lines.Line1 : lines.Line2|};")]
        [TestCase("string fatal = {|#5:null != lines.Line2 ? lines.Line2 : lines.Line1|};")]
        [TestCase("string fatal = {|#6:null == lines.Line2 ? lines.Line1 : lines.Line2|};")]
        public async Task TestIssue503Properties(string expression)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void ConditionalExpressionDetectsNotNull()
                    {
                        var lines = new Lines(ReadLine(), ReadLine());
                        Assert.That(lines.Line1, Is.Not.Null);
                        {{expression}}
                    }

                    private string? ReadLine() => null;

                    private sealed class Lines
                    {
                        public Lines(string? line1, string? line2)
                        {
                            Line1 = line1;
                            Line2 = line2;
                        }

                        public string? Line1 { get; }
                        public string? Line2 { get; }
                    }

            """);

            await VerifySuppressionAsync(testCode, "CS8600", isSuppressed: true);
        }

        [TestCase("class", "CS8634")]
        [TestCase("notnull", "CS8714")]
        public async Task TestIssue462Suppressed(string constraint, string diagnostic)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void Test()
                    {
                        object? possibleNull = GetNext();
                        object? assertedNotNull = possibleNull;
                        ClassicAssert.NotNull(assertedNotNull);
                        {|#0:DoNothing|}(assertedNotNull);
                    }

                    private static object? GetNext() => default;

                    private static void DoNothing<T>(T s)
                        where T : {{constraint}}
                    { }

            """);

            await VerifySuppressionAsync(testCode, diagnostic, isSuppressed: true);
        }

        [TestCase("class", "CS8634")]
        [TestCase("notnull", "CS8714")]
        public async Task TestIssue462AlsoSuppressed(string constraint, string diagnostic)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void Test()
                    {
                        object? possibleNull = GetNext();
                        object? assertedNotNull = GetNext();
                        ClassicAssert.NotNull(assertedNotNull);
                        {|#0:DoNothing|}(assertedNotNull, possibleNull);
                    }

                    private static object? GetNext() => default;

                    private static void DoNothing<T1, T2>(T1 p1, T2 p2)
                        where T1 : {{constraint}}
                        where T2 : class?
                    { }

            """);

            await VerifySuppressionAsync(testCode, diagnostic, isSuppressed: true);
        }

        [TestCase("class", "CS8634")]
        [TestCase("notnull", "CS8714")]
        public async Task TestIssue462SuppressesMultiple(string constraint, string diagnostic)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void Test()
                    {
                        object? possibleNull = GetNext();
                        object? assertedNotNull = GetNext();
                        ClassicAssert.NotNull(assertedNotNull);
                        {|#0:DoNothing|}(assertedNotNull, assertedNotNull);
                    }

                    private static object? GetNext() => default;

                    private static void DoNothing<T1, T2>(T1 p1, T2 p2)
                        where T1 : {{constraint}}
                        where T2 : {{constraint}}
                    { }

            """);

            await VerifySuppressionAsync(testCode, diagnostic, isSuppressed: true, diagnosticsPerMarker: 2);
        }

        [TestCase("class", "CS8634")]
        [TestCase("notnull", "CS8714")]
        public async Task TestIssue462SuppressesParams(string constraint, string diagnostic)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void Test()
                    {
                        object? possibleNull = GetNext();
                        object? assertedNotNull = GetNext();
                        ClassicAssert.NotNull(assertedNotNull);
                        {|#0:DoNothing|}(assertedNotNull, assertedNotNull);
                    }

                    private static object? GetNext() => default;

                    private static void DoNothing<T>(params T[] p)
                        where T : {{constraint}}
                    { }

            """);

            await VerifySuppressionAsync(testCode, diagnostic, isSuppressed: true);
        }

        [TestCase("class", "CS8634")]
        [TestCase("notnull", "CS8714")]
        public async Task TestIssue462NotSuppressed(string constraint, string diagnostic)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void Test()
                    {
                        object? possibleNull = GetNext();
                        object? assertedNotNull = possibleNull;
                        ClassicAssert.NotNull(assertedNotNull);
                        {|#0:DoNothing|}(possibleNull);
                    }

                    private static object? GetNext() => default;

                    private static void DoNothing<T>(T s)
                        where T : {{constraint}}
                    { }

            """);

            // The Analyzer doesn't do flow control and therefore doesn't know that
            // possibleNull is the same as assertedNotNull and hence also not null.
            await VerifySuppressionAsync(testCode, diagnostic, isSuppressed: false);
        }

        [TestCase("class", "CS8634")]
        [TestCase("notnull", "CS8714")]
        public async Task TestIssue462NotSuppressesParams(string constraint, string diagnostic)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void Test()
                    {
                        object? possibleNull = GetNext();
                        object? assertedNotNull = GetNext();
                        ClassicAssert.NotNull(assertedNotNull);
                        {|#0:DoNothing|}(assertedNotNull, possibleNull);
                    }

                    private static object? GetNext() => default;

                    private static void DoNothing<T>(params T[] p)
                        where T : {{constraint}}
                    { }

            """);

            await VerifySuppressionAsync(testCode, diagnostic, isSuppressed: false);
        }

        [TestCaseSource(nameof(Multiples))]
        public async Task TestIssue587SuppressedInsideAssertMultiple(AssertMultiple multiple)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                [Test]
                public void Test()
                {
                    Extra? oldExtra = GetResult();
                    Extra? extra = GetResult();

                    {{multiple.Start}}
                    {
                        Assert.That(oldExtra, Is.SameAs(extra));
                        Assert.That(extra, Is.Not.Null);
                    }{{multiple.End}}
                    {{multiple.Start}}
                    {
                        Assert.That({|#0:extra|}.Value, Is.EqualTo(8));
                        Assert.That(extra.Info, Is.EqualTo("Hi"));
                    }{{multiple.End}}
                }

                private static Extra? GetResult() => new(".NET", 8);

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
            """);

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: true);
        }

        [Test]
        public async Task TestNullSuppressionOperator()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
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
                        return {|#0:str.Inner|}; // warning: possible null reference return
                    }

                    private sealed class HasString
                    {
                        public HasString(string? inner)
                        {
                            Inner = inner;
                        }

                        public string? Inner { get; }
                    }

            """);

            await VerifySuppressionAsync(testCode, "CS8603", isSuppressed: true);
        }

        [Test]
        public async Task TestIssue739FullyQualified()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public void T()
                    {
                        object? x = null;
                        NUnit.Framework.Assert.That(x, Is.Not.Null);
                        M({|#0:x|});
                    }

                    void M(object o)
                    {
                    }

            """);

            await VerifySuppressionAsync(testCode, "CS8604", isSuppressed: true);
        }

        [Test]
        public async Task VariableAssertedNotNullInsideUsingBlock()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public void VariableAssertedNotNullInsideUsingBlock()
                    {
                        MemoryStream? stream = null;
                        using (stream = GetStream())
                        {
                            Assert.That(stream, Is.Not.Null);
                        }

                        Assert.That(() => {|#0:stream|}.Length, Throws.Exception);

                        static MemoryStream? GetStream() => null;
                    }

            """, "using System.IO;");

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: true);
        }

        [Test]
        public async Task VariableAssertedNotNullInsideUsingStatement()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public void VariableAssertedNotNullInsideUsingStatement()
                    {
                        MemoryStream? stream = null;
                        using (stream = GetStream())
                            Assert.That(stream, Is.Not.Null);

                        Assert.That(() => {|#0:stream|}.Length, Throws.Exception);

                        static MemoryStream? GetStream() => null;
                    }

            """, "using System.IO;");

            await VerifySuppressionAsync(testCode, "CS8602", isSuppressed: true);
        }

        // Expects the compiler warning at every {|#n:|} marker in the test code.
        // Some expressions produce the same warning more than once at the same start position.
        private static Task VerifySuppressionAsync(string testCode, string diagnosticId, bool isSuppressed, int diagnosticsPerMarker = 1)
        {
            DiagnosticResult[] expected = Regex.Matches(testCode, @"\{\|#(\d+):")
                .Cast<Match>()
                .SelectMany(m => Enumerable.Repeat(
                    DiagnosticResult.CompilerWarning(diagnosticId)
                        .WithLocation(int.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture))
                        .WithIsSuppressed(isSuppressed),
                    diagnosticsPerMarker))
                .ToArray();

            return VerifySuppressionAsync(testCode, expected);
        }

        private static async Task VerifySuppressionAsync(string testCode, params DiagnosticResult[] expected)
        {
            var test = new NUnitCSharpAnalyzerTest<DereferencePossiblyNullReferenceSuppressor>
            {
                TestCode = testCode,
                CompilerDiagnostics = CompilerDiagnostics.Warnings,
            };

            test.ExpectedDiagnostics.AddRange(expected);
            await test.RunAsync(TestSetup.CancellationToken);
        }

        public record struct AssertMultiple(string Name, string Start, string End)
        {
            public override readonly string ToString() => this.Name;
        }
    }
}
