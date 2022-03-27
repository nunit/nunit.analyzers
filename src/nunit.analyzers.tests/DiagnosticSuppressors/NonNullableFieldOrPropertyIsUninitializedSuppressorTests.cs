using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.DiagnosticSuppressors;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.DiagnosticSuppressors
{
    public class NonNullableFieldOrPropertyIsUninitializedSuppressorTests
    {
        private static readonly DiagnosticSuppressor suppressor = new NonNullableFieldOrPropertyIsUninitializedSuppressor();

        [Test]
        public async Task FieldNotAssigned()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                private string field;

                [Test]
                public void Test()
                {
                    field = string.Empty;
                    Assert.That(field, Is.Not.Null);
                }
            ");

            await DiagnosticsSuppressorAnalyzer.EnsureNotSuppressed(suppressor, testCode)
                                               .ConfigureAwait(false);
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public async Task FieldAssigned(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                private string field;

                [{attribute}]
                public void Initialize()
                {{
                    {prefix}field = string.Empty;
                }}

                [Test]
                public void Test()
                {{
                    Assert.That({prefix}field, Is.Not.Null);
                }}
            ");

            await DiagnosticsSuppressorAnalyzer.EnsureSuppressed(suppressor,
                NonNullableFieldOrPropertyIsUninitializedSuppressor.NullableFieldOrPropertyInitializedInSetUp, testCode)
                .ConfigureAwait(false);
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public async Task FieldAssignedUsingExpressionBody(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                private string field;

                [{attribute}]
                public void Initialize() => {prefix}field = string.Empty;

                [Test]
                public void Test() => Assert.That({prefix}field, Is.Not.Null);
            ");

            await DiagnosticsSuppressorAnalyzer.EnsureSuppressed(suppressor,
                NonNullableFieldOrPropertyIsUninitializedSuppressor.NullableFieldOrPropertyInitializedInSetUp, testCode)
                .ConfigureAwait(false);
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public async Task FieldNotAssignedInConstructor(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                private string testName;
                private string nunitField;

                public TestClass(string name)
                {{
                    {prefix}testName = name;
                }}

                [{attribute}]
                public void Initialize()
                {{
                    {prefix}nunitField = string.Empty;
                }}

                [Test]
                public void Test()
                {{
                    Assert.That({prefix}nunitField, Is.Not.Null);
                }}
            ");

            await DiagnosticsSuppressorAnalyzer.EnsureSuppressed(suppressor,
                NonNullableFieldOrPropertyIsUninitializedSuppressor.NullableFieldOrPropertyInitializedInSetUp, testCode)
                .ConfigureAwait(false);
        }

        [Test]
        public async Task PropertyNotAssigned()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                protected string Property {{ get; private set; }}

                [Test]
                public void Test()
                {{
                    Assert.That(Property, Is.Not.Null);
                }}
            ");

            await DiagnosticsSuppressorAnalyzer.EnsureNotSuppressed(suppressor, testCode)
                                               .ConfigureAwait(false);
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public async Task PropertyAssigned(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                protected string Property {{ get; private set; }}

                [{attribute}]
                public void Initialize()
                {{
                    {prefix}Property = string.Empty;
                }}

                [Test]
                public void Test()
                {{
                    Assert.That({prefix}Property, Is.Not.Null);
                }}
            ");

            await DiagnosticsSuppressorAnalyzer.EnsureSuppressed(suppressor,
                NonNullableFieldOrPropertyIsUninitializedSuppressor.NullableFieldOrPropertyInitializedInSetUp, testCode)
                .ConfigureAwait(false);
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public async Task FieldAssignedInCalledMethod(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                private string field;

                [{attribute}]
                public void Initialize()
                {{
                    {prefix}SetFields();
                }}

                [Test]
                public void Test()
                {{
                    Assert.That({prefix}field, Is.Not.Null);
                }}

                private void SetFields()
                {{
                    {prefix}field = string.Empty;
                }}
            ");

            await DiagnosticsSuppressorAnalyzer.EnsureSuppressed(suppressor,
                NonNullableFieldOrPropertyIsUninitializedSuppressor.NullableFieldOrPropertyInitializedInSetUp, testCode)
                .ConfigureAwait(false);
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public async Task FieldAssignedInCalledMethods(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                private double numericField;
                private string textField;

                [{attribute}]
                public void Initialize()
                {{
                    {prefix}SetFields();
                }}

                [Test]
                public void Test()
                {{
                    Assert.That({prefix}numericField, Is.Not.Zero);
                    Assert.That({prefix}textField, Is.Not.Null);
                }}

                public void SetFields()
                {{
                    {prefix}SetFields(3, 4);
                    {prefix}SetFields(""Seven"");
                }}
                
                private void SetFields(params double[] numbers)
                {{
                    double sum = 0;
                    for (int i = 0; i < numbers.Length; i++)
                        sum += numbers[i];
                    {prefix}numericField = sum;
                }}

                private void SetFields(string? text)
                {{
                    {prefix}textField = text ?? string.Empty;
                }}
            ");

            await DiagnosticsSuppressorAnalyzer.EnsureSuppressed(suppressor,
                NonNullableFieldOrPropertyIsUninitializedSuppressor.NullableFieldOrPropertyInitializedInSetUp, testCode)
                .ConfigureAwait(false);
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public async Task FieldIndirectAssignedThroughBaseClassSetupMethod(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@$"
            public class BaseClass
            {{
                [{attribute}]
                public void Initialize()
                {{
                    SetFields();
                }}

                protected virtual void SetFields(string text = ""Default"")
                {{
                }}
            }}

            [TestFixture]
            public class TestClass : BaseClass
            {{
                private string field;

                [Test]
                public void Test()
                {{
                    Assert.That({prefix}field, Is.EqualTo(""Default""));
                }}

                protected override void SetFields(string text)
                {{
                    {prefix}field = text;
                }}
            }}
            ");

            // This is not supported as the BaseClass could be in an external library.
            // We therefore cannot validate that its SetUp method calls into our overridden method.
            await DiagnosticsSuppressorAnalyzer.EnsureNotSuppressed(suppressor, testCode)
                                               .ConfigureAwait(false);
        }
    }
}
