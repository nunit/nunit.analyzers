using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TestMethodUsage;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.TestMethodUsage.TestMethodUsageAnalyzer>;

namespace NUnit.Analyzers.Tests.TestMethodUsage
{
    [TestFixture]
    public sealed class TestMethodUsageAnalyzerTests
    {
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
            var diagnostics = new TestMethodUsageAnalyzer().SupportedDiagnostics;

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

            Assert.That(diagnostics.Select(d => d.Id), Is.EquivalentTo(expectedIdentifiers));

            Assert.Multiple(() =>
            {
                foreach (var diagnostic in diagnostics)
                {
                    Assert.That(diagnostic.Title.ToString(CultureInfo.InvariantCulture), Is.Not.Empty,
                        $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Title)}");
                    Assert.That(diagnostic.Category, Is.EqualTo(Categories.Structure),
                        $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Category)}");
                    Assert.That(diagnostic.DefaultSeverity, Is.EqualTo(DiagnosticSeverity.Error),
                        $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.DefaultSeverity)}");
                }
            });

            var diagnosticMessage = diagnostics.Select(_ => _.MessageFormat.ToString(CultureInfo.InvariantCulture)).ToImmutableArray();

            Assert.Multiple(() =>
            {
                Assert.That(diagnosticMessage, Contains.Item(TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage),
                    $"{TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage} is missing.");
                Assert.That(diagnosticMessage, Contains.Item(TestMethodUsageAnalyzerConstants.SpecifiedExpectedResultForVoidMethodMessage),
                    $"{TestMethodUsageAnalyzerConstants.SpecifiedExpectedResultForVoidMethodMessage} is missing.");
            });
        }

        [TestCaseSource(nameof(SpecialConversions))]
        public async Task AnalyzeWhenExpectedResultIsProvidedCorrectlyWithSpecialConversion(string value, Type targetType)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public sealed class AnalyzeWhenExpectedResultIsProvidedCorrectlyWithSpecialConversion
                {
                    [TestCase("{{value}}", ExpectedResult = "{{value}}")]
                    public {{targetType.Name}} Test({{targetType.Name}} a) { return a; }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenExpectedResultIsProvidedCorrectly()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenExpectedResultIsProvidedCorrectly
                {
                    [TestCase(2, ExpectedResult = 3)]
                    public int Test(int a) { return 3; }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenExpectedResultIsProvidedAndReturnTypeIsVoid()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestMethodSpecifiedExpectedResultForVoidUsage).WithMessage(TestMethodUsageAnalyzerConstants.SpecifiedExpectedResultForVoidMethodMessage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenExpectedResultIsProvidedAndReturnTypeIsVoid
                {
                    [TestCase(2, {|#0:ExpectedResult = '3'|})]
                    public void Test(int a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenExpectedResultIsProvidedAndTypeIsIncorrect()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestMethodExpectedResultTypeMismatchUsage).WithMessage(string.Format(CultureInfo.InvariantCulture,
                    TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage, typeof(int).Name));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenExpectedResultIsProvidedAndTypeIsIncorrect
                {
                    [TestCase(2, {|#0:ExpectedResult = '3'|})]
                    public int Test(int a) { return 3; }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenExpectedResultIsProvidedAndPassesNullToValueType()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestMethodExpectedResultTypeMismatchUsage).WithMessage(string.Format(CultureInfo.InvariantCulture,
                    TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage, typeof(int).Name));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenExpectedResultIsProvidedAndPassesNullToValueType
                {
                    [TestCase(2, {|#0:ExpectedResult = null|})]
                    public int Test(int a) { return 3; }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenExpectedResultIsProvidedAndPassesNullToNullableType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenExpectedResultIsProvidedAndPassesNullToNullableType
                {
                    [TestCase(2, ExpectedResult = null)]
                    public int? Test(int a) { return null; }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenExpectedResultIsProvidedAndPassesValueToNullableType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenExpectedResultIsProvidedAndPassesValueToNullableType
                {
                    [TestCase(2, ExpectedResult = 2)]
                    public int? Test(int a) { return 2; }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenTestAttributeExpectedResultIsNotProvidedAndReturnTypeIsNotVoid()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestMethodNoExpectedResultButNonVoidReturnType);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenTestAttributeExpectedResultIsNotProvidedAndReturnTypeIsNotVoid
                {
                    [{|#0:Test|}]
                    public string Test3() => "12";
                }
            """);
            var message = "The method has non-void return type 'string', but no result is expected in ExpectedResult";
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithMessage(message).WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenTestCaseAttributeExpectedResultIsNotProvidedAndReturnTypeIsNotVoid()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestMethodNoExpectedResultButNonVoidReturnType);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenTestCaseAttributeExpectedResultIsNotProvidedAndReturnTypeIsNotVoid
                {
                    [{|#0:TestCase(1)|}]
                    public int Test4(int i) => 12;
                }
            """);
            var message = "The method has non-void return type 'int', but no result is expected in ExpectedResult";
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithMessage(message).WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenAsyncTestMethodHasGenericTaskReturnTypeAndNoExpectedResult()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestMethodAsyncNoExpectedResultAndNonTaskReturnTypeUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenAsyncTestMethodHasGenericTaskReturnTypeAndNotExpectedResult
                {
                    [{|#0:Test|}]
                    public async Task<int> AsyncGenericTaskTest() => await Task.FromResult(1);
                }
            """);
            var message = "The async test method must have a non-generic Task return type when no result is expected, but the return type was 'System.Threading.Tasks.Task<int>'";
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithMessage(message).WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenTestMethodHasGenericTaskReturnTypeAndNoExpectedResult()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestMethodAsyncNoExpectedResultAndNonTaskReturnTypeUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenTestMethodHasGenericTaskReturnTypeAndNotExpectedResult
                {
                    [{|#0:Test|}]
                    public Task<int> AsyncGenericTaskTest() => Task.FromResult(1);
                }
            """);
            var message = "The async test method must have a non-generic Task return type when no result is expected, but the return type was 'System.Threading.Tasks.Task<int>'";
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithMessage(message).WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenAsyncTestMethodHasVoidReturnTypeAndNoExpectedResult()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestMethodAsyncNoExpectedResultAndVoidReturnTypeUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenAsyncTestMethodHasVoidReturnTypeAndNoExpectedResult
                {
                    [{|#0:Test|}]
                    public async void AsyncVoidTest() => await Task.FromResult(1);
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenAsyncTestCaseMethodHasVoidReturnTypeAndNoExpectedResult()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestMethodAsyncNoExpectedResultAndVoidReturnTypeUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenAsyncTestCaseMethodHasVoidReturnTypeAndNoExpectedResult
                {
                    [{|#0:TestCase(4)|}]
                    public async void AsyncVoidTest(int x) => await Task.FromResult(1);
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenTestMethodHasTaskReturnTypeAndNoExpectedResult()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenTestMethodHasTaskReturnTypeAndNoExpectedResult
                {
                    [TestCase(100, 200)]
                    [TestCase(0, 0)]
                    public async Task ValidAsyncTest(int low, int high)
                    {
                        await Task.CompletedTask;
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenAsyncTestMethodHasGenericTaskReturnTypeAndExpectedResult()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenAsyncTestMethodHasGenericTaskReturnTypeAndExpectedResult
                {
                    [TestCase(ExpectedResult = 1)]
                    public async Task<int> AsyncGenericTaskTestCaseWithExpectedResult()
                    {
                        return await Task.Run(() => 1);
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenTestMethodHasGenericTaskReturnTypeAndExpectedResult()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenTestMethodHasGenericTaskReturnTypeAndExpectedResult
                {
                    [TestCase(ExpectedResult = 1)]
                    public Task<int> GenericTaskTestCaseWithExpectedResult()
                    {
                      return Task.Run(() => 1);
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenTestMethodHasCustomAwaitableReturnTypeAndExpectedResult()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenTestMethodHasCustomAwaitableReturnTypeAndExpectedResultIsIncorrect()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestMethodExpectedResultTypeMismatchUsage).WithMessage(string.Format(CultureInfo.InvariantCulture,
                    TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage, typeof(int).Name));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenTestMethodHasCustomAwaitableReturnTypeAndExpectedResultIsIncorrect
                {
                    [TestCase({|#0:ExpectedResult = '1'|})]
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
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

#if !NETFRAMEWORK
        [Test]
        public async Task AnalyzeWhenTestMethodHasValueTaskReturnTypeAndExpectedResult()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [TestCase(ExpectedResult = 1)]
                    public async ValueTask<int> GenericTaskTestCaseWithExpectedResult()
                    {
                        await Task.CompletedTask;
                        return 1;
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenTestMethodHasValueTaskReturnTypeAndExpectedResultIsIncorrect()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestMethodExpectedResultTypeMismatchUsage).WithMessage(string.Format(CultureInfo.InvariantCulture,
                    TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage, typeof(int).Name));

            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [TestCase({|#0:ExpectedResult = '1'|})]
                    public async ValueTask<int> GenericTaskTestCaseWithExpectedResult()
                    {
                        await Task.CompletedTask;
                        return 1;
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }
#endif

        [Test]
        public async Task AnalyzeWhenAsyncTestMethodHasTaskReturnTypeAndExpectedResult()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestMethodAsyncExpectedResultAndNonGenricTaskReturnTypeUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenAsyncTestMethodHasTaskReturnTypeAndExpectedResult
                {
                    [{|#0:TestCase(ExpectedResult = 1)|}]
                    public async Task AsyncTaskTestCaseWithExpectedResult()
                    {
                      await Task.Run(() => 1);
                    }
                }
            """);
            var message = "The async test method must have a Task<T> return type when a result is expected, but the return type was 'System.Threading.Tasks.Task'";
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithMessage(message).WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenTestMethodHasTaskReturnTypeAndExpectedResult()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestMethodAsyncExpectedResultAndNonGenricTaskReturnTypeUsage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenTestMethodHasTaskReturnTypeAndExpectedResult
                {
                    [{|#0:TestCase(ExpectedResult = 1)|}]
                    public Task TaskTestCaseWithExpectedResult()
                    {
                      return Task.Run(() => 1);
                    }
                }
            """);
            var message = "The async test method must have a Task<T> return type when a result is expected, but the return type was 'System.Threading.Tasks.Task'";
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithMessage(message).WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenTestMethodHasTypeParameterAsReturnType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenTestMethodHasTypeParameterAsReturnType
                {
                    [TestCase(1, ExpectedResult = 1)]
                    public T TestWithGenericReturnType<T>(T arg1)
                    {
                        return arg1;
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenTestMethodAlsoHasTestCaseSourceAttribute()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
                }
            """,
    additionalUsings: "using System.Collections;");
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenTestMethodAlsoHasTestCaseAttribute()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenTestMethodAlsoHasTestCaseAttribute
                {
                    [Test]
                    [TestCase(1, ExpectedResult = true)]
                    public bool Value_ShouldEqual_One(int value)
                    {
                        return value == 1;
                    }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenTestMethodAlsoHasCustomAttributeDerivingFromITestBuilder()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
                    public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test? suite)
                    {
                        return Array.Empty<TestMethod>();
                    }
                }
            """,
    additionalUsings: "using NUnit.Framework.Interfaces; using NUnit.Framework.Internal; using System.Collections.Generic;");
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenSimpleTestMethodHasParameters()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.SimpleTestMethodHasParameters);

            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [{|#0:Test|}]
                    [Category("Test")]
                    public void M(int p)
                    {
                        Assert.That(p, Is.EqualTo(42));
                    }
            """);

            var message = "The test method has '1' parameter(s), but only '0' argument(s) are supplied by attributes";
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithMessage(message).WithLocation(0));
        }

#if NUNIT4
        [Test]
        public async Task WhenTestMethodHasImplicitlySuppliedCancellationTokenParameterDueToCancelAfterOnMethod()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    [CancelAfter(50)]
                    public async Task InfiniteLoopWith50msCancelAfter(CancellationToken cancellationToken)
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                        }
                    }
            """, "using System.Threading;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task WhenTestMethodHasImplicitlySuppliedCancellationTokenParameterDueToCancelAfterOnClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                [CancelAfter(50)]
                public class TestClass
                {
                    [Test]
                    public async Task InfiniteLoopWith50msCancelAfter(CancellationToken cancellationToken)
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            """, "using System.Threading;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task WhenTestMethodHasImplicitlySuppliedCancellationTokenParameterDueToCancelAfterOnBaseClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [CancelAfter(50)]
                public abstract class BaseClass
                {
                }

                [TestFixture]
                public class TestClass : BaseClass
                {
                    [Test]
                    public async Task InfiniteLoopWith50msCancelAfter(CancellationToken cancellationToken)
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            """, "using System.Threading;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenSimpleTestMethodHasParameterSuppliedByValuesAndCancelAfterAttributes()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    [CancelAfter(10)]
                    public void M([Values(1, 2, 3)] int p, CancellationToken cancellationToken)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            Assert.Ignore("Cancelled");
                        Assert.That(p, Is.EqualTo(42));
                    }
            """, "using System.Threading;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task WhenTestMethodHasNoImplicitlySuppliedCancellationTokenParameter()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    [CancelAfter(50)]
                    public async Task InfiniteLoopWith50msCancelAfter()
                    {
                        CancellationToken cancellationToken = TestContext.CurrentContext.CancellationToken;
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                        }
                    }
            """, "using System.Threading;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task WhenTestMethodHasCancellationTokenParameter()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.SimpleTestMethodHasParameters);

            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [{|#0:Test|}]
                    public async Task InfiniteLoopWith50msCancelAfter(CancellationToken cancellationToken)
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                        }
                    }
            """, "using System.Threading;");

            var message = "The test method has '1' parameter(s), but only '0' argument(s) are supplied by attributes";
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithMessage(message).WithLocation(0).WithArguments("2", "1"));
        }
#endif

        [Test]
        public async Task AnalyzeWhenSimpleTestMethodHasParameterWithOutSuppliedValues()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.SimpleTestMethodHasParameters);

            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [{|#0:Test|}]
                    public void M(int p, [Values(1, 2, 3)] int q, int r)
                    {
                        Assert.That(p, Is.EqualTo(q + r));
                    }
            """);

            var message = "The test method has '3' parameter(s), but only '1' argument(s) are supplied by attributes";
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithMessage(message).WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenSimpleTestMethodHasParameterSuppliedByValuesAttribute()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public void M([Values(1, 2, 3)] int p)
                    {
                        Assert.That(p, Is.EqualTo(42));
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenSimpleTestMethodHasParameterSuppliedByRangeAttribute()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public void M([Range(1, 10)] int p)
                    {
                        Assert.That(p, Is.EqualTo(42));
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenSimpleTestMethodHasParameterSuppliedByTestCase()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    [TestCase(1)]
                    [TestCase(2)]
                    public void M(int p)
                    {
                        Assert.That(p, Is.EqualTo(42));
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenSimpleTestMethodHasParameterSuppliedByTestCaseSource()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenSimpleTestMethodHasParameterSuppliedByTestCaseSource
                {
                    [Test]
                    [TestCaseSource(nameof(Source))]
                    public void M(int p)
                    {
                        Assert.That(p, Is.EqualTo(42));
                    }

                    private static int[] Source => new[] { 1, 2, 3 };
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
