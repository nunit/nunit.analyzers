using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.DelegateUnnecessary
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public class DelegateUnnecessaryCodeFix : CodeFixProvider
    {
        internal const string RemoveAnonymousLambdaDescription = "Remove anonymous lambda as it is unnecessary";
        internal const string InvokedMethodExplicitly = "Explicitly call the method in user code, instead of implicit from the Assert";

        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(AnalyzerIdentifiers.DelegateUnnecessary);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            if (root is null)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            var diagnostic = context.Diagnostics.First();
            var node = root.FindNode(diagnostic.Location.SourceSpan);
            var argumentNode = node as ArgumentSyntax;
            if (argumentNode is null)
                return;

            context.CancellationToken.ThrowIfCancellationRequested();

            ArgumentSyntax original = argumentNode;

            if (argumentNode.Expression is ParenthesizedLambdaExpressionSyntax lambda)
            {
                if (lambda.ExpressionBody is null)
                    return;

                ArgumentSyntax replacement;
                SyntaxNode newRoot;

                if (diagnostic.Properties.ContainsKey(AnalyzerPropertyKeys.IsAsync))
                {
                    replacement = argumentNode.WithExpression(
                        SyntaxFactory.AwaitExpression(lambda.ExpressionBody)
                        .WithLeadingTrivia(argumentNode.GetLeadingTrivia())
                        .WithTrailingTrivia(argumentNode.GetTrailingTrivia()));

                    newRoot = CodeFixHelper.UpdateTestMethodSignatureIfNecessary(
                        root, original, replacement,
                        shouldBeAsync: true);
                }
                else
                {
                    replacement = argumentNode.WithExpression(lambda.ExpressionBody);
                    newRoot = root.ReplaceNode(original, replacement);
                }

                context.RegisterCodeFix(
                    CodeAction.Create(
                        RemoveAnonymousLambdaDescription,
                        _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                        RemoveAnonymousLambdaDescription), diagnostic);
            }
            else if (argumentNode.Expression is IdentifierNameSyntax identifier)
            {
                var invocation = SyntaxFactory.InvocationExpression(identifier, SyntaxFactory.ArgumentList());
                ArgumentSyntax replacement = argumentNode.WithExpression(invocation);

                context.RegisterCodeFix(
                    CodeAction.Create(
                        InvokedMethodExplicitly,
                        _ => Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(original, replacement))),
                        InvokedMethodExplicitly), diagnostic);
            }
        }
    }
}
