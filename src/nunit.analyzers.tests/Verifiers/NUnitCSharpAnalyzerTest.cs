using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace NUnit.Analyzers.Tests.Verifiers
{
    internal class NUnitCSharpAnalyzerTest<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        public NUnitCSharpAnalyzerTest()
        {
            this.ReferenceAssemblies = TestSetup.ReferenceAssemblies;
            TestSetup.Configure(this.TestState);
        }

        protected override ParseOptions CreateParseOptions()
        {
            return TestSetup.AugmentParseOptions(base.CreateParseOptions());
        }

        protected override CompilationOptions CreateCompilationOptions()
        {
            return TestSetup.AugmentCompilationOptions(base.CreateCompilationOptions());
        }
    }
}
