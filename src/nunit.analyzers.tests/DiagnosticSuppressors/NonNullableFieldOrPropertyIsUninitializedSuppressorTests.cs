using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.DiagnosticSuppressors;
using NUnit.Analyzers.Tests.Verifiers;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.DiagnosticSuppressors
{
    // Declared as a 'record' to prove it works even though it's useless in this context.
    internal sealed record NonNullableFieldOrPropertyIsUninitializedSuppressorTests
    {
        private static readonly DiagnosticResult ExpectedDiagnostic = DiagnosticResult.CompilerWarning("CS8618");

        private static readonly string[] TypeDeclarations = ["class", "record"];

        [TestCaseSource(nameof(TypeDeclarations))]
        public async Task FieldNotAssigned(string typeDeclaration)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                [TestFixture]
                public {{typeDeclaration}} TestClass
                {
                    private string {|#0:field|};

                    [Test]
                    public void Test()
                    {
                        field = string.Empty;
                        Assert.That(field, Is.Not.Null);
                    }
                }
                """);

            await VerifySuppressionAsync(testCode, isSuppressed: false, kind: "field", name: "field");
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public async Task FieldAssigned(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private string {|#0:field|};

                    [{{attribute}}]
                    public void Initialize()
                    {
                        {{prefix}}field = string.Empty;
                    }

                    [Test]
                    public void Test()
                    {
                        Assert.That({{prefix}}field, Is.Not.Null);
                    }

            """);

            await VerifySuppressionAsync(testCode, isSuppressed: true, kind: "field", name: "field");
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public async Task FieldAssignedUsingExpressionBody(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private string {|#0:field|};

                    [{{attribute}}]
                    public void Initialize() => {{prefix}}field = string.Empty;

                    [Test]
                    public void Test() => Assert.That({{prefix}}field, Is.Not.Null);

            """);

            await VerifySuppressionAsync(testCode, isSuppressed: true, kind: "field", name: "field");
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public async Task FieldNotAssignedInConstructor(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private string testName;
                    private string nunitField;

                    public {|#0:TestClass|}(string name)
                    {
                        {{prefix}}testName = name;
                    }

                    [{{attribute}}]
                    public void Initialize()
                    {
                        {{prefix}}nunitField = string.Empty;
                    }

                    [Test]
                    public void Test()
                    {
                        Assert.That({{prefix}}nunitField, Is.Not.Null);
                    }

            """);

            await VerifySuppressionAsync(testCode, isSuppressed: true, kind: "field", name: "nunitField");
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public async Task FieldAssignedUsingTupleDeconstruction(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private string {|#0:name|};
                    private string {|#1:description|};

                    [{{attribute}}]
                    public void Initialize()
                    {
                        ({{prefix}}name, {{prefix}}description) = GetNameAndDescription();
                    }

                    [Test]
                    public void Test()
                    {
                        Assert.That({{prefix}}name, Is.Not.Null);
                        Assert.That({{prefix}}description, Is.Not.Null);
                    }

                    private (string Name, string Description) GetNameAndDescription() => ("SomeName", "Some Description");

            """);

            await VerifySuppressionAsync(testCode, isSuppressed: true, kind: "field", names: ["name", "description"]);
        }

        [Test]
        public async Task FieldAssignedInTryFinally()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    private string {|#0:field1|};
                    private string {|#1:field2|};

                    [SetUp]
                    public void SetUp()
                    {
                        try
                        {
                            field1 = "NUnit";
                        }
                        finally
                        {
                            field2 = "Analyzers";
                        }
                    }

                    [Test]
                    public void Test()
                    {
                        Assert.That(field1, Is.Not.Null);
                        Assert.That(field2, Is.Not.Null);
                    }

            """);

            await VerifySuppressionAsync(testCode, isSuppressed: true, kind: "field", names: ["field1", "field2"]);
        }

        [TestCaseSource(nameof(TypeDeclarations))]
        public async Task PropertyNotAssigned(string typeDeclaration)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                [TestFixture]
                public {{typeDeclaration}} TestClass
                {
                    protected string {|#0:Property|} { get; private set; }

                    [Test]
                    public void Test()
                    {
                        Assert.That(Property, Is.Not.Null);
                    }
                }
                """);

            await VerifySuppressionAsync(testCode, isSuppressed: false, kind: "property", name: "Property");
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public async Task PropertyAssigned(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    protected string {|#0:Property|} { get; private set; }

                    [{{attribute}}]
                    public void Initialize()
                    {
                        {{prefix}}Property = string.Empty;
                    }

                    [Test]
                    public void Test()
                    {
                        Assert.That({{prefix}}Property, Is.Not.Null);
                    }

            """);

            await VerifySuppressionAsync(testCode, isSuppressed: true, kind: "property", name: "Property");
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public async Task PropertyAssignedUsingTupleDeconstruction(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    protected string {|#0:Name|} { get; private set; }
                    protected string {|#1:Description|} { get; private set; }

                    [{{attribute}}]
                    public void Initialize()
                    {
                        ({{prefix}}Name, {{prefix}}Description) = ("ThatName", "The Matching Description");
                    }

                    [Test]
                    public void Test()
                    {
                        Assert.That({{prefix}}Name, Is.Not.Null);
                        Assert.That({{prefix}}Description, Is.Not.Null);
                    }

            """);

            await VerifySuppressionAsync(testCode, isSuppressed: true, kind: "property", names: ["Name", "Description"]);
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public async Task FieldAssignedInCalledMethod(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private string {|#0:field|};

                    [{{attribute}}]
                    public void Initialize()
                    {
                        {{prefix}}SetFields();
                    }

                    [Test]
                    public void Test()
                    {
                        Assert.That({{prefix}}field, Is.Not.Null);
                    }

                    private void SetFields()
                    {
                        {{prefix}}field = string.Empty;
                    }

            """);

            await VerifySuppressionAsync(testCode, isSuppressed: true, kind: "field", name: "field");
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public async Task FieldAssignedInCalledLocalFunction(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private string {|#0:field|};

                    [{{attribute}}]
                    public void Initialize()
                    {
                        SetFields();

                        void SetFields()
                        {
                            {{prefix}}field = string.Empty;
                        }
                    }

                    [Test]
                    public void Test()
                    {
                        Assert.That({{prefix}}field, Is.Not.Null);
                    }

            """);

            await VerifySuppressionAsync(testCode, isSuppressed: true, kind: "field", name: "field");
        }

        [TestCase("async Task", "await ", "", "")]
        [TestCase("async Task", "await ", "this.", "")]
        [TestCase("async Task", "await ", "", ".ConfigureAwait(false)")]
        [TestCase("async Task", "await ", "this.", ".ConfigureAwait(false)")]
        [TestCase("void", "", "", ".Wait()")]
        [TestCase("void", "", "this.", ".Wait()")]
        public async Task FieldAssignedInAsyncCalledMethod(string returnType, string awaitKeyWord, string accessor, string suffix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private string {|#0:field|};

                    [SetUp]
                    public {{returnType}} Initialize()
                    {
                        {{awaitKeyWord}}{{accessor}}SetFields(){{suffix}};
                    }

                    [Test]
                    public void Test()
                    {
                        Assert.That({{accessor}}field, Is.Not.Null);
                    }

                    private Task SetFields()
                    {
                        {{accessor}}field = string.Empty;
                        return Task.CompletedTask;
                    }

            """);

            await VerifySuppressionAsync(testCode, isSuppressed: true, kind: "field", name: "field");
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public async Task FieldAssignedInCalledMethods(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    private double numericField;
                    private string {|#0:textField|};

                    [{{attribute}}]
                    public void Initialize()
                    {
                        {{prefix}}SetFields();
                    }

                    [Test]
                    public void Test()
                    {
                        Assert.That({{prefix}}numericField, Is.Not.Zero);
                        Assert.That({{prefix}}textField, Is.Not.Null);
                    }

                    public void SetFields()
                    {
                        {{prefix}}SetFields(3, 4);
                        {{prefix}}SetFields("Seven");
                    }

                    private void SetFields(params double[] numbers)
                    {
                        double sum = 0;
                        for (int i = 0; i < numbers.Length; i++)
                            sum += numbers[i];
                        {{prefix}}numericField = sum;
                    }

                    private void SetFields(string? text)
                    {
                        {{prefix}}textField = text ?? string.Empty;
                    }

            """);

            await VerifySuppressionAsync(testCode, isSuppressed: true, kind: "field", name: "textField");
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public async Task FieldIndirectAssignedThroughBaseClassSetupMethod(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public class BaseClass
                {
                    [{{attribute}}]
                    public void Initialize()
                    {
                        SetFields();
                    }

                    protected virtual void SetFields(string text = "Default")
                    {
                    }
                }

                [TestFixture]
                public class TestClass : BaseClass
                {
                    private string {|#0:field|};

                    [Test]
                    public void Test()
                    {
                        Assert.That({{prefix}}field, Is.EqualTo("Default"));
                    }

                    protected override void SetFields(string text)
                    {
                        {{prefix}}field = text;
                    }
                }

            """);

            // This is not supported as the BaseClass could be in an external library.
            // We therefore cannot validate that its SetUp method calls into our overridden method.
            await VerifySuppressionAsync(testCode, isSuppressed: false, kind: "field", name: "field");
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public async Task FieldAssignedThroughOverriddenBaseClassSetupMethod(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                #nullable enable

                public abstract class BaseClass
                {
                    [{{attribute}}]
                    public virtual void Initialize()
                    {
                    }
                }

                [TestFixture]
                public sealed class TestClass : BaseClass
                {
                    private const string Default = "Default";

                    private string {|#0:field|};

                    public override void Initialize()
                    {
                        {{prefix}}field = Default;
                    }

                    [Test]
                    public void Test()
                    {
                        Assert.That({{prefix}}field, Is.EqualTo(Default));
                    }
                }

            """);

            await VerifySuppressionAsync(testCode, isSuppressed: true, kind: "field", name: "field");
        }

        [Test]
        public async Task ShouldDealWithRecursiveMethods()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    private Dictionary<string, bool> _values = new();
                    private string {|#0:_lastOne|};

                    [SetUp]
                    public void SetUp()
                    {
                        Recurse("SetUp");
                    }

                    [Test]
                    public void MinimalRepro()
                    {
                        Recurse("help");
                    }

                    private void Recurse(string one, bool keepGoing = true)
                    {
                        _values[one] = keepGoing;
                        if (!keepGoing)
                        {
                            return;
                        }
                        Recurse(one, false);
                        _lastOne = one;
                    }

            """, "using System.Collections.Generic;");

            await VerifySuppressionAsync(testCode, isSuppressed: true, kind: "field", name: "_lastOne");
        }

        [Test]
        public async Task ShouldDealWithFieldInitializedInUsingStatement()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    private string {|#0:_configuration|};

                    [SetUp]
                    public void SetUp()
                    {
                        using (var reader = new StreamReader("Test.config"))
                        {
                            _configuration = reader.ReadToEnd();
                        }
                    }

            """, "using System.IO;");

            await VerifySuppressionAsync(testCode, isSuppressed: true, kind: "field", name: "_configuration");
        }

        [TestCase("additional_setup_methods")]
        [TestCase("additional_one_time_setup_methods")]
        public async Task DoesDetectWhenFieldIsInitializedInAdditionalSetUpMethod(string configuration)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public abstract class BaseTest
                {
                    [SetUp]
                    public void SetUp()
                    {
                        OnSetUp();
                    }

                    protected virtual void OnSetUp()
                    {
                    }
                }

                [TestFixture]
                public class InheritedTest : BaseTest
                {
                    private string {|#0:field|};

                    protected override void OnSetUp()
                    {
                        base.OnSetUp();
                        field = "SetUpWasCalled";
                    }

                    [Test]
                    public void Test()
                    {
                        Assert.That(field, Is.Not.Null.And.Not.Empty);
                    }
                }
            """);

            string analyzerConfig = $"dotnet_diagnostic.NUnit.{configuration} = OnSetUp";
            await VerifySuppressionAsync(testCode, isSuppressed: true, kind: "field", name: "field", additionalConfiguration: analyzerConfig);
        }

        [TestCaseSource(nameof(TypeDeclarations))]
        public async Task ShouldIgnoreUnrelatedTypeDeclarations(string typeDeclaration)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public {{typeDeclaration}} TestType
                {
                    public string {|#0:Field|};
                }
                """);

            await VerifySuppressionAsync(testCode, isSuppressed: false, kind: "field", name: "Field");
        }

        private static Task VerifySuppressionAsync(string source, bool isSuppressed, string kind, string name, string? additionalConfiguration = null)
            => VerifySuppressionAsync(source, isSuppressed, kind, [name], additionalConfiguration);

        private static async Task VerifySuppressionAsync(string source, bool isSuppressed, string kind, string[] names, string? additionalConfiguration = null)
        {
            var test = new NUnitCSharpAnalyzerTest<NonNullableFieldOrPropertyIsUninitializedSuppressor>
            {
                TestCode = source,
                CompilerDiagnostics = CompilerDiagnostics.Warnings,
            };

            for (int i = 0; i < names.Length; i++)
            {
                test.ExpectedDiagnostics.Add(ExpectedDiagnostic
                    .WithLocation(i)
                    .WithArguments(kind, names[i])
                    .WithIsSuppressed(isSuppressed));
            }

            if (additionalConfiguration is not null)
            {
                test.TestState.AddAnalyzerConfig(additionalConfiguration);
            }

            await test.RunAsync();
        }
    }
}
