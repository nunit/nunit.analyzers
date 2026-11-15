using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.TestCaseSourceUsage.TestCaseSourceUsesStringAnalyzer, NUnit.Analyzers.SourceCommon.UseNameofFix>;

namespace NUnit.Analyzers.Tests.TestCaseSourceUsage
{
    [TestFixture]
    public sealed class TestCaseSourceUsesStringAnalyzerTests
    {
        private static readonly DiagnosticResult expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseSourceStringUsage);

        [Test]
        public async Task AnalyzeWhenNameOfSameClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class AnalyzeWhenNameOfSameClass
                {
                    static string[] Tests = new[] { "Data" };

                    [TestCaseSource(nameof(Tests))]
                    public void Test(string data)
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

                    [TestCaseSource({|#0:nameof(Tests)|})]
                    public void Test()
                    {
                    }
                }
            """);
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseSourceSourceIsNotStatic)
                .WithMessage("The specified source 'Tests' is not static");
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0),
                Verify.Diagnostic(AnalyzerIdentifiers.TestCaseSourceMismatchInNumberOfTestMethodParameters).WithLocation(0).WithArguments("1", "0"));
        }

        [Test]
        public async Task NoNameOfSuggestionWhenStringLiteralMissingMember()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class NoWarningWhenStringLiteralMissingMember
                {
                    [TestCaseSource({|#0:"Missing"|})]
                    public void Test()
                    {
                    }
                }
            """);

            // Only the missing source is reported, not the suggestion to use nameof.
            await Verify.VerifyAnalyzerAsync(testCode, Verify.Diagnostic(AnalyzerIdentifiers.TestCaseSourceIsMissing).WithLocation(0).WithArguments("Missing"));
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

                    [TestCaseSource({|#0:"TestCases"|})]
                    public void Test(object data)
                    {
                        Assert.That(data, Is.Not.Null);
                    }
                }
            """);

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public class AnalyzeWhenStringConstant
                {
                    {{testCaseMember}}

                    [TestCaseSource(nameof(TestCases))]
                    public void Test(object data)
                    {
                        Assert.That(data, Is.Not.Null);
                    }
                }
            """);

            var message = "Consider using nameof(TestCases) instead of \"TestCases\"";
            await Verify.VerifyCodeFixAsync(testCode, expectedDiagnostic.WithMessage(message).WithLocation(0), fixedCode);
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

                    [TestCaseSource({|#0:"TestCases"|})]
                    public void Test(object data)
                    {
                        Assert.That(data, Is.Not.Null);
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

                    [TestCaseSource(nameof(TestCases))]
                    public void Test(object data)
                    {
                        Assert.That(data, Is.Not.Null);
                    }
                }
            """);

            var message = "Consider using nameof(TestCases) instead of \"TestCases\"";
            await Verify.VerifyCodeFixAsync(testCode, expectedDiagnostic.WithMessage(message).WithLocation(0), fixedCode);
        }

        [Test]
        public async Task AnalyzeWhenNumberOfParametersMatch()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class AnalyzeWhenNumberOfParametersMatch
                {
                    [TestCaseSource(nameof(TestData), new object[] { 1, 3, 5 })]
                    public void ShortName(int number)
                    {
                        Assert.That(number, Is.GreaterThanOrEqualTo(0));
                    }

                    static IEnumerable<int> TestData(int first, int second, int third)
                    {
                        yield return first;
                        yield return second;
                        yield return third;
                    }
                }
            """, additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenNumberOfParametersDoesNotMatchNoParametersExpected()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class AnalyzeWhenNumberOfParametersDoesNotMatchNoParametersExpected
                {
                    [TestCaseSource({|#0:nameof(TestData)|}, new object[] { 1 })]
                    public void ShortName(int number)
                    {
                        Assert.That(number, Is.GreaterThanOrEqualTo(0));
                    }

                    static IEnumerable<int> TestData()
                    {
                        yield return 1;
                        yield return 2;
                        yield return 3;
                    }
                }
            """, additionalUsings: "using System.Collections.Generic;");

            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseSourceMismatchInNumberOfParameters)
                .WithMessage("The TestCaseSource provides '1' parameter(s), but the target method expects '0' parameter(s)");
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenNumberOfParametersDoesNotMatchNoParametersProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class AnalyzeWhenNumberOfParametersDoesNotMatchNoParametersProvided
                {
                    [TestCaseSource({|#0:nameof(TestData)|})]
                    public void ShortName(int number)
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

            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseSourceMismatchInNumberOfParameters)
                .WithMessage("The TestCaseSource provides '0' parameter(s), but the target method expects '1' parameter(s)");
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenNumberOfParametersDoesNotMatch()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class AnalyzeWhenNumberOfParametersDoesNotMatch
                {
                    [TestCaseSource({|#0:nameof(TestData)|}, new object[] { 1, 2, 3 })]
                    public void ShortName(int number)
                    {
                        Assert.That(number, Is.GreaterThanOrEqualTo(0));
                    }

                    static IEnumerable<int> TestData(string dummy, int anotherDummy)
                    {
                        yield return 1;
                        yield return 2;
                        yield return 3;
                    }
                }
            """, additionalUsings: "using System.Collections.Generic;");

            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseSourceMismatchInNumberOfParameters)
                .WithMessage("The TestCaseSource provides '3' parameter(s), but the target method expects '2' parameter(s)");
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenNumberOfParametersDoesNotMatchInAnotherClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class AnalyzeWhenNumberOfParametersDoesNotMatchInAnotherClass
                {
                    [TestCaseSource(typeof(AnotherClass), {|#0:nameof(AnotherClass.TestData)|}, new object[] { 1, 2, 3 })]
                    public void ShortName(int number)
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

            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseSourceMismatchInNumberOfParameters)
                .WithMessage("The TestCaseSource provides '3' parameter(s), but the target method expects '2' parameter(s)");
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [TestCase("private static readonly TestCaseData[] TestCases = new TestCaseData[0];")]
        [TestCase("private static TestCaseData[] TestCases => new TestCaseData[0];")]
        [TestCase("private static TestCaseData[] TestCases() => new TestCaseData[0];")]
        [TestCase("private static Task<TestCaseData[]> TestCases() => Task.FromResult(new TestCaseData[0]);")]
        public async Task AnalyzeWhenSourceDoesProvideIEnumerable(string testCaseMember)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public class AnalyzeWhenSourceDoesProvideIEnumerable
                {
                    {{testCaseMember}}

                    [TestCaseSource(nameof(TestCases))]
                    public void Test(object data)
                    {
                    }
                }
            """);

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
                    public class AnalyzeWhenSourceDoesProvideIEnumerable
                    {
                #pragma warning disable CS0414 // Consider changing the field to a 'const'
                        {{testCaseMember}}
                #pragma warning restore CS0414

                        [TestCaseSource({|#0:nameof(TestCases)|})]
                        public void Test()
                        {
                        }
                    }
            """);

            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseSourceDoesNotReturnIEnumerable)
                .WithMessage($"The TestCaseSource does not return an I(Async)Enumerable or a type that implements I(Async)Enumerable. Instead it returns a '{returnType}'.");
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [TestCase("private static readonly TestCaseData[] TestCases = new TestCaseData[0];", "fields")]
        [TestCase("private static TestCaseData[] TestCases => new TestCaseData[0];", "properties")]
        public async Task AnalyzeWhenParametersProvidedToFieldOrProperty(string testCaseMember, string kind)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public class AnalyzeWhenParametersProvidedToFieldOrProperty
                {
                    {{testCaseMember}}

                    [TestCaseSource({|#0:nameof(TestCases)|}, new object[] { 1, 2, 3 })]
                    public void Test()
                    {
                    }
                }
            """);

            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseSourceSuppliesParametersToFieldOrProperty)
                .WithMessage($"The TestCaseSource provides '3' parameter(s), but {kind} cannot take parameters");
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0),
                Verify.Diagnostic(AnalyzerIdentifiers.TestCaseSourceMismatchInNumberOfTestMethodParameters).WithLocation(0).WithArguments(">0", "0"));
        }

        [Test]
        public async Task AnalyzeWhenSourceIsInAnotherClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class AnalyzeWhenSourceIsInAnotherClass
                {
                    [TestCaseSource(typeof(AnotherClass), "DivideCases")]
                    public void DivideTest(int n, int d, int q)
                    {
                        ClassicAssert.AreEqual(q, n / d);
                    }
                }

                class AnotherClass
                {
                    static object[] DivideCases =
                    {
                        new object[] { 12, 3, 4 },
                        new object[] { 12, 2, 6 },
                        new object[] { 12, 4, 3 }
                    };
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
                    [TestCaseSource(typeof(AnotherClass), "DivideCases")]
                    public void DivideTest(int n, int d, int q)
                    {
                        ClassicAssert.AreEqual(q, n / d);
                    }

                    class AnotherClass
                    {
                        static object[] DivideCases =
                        {
                            new object[] { 12, 3, 4 },
                            new object[] { 12, 2, 6 },
                            new object[] { 12, 4, 3 }
                        };
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
                    [TestCaseSource(typeof(AnotherClass), "TestStrings", new object[] { false })]
                    public void ShortName(string name)
                    {
                        Assert.That(name.Length, Is.LessThan(15));
                    }
                }

                class AnotherClass
                {
                    static IEnumerable<string> TestStrings(bool generateLongTestCase)
                    {
                        if (generateLongTestCase)
                            yield return "ThisIsAVeryLongNameThisIsAVeryLongName";
                        yield return "SomeName";
                        yield return "YetAnotherName";
                    }
                }
            """, additionalUsings: "using System.Collections.Generic;");
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenAllInformationIsProvidedByAttributeWhenDataClassIsInSeparateAssembly()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class Tests
                {
                    [Test]
                    [TestCaseSource(typeof(TestData), nameof(TestData.TestCaseDataProvider))]
                    public void Test1(int a, int b, int c)
                    {
                        Assert.That(a + b, Is.EqualTo(c));
                    }
                }
            """);

            var testDataCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public static class TestData
                {
                    public static IEnumerable TestCaseDataProvider()
                    {
                        yield return new object[] { 1, 2, 3 };
                        yield return new object[] { 2, 3, 5 };
                    }
                }
            """, "using System.Collections;");

            await Verify.VerifyAnalyzerAsync(testCode, test => test.TestState.Sources.Add(testDataCode));
        }

        [Test]
        public async Task FixWhenStringLiteralTargetsSourceInAnInnerClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public class FixWhenStringLiteralTargetsSourceInAnInnerClass
                {
                    [TestCaseSource(typeof(InnerClass), {|#0:"TestCases"|})]
                    public void TestData(string input) { }

                    public class InnerClass
                    {
                        public static string[] TestCases = new[] { "Data" };
                    }
                }
            """);

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public class FixWhenStringLiteralTargetsSourceInAnInnerClass
                {
                    [TestCaseSource(typeof(InnerClass), nameof(InnerClass.TestCases))]
                    public void TestData(string input) { }

                    public class InnerClass
                    {
                        public static string[] TestCases = new[] { "Data" };
                    }
                }
            """);

            var message = "Consider using nameof(InnerClass.TestCases) instead of \"TestCases\"";
            await Verify.VerifyCodeFixAsync(testCode, expectedDiagnostic.WithMessage(message).WithLocation(0), fixedCode);
        }

        [Test]
        public async Task FixWhenStringLiteralTargetsSourceInAnotherClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public class FixWhenStringLiteralTargetsSourceInAnotherClass
                {
                    [TestCaseSource(typeof(AnotherClass), {|#0:"TestCases"|})]
                    public void TestData(string input) { }
                }

                public class AnotherClass
                {
                    public static string[] TestCases = new[] { "Data" };
                }
            """);

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public class FixWhenStringLiteralTargetsSourceInAnotherClass
                {
                    [TestCaseSource(typeof(AnotherClass), nameof(AnotherClass.TestCases))]
                    public void TestData(string input) { }
                }

                public class AnotherClass
                {
                    public static string[] TestCases = new[] { "Data" };
                }
            """);

            var message = "Consider using nameof(AnotherClass.TestCases) instead of \"TestCases\"";
            await Verify.VerifyCodeFixAsync(testCode, expectedDiagnostic.WithMessage(message).WithLocation(0), fixedCode);
        }

        [Test]
        public async Task FixWhenStringLiteralTargetsSourceInAnInnerClassInAnotherClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public class FixWhenStringLiteralTargetsSourceInAnInnerClassInAnotherClass
                {
                    [TestCaseSource(typeof(AnotherClass.InnerClass), {|#0:"TestCases"|})]
                    public void TestData(string input) { }
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
                    [TestCaseSource(typeof(AnotherClass.InnerClass), nameof(AnotherClass.InnerClass.TestCases))]
                    public void TestData(string input) { }
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
            await Verify.VerifyCodeFixAsync(testCode, expectedDiagnostic.WithMessage(message).WithLocation(0), fixedCode);
        }

        [Test]
        public async Task NoWarningWhenNumberOfParametersOfTestWillFitIntoParamsArray()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class NoWarningWhenNumberOfParametersOfTestWillFitIntoParamsArray
                {
                    [TestCaseSource(nameof(TestData))]
                    public void ShortName(params int[] x)
                    {
                        Assert.That(x, Has.Length.GreaterThanOrEqualTo(0));
                    }

                    static IEnumerable<int> TestData()
                    {
                        for (int i = 1; i <= 3; i++)
                        {
                            yield return i;
                        }
                    }
                }
            """, additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoWarningWhenNumberOfParametersOfTestWillFitIntoParamsArrayForGenericMethod()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class NoWarningWhenNumberOfParametersOfTestWillFitIntoParamsArrayForGenericMethod
                {
                    [TestCaseSource(nameof(TestData))]
                    public void ShortName<T>(params T[] x)
                    {
                        Assert.That(x.Length, Is.GreaterThanOrEqualTo(0));
                    }

                    static IEnumerable<int> TestData()
                    {
                        for (int i = 1; i <= 3; i++)
                        {
                            yield return i;
                        }
                    }
                }
            """, additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoWarningWhenNumberOfParametersOfTestWillFitIntoOptionalParams()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class NoWarningWhenNumberOfParametersOfTestWillFitIntoOptionalParams
                {
                    [TestCaseSource(nameof(TestData))]
                    public void ShortName(int a, int b = 2)
                    {
                        Assert.That(a, Is.GreaterThanOrEqualTo(b));
                    }

                    static IEnumerable<int> TestData()
                    {
                        for (int i = 1; i <= 3; i++)
                        {
                            yield return i;
                        }
                    }
                }
            """, additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoWarningWhenNumberOfParametersOfTestWillFitIntoParamsArrayFlatArgs()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class NoWarningWhenNumberOfParametersOfTestWillFitIntoParamsArrayFlatArgs
                {
                    [TestCaseSource(nameof(TestData))]
                    public static void TestLotsOfNonRequiredParams(params int[] z)
                    {
                        Assert.That(z, Is.Not.Empty);
                    }

                    static IEnumerable<int[]> TestData()
                    {
                        yield return new int[] { 2, 4, 6, 8 };
                    }
                }
            """, additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenNumberOfParametersOfTestIsLessThanProvidedByTestCaseSource()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class AnalyzeWhenNumberOfParametersOfTestIsLessThanProvidedByTestCaseSource
                {
                    [TestCaseSource({|#0:nameof(TestData)|}, new object[] { 3 })]
                    public void ShortName()
                    {
                        Assert.That(3, Is.GreaterThanOrEqualTo(0));
                    }

                    static IEnumerable<int> TestData(int maximum)
                    {
                        for (int i = 1; i <= maximum; i++)
                        {
                            yield return i;
                        }
                    }
                }
            """, additionalUsings: "using System.Collections.Generic;");

            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseSourceMismatchInNumberOfTestMethodParameters)
                .WithMessage("The TestCaseSource provides '1' parameter(s), but the Test method expects '0' parameter(s)");
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenNumberOfParametersOfTestIsMoreThanProvidedByTestCaseSource()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class AnalyzeWhenNumberOfParametersOfTestIsMoreThanProvidedByTestCaseSource
                {
                    [TestCaseSource({|#0:nameof(TestData)|})]
                    public void ShortName(int x, int y)
                    {
                        Assert.That(x + y, Is.GreaterThanOrEqualTo(0));
                    }

                    static IEnumerable<int> TestData()
                    {
                        yield return 1;
                        yield return 2;
                        yield return 3;
                    }
                }
            """, additionalUsings: "using System.Collections.Generic;");

            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseSourceMismatchInNumberOfTestMethodParameters)
                .WithMessage("The TestCaseSource provides '1' parameter(s), but the Test method expects '2' parameter(s)");
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenParameterTypeOfTestDiffersFromTestCaseSource()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class AnalyzeWhenParameterTypeOfTestDiffersFromTestCaseSource
                {
                    [TestCaseSource({|#0:nameof(TestData)|})]
                    public void ShortName(string message)
                    {
                        Assert.That(message.Length, Is.GreaterThanOrEqualTo(0));
                    }

                    static IEnumerable<int> TestData()
                    {
                        yield return 1;
                        yield return 2;
                        yield return 3;
                    }
                }
            """, additionalUsings: "using System.Collections.Generic;");

            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseSourceMismatchWithTestMethodParameterType)
                .WithMessage("The TestCaseSource provides type 'int', but the Test method expects type 'string' for parameter 'message'");
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenNumberOfParametersOfTestIsNotEvidentFromTestSource()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class AnalyzeWhenNumberOfParametersOfTestIsNotEvidentFromTestSource
                {
                    [Explicit("The code is wrong, but it is too complex for the analyzer to detect this.")]
                    [TestCaseSource(nameof(TestData))]
                    public void ShortName(int n)
                    {
                        Assert.That(n, Is.GreaterThanOrEqualTo(0));
                    }

                    static IEnumerable<object> TestData()
                    {
                        yield return new object[] { 1, 2 };
                        yield return new object[] { 2, 3 };
                    }
                }
            """, additionalUsings: "using System.Collections.Generic;");

            // The TestCaseSource provides '>0' parameter(s), but the Test method expects '1' parameter(s)
            // Analyzing the actual code inside the TestCaseSource method is beyond the scope of the analyzer.
            await Verify.VerifyAnalyzerAsync(testCode);
        }

#if NUNIT4
        [Test]
        public async Task AnalyzeWhenNumberOfParametersOfGenericTestIsNotEvidentFromTestSource()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class AnalyzeWhenTestSourceProvidesDataWithSingleTypeArgs
                {
                    [TestCaseSource(nameof(TestData))]
                    public void Method<T>()
                    {
                        Assert.That(typeof(T).IsValueType, Is.True);
                    }

                    static IEnumerable<TestCaseData> TestData()
                    {
                        yield return new TestCaseData { TypeArgs = new Type[] { typeof(int) } };
                    }
                }
            """, additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }
#endif

        [TestCase("IEnumerable", "object", "System.Collections")]
        [TestCase("IEnumerable<object>", "object", "System.Collections.Generic")]
        [TestCase("IEnumerable<TestCaseData>", "TestCaseData", "System.Collections.Generic")]
        [TestCase("IEnumerable<TestCaseParameters>", "TestCaseParameters", "System.Collections.Generic; using NUnit.Framework.Internal")]
        [TestCase("IEnumerable<int>", "int", "System.Collections.Generic")]
        public async Task NoIssueIsRaisedWhenOneParameterIsExpectedAndTypeCannotBeDetermined(string enumerableType, string testCaseType, string collections)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                using {{collections}};

                public class NoIssueIsRaisedWhenOneParameterIsExpectedAndTypeCannotBeDetermined
                {
                    [TestCaseSource(nameof(TestData))]
                    public void ShortName(int n)
                    {
                        Assert.That(n, Is.GreaterThanOrEqualTo(0));
                    }

                    public static {{enumerableType}} TestData() => Array.Empty<{{testCaseType}}>();
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("IEnumerable", "object", "System.Collections")]
        [TestCase("IEnumerable<object>", "object", "System.Collections.Generic")]
        [TestCase("IEnumerable<TestCaseData>", "TestCaseData", "System.Collections.Generic")]
        [TestCase("IEnumerable<TestCaseParameters>", "TestCaseParameters", "System.Collections.Generic; using NUnit.Framework.Internal")]
        public async Task NoIssueIsRaisedWhenMultipleParameterAreExpectedAndTypeCannotBeDetermined(string enumerableType, string testCaseType, string collections)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                using {{collections}};

                public class NoIssueIsRaisedWhenMultipleParameterAreExpectedAndTypeCannotBeDetermined
                {
                    [TestCaseSource(nameof(TestData))]
                    public void ShortName(int first, int second)
                    {
                        Assert.That(first, Is.LessThan(second));
                    }

                    public static {{enumerableType}} TestData() => Array.Empty<{{testCaseType}}>();
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

#if NUNIT4
        [Test]
        public async Task AnalyzeWhenNumberOfParametersMatchExcludingImplicitSuppliedCancellationTokenDueToCancelAfterOnMethod()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class AnalyzeWhenNumberOfParametersMatch
                {
                    [TestCaseSource(nameof(TestData), new object[] { 1, 3, 5 })]
                    [CancelAfter(10)]
                    public void ShortName(int number, CancellationToken cancellationToken)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            Assert.Ignore("Cancelled");
                        Assert.That(number, Is.GreaterThanOrEqualTo(0));
                    }

                    static IEnumerable<int> TestData(int first, int second, int third)
                    {
                        yield return first;
                        yield return second;
                        yield return third;
                    }
                }
            """, additionalUsings: "using System.Collections.Generic;using System.Threading;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenNumberOfParametersMatchExcludingImplicitSuppliedCancellationTokenDueToCancelAfterOnClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                [CancelAfter(100)]
                public class AnalyzeWhenNumberOfParametersMatch
                {
                    [TestCaseSource(nameof(TestData), new object[] { 1, 3, 5 })]
                    public void ShortName(int number, CancellationToken cancellationToken)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            Assert.Ignore("Cancelled");
                        Assert.That(number, Is.GreaterThanOrEqualTo(0));
                    }

                    static IEnumerable<int> TestData(int first, int second, int third)
                    {
                        yield return first;
                        yield return second;
                        yield return third;
                    }
                }
            """, additionalUsings: "using System.Collections.Generic;using System.Threading;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenNumberOfParametersMatchNoImplicitlySuppliedCancellationTokenParameter()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class AnalyzeWhenNumberOfParametersMatch
                {
                    [TestCaseSource(nameof(TestData), new object[] { 1, 3, 5 })]
                    [CancelAfter(10)]
                    public void ShortName(int number)
                    {
                        if (TestContext.CurrentContext.CancellationToken.IsCancellationRequested)
                            Assert.Ignore("Cancelled");
                        Assert.That(number, Is.GreaterThanOrEqualTo(0));
                    }

                    static IEnumerable<int> TestData(int first, int second, int third)
                    {
                        yield return first;
                        yield return second;
                        yield return third;
                    }
                }
            """, additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenNumberOfParametersDoesNotMatchNoParametersExpectedNoImplicitSuppliedCancellationToken()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class AnalyzeWhenNumberOfParametersDoesNotMatchNoParametersExpected
                {
                    [TestCaseSource({|#0:nameof(TestData)|}, new object[] { 1 })]
                    public void ShortName(CancellationToken cancellationToken)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            Assert.Ignore("Cancelled");
                    }

                    static IEnumerable<int> TestData()
                    {
                        yield return 1;
                        yield return 2;
                        yield return 3;
                    }
                }
            """, additionalUsings: "using System.Collections.Generic;using System.Threading;");

            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseSourceMismatchInNumberOfParameters)
                .WithMessage("The TestCaseSource provides '1' parameter(s), but the target method expects '0' parameter(s)");
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0),
                Verify.Diagnostic(AnalyzerIdentifiers.TestCaseSourceMismatchWithTestMethodParameterType).WithLocation(0).WithArguments("int", "System.Threading.CancellationToken", "cancellationToken"));
        }

#endif
    }
}
