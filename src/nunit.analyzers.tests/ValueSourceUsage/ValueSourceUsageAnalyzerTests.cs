using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.ValueSourceUsage.ValueSourceUsageAnalyzer, NUnit.Analyzers.SourceCommon.UseNameofFix>;

namespace NUnit.Analyzers.Tests.ValueSourceUsage
{
    [TestFixture]
    public sealed class ValueSourceUsageAnalyzerTests
    {
        private static readonly DiagnosticResult expectedDiagnosticCodeFix = Verify.Diagnostic(AnalyzerIdentifiers.ValueSourceStringUsage);

        [Test]
        public async Task AnalyzeWhenNameOfSameClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class AnalyzeWhenNameOfSameClass
                {
                    static string[] Tests = new[] { "Data" };

                    [Test]
                    public void Test([ValueSource(nameof(Tests))] string input)
                    {
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenNameOfSameClassNotStatic()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class AnalyzeWhenNameOfSameClassNotStatic
                {
                    string[] Tests = new[] { "Data" };

                    [Test]
                    public void Test([ValueSource({|#0:nameof(Tests)|})] string input)
                    {
                    }
                }
            """);
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ValueSourceIsNotStatic)
                .WithMessage("The specified source 'Tests' is not static");
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task NoNameOfSuggestionWhenStringLiteralMissingMember()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class NoWarningWhenStringLiteralMissingMember
                {
                    [Test]
                    public void Test([ValueSource({|#0:"Missing"|})] string input)
                    {
                    }
                }
            """);

            // Only the missing source is reported, not the suggestion to use nameof.
            await Verify.VerifyAnalyzerAsync(testCode, Verify.Diagnostic(AnalyzerIdentifiers.ValueSourceIsMissing).WithLocation(0).WithArguments("Missing"));
        }

        [TestCase("private static readonly TestCaseData[] TestCases = new TestCaseData[0];")]
        [TestCase("private static TestCaseData[] TestCases => new TestCaseData[0];")]
        [TestCase("private static TestCaseData[] TestCases() => new TestCaseData[0];")]
        public async Task FixWhenStringLiteral(string testCaseMember)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public class AnalyzeWhenStringConstant
                {
                    {{testCaseMember}}

                    [Test]
                    public void Test([ValueSource({|#0:"TestCases"|})] string input)
                    {
                    }
                }
            """);

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public class AnalyzeWhenStringConstant
                {
                    {{testCaseMember}}

                    [Test]
                    public void Test([ValueSource(nameof(TestCases))] string input)
                    {
                    }
                }
            """);

            var message = "Consider using nameof(TestCases) instead of \"TestCases\"";
            await Verify.VerifyCodeFixAsync(testCode, expectedDiagnosticCodeFix.WithMessage(message).WithLocation(0), fixedCode);
        }

        [Test]
        public async Task FixWhenMultipleUnrelatedAttributes()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class AnalyzeWhenMultipleUnrelatedAttributes
                {
                    private static readonly TestCaseData[] TestCases = new TestCaseData[0];

                    [Test]
                    public void UnrelatedTest()
                    {
                    }

                    [Test]
                    public void Test([ValueSource({|#0:"TestCases"|})] string input)
                    {
                    }
                }
            """);

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class AnalyzeWhenMultipleUnrelatedAttributes
                {
                    private static readonly TestCaseData[] TestCases = new TestCaseData[0];

                    [Test]
                    public void UnrelatedTest()
                    {
                    }

                    [Test]
                    public void Test([ValueSource(nameof(TestCases))] string input)
                    {
                    }
                }
            """);

            var message = "Consider using nameof(TestCases) instead of \"TestCases\"";
            await Verify.VerifyCodeFixAsync(testCode, expectedDiagnosticCodeFix.WithMessage(message).WithLocation(0), fixedCode);
        }

        [Test]
        public async Task AnalyzeWhenMethodExpectsParameters()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class AnalyzeWhenMethodExpectsParameters
                {
                    [Test]
                    public void ShortName([ValueSource({|#0:nameof(TestData)|})] int number)
                    {
                        Assert.That(number, Is.GreaterThanOrEqualTo(0));
                    }

                    static IEnumerable<int> TestData(string dummy)
                    {
                        yield return 1;
                        yield return 2;
                        yield return 3;
                    }
                }
            """, additionalUsings: "using System.Collections.Generic;");

            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ValueSourceMethodExpectParameters)
                .WithMessage("The ValueSource cannot supply parameters, but the target method expects '1' parameter(s)");
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenMethodExpectsParametersInAnotherClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class AnalyzeWhenMethodExpectsParametersInAnotherClass
                {
                    [Test]
                    public void ShortName([ValueSource(typeof(AnotherClass), {|#0:nameof(AnotherClass.TestData)|})] int number)
                    {
                        Assert.That(number, Is.GreaterThanOrEqualTo(0));
                    }
                }

                public class AnotherClass
                {
                    public static IEnumerable<int> TestData(string dummy, int anotherDummy)
                    {
                        yield return 1;
                        yield return 2;
                        yield return 3;
                    }
                }
            """, additionalUsings: "using System.Collections.Generic;");

            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ValueSourceMethodExpectParameters)
                .WithMessage("The ValueSource cannot supply parameters, but the target method expects '2' parameter(s)");
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [TestCase("private static readonly TestCaseData[] TestCases = new TestCaseData[0];")]
        [TestCase("private static TestCaseData[] TestCases => new TestCaseData[0];")]
        [TestCase("private static TestCaseData[] TestCases() => new TestCaseData[0];")]
        [TestCase("private static Task<TestCaseData[]> TestCases() => Task.FromResult(new TestCaseData[0]);")]
        [TestCase("private static async IAsyncEnumerable<int> TestCases() { foreach (var value in new[] { 0 }) { yield return value; await Task.Yield(); } }", "using System.Collections.Generic;")]
        public async Task AnalyzeWhenSourceDoesProvideIEnumerable(string testCaseMember, string? additionalUsings = null)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public class AnalyzeWhenSourceDoesProvideIEnumerable
                {
                    {{testCaseMember}}

                    [Test]
                    public void Test([ValueSource(nameof(TestCases))] int number)
                    {
                    }
                }
            """, additionalUsings);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("private static readonly object? TestCases = null;", "object?")]
        [TestCase("private static object? TestCases => null;", "object?")]
        [TestCase("private static object? TestCases() => null;", "object?")]
        [TestCase("private static readonly int TestCases = 1;", "int")]
        [TestCase("private static readonly Task<int[]> TestCases = Task.FromResult(new int[] { 1 });", "System.Threading.Tasks.Task<int[]>")]
        [TestCase("private static int TestCases => 1;", "int")]
        [TestCase("private static Task<int[]> TestCases => Task.FromResult(new int[] { 1 });", "System.Threading.Tasks.Task<int[]>")]
        [TestCase("private static int TestCases() => 1;", "int")]
        [TestCase("private static Task<int> TestCases() => Task.FromResult(1);", "int")]
        [TestCase("private static readonly PlatformID TestCases = PlatformID.Unix;", "System.PlatformID")]
        [TestCase("private static PlatformID TestCases => PlatformID.Unix;", "System.PlatformID")]
        [TestCase("private static PlatformID TestCases() => PlatformID.Unix;", "System.PlatformID")]
        public async Task AnalyzeWhenSourceDoesNotProvideIEnumerable(string testCaseMember, string returnType)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                    public class AnalyzeWhenSourceDoesNotProvideIEnumerable
                    {
                #pragma warning disable CS0414 // Consider changing the field to a 'const'
                        {{testCaseMember}}
                #pragma warning restore CS0414

                        [Test]
                        public void Test([ValueSource({|#0:nameof(TestCases)|})] int number)
                        {
                        }
                    }
            """);

            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ValueSourceDoesNotReturnIEnumerable)
                .WithMessage($"The ValueSource does not return an I(Async)Enumerable or a type that implements I(Async)Enumerable. Instead it returns a '{returnType}'.");
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenSourceIsInAnotherClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class AnalyzeWhenSourceIsInAnotherClass
                {
                    [Test]
                    public void DivideTest([ValueSource(typeof(AnotherClass), "Numbers")] int n)
                    {
                        Assert.That(n, Is.GreaterThan(0));
                    }
                }

                class AnotherClass
                {
                    static int[] Numbers => new int[] { 1, 2, 3 };
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenSourceIsInNestedClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class AnalyzeWhenSourceIsInNestedClass
                {
                    [Test]
                    public void DivideTest([ValueSource(typeof(AnotherClass), "Numbers")] int n)
                    {
                        Assert.That(n, Is.GreaterThan(0));
                    }

                    class AnotherClass
                    {
                        static int[] Numbers => new int[] { 1, 2, 3 };
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenAllInformationIsProvidedByAttribute()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class AnalyzeWhenAllInformationIsProvidedByAttribute
                {
                    [Test]
                    public void ShortName([ValueSource(typeof(AnotherClass), "TestStrings")] string name)
                    {
                        Assert.That(name.Length, Is.LessThan(15));
                    }
                }

                class AnotherClass
                {
                    static IEnumerable<string> TestStrings()
                    {
                        yield return "SomeName";
                        yield return "YetAnotherName";
                    }
                }
            """, additionalUsings: "using System.Collections.Generic;");
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task FixWhenStringLiteralTargetsSourceInAnInnerClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public class FixWhenStringLiteralTargetsSourceInAnInnerClass
                {
                    [Test]
                    public void TestData([ValueSource(typeof(InnerClass), {|#0:"TestCases"|})] string input) { }

                    public class InnerClass
                    {
                        public static string[] TestCases = new[] { "Data" };
                    }
                }
            """);

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public class FixWhenStringLiteralTargetsSourceInAnInnerClass
                {
                    [Test]
                    public void TestData([ValueSource(typeof(InnerClass), nameof(InnerClass.TestCases))] string input) { }

                    public class InnerClass
                    {
                        public static string[] TestCases = new[] { "Data" };
                    }
                }
            """);

            var message = "Consider using nameof(InnerClass.TestCases) instead of \"TestCases\"";
            await Verify.VerifyCodeFixAsync(testCode, expectedDiagnosticCodeFix.WithMessage(message).WithLocation(0), fixedCode);
        }

        [Test]
        public async Task FixWhenStringLiteralTargetsSourceInAnotherClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public class FixWhenStringLiteralTargetsSourceInAnotherClass
                {
                    [Test]
                    public void TestData([ValueSource(typeof(AnotherClass), {|#0:"TestCases"|})] string input) { }
                }

                public class AnotherClass
                {
                    public static string[] TestCases = new[] { "Data" };
                }
            """);

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public class FixWhenStringLiteralTargetsSourceInAnotherClass
                {
                    [Test]
                    public void TestData([ValueSource(typeof(AnotherClass), nameof(AnotherClass.TestCases))] string input) { }
                }

                public class AnotherClass
                {
                    public static string[] TestCases = new[] { "Data" };
                }
            """);

            var message = "Consider using nameof(AnotherClass.TestCases) instead of \"TestCases\"";
            await Verify.VerifyCodeFixAsync(testCode, expectedDiagnosticCodeFix.WithMessage(message).WithLocation(0), fixedCode);
        }

        [Test]
        public async Task FixWhenStringLiteralTargetsSourceInAnInnerClassInAnotherClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public class FixWhenStringLiteralTargetsSourceInAnInnerClassInAnotherClass
                {
                    [Test]
                    public void TestData([ValueSource(typeof(AnotherClass.InnerClass), {|#0:"TestCases"|})] string input) { }
                }

                public class AnotherClass
                {
                    public class InnerClass
                    {
                        public static string[] TestCases = new[] { "Data" };
                    }
                }
            """);

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public class FixWhenStringLiteralTargetsSourceInAnInnerClassInAnotherClass
                {
                    [Test]
                    public void TestData([ValueSource(typeof(AnotherClass.InnerClass), nameof(AnotherClass.InnerClass.TestCases))] string input) { }
                }

                public class AnotherClass
                {
                    public class InnerClass
                    {
                        public static string[] TestCases = new[] { "Data" };
                    }
                }
            """);

            var message = "Consider using nameof(AnotherClass.InnerClass.TestCases) instead of \"TestCases\"";
            await Verify.VerifyCodeFixAsync(testCode, expectedDiagnosticCodeFix.WithMessage(message).WithLocation(0), fixedCode);
        }
    }
}
