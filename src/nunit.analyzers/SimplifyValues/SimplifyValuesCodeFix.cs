using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.SimplifyValues;

[ExportCodeFixProvider(LanguageNames.CSharp)]
[Shared]
public class SimplifyValuesCodeFix : CodeFixProvider
{
    internal const string SimplifyValuesTitle = "Simplify the Values attribute";

    public override ImmutableArray<string> FixableDiagnosticIds { get; }
        = ImmutableArray.Create(AnalyzerIdentifiers.SimplifyValues);

    public sealed override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

        if (root is null || semanticModel is null)
            return;

        context.CancellationToken.ThrowIfCancellationRequested();

        var attributeNode = root.FindNode(context.Span) as AttributeSyntax;
        if (attributeNode is null)
            return;

        var newRoot = root
            .ReplaceNode(attributeNode, attributeNode.WithArgumentList(null));

        var codeAction = CodeAction.Create(
            SimplifyValuesTitle,
            _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
            SimplifyValuesTitle);

        context.RegisterCodeFix(codeAction, context.Diagnostics);
    }
}
