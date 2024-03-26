using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.DisposeFieldsInTearDown;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.DisposeFieldsInTearDown
{
    public sealed class DisposeFieldsAndPropertiesInTearDownAnalyzerTests
    {
        private const string DummyDisposable = @"
        sealed class DummyDisposable : IDisposable
        {
            public void Dispose() {}
        }";

        private const string DummyExplicitDisposable = @"
        sealed class DummyExplicitDisposable : IAnotherInterface, IDisposable
        {
            void IDisposable.Dispose() {}
        }
        public interface IAnotherInterface
        {
        }";

        // CA2000 allows transfer of ownership using ICollection<IDisposable>.Add
        private const string Disposer = @"
        private sealed class Disposer : IDisposable
        {
            public void Dispose() {}

            public T? Add<T>(T? resource) => resource;
        }
        ";

        private static readonly DiagnosticAnalyzer analyzer = new DisposeFieldsAndPropertiesInTearDownAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.FieldIsNotDisposedInTearDown);

        [Test]
        public void AnalyzeWhenFieldIsDisposedInCorrectMethod(
            [Values("", "OneTime")] string attributePrefix,
            [Values("", "this.")] string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private IDisposable? field;

        [{attributePrefix}SetUp]
        public void SetUpMethod()
        {{
            {prefix}field = new DummyDisposable();
        }}

        [{attributePrefix}TearDown]
        public void TearDownMethod()
        {{
            {prefix}field?.Dispose();
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenFieldIsConditionallyDisposed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private object field = new DummyDisposable();

        [OneTimeTearDown]
        public void TearDownMethod()
        {{
            if (field is IDisposable disposable)
                disposable.Dispose();
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenFieldWithInitializerIsDisposedInOneTimeTearDownMethod()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private IDisposable field = new DummyDisposable();

        [OneTimeTearDown]
        public void TearDownMethod()
        {{
            field.Dispose();
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenFieldSetInConstructorIsDisposedInOneTimeTearDownMethod()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private IDisposable field;

        public TestClass()
        {{
            field = new DummyDisposable();
        }}

        [OneTimeTearDown]
        public void TearDownMethod()
        {{
            field.Dispose();
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenExplicitlyInterfaceImplementedDisposableFieldSetInConstructorIsDisposedInOneTimeTearDownMethod()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private IAnotherInterface field;

        public TestClass()
        {{
            field = new DummyExplicitDisposable();
        }}

        [OneTimeTearDown]
        public void TearDownMethod()
        {{
            ((IDisposable)field).Dispose();
        }}

        {DummyExplicitDisposable}
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase("", "OneTime")]
        [TestCase("OneTime", "")]
        public void AnalyzeWhenFieldIsDisposedInWrongMethod(string attributePrefix1, string attributePrefix2)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private IDisposable? ↓field;

        [{attributePrefix1}SetUp]
        public void SetUpMethod()
        {{
            field = new DummyDisposable();
        }}

        [{attributePrefix2}TearDown]
        public void TearDownMethod()
        {{
            field?.Dispose();
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [TestCase("SetUp")]
        [TestCase("Test")]
        public void AnalyzeWhenFieldIsNotDisposed(string attribute)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private object? ↓field;

        [{attribute}]
        public void SomeMethod()
        {{
            field = new DummyDisposable();
        }}

        [TearDown]
        public void TearDownMethod()
        {{
            field = null;
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [TestCase("SetUp")]
        [TestCase("Test")]
        public void AnalyzeWhenFieldTypeParameterIsNotDisposed(string attribute)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
        public class TestClass<T>
            where T : IDisposable, new()
        {{
            private T? ↓field;

            [{attribute}]
            public void SomeMethod()
            {{
                field = new T();
            }}

            {DummyDisposable}
        }}
        ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenFieldWithInitializerIsNotDisposed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private object? ↓field = new DummyDisposable();

        [TearDown]
        public void TearDownMethod()
        {{
            field = null;
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenFieldSetInConstructorIsNotDisposed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private object? ↓field;

        public TestClass() => field = new DummyDisposable();

        [TearDown]
        public void TearDownMethod()
        {{
            field = null;
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenReadOnlyFieldSetInConstructorIsNotDisposed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private readonly object? ↓field;

        public TestClass() => field = new DummyDisposable();

        [TearDown]
        public void TearDownMethod()
        {{
            Assert.That(field, Is.Not.Null);
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void FieldConditionallyAssignedInCalledLocalMethod()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private object? ↓field;
        private bool initializeField;

        public TestClass(bool initializeField) => this.initializeField = initializeField;

        [SetUp]
        public async Task SetUp()
        {{
            if (initializeField)
                await InitializeField().ConfigureAwait(false);
        }}

        [TearDown]
        public void TearDownMethod()
        {{
            field = null;
        }}

        private Task InitializeField()
        {{
            field = new DummyDisposable();
            return Task.CompletedTask;
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenPropertyBackingFieldIsDisposed(
            [Values("field", "Property")] string fieldOrProperty)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private IDisposable? field;

        private IDisposable? Property
        {{
            get => field;
            set => this.field = value;
        }}

        [SetUp]
        public void SetUpMethod()
        {{
            {fieldOrProperty} = new DummyDisposable();
        }}

        [TearDown]
        public void TearDownMethod()
        {{
            {fieldOrProperty}?.Dispose();
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenFieldIsDisposedInSpecialExtensionMethod()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public class TestClass
    {{
        private object? field;

        [SetUp]
        public void SetUpMethod()
        {{
            field = new DummyDisposable();
        }}

        [TearDown]
        public void TearDownMethod()
        {{
            field.DisposeIfDisposable();
        }}

        {DummyDisposable}
    }}

    public static class DisposableExtensions
    {{
        public static void DisposeIfDisposable<T>(this T instance)
        {{
            if (instance is IDisposable disposable)
                disposable.Dispose();
        }}
    }}
        ");

            const string AnalyzerConfig = "dotnet_diagnostic.NUnit1032.additional_dispose_methods = DisposeIfDisposable";
            Settings settings = Settings.Default.WithAnalyzerConfig(AnalyzerConfig);
            RoslynAssert.Valid(analyzer, testCode, settings);
        }

        [Test]
        public void AnalyzeWhenFieldIsDisposedInSpecialMethodWithParameter()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private object? field;

        [SetUp]
        public void SetUpMethod()
        {{
            field = new DummyDisposable();
        }}

        [TearDown]
        public void TearDownMethod()
        {{
            Release(field);
        }}

        private void Release<T>(T instance)
        {{
            if (instance is IDisposable disposable)
                disposable.Dispose();
        }}
        
        {DummyDisposable}
        ");

            const string AnalyzerConfig = "dotnet_diagnostic.NUnit1032.additional_dispose_methods = Release";
            Settings settings = Settings.Default.WithAnalyzerConfig(AnalyzerConfig);
            RoslynAssert.Valid(analyzer, testCode, settings);
        }

        [Test]
        public void AnalyzeWhenFieldIsDisposedUsingFactoryWithParameter()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public class TestClass
    {{
        private IFactory<DummyDisposable> _factory;
        private DummyDisposable? _field;

        public TestClass(IFactory<DummyDisposable> factory) => _factory = factory;

        [SetUp]
        public void SetUpMethod() => _field = _factory.Create();

        [TearDown]
        public void TearDownMethod() => _factory.Release(_field!);
    }}

    public {DummyDisposable}

    public interface IFactory<T>
    {{
        T Create();
        void Release(T instance);
    }}
        ");

            const string AnalyzerConfig = "dotnet_diagnostic.NUnit1032.additional_dispose_methods = Release";
            Settings settings = Settings.Default.WithAnalyzerConfig(AnalyzerConfig);
            RoslynAssert.Valid(analyzer, testCode, settings);
        }

        [Test]
        public void AnalyzeWhenPropertyIsDisposedInCorrectMethod(
            [Values("", "OneTime")] string attributePrefix,
            [Values("", "this.")] string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        protected IDisposable? Property {{ get; private set; }}

        [{attributePrefix}SetUp]
        public void SetUpMethod()
        {{
            {prefix}Property = new DummyDisposable();
        }}

        [{attributePrefix}TearDown]
        public void TearDownMethod()
        {{
            {prefix}Property?.Dispose();
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenPropertyWithInitializerIsDisposedInOneTimeTearDownMethod()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private IDisposable Property {{ get; }} = new DummyDisposable();

        [OneTimeTearDown]
        public void TearDownMethod()
        {{
            Property.Dispose();
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenPropertySetInConstructorIsDisposedInOneTimeTearDownMethod()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private IDisposable Property {{ get; }}

        public TestClass()
        {{
            Property = new DummyDisposable();
        }}

        [OneTimeTearDown]
        public void TearDownMethod()
        {{
            Property.Dispose();
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase("", "OneTime")]
        [TestCase("OneTime", "")]
        public void AnalyzeWhenPropertyIsDisposedInWrongMethod(string attributePrefix1, string attributePrefix2)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
         ↓protected IDisposable? Property {{ get; private set; }}

        [{attributePrefix1}SetUp]
        public void SetUpMethod()
        {{
            Property = new DummyDisposable();
        }}

        [{attributePrefix2}TearDown]
        public void TearDownMethod()
        {{
            Property?.Dispose();
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [TestCase("SetUp")]
        [TestCase("Test")]
        public void AnalyzeWhenPropertyIsNotDisposed(string attribute)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        ↓protected object? Property {{ get; private set; }}

        [{attribute}]
        public void SomeMethod()
        {{
            Property = new DummyDisposable();
        }}

        [TearDown]
        public void TearDownMethod()
        {{
            Property = null;
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenPropertyWithInitializerIsNotDisposed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        ↓protected object? Property {{ get; private set; }} = new DummyDisposable();

        [TearDown]
        public void TearDownMethod()
        {{
            Property = null;
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenReadOnlyPropertyWithInitializerIsNotDisposed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        ↓protected object? Property {{ get; }} = new DummyDisposable();

        [TearDown]
        public void TearDownMethod()
        {{
            Assert.That(Property, Is.Not.Null);
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenPropertySetInConstructorIsNotDisposed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        ↓protected object? Property {{ get; private set; }}

        public TestClass() => Property = new DummyDisposable();

        [TearDown]
        public void TearDownMethod()
        {{
            Property = null;
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenReadOnlyPropertySetInConstructorIsNotDisposed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        ↓protected object? Property {{ get; }}

        public TestClass() => Property = new DummyDisposable();

        [TearDown]
        public void TearDownMethod()
        {{
            Assert.That(Property, Is.Not.Null);
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [TestCase("")]
        [TestCase("OneTime")]
        public void AnalyzeWhenFieldIsDisposedUsingDisposer(string attributePrefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private Disposer? disposer;
        private object? field;

        [{attributePrefix}SetUp]
        public void SetUpMethod()
        {{
            disposer = new Disposer();
            field = disposer.Add(new DummyDisposable());
        }}

        [{attributePrefix}TearDown]
        public void TearDownMethod()
        {{
            disposer?.Dispose();
        }}

        {DummyDisposable}

        {Disposer}
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenFieldIsDisposedInTryOrFinallyStatement()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private IDisposable? field1;
        private IDisposable? field2;

        [SetUp]
        public void SetUpMethod()
        {{
            field1 = new DummyDisposable();
            field2 = new DummyDisposable();
        }}

        [TearDown]
        public void TearDownMethod()
        {{
            try
            {{
                field1?.Dispose();
            }}
            finally
            {{
                field2?.Dispose();
            }}
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase("Dispose")]
        [TestCase("Close")]
        public void AnalyzeWhenFieldIsDisposedUsing(string method)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private DummyWriter? field;

        [SetUp]
        public void SetUpMethod()
        {{
            field = new DummyWriter();
        }}

        [TearDown]
        public void TearDownMethod()
        {{
            field?.{method}();
        }}

        private sealed class DummyWriter : IDisposable
        {{
            public void Dispose() {{}}

            public void Close() => Dispose();
        }}");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenFieldIsCopied()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        private NestedDisposable? field1;
        private IDisposable? field2;

        [SetUp]
        public void SetUpMethod()
        {
            field1 = new NestedDisposable();
            field2 = field1.Member;
        }

        [TearDown]
        public void TearDownMethod()
        {
            field1?.Dispose();
        }

        private sealed class NestedDisposable : IDisposable
        {
            public IDisposable Member { get; } = new DummyDisposable();

            public void Dispose() => Member.Dispose();
        }
        " + DummyDisposable);

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase("new Task(() => { })")]
        [TestCase("new System.IO.MemoryStream(128)")]
        [TestCase("new System.IO.StringReader(\"NUnit.Analyzers\")")]
        public void AnalyzeWhenFieldDoesNotNeedDisposing(string expressionSyntax)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private IDisposable? field;

        [SetUp]
        public void SetUpMethod()
        {{
            field = {expressionSyntax};
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenClassIsNotAnNUnitTestFixture()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
        public sealed class NotATestClass
        {{
            private IDisposable? field;

            public void SomeMethod()
            {{
                field = new DummyDisposable();
            }}

            {DummyDisposable}
        }}
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }

#if !NETFRAMEWORK
        [Test]
        public void AnalyzeWhenFieldIsAsyncDisposable(
            [Values("DisposeAsync", "CloseAsync")] string method,
            [Values("", "this.")] string prefix,
            [Values("", ".ConfigureAwait(false)")] string configureAwait)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private DummyAsyncWriter? field;

        [SetUp]
        public void SetUpMethod()
        {{
            field = new DummyAsyncWriter();
        }}

        [TearDown]
        public async Task TearDownMethod()
        {{
            if (field is not null)
                await {prefix}field.{method}(){configureAwait};
        }}

        private sealed class DummyAsyncWriter : IAsyncDisposable
        {{
            public ValueTask DisposeAsync() => default(ValueTask);

            public Task CloseAsync() => DisposeAsync().AsTask();
        }}");

            RoslynAssert.Valid(analyzer, testCode);
        }
#endif

        [Test]
        public void ShouldNotAnalyzeWhenInstancePerTestCase()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@$"
            [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
            public class TestClass
            {{
                private readonly IDisposable? field;

                public TestClass()
                {{
                    field = new DummyDisposable();
                }}

                [Test]
                public void Test()
                {{
                    Assert.That(field, Is.Not.Null);
                }}

                {DummyDisposable}
            }}
            ");

            // InstancePerTestCase mean test should use IDisposable
            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ShouldDealWithDirectRecursiveMethods()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                private Dictionary<string, bool> _values = new();

                [Test]
                public void MinimalRepro()
                {
                    Recurse(""help"");
                }

                private void Recurse(string one, bool keepGoing = true)
                {
                    _values[one] = keepGoing;
                    if (keepGoing)
                    {
                        Recurse(one, false);
                    }
                }
            ", "using System.Collections.Generic;");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ShouldDealWithIndirectRecursiveMethods()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                private Dictionary<string, string> _values = new();

                [Test]
                public void Indirect()
                {
                    A(""help"");
                }

                private void A(string one, bool keepGoing = true)
                {
                    if (keepGoing)
                    {
                        _values[one] = nameof(A);
                        B(one, false);
                    }
                }

                private void B(string one, bool keepGoing)
                {
                    if (keepGoing)
                    {
                        _values[one] = nameof(B);
                        A(one, false);
                    }
                }
            ", "using System.Collections.Generic;");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase("")]
        [TestCase("static")]
        public void StaticFieldsNeedDisposingInOneTimeTearDown(string modifier)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
        public {modifier} class StaticFields
        {{
            private static readonly IDisposable? ↓field = new DummyDisposable();

            [Test]
            public {modifier} void TestMethod()
            {{
                Assert.That(field, Is.Not.Null);
            }}

            {DummyDisposable}
        }}
        ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [TestCase("")]
        [TestCase("static")]
        public void StaticFieldsAreDisposed(string modifier)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
        public {modifier} class StaticFields
        {{
            private static readonly IDisposable? field = new DummyDisposable();

            [OneTimeTearDown]
            public {modifier} void OneTimeTearDown()
            {{
                field?.Dispose();
            }}

            [Test]
            public {modifier} void TestMethod()
            {{
                Assert.That(field, Is.Not.Null);
            }}

            {DummyDisposable}
        }}
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void EarlyOutForNonSetableFieldsAndProperties()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
        public partial class TestFixture
        {
            public const double ToleranceConstant = 1E-10;

            public static readonly double ToleranceReadOnlyField = 1E-10;

            public static double ToleranceExpressionProperty => 1E-10;

            public static double ToleranceReadOnlyProperty { get; } = 1E-10;

            public static object ToleranceGetOnlyExpressionProperty
            {
                get => 1E-10;
            }

            public static object ToleranceGetOnlyProperty
            {
                get
                {
                    return 1E-10;
                }
            }

            [Test]
            public void SomeTest() => throw new NotImplementedException();
        }");

            int earlyOutCount = DisposeFieldsAndPropertiesInTearDownAnalyzer.EarlyOutCount;
            RoslynAssert.Valid(analyzer, testCode);
            Assert.That(DisposeFieldsAndPropertiesInTearDownAnalyzer.EarlyOutCount, Is.GreaterThan(earlyOutCount));
        }

        [Test]
        public void DoesNotThrowExceptionsWhenUsingPartialClasses()
        {
            var testCodePart1 = TestUtility.WrapClassInNamespaceAndAddUsing($@"
        public partial class TestFixture
        {{
            private const double Tolerance = 1E-10;

            private static IDisposable? Property {{ get; set; }}

            [SetUp]
            public void SetUp()
            {{
                Property = new DummyDisposable();
            }}

            [TearDown]
            public void TearDown()
            {{
                Property?.Dispose();
            }}

            private static void SomeAsserts() => throw new InvalidOperationException();

            {DummyDisposable}
        }}");

            var testCodePart2 = TestUtility.WrapClassInNamespaceAndAddUsing(@"
        public partial class TestFixture
        {
            [Test]
            public void SomeTest()
            {
                SomeAsserts();
            }
        }");

            RoslynAssert.Valid(analyzer, testCodePart1, testCodePart2);
        }

        [Test]
        public void DoesNotThrowExceptionsWhenUsingExplicitProperties()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
        [TestFixture]
        public abstract class Test : IDataProvider
        {
            private DataReader DataReader;

            DataReader IDataProvider.DataReader
            {
                get => DataReader;
                set => DataReader = value;
            }

            protected Test() => DataReader = new DataReader(this);

            [OneTimeSetUp]
            public void SetListener() => throw new NotImplementedException();
        }

        public interface IDataProvider
        {
            DataReader DataReader
            {
                get;
                set;
            }
        }

        public sealed class DataReader
        {
            public DataReader(IDataProvider provider) => throw new NotImplementedException();
        }");

            RoslynAssert.Valid(analyzer, testCode);
        }
    }
}
