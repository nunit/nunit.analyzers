using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.DiagnosticSuppressors;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.DiagnosticSuppressors
{
    public class DisposableFieldsSuppressorTests
    {
        private static readonly DiagnosticSuppressor suppressor = new TypesThatOwnDisposableFieldsShouldBeDisposableSuppressor();
        private DiagnosticAnalyzer analyzer;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Find the NetAnalyzers assembly (note version should match the one referenced)
            string netAnalyzersPath = Path.Combine(PathHelper.GetNuGetPackageDirectory(),
                "microsoft.codeanalysis.netanalyzers/7.0.4/analyzers/dotnet/cs/Microsoft.CodeAnalysis.CSharp.NetAnalyzers.dll");
            Assembly netAnalyzerAssembly = Assembly.LoadFrom(netAnalyzersPath);
            Type analyzerType = netAnalyzerAssembly.GetType("Microsoft.CodeQuality.CSharp.Analyzers.ApiDesignGuidelines.CSharpTypesThatOwnDisposableFieldsShouldBeDisposableAnalyzer", true)!;
            this.analyzer = (DiagnosticAnalyzer)Activator.CreateInstance(analyzerType)!;

            this.analyzer = new DefaultEnabledAnalyzer(this.analyzer);
        }

        [Test]
        public async Task FieldNotDisposed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                private IDisposable? field;

                [Test]
                public void Test()
                {
                    field = new DummyDisposable();
                    Assert.That(field, Is.Not.Null);
                }

                private sealed class DummyDisposable : IDisposable
                {
                    public void Dispose() {}
                }
            ");

            // This rule doesn't care. Actual checking is done in DisposeFieldsInTearDownAnalyzer
            await TestHelpers.Suppressed(this.analyzer, suppressor, testCode).ConfigureAwait(true);
        }

        [TestCase(NUnitFrameworkConstants.NameOfTearDownAttribute, "")]
        [TestCase(NUnitFrameworkConstants.NameOfOneTimeTearDownAttribute, "this.")]
        public async Task FieldDisposed(string attribute, string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
                private IDisposable? field1;
                private IDisposable? field2;

                [{attribute}]
                public void CleanUp()
                {{
                    {prefix}field1?.Dispose();
                    if ({prefix}field2 is not null)
                    {{
                        {prefix}field2.Dispose();
                    }}
                }}

                [Test]
                public void Test1()
                {{
                    {prefix}field1 = new DummyDisposable();
                    Assert.That({prefix}field1, Is.Not.Null);
                }}

                [Test]
                public void Test2()
                {{
                    {prefix}field2 = new DummyDisposable();
                    Assert.That({prefix}field2, Is.Not.Null);
                }}

                private sealed class DummyDisposable : IDisposable
                {{
                    public void Dispose() {{}}
                }}
            ");

            await TestHelpers.Suppressed(this.analyzer, suppressor, testCode).ConfigureAwait(true);
        }

        [Test]
        public async Task ShouldNotSuppressWhenInstancePerTestCase()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
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

                private sealed class DummyDisposable : IDisposable
                {
                    public void Dispose() {}
                }
            }
            ");

            // InstancePerTestCase mean test should use IDisposable
            await TestHelpers.NotSuppressed(this.analyzer, suppressor, testCode).ConfigureAwait(true);
        }
    }
}
