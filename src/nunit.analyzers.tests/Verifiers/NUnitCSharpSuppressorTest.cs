using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NUnit.Analyzers.Tests.Verifiers
{
    /// <summary>
    /// Test for a <see cref="DiagnosticSuppressor"/> that suppresses diagnostics reported by other analyzers,
    /// e.g. ones from Microsoft.CodeAnalysis.NetAnalyzers. Add the analyzers whose diagnostics should be
    /// suppressed to <see cref="SuppressedAnalyzers"/>; they are run together with <typeparamref name="TSuppressor"/>.
    /// </summary>
    /// <remarks>
    /// The analyzers are passed as instances rather than as a type parameter, because the NetAnalyzers ones
    /// are loaded by reflection from the NuGet package and cannot be referenced at compile time.
    /// </remarks>
    /// <typeparam name="TSuppressor">The suppressor to verify.</typeparam>
    internal sealed class NUnitCSharpSuppressorTest<TSuppressor> : NUnitCSharpAnalyzerTest<TSuppressor>
        where TSuppressor : DiagnosticSuppressor, new()
    {
        public List<DiagnosticAnalyzer> SuppressedAnalyzers { get; } = new();

        protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
            => base.GetDiagnosticAnalyzers().Concat(this.SuppressedAnalyzers);

        protected override CompilationOptions CreateCompilationOptions()
        {
            CompilationOptions options = base.CreateCompilationOptions();

            // Many NetAnalyzers rules are disabled by default; make sure they report with at least warning severity.
            var specificDiagnosticOptions = options.SpecificDiagnosticOptions;
            foreach (DiagnosticDescriptor descriptor in this.SuppressedAnalyzers.SelectMany(a => a.SupportedDiagnostics))
            {
                specificDiagnosticOptions = specificDiagnosticOptions.SetItem(
                    descriptor.Id,
                    descriptor.DefaultSeverity == DiagnosticSeverity.Error ? ReportDiagnostic.Error : ReportDiagnostic.Warn);
            }

            return options.WithSpecificDiagnosticOptions(specificDiagnosticOptions);
        }
    }
}
