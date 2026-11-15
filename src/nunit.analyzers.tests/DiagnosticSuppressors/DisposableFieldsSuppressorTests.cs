using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.DiagnosticSuppressors;
using NUnit.Analyzers.Tests.Verifiers;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.DiagnosticSuppressors
{
    public class DisposableFieldsSuppressorTests
    {
        private DiagnosticAnalyzer analyzer;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Find the NetAnalyzers assembly (note version should match the one referenced)
            string netAnalyzersPath = Path.Combine(PathHelper.GetNuGetPackageDirectory(),
                "microsoft.codeanalysis.netanalyzers/8.0.0/analyzers/dotnet/cs/Microsoft.CodeAnalysis.NetAnalyzers.dll");
            Assembly netAnalyzerAssembly = Assembly.LoadFrom(netAnalyzersPath);
            Type analyzerType = netAnalyzerAssembly.GetType("Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.TypesThatOwnDisposableFieldsShouldBeDisposableAnalyzer", true)!;
            this.analyzer = (DiagnosticAnalyzer)Activator.CreateInstance(analyzerType)!;
        }

        [Test]
        public async Task FieldNotDisposed()
        {
            var testCode = WrapInTestClass("""
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

            """);

            // This rule doesn't care. Actual checking is done in DisposeFieldsInTearDownAnalyzer
            await this.VerifySuppressionAsync(testCode, isSuppressed: true);
        }

        [TestCase(NUnitFrameworkConstants.NameOfTearDownAttribute, "")]
        [TestCase(NUnitFrameworkConstants.NameOfOneTimeTearDownAttribute, "this.")]
        public async Task FieldDisposed(string attribute, string prefix)
        {
            var testCode = WrapInTestClass($$"""
                    private IDisposable? field1;
                    private IDisposable? field2;

                    [{{attribute}}]
                    public void CleanUp()
                    {
                        {{prefix}}field1?.Dispose();
                        if ({{prefix}}field2 is not null)
                        {
                            {{prefix}}field2.Dispose();
                        }
                    }

                    [Test]
                    public void Test1()
                    {
                        {{prefix}}field1 = new DummyDisposable();
                        Assert.That({{prefix}}field1, Is.Not.Null);
                    }

                    [Test]
                    public void Test2()
                    {
                        {{prefix}}field2 = new DummyDisposable();
                        Assert.That({{prefix}}field2, Is.Not.Null);
                    }

                    private sealed class DummyDisposable : IDisposable
                    {
                        public void Dispose() {}
                    }

            """);

            await this.VerifySuppressionAsync(testCode, isSuppressed: true);
        }

        [Test]
        public async Task ShouldNotSuppressWhenInstancePerTestCase()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
                public class {|#0:TestClass|}
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

            """);

            // InstancePerTestCase mean test should use IDisposable
            await this.VerifySuppressionAsync(testCode, isSuppressed: false);
        }

        private static string WrapInTestClass(string members) => TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                [TestFixture]
                public class {|#0:TestClass|}
                {
            {{members}}
                }
            """);

        private async Task VerifySuppressionAsync(string testCode, bool isSuppressed)
        {
            string id = this.analyzer.SupportedDiagnostics[0].Id;
            Assert.That(new TypesThatOwnDisposableFieldsShouldBeDisposableSuppressor().SupportedSuppressions.Select(x => x.SuppressedDiagnosticId), Does.Contain(id));

            var test = new NUnitCSharpSuppressorTest<TypesThatOwnDisposableFieldsShouldBeDisposableSuppressor>
            {
                TestCode = testCode,
                SuppressedAnalyzers = { this.analyzer },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult(id, DiagnosticSeverity.Warning).WithLocation(0).WithIsSuppressed(isSuppressed),
                },
            };

            await test.RunAsync();
        }
    }
}
