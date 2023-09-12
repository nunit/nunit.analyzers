using System.Threading.Tasks;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.DiagnosticSuppressors;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.DiagnosticSuppressors
{
    public class NonNullableFieldOrPropertyIsUninitializedSuppressorTests
    {
        private static readonly DiagnosticSuppressor suppressor = new NonNullableFieldOrPropertyIsUninitializedSuppressor();

        [Test]
        public void FieldNotAssigned()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                private string ↓field;

                [Test]
                public void Test()
                {
                    field = string.Empty;
                    Assert.That(field, Is.Not.Null);
                }
            ");

            RoslynAssert.NotSuppressed(suppressor, testCode);
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public void FieldAssigned(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                private string ↓field;

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

            RoslynAssert.Suppressed(suppressor, testCode);
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public void FieldAssignedUsingExpressionBody(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                private string ↓field;

                [{attribute}]
                public void Initialize() => {prefix}field = string.Empty;

                [Test]
                public void Test() => Assert.That({prefix}field, Is.Not.Null);
            ");

            RoslynAssert.Suppressed(suppressor, testCode);
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public void FieldNotAssignedInConstructor(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                private string testName;
                private string nunitField;

                public ↓TestClass(string name)
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

            RoslynAssert.Suppressed(suppressor, testCode);
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public void FieldAssignedUsingTupleDeconstruction(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                private string ↓name;
                private string ↓description;

                [{attribute}]
                public void Initialize()
                {{
                    ({prefix}name, {prefix}description) = GetNameAndDescription();
                }}

                [Test]
                public void Test()
                {{
                    Assert.That({prefix}name, Is.Not.Null);
                    Assert.That({prefix}description, Is.Not.Null);
                }}

                private (string Name, string Description) GetNameAndDescription() => (""SomeName"", ""Some Description"");
            ");

            RoslynAssert.Suppressed(suppressor, testCode);
        }

        [Test]
        public void FieldAssignedInTryFinally()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                private string ↓field1;
                private string ↓field2;

                [SetUp]
                public void SetUp()
                {
                    try
                    {
                        field1 = ""NUnit"";
                    }
                    finally
                    {
                        field2 = ""Analyzers"";
                    }
                }

                [Test]
                public void Test()
                {
                    Assert.That(field1, Is.Not.Null);
                    Assert.That(field2, Is.Not.Null);
                }
            ");

            RoslynAssert.Suppressed(suppressor, testCode);
        }

        [Test]
        public void PropertyNotAssigned()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                protected string ↓Property {{ get; private set; }}

                [Test]
                public void Test()
                {{
                    Assert.That(Property, Is.Not.Null);
                }}
            ");

            RoslynAssert.NotSuppressed(suppressor, testCode);
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public void PropertyAssigned(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                protected string ↓Property {{ get; private set; }}

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

            RoslynAssert.Suppressed(suppressor, testCode);
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public void PropertyAssignedUsingTupleDeconstruction(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                protected string ↓Name {{ get; private set; }}
                protected string ↓Description {{ get; private set; }}

                [{attribute}]
                public void Initialize()
                {{
                    ({prefix}Name, {prefix}Description) = (""ThatName"", ""The Matching Description"");
                }}

                [Test]
                public void Test()
                {{
                    Assert.That({prefix}Name, Is.Not.Null);
                    Assert.That({prefix}Description, Is.Not.Null);
                }}
            ");

            RoslynAssert.Suppressed(suppressor, testCode);
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public void FieldAssignedInCalledMethod(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                private string ↓field;

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

            RoslynAssert.Suppressed(suppressor, testCode);
        }

        [TestCase("async Task", "await ", "", "")]
        [TestCase("async Task", "await ", "this.", "")]
        [TestCase("async Task", "await ", "", ".ConfigureAwait(false)")]
        [TestCase("async Task", "await ", "this.", ".ConfigureAwait(false)")]
        [TestCase("void", "", "", ".Wait()")]
        [TestCase("void", "", "this.", ".Wait()")]
        public void FieldAssignedInAsyncCalledMethod(string returnType, string awaitKeyWord, string accessor, string suffix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                private string ↓field;

                [SetUp]
                public {returnType} Initialize()
                {{
                    {awaitKeyWord}{accessor}SetFields(){suffix};
                }}

                [Test]
                public void Test()
                {{
                    Assert.That({accessor}field, Is.Not.Null);
                }}

                private Task SetFields()
                {{
                    {accessor}field = string.Empty;
                    return Task.CompletedTask;
                }}
            ");

            RoslynAssert.Suppressed(suppressor, testCode);
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public void FieldAssignedInCalledMethods(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                private double numericField;
                private string ↓textField;

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

            RoslynAssert.Suppressed(suppressor, testCode);
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public void FieldIndirectAssignedThroughBaseClassSetupMethod(string attribute, string prefix)
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
                private string ↓field;

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
            RoslynAssert.NotSuppressed(suppressor, testCode);
        }

        [TestCase("SetUp", "")]
        [TestCase("OneTimeSetUp", "this.")]
        public void FieldAssignedThroughOverriddenBaseClassSetupMethod(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@$"
            #nullable enable

            public abstract class BaseClass
            {{
                [{attribute}]
                public virtual void Initialize()
                {{
                }}
            }}

            [TestFixture]
            public sealed class TestClass : BaseClass
            {{
                private const string Default = ""Default"";

                private string ↓field;

                public override void Initialize()
                {{
                    {prefix}field = Default;
                }}

                [Test]
                public void Test()
                {{
                    Assert.That({prefix}field, Is.EqualTo(Default));
                }}
            }}
            ");

            RoslynAssert.Suppressed(suppressor, testCode);
        }
    }
}
