using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace NUnit.Analyzers.Tests.Verifiers
{
    /// <summary>
    /// Helper for verifying <typeparamref name="TAnalyzer"/> and <typeparamref name="TCodeFix"/> without needing
    /// to configure the required NUnit metadata references and parse options for every test.
    /// Use it through a using alias, e.g. <c>using Verify = NUnitCSharpCodeFixVerifier&lt;MyAnalyzer, MyCodeFix&gt;;</c>.
    /// </summary>
    /// <typeparam name="TAnalyzer">The analyzer to verify.</typeparam>
    /// <typeparam name="TCodeFix">The code fix to verify.</typeparam>
    internal sealed class NUnitCSharpCodeFixVerifier<TAnalyzer, TCodeFix> : CodeFixVerifier<TAnalyzer, TCodeFix, NUnitCSharpCodeFixTest<TAnalyzer, TCodeFix>, DefaultVerifier>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        // The methods below hide the inherited ones to pass TestSetup.CancellationToken.

        /// <summary>
        /// Verifies that <paramref name="source"/> produces the <paramref name="expected"/> diagnostics.
        /// </summary>
        public static new Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
            => VerifyAnalyzerAsync(source, _ => { }, expected);

        /// <summary>
        /// Verifies that applying the first code action offered for the diagnostics marked up in
        /// <paramref name="source"/> results in <paramref name="fixedSource"/>.
        /// </summary>
        public static new Task VerifyCodeFixAsync(string source, string fixedSource)
            => VerifyCodeFixAsync(source, DiagnosticResult.EmptyDiagnosticResults, fixedSource, fixTitle: null);

        /// <summary>
        /// Verifies that applying the first code action offered for the <paramref name="expected"/>
        /// diagnostic in <paramref name="source"/> results in <paramref name="fixedSource"/>.
        /// </summary>
        public static new Task VerifyCodeFixAsync(string source, DiagnosticResult expected, string fixedSource)
            => VerifyCodeFixAsync(source, [expected], fixedSource, fixTitle: null);

        /// <summary>
        /// Verifies that applying the first code action offered for the <paramref name="expected"/>
        /// diagnostics in <paramref name="source"/> results in <paramref name="fixedSource"/>.
        /// </summary>
        public static new Task VerifyCodeFixAsync(string source, DiagnosticResult[] expected, string fixedSource)
            => VerifyCodeFixAsync(source, expected, fixedSource, fixTitle: null);

        /// <summary>
        /// Verifies that <paramref name="source"/> produces the <paramref name="expected"/> diagnostics,
        /// after applying test specific settings with <paramref name="configure"/>.
        /// </summary>
        /// <remarks>
        /// See <see cref="NUnitCSharpAnalyzerVerifier{TAnalyzer}.VerifyAnalyzerAsync(string, Action{NUnitCSharpAnalyzerTest{TAnalyzer}}, DiagnosticResult[])"/>.
        /// </remarks>
        public static Task VerifyAnalyzerAsync(string source, Action<NUnitCSharpCodeFixTest<TAnalyzer, TCodeFix>> configure, params DiagnosticResult[] expected)
        {
            var test = new NUnitCSharpCodeFixTest<TAnalyzer, TCodeFix>
            {
                TestCode = source,
            };

            test.ExpectedDiagnostics.AddRange(expected);
            configure(test);
            return test.RunAsync(TestSetup.CancellationToken);
        }

        /// <summary>
        /// Verifies that applying the code action titled <paramref name="fixTitle"/> to the diagnostics
        /// marked up in <paramref name="source"/> results in <paramref name="fixedSource"/>.
        /// </summary>
        public static Task VerifyCodeFixAsync(string source, string fixedSource, string fixTitle)
            => VerifyCodeFixAsync(source, DiagnosticResult.EmptyDiagnosticResults, fixedSource, fixTitle);

        /// <summary>
        /// Verifies that applying the code action titled <paramref name="fixTitle"/> to the <paramref name="expected"/>
        /// diagnostics in <paramref name="source"/> results in <paramref name="fixedSource"/>.
        /// When <paramref name="fixTitle"/> is <see langword="null"/> the first code action offered is applied.
        /// Use <paramref name="configure"/> for test specific settings, see
        /// <see cref="NUnitCSharpAnalyzerVerifier{TAnalyzer}.VerifyAnalyzerAsync(string, Action{NUnitCSharpAnalyzerTest{TAnalyzer}}, DiagnosticResult[])"/>.
        /// </summary>
        public static Task VerifyCodeFixAsync(string source, DiagnosticResult expected, string fixedSource, string? fixTitle,
            Action<NUnitCSharpCodeFixTest<TAnalyzer, TCodeFix>>? configure = null)
            => VerifyCodeFixAsync(source, [expected], fixedSource, fixTitle, configure);

        /// <summary>
        /// Verifies that applying the code action titled <paramref name="fixTitle"/> to the <paramref name="expected"/>
        /// diagnostics in <paramref name="source"/> results in <paramref name="fixedSource"/>.
        /// When <paramref name="fixTitle"/> is <see langword="null"/> the first code action offered is applied.
        /// Use <paramref name="configure"/> for test specific settings, see
        /// <see cref="NUnitCSharpAnalyzerVerifier{TAnalyzer}.VerifyAnalyzerAsync(string, Action{NUnitCSharpAnalyzerTest{TAnalyzer}}, DiagnosticResult[])"/>.
        /// </summary>
        public static Task VerifyCodeFixAsync(string source, DiagnosticResult[] expected, string fixedSource, string? fixTitle,
            Action<NUnitCSharpCodeFixTest<TAnalyzer, TCodeFix>>? configure = null)
        {
            var test = new NUnitCSharpCodeFixTest<TAnalyzer, TCodeFix>
            {
                TestCode = source,
                FixedCode = fixedSource,
            };

            if (fixTitle is not null)
            {
                // Our code fixes use the title as the equivalence key, so this selects the action by title.
                test.CodeActionEquivalenceKey = fixTitle;
                test.CodeActionVerifier = (codeAction, verifier) => verifier.Equal(fixTitle, codeAction.Title);
            }

            test.ExpectedDiagnostics.AddRange(expected);
            configure?.Invoke(test);
            return test.RunAsync(TestSetup.CancellationToken);
        }
    }
}
