using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests
{
    internal static class TestHelpers
    {
        internal static Compilation CreateCompilation(string? code = null, Settings? settings = null)
        {
            var syntaxTrees = code is null
                ? null
                : new[] { CSharpSyntaxTree.ParseText(code) };

            settings ??= Settings.Default;

            return CSharpCompilation.Create(Guid.NewGuid().ToString("N"),
                syntaxTrees,
                references: settings.MetadataReferences,
                options: settings.CompilationOptions);
        }

        internal static async Task SuppressedOrNot(DiagnosticAnalyzer analyzer, DiagnosticSuppressor suppressor, string code, bool isSuppressed, Settings? settings = null)
        {
            string id = analyzer.SupportedDiagnostics[0].Id;
            Assert.That(suppressor.SupportedSuppressions[0].SuppressedDiagnosticId, Is.EqualTo(id));

            settings ??= Settings.Default;
            settings = settings.WithCompilationOptions(Settings.Default.CompilationOptions.WithWarningOrError(analyzer.SupportedDiagnostics));

            Compilation compilation = CreateCompilation(code, settings);

            CompilationWithAnalyzers compilationWithAnalyzer = compilation
                .WithAnalyzers(ImmutableArray.Create(analyzer));
            ImmutableArray<Diagnostic> diagnostics = await compilationWithAnalyzer.GetAllDiagnosticsAsync().ConfigureAwait(false);
            Assert.That(diagnostics, Has.Length.EqualTo(1));
            Assert.That(diagnostics[0].Id, Is.EqualTo(id));

            CompilationWithAnalyzers compilationWithAnalyzerAndSuppressor = compilation
                .WithAnalyzers(ImmutableArray.Create(analyzer, suppressor));
            diagnostics = await compilationWithAnalyzerAndSuppressor.GetAllDiagnosticsAsync().ConfigureAwait(false);
            Assert.That(diagnostics, Has.Length.EqualTo(1));
            Assert.That(diagnostics[0].Id, Is.EqualTo(id));
            Assert.That(diagnostics[0].IsSuppressed, Is.EqualTo(isSuppressed));
        }

        internal static Task NotSuppressed(DiagnosticAnalyzer analyzer, DiagnosticSuppressor suppressor, string code, Settings? settings = null)
            => SuppressedOrNot(analyzer, suppressor, code, false, settings);

        internal static Task Suppressed(DiagnosticAnalyzer analyzer, DiagnosticSuppressor suppressor, string code, Settings? settings = null)
            => SuppressedOrNot(analyzer, suppressor, code, true, settings);

        internal static async Task<(SyntaxNode Node, SemanticModel Model)> GetRootAndModel(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);

            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToString("N"),
                syntaxTrees: new[] { tree },
                references: Settings.Default.MetadataReferences,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var model = compilation.GetSemanticModel(tree);
            var root = await tree.GetRootAsync().ConfigureAwait(false);

            return (root, model);
        }
    }
}
