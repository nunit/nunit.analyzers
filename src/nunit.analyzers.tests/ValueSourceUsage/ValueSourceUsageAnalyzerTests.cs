using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.SourceCommon;
using NUnit.Analyzers.ValueSourceUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ValueSourceUsage
{
    [TestFixture]
    public sealed class ValueSourceUsageAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new ValueSourceUsageAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnosticCodeFix = ExpectedDiagnostic.Create(AnalyzerIdentifiers.ValueSourceStringUsage);
        private static readonly CodeFixProvider fix = new UseNameofFix();

        [Test]
        public void AnalyzeWhenNameOfSameClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenNameOfSameClass
    {
        static string[] Tests = new[] { ""Data"" };

        [Test]
        public void Test([ValueSource(nameof(Tests))] string input)
        {
        }
    }");
            AnalyzerAssert.Valid<ValueSourceUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenNameOfSameClassNotStatic()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenNameOfSameClassNotStatic
    {
        string[] Tests = new[] { ""Data"" };

        [Test]
        public void Test([ValueSource(↓nameof(Tests))] input)
        {
        }
    }");
            var expectedDiagnostic = ExpectedDiagnostic
                .Create(AnalyzerIdentifiers.ValueSourceIsNotStatic)
                .WithMessage("The specified source 'Tests' is not static.");
            AnalyzerAssert.Diagnostics<ValueSourceUsageAnalyzer>(expectedDiagnostic, testCode);
        }

        [Test]
        public void NoWarningWhenStringLiteralMissingMember()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class NoWarningWhenStringLiteralMissingMember
    {
        [Test]
        public void Test([ValueSource(""Missing"")] string input)
        {
        }
    }");
            var descriptor = new DiagnosticDescriptor(AnalyzerIdentifiers.ValueSourceStringUsage, string.Empty, string.Empty, string.Empty, DiagnosticSeverity.Warning, true);
            AnalyzerAssert.Valid(analyzer, descriptor, testCode);
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

        [Test]
        public void Test([ValueSource(↓""TestCases"")] string input)
        {{
        }}
    }}");

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public class AnalyzeWhenStringConstant
    {{
        {testCaseMember}

        [Test]
        public void Test([ValueSource(nameof(TestCases))] string input)
        {{
        }}
    }}");

            var message = "Consider using nameof(TestCases) instead of \"TestCases\".";
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnosticCodeFix.WithMessage(message), testCode, fixedCode);
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

        [Test]
        public void Test([ValueSource(↓""TestCases"")] string input)
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

        [Test]
        public void Test([ValueSource(nameof(TestCases))] string input)
        {
        }
    }");

            var message = "Consider using nameof(TestCases) instead of \"TestCases\".";
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnosticCodeFix.WithMessage(message), testCode, fixedCode);
        }

        [Test]
        public void AnalyzeWhenMethodExpectsParameters()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    [TestFixture]
    public class AnalyzeWhenMethodExpectsParameters
    {
        [Test]
        public void ShortName([ValueSource(↓nameof(TestData))] int number)
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
                .Create(AnalyzerIdentifiers.ValueSourceMethodExpectParameters)
                .WithMessage("The ValueSource cannot supply parameters, but the target method expects '1' parameter(s).");
            AnalyzerAssert.Diagnostics<ValueSourceUsageAnalyzer>(expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenMethodExpectsParametersInAnotherClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    [TestFixture]
    public class AnalyzeWhenMethodExpectsParametersInAnotherClass
    {
        [Test]
        public void ShortName([ValueSource(typeof(AnotherClass), ↓nameof(AnotherClass.TestData))] int number)
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
                .Create(AnalyzerIdentifiers.ValueSourceMethodExpectParameters)
                .WithMessage("The ValueSource cannot supply parameters, but the target method expects '2' parameter(s).");
            AnalyzerAssert.Diagnostics<ValueSourceUsageAnalyzer>(expectedDiagnostic, testCode);
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

        [Test]
        public void Test([ValueSource(nameof(TestCases))] int number)
        {{
        }}
    }}");

            AnalyzerAssert.Valid<ValueSourceUsageAnalyzer>(testCode);
        }

        [TestCase("private static readonly object TestCases = null;", "object")]
        [TestCase("private static object TestCases => null;", "object")]
        [TestCase("private static object TestCases() => null;", "object")]
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
        {testCaseMember}

        [Test]
        public void Test([ValueSource(nameof(TestCases))] int number)
        {{
        }}
    }}");

            var expectedDiagnostic = ExpectedDiagnostic
                .Create(AnalyzerIdentifiers.ValueSourceDoesNotReturnIEnumerable)
                .WithMessage($"The ValueSource does not return an IEnumerable or a type that implements IEnumerable. Instead it returns a '{returnType}'.");
            AnalyzerAssert.Diagnostics<ValueSourceUsageAnalyzer>(expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenSourceIsInAnotherClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenSourceIsInAnotherClass
    {
        [Test]
        public void DivideTest([ValueSource(typeof(AnotherClass), ""Numbers"")] int n)
        {
            Assert.That(n, Is.GreaterThan(0));
        }
    }

    class AnotherClass
    {
        static int[] Numbers => new int[] { 1, 2, 3 };
    }");
            AnalyzerAssert.Valid<ValueSourceUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenSourceIsInNestedClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenSourceIsInNestedClass
    {
        [Test]
        public void DivideTest([ValueSource(typeof(AnotherClass), ""Numbers"")] int n)
        {
            Assert.That(n, Is.GreaterThan(0));
        }

        class AnotherClass
        {
            static int[] Numbers => new int[] { 1, 2, 3 };
        }
    }");
            AnalyzerAssert.Valid<ValueSourceUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenAllInformationIsProvidedByAttribute()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenAllInformationIsProvidedByAttribute
    {
        [Test]
        public void ShortName([ValueSource(typeof(AnotherClass), ""TestStrings"")] string name)
        {
            Assert.That(name.Length, Is.LessThan(15));
        }
    }

    class AnotherClass
    {
        static IEnumerable<string> TestStrings()
        {
            yield return ""SomeName"";
            yield return ""YetAnotherName"";
        }
    }", additionalUsings: "using System.Collections.Generic;");
            AnalyzerAssert.Valid<ValueSourceUsageAnalyzer>(testCode);
        }

        [Test]
        public void FixWhenStringLiteralTargetsSourceInAnInnerClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public class FixWhenStringLiteralTargetsSourceInAnInnerClass
    {{
        [Test]
        public void TestData([ValueSource(typeof(InnerClass), ""TestCases"")] string input) {{ }}

        public class InnerClass
        {{
            public static string[] TestCases = new[] {{ ""Data"" }};
        }}
    }}");

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public class FixWhenStringLiteralTargetsSourceInAnInnerClass
    {{
        [Test]
        public void TestData([ValueSource(typeof(InnerClass), nameof(InnerClass.TestCases))] string input) {{ }}

        public class InnerClass
        {{
            public static string[] TestCases = new[] {{ ""Data"" }};
        }}
    }}");

            var message = "Consider using nameof(InnerClass.TestCases) instead of \"TestCases\".";
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnosticCodeFix.WithMessage(message), testCode, fixedCode);
        }

        [Test]
        public void FixWhenStringLiteralTargetsSourceInAnotherClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public class FixWhenStringLiteralTargetsSourceInAnotherClass
    {{
        [Test]
        public void TestData([ValueSource(typeof(AnotherClass), ""TestCases"")] string input) {{ }}
    }}

    public class AnotherClass
    {{
        public static string[] TestCases = new[] {{ ""Data"" }};
    }}");

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public class FixWhenStringLiteralTargetsSourceInAnotherClass
    {{
        [Test]
        public void TestData([ValueSource(typeof(AnotherClass), nameof(AnotherClass.TestCases))] string input) {{ }}
    }}

    public class AnotherClass
    {{
        public static string[] TestCases = new[] {{ ""Data"" }};
    }}");

            var message = "Consider using nameof(AnotherClass.TestCases) instead of \"TestCases\".";
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnosticCodeFix.WithMessage(message), testCode, fixedCode);
        }

        [Test]
        public void FixWhenStringLiteralTargetsSourceInAnInnerClassInAnotherClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public class FixWhenStringLiteralTargetsSourceInAnInnerClassInAnotherClass
    {{
        [Test]
        public void TestData([ValueSource(typeof(AnotherClass.InnerClass), ""TestCases"")] string input) {{ }}
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
        [Test]
        public void TestData([ValueSource(typeof(AnotherClass.InnerClass), nameof(AnotherClass.InnerClass.TestCases))] string input) {{ }}
    }}

    public class AnotherClass
    {{
        public class InnerClass
        {{
            public static string[] TestCases = new[] {{ ""Data"" }};
        }}
    }}");

            var message = "Consider using nameof(AnotherClass.InnerClass.TestCases) instead of \"TestCases\".";
            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnosticCodeFix.WithMessage(message), testCode, fixedCode);
        }
    }
}
