using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.UseAssertThatAsync;

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class UseAssertThatAsyncCodeFix : CodeFixProvider
{
    internal const string WrapWithAssertMultiple = "Wrap with Assert.Multiple call";

    public sealed override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(AnalyzerIdentifiers.UseAssertThatAsync);

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var invocation = root.FindNode(diagnosticSpan) as InvocationExpressionSyntax;
        if (invocation is null)
            return;

        var argumentList = invocation.ArgumentList;

        // TODO: fix oder
        var awaitExpression = argumentList.Arguments[0].Expression as AwaitExpressionSyntax;

        if (awaitExpression is null)
            return;

        var newInvocation = invocation
            .WithExpression(
                SyntaxFactory.AwaitExpression(
                    SyntaxFactory.IdentifierName("Assert.ThatAsync")))
            .WithArgumentList(SyntaxFactory.ArgumentList(
                SyntaxFactory.SeparatedList(
                [
                    SyntaxFactory.Argument(SyntaxFactory.ParenthesizedLambdaExpression(awaitExpression.Expression)),
                    argumentList.Arguments[1]
                ])));

        var newRoot = root.ReplaceNode(invocation, newInvocation);
        context.RegisterCodeFix(
            CodeAction.Create(
                UseAssertThatAsyncConstants.Title,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                UseAssertThatAsyncConstants.Description),
            diagnostic);
    }
}
