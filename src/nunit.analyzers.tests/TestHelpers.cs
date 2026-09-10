using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
            => CreateCompilation(code is null ? null : [code], settings);

        internal static Compilation CreateCompilation(string[]? code, Settings? settings = null)
        {
            IEnumerable<SyntaxTree>? syntaxTrees = code?.Select(text => CSharpSyntaxTree.ParseText(text));

            settings ??= Settings.Default;

            return CSharpCompilation.Create(Guid.NewGuid().ToString("N"),
                syntaxTrees,
                references: settings.MetadataReferences,
                options: settings.CompilationOptions);
        }

        internal static async Task SuppressedOrNot(DiagnosticAnalyzer analyzer, DiagnosticSuppressor suppressor, string[] code, bool isSuppressed, Settings? settings = null)
        {
            string id = analyzer.SupportedDiagnostics[0].Id;
            Assert.That(suppressor.SupportedSuppressions.Select(x => x.SuppressedDiagnosticId), Does.Contain(id));

            settings ??= Settings.Default;
            settings = settings.WithCompilationOptions(Settings.Default.CompilationOptions.WithWarningOrError(analyzer.SupportedDiagnostics));

            Compilation compilation = CreateCompilation(code, settings);

            CompilationWithAnalyzers compilationWithAnalyzer = compilation
                .WithAnalyzers([analyzer]);
            ImmutableArray<Diagnostic> diagnostics = await compilationWithAnalyzer.GetAllDiagnosticsAsync().ConfigureAwait(false);
            Assert.That(diagnostics, Has.Length.EqualTo(1));
            Assert.That(diagnostics[0].Id, Is.EqualTo(id));

            AnalyzerOptions? analyzerOptions = null;
            if (!string.IsNullOrEmpty(settings.AnalyzerConfig))
            {
                analyzerOptions = new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty, TestAnalyzerConfigOptionsProvider.Create(settings.AnalyzerConfig));
            }

            CompilationWithAnalyzers compilationWithAnalyzerAndSuppressor = compilation
                .WithAnalyzers([analyzer, suppressor], analyzerOptions);
            diagnostics = await compilationWithAnalyzerAndSuppressor.GetAllDiagnosticsAsync().ConfigureAwait(false);
            Assert.That(diagnostics, Has.Length.EqualTo(1));
            Assert.That(diagnostics[0].Id, Is.EqualTo(id));
            Assert.That(diagnostics[0].IsSuppressed, Is.EqualTo(isSuppressed));
        }

        internal static Task NotSuppressed(DiagnosticAnalyzer analyzer, DiagnosticSuppressor suppressor, string[] code, Settings? settings = null)
            => SuppressedOrNot(analyzer, suppressor, code, false, settings);

        internal static Task NotSuppressed(DiagnosticAnalyzer analyzer, DiagnosticSuppressor suppressor, string code, Settings? settings = null)
            => SuppressedOrNot(analyzer, suppressor, [code], false, settings);

        internal static Task Suppressed(DiagnosticAnalyzer analyzer, DiagnosticSuppressor suppressor, string[] code, Settings? settings = null)
            => SuppressedOrNot(analyzer, suppressor, code, true, settings);

        internal static Task Suppressed(DiagnosticAnalyzer analyzer, DiagnosticSuppressor suppressor, string code, Settings? settings = null)
            => SuppressedOrNot(analyzer, suppressor, [code], true, settings);

        internal static (SyntaxTree Tree, Compilation Compilation) GetTreeAndCompilation(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);

            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToString("N"),
                syntaxTrees: [tree],
                references: Settings.Default.MetadataReferences,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            return (tree, compilation);
        }

        internal static async Task<(SyntaxNode Node, Compilation Compilation, SemanticModel Model)> GetRootCompilationAndModel(string code)
        {
            (SyntaxTree tree, Compilation compilation) = GetTreeAndCompilation(code);
            var model = compilation.GetSemanticModel(tree);
            var root = await tree.GetRootAsync().ConfigureAwait(false);

            return (root, compilation, model);
        }

        internal static async Task<(SyntaxNode Node, SemanticModel Model)> GetRootAndModel(string code)
        {
            (SyntaxNode node, _, SemanticModel model) = await GetRootCompilationAndModel(code).ConfigureAwait(false);

            return (node, model);
        }

        private class TestAnalyzerConfigOptionsProvider(Dictionary<string, string> options) : AnalyzerConfigOptionsProvider
        {
            private readonly AnalyzerConfigOptions globalOptions = new TestAnalyzerConfigOptions(options);

            public override AnalyzerConfigOptions GlobalOptions => this.globalOptions;

            public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => this.globalOptions;

            public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => this.globalOptions;

            public static TestAnalyzerConfigOptionsProvider Create(string additionalConfig)
            {
                var options = new Dictionary<string, string>(AnalyzerConfigOptions.KeyComparer);

                using (var reader = new System.IO.StringReader(additionalConfig))
                {
                    string? line;
                    while ((line = reader.ReadLine()) is not null)
                    {
                        // This is a simplistic parser that doesn't handle comments.
                        int indexOfEquals = line.IndexOf('=');
                        if (indexOfEquals > 0)
                        {
                            string key = line.AsSpan(0, indexOfEquals).Trim().ToString();
                            string value = line.AsSpan(indexOfEquals + 1).Trim().ToString();
                            options[key] = value;
                        }
                    }
                }

                return new TestAnalyzerConfigOptionsProvider(options);
            }

            private sealed class TestAnalyzerConfigOptions(Dictionary<string, string> options) : AnalyzerConfigOptions
            {
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
                // We cannot add the [NotNullWhen(true)] attribute as there are 2 instances:
                // * From the .NET8+ runtime
                // * From Nullable (via nunit.analyzer project)
                // Even though the latter are declared internal, the InternalsVisibleTo attribute in the nunit.analyzers project makes it visible.
                // Alternative solution is to mark all internal members as public and remove the InternalsVisibleTo attribute.
                public override bool TryGetValue(string key, out string? value) => options.TryGetValue(key, out value);
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
            }
        }
    }
}
