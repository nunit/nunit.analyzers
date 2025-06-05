using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.UseAssertEnterMultipleScope
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public class UseAssertEnterMultipleScopeCodeFix : CodeFixProvider
    {
        internal const string UseAssertEnterMultipleScopeMethod = "Use Assert.EnterMultipleScope method";

        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(AnalyzerIdentifiers.UseAssertEnterMultipleScope);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            if (root is null)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            var node = root.FindNode(context.Span);

            var expressionStatementSyntax = FindNearestParentOfType<ExpressionStatementSyntax>(node);
            if (expressionStatementSyntax is null)
            {
                return;
            }

            var blockSyntax = FindBlockSyntax(node);
            if (blockSyntax is null)
            {
                return;
            }

            var newSyntax = CreateUsingStatementSyntax(blockSyntax, expressionStatementSyntax.GetTrailingTrivia());

            var newRoot = root.ReplaceNode(expressionStatementSyntax, newSyntax);

            var codeAction = CodeAction.Create(
                UseAssertEnterMultipleScopeMethod,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                UseAssertEnterMultipleScopeMethod);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        private static BlockSyntax? FindBlockSyntax(SyntaxNode node)
        {
            if (node is not InvocationExpressionSyntax invocationExprSyntax ||
                invocationExprSyntax.ArgumentList.Arguments.Count != 1 ||
                invocationExprSyntax.ArgumentList.Arguments[0].Expression
                    is not ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpressionSyntax)
            {
                return null;
            }

            return parenthesizedLambdaExpressionSyntax.Block;
        }

        private static UsingStatementSyntax CreateUsingStatementSyntax(BlockSyntax blockSyntax, SyntaxTriviaList trailingTrivia) =>
            SyntaxFactory.UsingStatement(blockSyntax)
                .WithExpression(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfAssert),
                            SyntaxFactory.IdentifierName(NUnitV4FrameworkConstants.NameOfEnterMultipleScope))))
                .WithTrailingTrivia(trailingTrivia);

        private static T? FindNearestParentOfType<T>(SyntaxNode node)
            where T : SyntaxNode
        {
            var current = node;
            while (current is not null && current is not T)
            {
                current = current.Parent;
            }

            return current as T;
        }
    }
}
