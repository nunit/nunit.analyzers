using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace NUnit.Analyzers.Tests.Verifiers
{
    /// <summary>
    /// Helper for verifying <typeparamref name="TAnalyzer"/> without needing to configure
    /// the required NUnit metadata references and parse options for every test.
    /// Use it through a using alias, e.g. <c>using Verify = NUnitCSharpAnalyzerVerifier&lt;MyAnalyzer&gt;;</c>.
    /// </summary>
    /// <typeparam name="TAnalyzer">The analyzer to verify.</typeparam>
    internal sealed class NUnitCSharpAnalyzerVerifier<TAnalyzer> : AnalyzerVerifier<TAnalyzer, NUnitCSharpAnalyzerTest<TAnalyzer>, DefaultVerifier>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        /// <summary>
        /// Verifies that <paramref name="source"/> produces the <paramref name="expected"/> diagnostics.
        /// </summary>
        /// <remarks>
        /// Hides the inherited method to pass <see cref="TestSetup.CancellationToken"/>.
        /// </remarks>
        public static new Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
            => VerifyAnalyzerAsync(source, _ => { }, expected);

        /// <summary>
        /// Verifies that <paramref name="source"/> produces the <paramref name="expected"/> diagnostics,
        /// after applying test specific settings with <paramref name="configure"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// await Verify.VerifyAnalyzerAsync(testCode, test =>
        /// {
        ///     test.TestState.Sources.Add(otherFile);
        ///     test.TestState.AddAnalyzerConfig("dotnet_diagnostic.NUnit1234.some_option = true");
        ///     test.CompilerDiagnostics = CompilerDiagnostics.None;
        /// }, expected);
        /// </code>
        /// </example>
        public static Task VerifyAnalyzerAsync(string source, Action<NUnitCSharpAnalyzerTest<TAnalyzer>> configure, params DiagnosticResult[] expected)
        {
            var test = new NUnitCSharpAnalyzerTest<TAnalyzer>
            {
                TestCode = source,
            };

            test.ExpectedDiagnostics.AddRange(expected);
            configure(test);
            return test.RunAsync(TestSetup.CancellationToken);
        }
    }
}
