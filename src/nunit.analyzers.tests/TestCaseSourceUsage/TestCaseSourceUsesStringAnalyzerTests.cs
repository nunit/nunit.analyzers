using System.Linq;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.SourceCommon;
using NUnit.Analyzers.TestCaseSourceUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.TestCaseSourceUsage
{
    [TestFixture]
    public sealed class TestCaseSourceUsesStringAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new TestCaseSourceUsesStringAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.TestCaseSourceStringUsage);
        private static readonly CodeFixProvider fix = new UseNameofFix();

        [Test]
        public void AnalyzeWhenNameOfSameClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenNameOfSameClass
    {
        static string[] Tests = new[] { ""Data"" };

        [TestCaseSource(nameof(Tests))]
        public void Test(string data)
        {
        }
    }");
            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenNameOfSameClassNotStatic()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenNameOfSameClassNotStatic
    {
        string[] Tests = new[] { ""Data"" };

        [TestCaseSource(↓nameof(Tests))]
        public void Test()
        {
        }
    }");
            var expectedDiagnostic = ExpectedDiagnostic
                .Create(AnalyzerIdentifiers.TestCaseSourceSourceIsNotStatic)
                .WithMessage("The specified source 'Tests' is not static");
            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void NoWarningWhenStringLiteralMissingMember()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class NoWarningWhenStringLiteralMissingMember
    {
        [TestCaseSource(""Missing"")]
        public void Test()
        {
        }
    }");
            var descriptor = new DiagnosticDescriptor(AnalyzerIdentifiers.TestCaseSourceStringUsage, string.Empty, string.Empty, string.Empty, DiagnosticSeverity.Warning, true);
            RoslynAssert.Valid(analyzer, descriptor, testCode);
        }

        [TestCase("private static readonly TestCaseData[] TestCases = new TestCaseData[0];")]
        [TestCase("private static TestCaseData[] TestCases => new TestCaseData[0];")]
        [TestCase("private static TestCaseData[] TestCases() => new TestCaseData[0];")]
        public void FixWhenStringLiteral(string testCaseMember)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public class AnalyzeWhenStringConstant
    {{
        {testCaseMember}

        [TestCaseSource(↓""TestCases"")]
        public void Test()
        {{
        }}
    }}");

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public class AnalyzeWhenStringConstant
    {{
        {testCaseMember}

        [TestCaseSource(nameof(TestCases))]
        public void Test()
        {{
        }}
    }}");

            var message = "Consider using nameof(TestCases) instead of \"TestCases\"";
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic.WithMessage(message), testCode, fixedCode);
        }

        [Test]
        public void FixWhenMultipleUnrelatedAttributes()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    [TestFixture]
    public class AnalyzeWhenMultipleUnrelatedAttributes
    {
        private static readonly TestCaseData[] TestCases = new TestCaseData[0];

        [Test]
        public void UnrelatedTest()
        {
        }

        [TestCaseSource(↓""TestCases"")]
        public void Test()
        {
        }
    }");

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    [TestFixture]
    public class AnalyzeWhenMultipleUnrelatedAttributes
    {
        private static readonly TestCaseData[] TestCases = new TestCaseData[0];

        [Test]
        public void UnrelatedTest()
        {
        }

        [TestCaseSource(nameof(TestCases))]
        public void Test()
        {
        }
    }");

            var message = "Consider using nameof(TestCases) instead of \"TestCases\"";
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic.WithMessage(message), testCode, fixedCode);
        }

        [Test]
        public void AnalyzeWhenNumberOfParametersMatch()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
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
    }", additionalUsings: "using System.Collections.Generic;");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenNumberOfParametersDoesNotMatchNoParametersExpected()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    [TestFixture]
    public class AnalyzeWhenNumberOfParametersDoesNotMatchNoParametersExpected
    {
        [TestCaseSource(↓nameof(TestData), new object[] { 1 })]
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
    }", additionalUsings: "using System.Collections.Generic;");

            var expectedDiagnostic = ExpectedDiagnostic
                .Create(AnalyzerIdentifiers.TestCaseSourceMismatchInNumberOfParameters)
                .WithMessage("The TestCaseSource provides '1' parameter(s), but the target method expects '0' parameter(s)");
            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenNumberOfParametersDoesNotMatchNoParametersProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    [TestFixture]
    public class AnalyzeWhenNumberOfParametersDoesNotMatchNoParametersProvided
    {
        [TestCaseSource(↓nameof(TestData))]
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
    }", additionalUsings: "using System.Collections.Generic;");

            var expectedDiagnostic = ExpectedDiagnostic
                .Create(AnalyzerIdentifiers.TestCaseSourceMismatchInNumberOfParameters)
                .WithMessage("The TestCaseSource provides '0' parameter(s), but the target method expects '1' parameter(s)");
            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenNumberOfParametersDoesNotMatch()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    [TestFixture]
    public class AnalyzeWhenNumberOfParametersDoesNotMatch
    {
        [TestCaseSource(↓nameof(TestData), new object[] { 1, 2, 3 })]
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
    }", additionalUsings: "using System.Collections.Generic;");

            var expectedDiagnostic = ExpectedDiagnostic
                .Create(AnalyzerIdentifiers.TestCaseSourceMismatchInNumberOfParameters)
                .WithMessage("The TestCaseSource provides '3' parameter(s), but the target method expects '2' parameter(s)");
            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenNumberOfParametersDoesNotMatchInAnotherClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    [TestFixture]
    public class AnalyzeWhenNumberOfParametersDoesNotMatchInAnotherClass
    {
        [TestCaseSource(typeof(AnotherClass), ↓nameof(AnotherClass.TestData), new object[] { 1, 2, 3 })]
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
    }", additionalUsings: "using System.Collections.Generic;");

            var expectedDiagnostic = ExpectedDiagnostic
                .Create(AnalyzerIdentifiers.TestCaseSourceMismatchInNumberOfParameters)
                .WithMessage("The TestCaseSource provides '3' parameter(s), but the target method expects '2' parameter(s)");
            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [TestCase("private static readonly TestCaseData[] TestCases = new TestCaseData[0];")]
        [TestCase("private static TestCaseData[] TestCases => new TestCaseData[0];")]
        [TestCase("private static TestCaseData[] TestCases() => new TestCaseData[0];")]
        public void AnalyzeWhenSourceDoesProvideIEnumerable(string testCaseMember)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public class AnalyzeWhenSourceDoesProvideIEnumerable
    {{
        {testCaseMember}

        [TestCaseSource(nameof(TestCases))]
        public void Test(object data)
        {{
        }}
    }}");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase("private static readonly object? TestCases = null;", "object?")]
        [TestCase("private static object? TestCases => null;", "object?")]
        [TestCase("private static object? TestCases() => null;", "object?")]
        [TestCase("private static readonly int TestCases = 1;", "int")]
        [TestCase("private static int TestCases => 1;", "int")]
        [TestCase("private static int TestCases() => 1;", "int")]
        [TestCase("private static readonly PlatformID TestCases = PlatformID.Unix;", "System.PlatformID")]
        [TestCase("private static PlatformID TestCases => PlatformID.Unix;", "System.PlatformID")]
        [TestCase("private static PlatformID TestCases() => PlatformID.Unix;", "System.PlatformID")]
        public void AnalyzeWhenSourceDoesNotProvideIEnumerable(string testCaseMember, string returnType)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public class AnalyzeWhenSourceDoesProvideIEnumerable
    {{
#pragma warning disable CS0414 // Consider changing the field to a 'const'
        {testCaseMember}
#pragma warning restore CS0414

        [TestCaseSource(nameof(TestCases))]
        public void Test()
        {{
        }}
    }}");

            var expectedDiagnostic = ExpectedDiagnostic
                .Create(AnalyzerIdentifiers.TestCaseSourceDoesNotReturnIEnumerable)
                .WithMessage($"The TestCaseSource does not return an IEnumerable or a type that implements IEnumerable. Instead it returns a '{returnType}'.");
            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [TestCase("private static readonly TestCaseData[] TestCases = new TestCaseData[0];", "fields")]
        [TestCase("private static TestCaseData[] TestCases => new TestCaseData[0];", "properties")]
        public void AnalyzeWhenParametersProvidedToFieldOrProperty(string testCaseMember, string kind)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public class AnalyzeWhenParametersProvidedToFieldOrProperty
    {{
        {testCaseMember}

        [TestCaseSource(nameof(TestCases), new object[] {{ 1, 2, 3 }})]
        public void Test()
        {{
        }}
    }}");

            var expectedDiagnostic = ExpectedDiagnostic
                .Create(AnalyzerIdentifiers.TestCaseSourceSuppliesParametersToFieldOrProperty)
                .WithMessage($"The TestCaseSource provides '3' parameter(s), but {kind} cannot take parameters");
            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenSourceIsInAnotherClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenSourceIsInAnotherClass
    {
        [TestCaseSource(typeof(AnotherClass), ""DivideCases"")]
        public void DivideTest(int n, int d, int q)
        {
            Assert.AreEqual(q, n / d);
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
    }");
            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenSourceIsInNestedClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenSourceIsInNestedClass
    {
        [TestCaseSource(typeof(AnotherClass), ""DivideCases"")]
        public void DivideTest(int n, int d, int q)
        {
            Assert.AreEqual(q, n / d);
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
    }");
            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenAllInformationIsProvidedByAttribute()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenAllInformationIsProvidedByAttribute
    {
        [TestCaseSource(typeof(AnotherClass), ""TestStrings"", new object[] { false })]
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
                yield return ""ThisIsAVeryLongNameThisIsAVeryLongName"";
            yield return ""SomeName"";
            yield return ""YetAnotherName"";
        }
    }", additionalUsings: "using System.Collections.Generic;");
            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenAllInformationIsProvidedByAttributeWhenDataClassIsInSeparateAssembly()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
        public class Tests
        {
            [Test]
            [TestCaseSource(typeof(TestData), nameof(TestData.TestCaseDataProvider))]
            public void Test1(int a, int b, int c)
            {
                Assert.That(a + b, Is.EqualTo(c));
            }
        }");

            var testDataCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
        public static class TestData
        {
            public static IEnumerable TestCaseDataProvider()
            {
                yield return new object[] { 1, 2, 3 };
                yield return new object[] { 2, 3, 5 };
            }
        }", "using System.Collections;");

            var solution = CodeFactory.CreateSolution(new[] { testCode, testDataCode });

            RoslynAssert.Valid(analyzer, solution);
        }

        [Test]
        public void FixWhenStringLiteralTargetsSourceInAnInnerClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public class FixWhenStringLiteralTargetsSourceInAnInnerClass
    {{
        [TestCaseSource(typeof(InnerClass), ""TestCases"")]
        public void TestData(string input) {{ }}

        public class InnerClass
        {{
            public static string[] TestCases = new[] {{ ""Data"" }};
        }}
    }}");

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public class FixWhenStringLiteralTargetsSourceInAnInnerClass
    {{
        [TestCaseSource(typeof(InnerClass), nameof(InnerClass.TestCases))]
        public void TestData(string input) {{ }}

        public class InnerClass
        {{
            public static string[] TestCases = new[] {{ ""Data"" }};
        }}
    }}");

            var message = "Consider using nameof(InnerClass.TestCases) instead of \"TestCases\"";
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic.WithMessage(message), testCode, fixedCode);
        }

        [Test]
        public void FixWhenStringLiteralTargetsSourceInAnotherClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public class FixWhenStringLiteralTargetsSourceInAnotherClass
    {{
        [TestCaseSource(typeof(AnotherClass), ""TestCases"")]
        public void TestData(string input) {{ }}
    }}

    public class AnotherClass
    {{
        public static string[] TestCases = new[] {{ ""Data"" }};
    }}");

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public class FixWhenStringLiteralTargetsSourceInAnotherClass
    {{
        [TestCaseSource(typeof(AnotherClass), nameof(AnotherClass.TestCases))]
        public void TestData(string input) {{ }}
    }}

    public class AnotherClass
    {{
        public static string[] TestCases = new[] {{ ""Data"" }};
    }}");

            var message = "Consider using nameof(AnotherClass.TestCases) instead of \"TestCases\"";
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic.WithMessage(message), testCode, fixedCode);
        }

        [Test]
        public void FixWhenStringLiteralTargetsSourceInAnInnerClassInAnotherClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public class FixWhenStringLiteralTargetsSourceInAnInnerClassInAnotherClass
    {{
        [TestCaseSource(typeof(AnotherClass.InnerClass), ""TestCases"")]
        public void TestData(string input) {{ }}
    }}

    public class AnotherClass
    {{
        public class InnerClass
        {{
            public static string[] TestCases = new[] {{ ""Data"" }};
        }}
    }}");

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public class FixWhenStringLiteralTargetsSourceInAnInnerClassInAnotherClass
    {{
        [TestCaseSource(typeof(AnotherClass.InnerClass), nameof(AnotherClass.InnerClass.TestCases))]
        public void TestData(string input) {{ }}
    }}

    public class AnotherClass
    {{
        public class InnerClass
        {{
            public static string[] TestCases = new[] {{ ""Data"" }};
        }}
    }}");

            var message = "Consider using nameof(AnotherClass.InnerClass.TestCases) instead of \"TestCases\"";
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic.WithMessage(message), testCode, fixedCode);
        }

        [Test]
        public void AnalyzeWhenNumberOfParametersOfTestIsLessThanProvidedByTestCaseSource()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    [TestFixture]
    public class AnalyzeWhenNumberOfParametersDoesNotMatchNoParametersExpected
    {
        [TestCaseSource(↓nameof(TestData), new object[] { 3 })]
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
    }", additionalUsings: "using System.Collections.Generic;");

            var expectedDiagnostic = ExpectedDiagnostic
                .Create(AnalyzerIdentifiers.TestCaseSourceMismatchInNumberOfTestMethodParameters)
                .WithMessage("The TestCaseSource provides '1' parameter(s), but the Test method expects '0' parameter(s)");
            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenNumberOfParametersOfTestIsMoreThanProvidedByTestCaseSource()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    [TestFixture]
    public class AnalyzeWhenNumberOfParametersDoesNotMatchNoParametersExpected
    {
        [TestCaseSource(↓nameof(TestData))]
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
    }", additionalUsings: "using System.Collections.Generic;");

            var expectedDiagnostic = ExpectedDiagnostic
                .Create(AnalyzerIdentifiers.TestCaseSourceMismatchInNumberOfTestMethodParameters)
                .WithMessage("The TestCaseSource provides '1' parameter(s), but the Test method expects '2' parameter(s)");
            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenParameterTypeOfTestDiffersFromTestCaseSource()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    [TestFixture]
    public class AnalyzeWhenNumberOfParametersDoesNotMatchNoParametersExpected
    {
        [TestCaseSource(↓nameof(TestData))]
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
    }", additionalUsings: "using System.Collections.Generic;");

            var expectedDiagnostic = ExpectedDiagnostic
                .Create(AnalyzerIdentifiers.TestCaseSourceMismatchWithTestMethodParameterType)
                .WithMessage("The TestCaseSource provides type 'int', but the Test method expects type 'string' for parameter 'message'");
            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        [Explicit("The code is wrong, but it is too complext for the analyzer to detect this.")]
        public void AnalyzeWhenNumberOfParametersOfTestIsNotEvidentFromTestSource()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    [TestFixture]
    public class AnalyzeWhenNumberOfParametersDoesNotMatchNoParametersExpected
    {
        [TestCaseSource(↓nameof(TestData))]
        public void ShortName(int n)
        {
            Assert.That(n, Is.GreaterThanOrEqualTo(0));
        }

        static IEnumerable<object> TestData()
        {
            yield return new object[] { 1, 2 };
            yield return new object[] { 2, 3 };
        }
    }", additionalUsings: "using System.Collections.Generic;");

            var expectedDiagnostic = ExpectedDiagnostic
                .Create(AnalyzerIdentifiers.TestCaseSourceMismatchInNumberOfTestMethodParameters)
                .WithMessage("The TestCaseSource provides '>0' parameter(s), but the Test method expects '1' parameter(s)");
            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }
    }
}
