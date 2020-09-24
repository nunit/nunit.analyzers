using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TestMethodUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.TestMethodUsage
{
    [TestFixture]
    public sealed class TestMethodUsageAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new TestMethodUsageAnalyzer();

        private static IEnumerable<TestCaseData> SpecialConversions
        {
            get
            {
                yield return new TestCaseData("2019-10-10", typeof(DateTime));
                yield return new TestCaseData("23:59:59", typeof(TimeSpan));
                yield return new TestCaseData("2019-10-10", typeof(DateTimeOffset));
                yield return new TestCaseData("2019-10-14T19:15:25+00:00", typeof(DateTimeOffset));
            }
        }

        [Test]
        public void VerifySupportedDiagnostics()
        {
            var analyzer = new TestMethodUsageAnalyzer();
            var diagnostics = analyzer.SupportedDiagnostics;

            var expectedIdentifiers = new List<string>
            {
                AnalyzerIdentifiers.TestMethodExpectedResultTypeMismatchUsage,
                AnalyzerIdentifiers.TestMethodSpecifiedExpectedResultForVoidUsage,
                AnalyzerIdentifiers.TestMethodNoExpectedResultButNonVoidReturnType,
                AnalyzerIdentifiers.TestMethodAsyncNoExpectedResultAndVoidReturnTypeUsage,
                AnalyzerIdentifiers.TestMethodAsyncNoExpectedResultAndNonTaskReturnTypeUsage,
                AnalyzerIdentifiers.TestMethodAsyncExpectedResultAndNonGenricTaskReturnTypeUsage,
                AnalyzerIdentifiers.SimpleTestMethodHasParameters
            };
            CollectionAssert.AreEquivalent(expectedIdentifiers, diagnostics.Select(d => d.Id));

            foreach (var diagnostic in diagnostics)
            {
                Assert.That(diagnostic.Title.ToString(CultureInfo.InvariantCulture), Is.Not.Empty,
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Title)}");
                Assert.That(diagnostic.Category, Is.EqualTo(Categories.Structure),
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Category)}");
                Assert.That(diagnostic.DefaultSeverity, Is.EqualTo(DiagnosticSeverity.Error),
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.DefaultSeverity)}");
            }

            var diagnosticMessage = diagnostics.Select(_ => _.MessageFormat.ToString(CultureInfo.InvariantCulture)).ToImmutableArray();

            Assert.That(diagnosticMessage, Contains.Item(TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage),
                $"{TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage} is missing.");
            Assert.That(diagnosticMessage, Contains.Item(TestMethodUsageAnalyzerConstants.SpecifiedExpectedResultForVoidMethodMessage),
                $"{TestMethodUsageAnalyzerConstants.SpecifiedExpectedResultForVoidMethodMessage} is missing.");
        }

        [TestCaseSource(nameof(SpecialConversions))]
        public void AnalyzeWhenExpectedResultIsProvidedCorrectlyWithSpecialConversion(string value, Type targetType)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public sealed class AnalyzeWhenExpectedResultIsProvidedCorrectlyWithSpecialConversion
    {{
        [TestCase(""{value}"", ExpectedResult = ""{value}"")]
        public {targetType.Name} Test({targetType.Name} a) {{ return a; }}
    }}");
            AnalyzerAssert.Valid<TestMethodUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenExpectedResultIsProvidedCorrectly()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenExpectedResultIsProvidedCorrectly
    {
        [TestCase(2, ExpectedResult = 3)]
        public int Test(int a) { return 3; }
    }");
            AnalyzerAssert.Valid<TestMethodUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenExpectedResultIsProvidedAndReturnTypeIsVoid()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestMethodSpecifiedExpectedResultForVoidUsage,
                TestMethodUsageAnalyzerConstants.SpecifiedExpectedResultForVoidMethodMessage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenExpectedResultIsProvidedAndReturnTypeIsVoid
    {
        [TestCase(2, ↓ExpectedResult = '3')]
        public void Test(int a) { }
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenExpectedResultIsProvidedAndTypeIsIncorrect()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestMethodExpectedResultTypeMismatchUsage,
                string.Format(CultureInfo.InvariantCulture,
                    TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage, typeof(int).Name));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenExpectedResultIsProvidedAndTypeIsIncorrect
    {
        [TestCase(2, ↓ExpectedResult = '3')]
        public int Test(int a) { return 3; }
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenExpectedResultIsProvidedAndPassesNullToValueType()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestMethodExpectedResultTypeMismatchUsage,
                string.Format(CultureInfo.InvariantCulture,
                    TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage, typeof(int).Name));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenExpectedResultIsProvidedAndPassesNullToValueType
    {
        [TestCase(2, ↓ExpectedResult = null)]
        public int Test(int a) { return 3; }
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenExpectedResultIsProvidedAndPassesNullToNullableType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenExpectedResultIsProvidedAndPassesNullToNullableType
    {
        [TestCase(2, ExpectedResult = null)]
        public int? Test(int a) { return null; }
    }");
            AnalyzerAssert.Valid<TestMethodUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenExpectedResultIsProvidedAndPassesValueToNullableType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenExpectedResultIsProvidedAndPassesValueToNullableType
    {
        [TestCase(2, ExpectedResult = 2)]
        public int? Test(int a) { return 2; }
    }");
            AnalyzerAssert.Valid<TestMethodUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenTestAttributeExpectedResultIsNotProvidedAndReturnTypeIsNotVoid()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestMethodNoExpectedResultButNonVoidReturnType);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenTestAttributeExpectedResultIsNotProvidedAndReturnTypeIsNotVoid
    {
        [↓Test]
        public string Test3() => ""12"";
    }");
            var message = "The method has non-void return type 'string', but no result is expected in ExpectedResult.";
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic.WithMessage(message), testCode);
        }

        [Test]
        public void AnalyzeWhenTestCaseAttributeExpectedResultIsNotProvidedAndReturnTypeIsNotVoid()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestMethodNoExpectedResultButNonVoidReturnType);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenTestCaseAttributeExpectedResultIsNotProvidedAndReturnTypeIsNotVoid
    {
        [↓TestCase(1)]
        public int Test4(int i) => ""12"";
    }");
            var message = "The method has non-void return type 'int', but no result is expected in ExpectedResult.";
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic.WithMessage(message), testCode);
        }

        [Test]
        public void AnalyzeWhenAsyncTestMethodHasGenericTaskReturnTypeAndNoExpectedResult()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestMethodAsyncNoExpectedResultAndNonTaskReturnTypeUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenAsyncTestMethodHasGenericTaskReturnTypeAndNotExpectedResult
    {
        [↓Test]
        public async Task<int> AsyncGenericTaskTest() => await Task.FromResult(1);
    }");
            var message = "The async test method must have a non-generic Task return type when no result is expected, but the return type was 'System.Threading.Tasks.Task<int>'.";
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic.WithMessage(message), testCode);
        }

        [Test]
        public void AnalyzeWhenTestMethodHasGenericTaskReturnTypeAndNoExpectedResult()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestMethodAsyncNoExpectedResultAndNonTaskReturnTypeUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenTestMethodHasGenericTaskReturnTypeAndNotExpectedResult
    {
        [↓Test]
        public Task<int> AsyncGenericTaskTest() => Task.FromResult(1);
    }");
            var message = "The async test method must have a non-generic Task return type when no result is expected, but the return type was 'System.Threading.Tasks.Task<int>'.";
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic.WithMessage(message), testCode);
        }

        [Test]
        public void AnalyzeWhenAsyncTestMethodHasVoidReturnTypeAndNoExpectedResult()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestMethodAsyncNoExpectedResultAndVoidReturnTypeUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenAsyncTestMethodHasVoidReturnTypeAndNoExpectedResult
    {
        [↓Test]
        public async void AsyncVoidTest() => await Task.FromResult(1);
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenAsyncTestCaseMethodHasVoidReturnTypeAndNoExpectedResult()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestMethodAsyncNoExpectedResultAndVoidReturnTypeUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenAsyncTestCaseMethodHasVoidReturnTypeAndNoExpectedResult
    {
        [↓TestCase(4)]
        public async void AsyncVoidTest(int x) => await Task.FromResult(1);
    }");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenTestMethodHasTaskReturnTypeAndNoExpectedResult()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenTestMethodHasTaskReturnTypeAndNoExpectedResult
    {
        [TestCase(100, 200)]
        [TestCase(0, 0)]
        public async Task ValidAsyncTest(int low, int high)
        {
        }
    }");
            AnalyzerAssert.Valid<TestMethodUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenAsyncTestMethodHasGenericTaskReturnTypeAndExpectedResult()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenAsyncTestMethodHasGenericTaskReturnTypeAndExpectedResult
    {
        [TestCase(ExpectedResult = 1)]
        public async Task<int> AsyncGenericTaskTestCaseWithExpectedResult()
        {
          return await Task.Run(() => 1);
        }
    }");
            AnalyzerAssert.Valid<TestMethodUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenTestMethodHasGenericTaskReturnTypeAndExpectedResult()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenTestMethodHasGenericTaskReturnTypeAndExpectedResult
    {
        [TestCase(ExpectedResult = 1)]
        public Task<int> GenericTaskTestCaseWithExpectedResult()
        {
          return Task.Run(() => 1);
        }
    }");
            AnalyzerAssert.Valid<TestMethodUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenTestMethodHasCustomAwaitableReturnTypeAndExpectedResult()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenTestMethodHasCustomAwaitableReturnTypeAndExpectedResult
    {
        [TestCase(ExpectedResult = 1)]
        public CustomAwaitable GenericTaskTestCaseWithExpectedResult()
        {
            return new CustomAwaitable();
        }
    }

    public class CustomAwaitable
    {
        public System.Runtime.CompilerServices.TaskAwaiter<int> GetAwaiter()
        {
            return Task.FromResult(1).GetAwaiter();
        }
    }
");
            AnalyzerAssert.Valid<TestMethodUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenTestMethodHasCustomAwaitableReturnTypeAndExpectedResultIsIncorrect()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestMethodExpectedResultTypeMismatchUsage,
                string.Format(CultureInfo.InvariantCulture,
                    TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage, typeof(int).Name));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenTestMethodHasCustomAwaitableReturnTypeAndExpectedResultIsIncorrect
    {
        [TestCase(ExpectedResult = '1')]
        public CustomAwaitable GenericTaskTestCaseWithExpectedResult()
        {
            return new CustomAwaitable();
        }
    }

    public class CustomAwaitable
    {
        public System.Runtime.CompilerServices.TaskAwaiter<int> GetAwaiter()
        {
            return Task.FromResult(1).GetAwaiter();
        }
    }
");
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

#if !NET461
        [Test]
        public void AnalyzeWhenTestMethodHasValueTaskReturnTypeAndExpectedResult()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                [TestCase(ExpectedResult = 1)]
                public async ValueTask<int> GenericTaskTestCaseWithExpectedResult()
                {
                    return 1;
                }");

            AnalyzerAssert.Valid<TestMethodUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenTestMethodHasValueTaskReturnTypeAndExpectedResultIsIncorrect()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestMethodExpectedResultTypeMismatchUsage,
                string.Format(CultureInfo.InvariantCulture,
                    TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage, typeof(int).Name));

            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                [TestCase(ExpectedResult = '1')]
                public async ValueTask<int> GenericTaskTestCaseWithExpectedResult()
                {
                    return 1;
                }");

            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }
#endif

        [Test]
        public void AnalyzeWhenAsyncTestMethodHasTaskReturnTypeAndExpectedResult()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestMethodAsyncExpectedResultAndNonGenricTaskReturnTypeUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenAsyncTestMethodHasTaskReturnTypeAndExpectedResult
    {
        [↓TestCase(ExpectedResult = 1)]
        public async Task AsyncTaskTestCaseWithExpectedResult()
        {
          await Task.Run(() => 1);
        }
    }");
            var message = "The async test method must have a Task<T> return type when a result is expected, but the return type was 'System.Threading.Tasks.Task'.";
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic.WithMessage(message), testCode);
        }

        [Test]
        public void AnalyzeWhenTestMethodHasTaskReturnTypeAndExpectedResult()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestMethodAsyncExpectedResultAndNonGenricTaskReturnTypeUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenTestMethodHasTaskReturnTypeAndExpectedResult
    {
        [↓TestCase(ExpectedResult = 1)]
        public Task TaskTestCaseWithExpectedResult()
        {
          return Task.Run(() => 1);
        }
    }");
            var message = "The async test method must have a Task<T> return type when a result is expected, but the return type was 'System.Threading.Tasks.Task'.";
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic.WithMessage(message), testCode);
        }

        [Test]
        public void AnalyzeWhenTestMethodHasTypeParameterAsReturnType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenTestMethodHasTypeParameterAsReturnType
    {
        [TestCase(1, ExpectedResult = 1)]
        public T TestWithGenericReturnType<T>(T arg1)
        {
            return arg1;
        }
    }");
            AnalyzerAssert.Valid<TestMethodUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenTestMethodAlsoHasTestCaseSourceAttribute()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenTestMethodAlsoHasTestCaseSourceAttribute
    {
        public static IEnumerable MyTestSource
        {
            get
            {
                yield return new TestCaseData(1).Returns(true);
                yield return new TestCaseData(0).Returns(false);
            }
        }

        [Test]
        [TestCaseSource(typeof(AnalyzeWhenTestMethodAlsoHasTestCaseSourceAttribute), nameof(MyTestSource))]
        public bool Value_ShouldEqual_One(int value)
        {
            return value == 1;
        }
    }",
    additionalUsings: "using System.Collections;");
            AnalyzerAssert.Valid<TestMethodUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenTestMethodAlsoHasTestCaseAttribute()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenTestMethodAlsoHasTestCaseAttribute
    {
        [Test]
        [TestCase(1, ExpectedResult = true)]
        public bool Value_ShouldEqual_One(int value)
        {
            return value == 1;
        }
    }");
            AnalyzerAssert.Valid<TestMethodUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenTestMethodAlsoHasCustomAttributeDerivingFromITestBuilder()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenTestMethodAlsoHasCustomAttributeDerivingFromITestBuilder
    {
        [Test]
        [SomeTestBuilderAttribute]
        public bool Value_ShouldEqual_One(int value)
        {
            return value == 1;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class SomeTestBuilderAttribute : NUnitAttribute, ITestBuilder
    {
        public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test suite)
        {
            return Array.Empty<TestMethod>();
        }
    }",
    additionalUsings: "using NUnit.Framework.Interfaces; using NUnit.Framework.Internal; using System.Collections.Generic;");
            AnalyzerAssert.Valid<TestMethodUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenSimpleTestMethodHasParameters()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
               AnalyzerIdentifiers.SimpleTestMethodHasParameters);

            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
    [↓Test]
    [Category(""Test"")]
    public void M(int p)
    {
        Assert.That(p, Is.EqualTo(42));
    }");

            var message = "The test method has '1' parameter(s), but only '0' argument(s) are supplied by attributes.";
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic.WithMessage(message), testCode);
        }

        [Test]
        public void AnalyzeWhenSimpleTestMethodHasParameterWithOutSuppliedValues()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
               AnalyzerIdentifiers.SimpleTestMethodHasParameters);

            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
    [↓Test]
    public void M(int p, [Values(1, 2, 3)] int q, int r)
    {
        Assert.That(p, Is.EqualTo(q + r));
    }");

            var message = "The test method has '3' parameter(s), but only '1' argument(s) are supplied by attributes.";
            AnalyzerAssert.Diagnostics(analyzer, expectedDiagnostic.WithMessage(message), testCode);
        }

        [Test]
        public void AnalyzeWhenSimpleTestMethodHasParameterSuppliedByValuesAttribute()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
    [Test]
    public void M([Values(1, 2, 3)] int p)
    {
        Assert.That(p, Is.EqualTo(42));
    }");

            AnalyzerAssert.Valid<TestMethodUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenSimpleTestMethodHasParameterSuppliedByRangeAttribute()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
    [Test]
    public void M([Range(1, 10)] int p)
    {
        Assert.That(p, Is.EqualTo(42));
    }");

            AnalyzerAssert.Valid<TestMethodUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenSimpleTestMethodHasParameterSuppliedByTestCase()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public void M(int p)
    {
        Assert.That(p, Is.EqualTo(42));
    }");

            AnalyzerAssert.Valid<TestMethodUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenSimpleTestMethodHasParameterSuppliedByTestCaseSource()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenSimpleTestMethodHasParameterSuppliedByTestCaseSource
    {
        [Test]
        [TestCaseSource(nameof(Source))]
        public void M(int p)
        {
            Assert.That(p, Is.EqualTo(42));
        }

        private static int[] Source => new[] { 1, 2, 3 };
    }");

            AnalyzerAssert.Valid<TestMethodUsageAnalyzer>(testCode);
        }
    }
}
