using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.DisposeFieldsInTearDown;
using NUnit.Analyzers.Tests.Verifiers;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.DisposeFieldsInTearDown.DisposeFieldsAndPropertiesInTearDownAnalyzer>;

namespace NUnit.Analyzers.Tests.DisposeFieldsInTearDown
{
    public sealed class DisposeFieldsAndPropertiesInTearDownAnalyzerTests
    {
        private const string DummyDisposable = """
            sealed class DummyDisposable : IDisposable
            {
                public void Dispose() {}
            }
        """;

        private const string DummyExplicitDisposable = """
            sealed class DummyExplicitDisposable : IAnotherInterface, IDisposable
            {
                void IDisposable.Dispose() {}
            }
            public interface IAnotherInterface
            {
            }
        """;

        // CA2000 allows transfer of ownership using ICollection<IDisposable>.Add
        private const string Disposer = """
            private sealed class Disposer : IDisposable
            {
                public void Dispose() {}

                public T? Add<T>(T? resource) => resource;
            }

        """;

        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.FieldIsNotDisposedInTearDown);

        [Test]
        public async Task AnalyzeWhenFieldIsDisposedInCorrectMethod(
            [Values("", "OneTime")] string attributePrefix,
            [Values("", "this.")] string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private IDisposable? field;

                    [{{attributePrefix}}SetUp]
                    public void SetUpMethod()
                    {
                        {{prefix}}field = new DummyDisposable();
                    }

                    [{{attributePrefix}}TearDown]
                    public void TearDownMethod()
                    {
                        {{prefix}}field?.Dispose();
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("IDisposable")]
        [TestCase("System.IDisposable")]
        public async Task AnalyzeWhenFieldIsConditionallyDisposedUsingIsIDisposable(string interfaceName)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private object field = new DummyDisposable();

                    [OneTimeTearDown]
                    public void TearDownMethod()
                    {
                        if (field is {{interfaceName}} disposable)
                            disposable.Dispose();
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("IDisposable")]
        [TestCase("System.IDisposable")]
        public async Task AnalyzeWhenFieldIsConditionallyDisposedUsingAsIDisposable(string interfaceName)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private object field = new DummyDisposable();

                    [OneTimeTearDown]
                    public void TearDownMethod()
                    {
                        (field as {{interfaceName}})?.Dispose();
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenFieldWithInitializerIsDisposedInOneTimeTearDownMethod()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private IDisposable field = new DummyDisposable();

                    [OneTimeTearDown]
                    public void TearDownMethod()
                    {
                        field.Dispose();
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenFieldSetInConstructorIsDisposedInOneTimeTearDownMethod()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private IDisposable field;

                    public TestClass()
                    {
                        field = new DummyDisposable();
                    }

                    [OneTimeTearDown]
                    public void TearDownMethod()
                    {
                        field.Dispose();
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenExplicitlyInterfaceImplementedDisposableFieldSetInConstructorIsDisposedInOneTimeTearDownMethod()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private IAnotherInterface field;

                    public TestClass()
                    {
                        field = new DummyExplicitDisposable();
                    }

                    [OneTimeTearDown]
                    public void TearDownMethod()
                    {
                        ((IDisposable)field).Dispose();
                    }

                    {{DummyExplicitDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("", "OneTime")]
        [TestCase("OneTime", "")]
        public async Task AnalyzeWhenFieldIsDisposedInWrongMethod(string attributePrefix1, string attributePrefix2)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private IDisposable? {|#0:field|};

                    [{{attributePrefix1}}SetUp]
                    public void SetUpMethod()
                    {
                        field = new DummyDisposable();
                    }

                    [{{attributePrefix2}}TearDown]
                    public void TearDownMethod()
                    {
                        field?.Dispose();
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("field", "field", $"{attributePrefix1}TearDownAttribute"));
        }

        [TestCase("SetUp")]
        [TestCase("Test")]
        public async Task AnalyzeWhenFieldIsNotDisposed(string attribute)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private object? {|#0:field|};

                    [{{attribute}}]
                    public void SomeMethod()
                    {
                        field = new DummyDisposable();
                    }

                    [TearDown]
                    public void TearDownMethod()
                    {
                        field = null;
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("field", "field", "TearDownAttribute"));
        }

        [Test]
        public async Task AnalyzeWhenFieldInitializedInLocalFunctionIsNotDisposed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private object? {|#0:field|};

                    [SetUp]
                    public void SetUp()
                    {
                        Initialize();

                        void Initialize()
                        {
                            field = new DummyDisposable();
                        }
                    }

                    [TearDown]
                    public void TearDownMethod()
                    {
                        field = null;
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("field", "field", "TearDownAttribute"));
        }

        [TestCase("SetUp")]
        [TestCase("Test")]
        public async Task AnalyzeWhenFieldTypeParameterIsNotDisposed(string attribute)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public class TestClass<T>
                    where T : IDisposable, new()
                {
                    private T? {|#0:field|};

                    [{{attribute}}]
                    public void SomeMethod()
                    {
                        field = new T();
                    }

                    {{DummyDisposable}}
                }

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("field", "field", "TearDownAttribute"));
        }

        [Test]
        public async Task AnalyzeWhenFieldWithInitializerIsNotDisposed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private object? {|#0:field = new DummyDisposable()|};

                    [TearDown]
                    public void TearDownMethod()
                    {
                        field = null;
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("field", "field", "OneTimeTearDownAttribute"));
        }

        [Test]
        public async Task AnalyzeWhenFieldSetInConstructorIsNotDisposed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private object? {|#0:field|};

                    public TestClass() => field = new DummyDisposable();

                    [TearDown]
                    public void TearDownMethod()
                    {
                        field = null;
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("field", "field", "OneTimeTearDownAttribute"));
        }

        [Test]
        public async Task AnalyzeWhenReadOnlyFieldSetInConstructorIsNotDisposed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private readonly object? {|#0:field|};

                    public TestClass() => field = new DummyDisposable();

                    [TearDown]
                    public void TearDownMethod()
                    {
                        Assert.That(field, Is.Not.Null);
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("field", "field", "OneTimeTearDownAttribute"));
        }

        [Test]
        public async Task FieldConditionallyAssignedInCalledLocalFunction()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private object? {|#0:field|};
                    private bool initializeField;

                    public TestClass(bool initializeField) => this.initializeField = initializeField;

                    [SetUp]
                    public async Task SetUp()
                    {
                        if (initializeField)
                            await InitializeField().ConfigureAwait(false);
                    }

                    [TearDown]
                    public void TearDownMethod()
                    {
                        field = null;
                    }

                    private Task InitializeField()
                    {
                        field = new DummyDisposable();
                        return Task.CompletedTask;
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("field", "field", "TearDownAttribute"));
        }

        [Test]
        public async Task AnalyzeWhenPropertyBackingFieldIsDisposed(
            [Values("field", "Property")] string fieldOrProperty)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private IDisposable? field;

                    private IDisposable? Property
                    {
                        get => field;
                        set => this.field = value;
                    }

                    [SetUp]
                    public void SetUpMethod()
                    {
                        {{fieldOrProperty}} = new DummyDisposable();
                    }

                    [TearDown]
                    public void TearDownMethod()
                    {
                        {{fieldOrProperty}}?.Dispose();
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenFieldIsDisposedInSpecialExtensionMethod()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public class TestClass
                {
                    private object? field;

                    [SetUp]
                    public void SetUpMethod()
                    {
                        field = new DummyDisposable();
                    }

                    [TearDown]
                    public void TearDownMethod()
                    {
                        field.DisposeIfDisposable();
                    }

                    {{DummyDisposable}}
                }

                public static class DisposableExtensions
                {
                    public static void DisposeIfDisposable<T>(this T instance)
                    {
                        if (instance is IDisposable disposable)
                            disposable.Dispose();
                    }
                }

            """);

            const string analyzerConfig = "dotnet_diagnostic.NUnit1032.additional_dispose_methods = DisposeIfDisposable";
            await Verify.VerifyAnalyzerAsync(testCode, test => test.TestState.AddAnalyzerConfig(analyzerConfig));
        }

        [Test]
        public async Task AnalyzeWhenFieldIsDisposedInSpecialMethodWithParameter()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private object? field;

                    [SetUp]
                    public void SetUpMethod()
                    {
                        field = new DummyDisposable();
                    }

                    [TearDown]
                    public void TearDownMethod()
                    {
                        Release(field);
                    }

                    private void Release<T>(T instance)
                    {
                        if (instance is IDisposable disposable)
                            disposable.Dispose();
                    }

                    {{DummyDisposable}}

            """);

            const string analyzerConfig = "dotnet_diagnostic.NUnit1032.additional_dispose_methods = Release";
            await Verify.VerifyAnalyzerAsync(testCode, test => test.TestState.AddAnalyzerConfig(analyzerConfig));
        }

        [Test]
        public async Task AnalyzeWhenFieldIsDisposedInMemberMethod()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private IDisposable? field;

                    [SetUp]
                    public void SetUpMethod()
                    {
                        field = new DummyDisposable();
                    }

                    [TearDown]
                    public void TearDownMethod()
                    {
                        CleanUp();
                    }

                    private void CleanUp()
                    {
                        field?.Dispose();
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenFieldIsDisposedInLocalMethod()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private IDisposable? field;

                    [SetUp]
                    public void SetUpMethod()
                    {
                        field = new DummyDisposable();
                    }

                    [TearDown]
                    public void TearDownMethod()
                    {
                        CleanUp();

                        void CleanUp()
                        {
                            field?.Dispose();
                        }
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenFieldIsDisposedUsingFactoryWithParameter()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public class TestClass
                {
                    private IFactory<DummyDisposable> _factory;
                    private DummyDisposable? _field;

                    public TestClass(IFactory<DummyDisposable> factory) => _factory = factory;

                    [SetUp]
                    public void SetUpMethod() => _field = _factory.Create();

                    [TearDown]
                    public void TearDownMethod() => _factory.Release(_field!);
                }

                public {{DummyDisposable}}

                public interface IFactory<T>
                {
                    T Create();
                    void Release(T instance);
                }

            """);

            const string analyzerConfig = "dotnet_diagnostic.NUnit1032.additional_dispose_methods = Release";
            await Verify.VerifyAnalyzerAsync(testCode, test => test.TestState.AddAnalyzerConfig(analyzerConfig));
        }

        [Test]
        public async Task AnalyzeWhenPropertyIsDisposedInCorrectMethod(
            [Values("", "OneTime")] string attributePrefix,
            [Values("", "this.")] string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    protected IDisposable? Property { get; private set; }

                    [{{attributePrefix}}SetUp]
                    public void SetUpMethod()
                    {
                        {{prefix}}Property = new DummyDisposable();
                    }

                    [{{attributePrefix}}TearDown]
                    public void TearDownMethod()
                    {
                        {{prefix}}Property?.Dispose();
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenPropertyWithInitializerIsDisposedInOneTimeTearDownMethod()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private IDisposable Property { get; } = new DummyDisposable();

                    [OneTimeTearDown]
                    public void TearDownMethod()
                    {
                        Property.Dispose();
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenPropertySetInConstructorIsDisposedInOneTimeTearDownMethod()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private IDisposable Property { get; }

                    public TestClass()
                    {
                        Property = new DummyDisposable();
                    }

                    [OneTimeTearDown]
                    public void TearDownMethod()
                    {
                        Property.Dispose();
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("", "OneTime")]
        [TestCase("OneTime", "")]
        public async Task AnalyzeWhenPropertyIsDisposedInWrongMethod(string attributePrefix1, string attributePrefix2)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                     {|#0:protected IDisposable? Property { get; private set; }|}

                    [{{attributePrefix1}}SetUp]
                    public void SetUpMethod()
                    {
                        Property = new DummyDisposable();
                    }

                    [{{attributePrefix2}}TearDown]
                    public void TearDownMethod()
                    {
                        Property?.Dispose();
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("property", "Property", $"{attributePrefix1}TearDownAttribute"));
        }

        [TestCase("SetUp")]
        [TestCase("Test")]
        public async Task AnalyzeWhenPropertyIsNotDisposed(string attribute)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    {|#0:protected object? Property { get; private set; }|}

                    [{{attribute}}]
                    public void SomeMethod()
                    {
                        Property = new DummyDisposable();
                    }

                    [TearDown]
                    public void TearDownMethod()
                    {
                        Property = null;
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("property", "Property", "TearDownAttribute"));
        }

        [Test]
        public async Task AnalyzeWhenPropertyWithInitializerIsNotDisposed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    {|#0:protected object? Property { get; private set; } = new DummyDisposable();|}

                    [TearDown]
                    public void TearDownMethod()
                    {
                        Property = null;
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("property", "Property", "OneTimeTearDownAttribute"));
        }

        [Test]
        public async Task AnalyzeWhenReadOnlyPropertyWithInitializerIsNotDisposed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    {|#0:protected object? Property { get; } = new DummyDisposable();|}

                    [TearDown]
                    public void TearDownMethod()
                    {
                        Assert.That(Property, Is.Not.Null);
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("property", "Property", "OneTimeTearDownAttribute"));
        }

        [Test]
        public async Task AnalyzeWhenPropertySetInConstructorIsNotDisposed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    {|#0:protected object? Property { get; private set; }|}

                    public TestClass() => Property = new DummyDisposable();

                    [TearDown]
                    public void TearDownMethod()
                    {
                        Property = null;
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("property", "Property", "OneTimeTearDownAttribute"));
        }

        [Test]
        public async Task AnalyzeWhenReadOnlyPropertySetInConstructorIsNotDisposed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    {|#0:protected object? Property { get; }|}

                    public TestClass() => Property = new DummyDisposable();

                    [TearDown]
                    public void TearDownMethod()
                    {
                        Assert.That(Property, Is.Not.Null);
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("property", "Property", "OneTimeTearDownAttribute"));
        }

        [TestCase("")]
        [TestCase("OneTime")]
        public async Task AnalyzeWhenFieldIsDisposedUsingDisposer(string attributePrefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private Disposer? disposer;
                    private object? field;

                    [{{attributePrefix}}SetUp]
                    public void SetUpMethod()
                    {
                        disposer = new Disposer();
                        field = disposer.Add(new DummyDisposable());
                    }

                    [{{attributePrefix}}TearDown]
                    public void TearDownMethod()
                    {
                        disposer?.Dispose();
                    }

                    {{DummyDisposable}}

                    {{Disposer}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenFieldIsDisposedInTryOrFinallyStatement()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private IDisposable? field1;
                    private IDisposable? field2;

                    [SetUp]
                    public void SetUpMethod()
                    {
                        field1 = new DummyDisposable();
                        field2 = new DummyDisposable();
                    }

                    [TearDown]
                    public void TearDownMethod()
                    {
                        try
                        {
                            field1?.Dispose();
                        }
                        finally
                        {
                            field2?.Dispose();
                        }
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("Dispose")]
        [TestCase("Close")]
        public async Task AnalyzeWhenFieldIsDisposedUsing(string method)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private DummyWriter? field;

                    [SetUp]
                    public void SetUpMethod()
                    {
                        field = new DummyWriter();
                    }

                    [TearDown]
                    public void TearDownMethod()
                    {
                        field?.{{method}}();
                    }

                    private sealed class DummyWriter : IDisposable
                    {
                        public void Dispose() {}

                        public void Close() => Dispose();
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenFieldIsCopied()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
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

            """ + "\n" + DummyDisposable);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("new Task(() => { })")]
        [TestCase("new System.IO.MemoryStream(128)")]
        [TestCase("new System.IO.StringReader(\"NUnit.Analyzers\")")]
        public async Task AnalyzeWhenFieldDoesNotNeedDisposing(string expressionSyntax)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private IDisposable? field;

                    [SetUp]
                    public void SetUpMethod()
                    {
                        field = {{expressionSyntax}};
                    }

                    {{DummyDisposable}}

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenClassIsNotAnNUnitTestFixture()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public sealed class NotATestClass
                {
                    private IDisposable? field;

                    public void SomeMethod()
                    {
                        field = new DummyDisposable();
                    }

                    {{DummyDisposable}}
                }

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

#if !NETFRAMEWORK
        [Test]
        public async Task AnalyzeWhenFieldIsAsyncDisposable(
            [Values("DisposeAsync", "CloseAsync")] string method,
            [Values("", "this.")] string prefix,
            [Values("", ".ConfigureAwait(false)")] string configureAwait)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private DummyAsyncWriter? field;

                    [SetUp]
                    public void SetUpMethod()
                    {
                        field = new DummyAsyncWriter();
                    }

                    [TearDown]
                    public async Task TearDownMethod()
                    {
                        if (field is not null)
                            await {{prefix}}field.{{method}}(){{configureAwait}};
                    }

                    private sealed class DummyAsyncWriter : IAsyncDisposable
                    {
                        public ValueTask DisposeAsync() => default(ValueTask);

                        public Task CloseAsync() => DisposeAsync().AsTask();
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }
#endif

        [Test]
        public async Task ShouldNotAnalyzeWhenInstancePerTestCase()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
                public class TestClass
                {
                    private readonly IDisposable? field;

                    public TestClass()
                    {
                        field = new DummyDisposable();
                    }

                    [Test]
                    public void Test()
                    {
                        Assert.That(field, Is.Not.Null);
                    }

                    {{DummyDisposable}}
                }

            """);

            // InstancePerTestCase mean test should use IDisposable
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ShouldDealWithDirectRecursiveMethods()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    private Dictionary<string, bool> _values = new();

                    [Test]
                    public void MinimalRepro()
                    {
                        Recurse("help");
                    }

                    private void Recurse(string one, bool keepGoing = true)
                    {
                        _values[one] = keepGoing;
                        if (keepGoing)
                        {
                            Recurse(one, false);
                        }
                    }

            """, "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ShouldDealWithIndirectRecursiveMethods()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    private Dictionary<string, string> _values = new();

                    [Test]
                    public void Indirect()
                    {
                        A("help");
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

            """, "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("")]
        [TestCase("static")]
        public async Task StaticFieldsNeedDisposingInOneTimeTearDown(string modifier)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public {{modifier}} class StaticFields
                {
                    private static readonly IDisposable? {|#0:field = new DummyDisposable()|};

                    [Test]
                    public {{modifier}} void TestMethod()
                    {
                        Assert.That(field, Is.Not.Null);
                    }

                    {{DummyDisposable}}
                }

            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("field", "field", "OneTimeTearDownAttribute"));
        }

        [TestCase("")]
        [TestCase("static")]
        public async Task StaticFieldsAreDisposed(string modifier)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public {{modifier}} class StaticFields
                {
                    private static readonly IDisposable? field = new DummyDisposable();

                    [OneTimeTearDown]
                    public {{modifier}} void OneTimeTearDown()
                    {
                        field?.Dispose();
                    }

                    [Test]
                    public {{modifier}} void TestMethod()
                    {
                        Assert.That(field, Is.Not.Null);
                    }

                    {{DummyDisposable}}
                }

            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task EarlyOutForNonSetableFieldsAndProperties()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
                }
            """);

            int earlyOutCount = DisposeFieldsAndPropertiesInTearDownAnalyzer.EarlyOutCount;
            await Verify.VerifyAnalyzerAsync(testCode);
            Assert.That(DisposeFieldsAndPropertiesInTearDownAnalyzer.EarlyOutCount, Is.GreaterThan(earlyOutCount));
        }

        [Test]
        public async Task DoesNotThrowExceptionsWhenUsingPartialClasses()
        {
            var testCodePart1 = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public partial class TestFixture
                {
                    private const double Tolerance = 1E-10;

                    private IDisposable? field;

                    private static IDisposable? Property { get; set; }

                    [SetUp]
                    public void SetUp()
                    {
                        Property = new DummyDisposable();
                    }

                    [TearDown]
                    public void TearDown()
                    {
                        Property?.Dispose();
                    }

                    private static void SomeAsserts() => throw new InvalidOperationException();

                    {{DummyDisposable}}
                }
            """);

            var testCodePart2 = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public partial class TestFixture
                {
                    [Test]
                    public void SomeTest()
                    {
                        // field needs Disposal, but as TearDown is in a different source 'file' we cannot check if it is.
                        field = new DummyDisposable();
                        SomeAsserts();
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCodePart1, test => test.TestState.Sources.Add(testCodePart2));
        }

        [Test]
        public async Task DoesNotThrowExceptionsWhenUsingExplicitProperties()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("")]
        [TestCase("one_time_")]
        public async Task DoesNotFireWhenAdditionalSetUpTearDownMethodsAreConfigured(string prefix)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public abstract class BaseTest
                {
                    [SetUp]
                    public void SetUp()
                    {
                        OnSetUp();
                    }

                    [TearDown]
                    public void TearDown()
                    {
                        OnTearDown();
                    }

                    protected virtual void OnSetUp()
                    {
                    }

                    protected virtual void OnTearDown()
                    {
                    }
                }

                [TestFixture]
                public class InheritedTest : BaseTest
                {
                    private IDisposable? disposable;

                    protected override void OnSetUp()
                    {
                        base.OnSetUp();
                        disposable = new DummyDisposable();
                    }

                    protected override void OnTearDown()
                    {
                        disposable?.Dispose();
                        base.OnTearDown();
                    }
                }

            """ + "\n" + DummyDisposable);

            string analyzerConfig = $$"""
                dotnet_diagnostic.NUnit.additional_{{prefix}}setup_methods = OnSetUp
                dotnet_diagnostic.NUnit.additional_{{prefix}}teardown_methods = OnTearDown
            """;
            await Verify.VerifyAnalyzerAsync(testCode, test => test.TestState.AddAnalyzerConfig(analyzerConfig));
        }

        [Test]
        public async Task DoesFireWhenAdditionalSetUpTearDownMethodsAreNotOverrides()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""

                [TestFixture]
                public class InheritedTest
                {
                    private IDisposable? {|#0:disposable|};

                    protected void OnSetUp()
                    {
                        disposable = new DummyDisposable();
                    }

                    protected void OnTearDown()
                    {
                        disposable?.Dispose();
                    }
                }

            """ + "\n" + DummyDisposable);

            const string analyzerConfig = """
                dotnet_diagnostic.NUnit1032.additional_setup_methods = OnSetUp
                dotnet_diagnostic.NUnit1032.additional_teardown_methods = OnTearDown
            """;
            await Verify.VerifyAnalyzerAsync(testCode, test => test.TestState.AddAnalyzerConfig(analyzerConfig), expectedDiagnostic.WithLocation(0).WithArguments("field", "disposable", "TearDownAttribute"));
        }

        [Test]
        public async Task ShouldDetectFieldInitializedInUsingStatement()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class TestClass
                {
                    private IDisposable? {|#0:disposable|};

                    [SetUp]
                    public void SetUp()
                    {
                        using (var reader = new StreamReader("Test.config"))
                        {
                            string configuration = reader.ReadToEnd();
                            if (configuration == "CreateDisposable")
                            {
                                disposable = new DummyDisposable();
                            }
                        }
                    }
                }

            """ + "\n" + DummyDisposable, "using System.IO;");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("field", "disposable", "TearDownAttribute"));
        }
    }
}
