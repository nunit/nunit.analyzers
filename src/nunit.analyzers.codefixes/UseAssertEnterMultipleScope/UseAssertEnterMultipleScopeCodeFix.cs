using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Helpers;

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

            var expressionStatementSyntax = node.FirstAncestorOrSelf<ExpressionStatementSyntax>();
            if (expressionStatementSyntax is null)
            {
                return;
            }

            var (blockSyntax, lambdaHasAsyncKeyword) = FindBlockSyntax(node);
            if (blockSyntax is null)
            {
                return;
            }

            var newBodySyntax = CreateUsingStatementSyntax(blockSyntax,
                expressionStatementSyntax.GetLeadingTrivia(), expressionStatementSyntax.GetTrailingTrivia());

            // Replace body syntax and add annotation to have a reference to the new body syntax in the updated root
            var newRoot = CodeFixHelper.UpdateTestMethodSignatureIfNecessary(
                root, expressionStatementSyntax, newBodySyntax,
                shouldBeAsync: lambdaHasAsyncKeyword);

            var codeAction = CodeAction.Create(
                UseAssertEnterMultipleScopeMethod,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                UseAssertEnterMultipleScopeMethod);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        private static (BlockSyntax? Block, bool HasAsyncKeyword) FindBlockSyntax(SyntaxNode node)
        {
            if (node is not InvocationExpressionSyntax invocationExprSyntax ||
                invocationExprSyntax.ArgumentList.Arguments.Count != 1 ||
                invocationExprSyntax.ArgumentList.Arguments[0].Expression
                    is not ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpressionSyntax)
            {
                return (null, false);
            }

            return (parenthesizedLambdaExpressionSyntax.Block, parenthesizedLambdaExpressionSyntax.AsyncKeyword != default);
        }

        private static UsingStatementSyntax CreateUsingStatementSyntax(BlockSyntax blockSyntax, SyntaxTriviaList leadingTrivia, SyntaxTriviaList trailingTrivia) =>
            SyntaxFactory.UsingStatement(blockSyntax)
                .WithExpression(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfAssert),
                            SyntaxFactory.IdentifierName(NUnitV4FrameworkConstants.NameOfEnterMultipleScope))))
                .WithLeadingTrivia(leadingTrivia)
                .WithTrailingTrivia(trailingTrivia);
    }
}
