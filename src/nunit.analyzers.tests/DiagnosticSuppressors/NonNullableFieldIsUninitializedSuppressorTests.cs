using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.DiagnosticSuppressors;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.DiagnosticSuppressors
{
    public class NonNullableFieldIsUninitializedSuppressorTests
    {
        private static readonly DiagnosticSuppressor suppressor = new NonNullableFieldIsUninitializedSuppressor();

        [Test]
        public async Task FieldNotAssigned()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                #nullable enable

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
                #nullable enable

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
                NonNullableFieldIsUninitializedSuppressor.NullableFieldInitializedInSetUp, testCode)
                .ConfigureAwait(false);
        }
    }
}
