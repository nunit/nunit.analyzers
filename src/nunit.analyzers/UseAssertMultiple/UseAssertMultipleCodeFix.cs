using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.UseAssertMultiple
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public class UseAssertMultipleCodeFix : CodeFixProvider
    {
        internal const string WrapWithAssertMultiple = "Wrap with Assert.Multiple call";

        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(AnalyzerIdentifiers.UseAssertMultiple);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            if (root is null)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            SyntaxNode node = root.FindNode(context.Span);

            if (node is not InvocationExpressionSyntax assertExpression)
                return;

            var assertStatement = assertExpression.Parent;
            var previousArguments = new HashSet<string>(StringComparer.Ordinal)
            {
                assertExpression.ArgumentList.Arguments[0].ToString()
            };

            var statementsBeforeAssertMultiple = new List<StatementSyntax>();
            var statementsInsideAssertMultiple = new List<StatementSyntax>();
            var statementsAfterAssertMultiple = new List<StatementSyntax>();
            var statements = new[] { statementsBeforeAssertMultiple, statementsInsideAssertMultiple, statementsAfterAssertMultiple };

            // Find the block holding the Assert that need to combined into an Assert.Multple
            var block = node.Ancestors().OfType<BlockSyntax>().FirstOrDefault();
            if (block is null)
                return;

            // Move all statements based upon their location.
            bool needsAsync = false;
            int when = 0;
            foreach (var statement in block.Statements)
            {
                if (statement == assertStatement)
                {
                    when = 1;
                }
                else if (when == 1)
                {
                    if (statement is ExpressionStatementSyntax expressionStatement &&
                        AssertHelper.IsAssert(expressionStatement.Expression, out string member, out ArgumentListSyntax? argumentList) &&
                        member == NUnitFrameworkConstants.NameOfAssertThat &&
                        UseAssertMultipleAnalyzer.IsIndependent(previousArguments, argumentList.Arguments[0].ToString()))
                    {
                        // Can be merged
                        // Check if this expression uses 'await'.
                        needsAsync |= statement.DescendantNodesAndSelf(x => true, false)
                                               .OfType<AwaitExpressionSyntax>()
                                               .Any();
                    }
                    else
                    {
                        when = 2;
                    }
                }

                statements[when].Add(statement);
            }

            // If there was an empty line between the code above the Assert, then keep it.
            SyntaxTrivia? endOfLineTrivia = default;

            var firstStatement = statementsInsideAssertMultiple[0];
            if (firstStatement.HasLeadingTrivia)
            {
                SyntaxTriviaList trivia = firstStatement.GetLeadingTrivia();
                SyntaxTrivia firstTrivia = trivia.First();
                if (firstTrivia.IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    // Remember the trivia and delete it from the first statement inside the Assert.Multiple
                    endOfLineTrivia = firstTrivia;
                    statementsInsideAssertMultiple[0] = firstStatement.ReplaceTrivia(firstTrivia, Enumerable.Empty<SyntaxTrivia>());
                }
            }

            ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpression =
                SyntaxFactory.ParenthesizedLambdaExpression(
                    SyntaxFactory.Block(statementsInsideAssertMultiple));

            if (needsAsync)
            {
                parenthesizedLambdaExpression = parenthesizedLambdaExpression
                    .WithAsyncKeyword(SyntaxFactory.Token(SyntaxKind.AsyncKeyword));
            }

            var assertMultiple = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfAssert),
                        SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfMultiple)),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.Argument(parenthesizedLambdaExpression)
                        }))))
                .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed)
                .WithAdditionalAnnotations(Formatter.Annotation);

            if (endOfLineTrivia is not null)
            {
                // Add the remembered blank line to go before the Assert.Multiple statement.
                assertMultiple = assertMultiple.WithLeadingTrivia(endOfLineTrivia.Value);
            }

            // Comments at the end of a block are not associated with the last statement but with the closing brace
            // Keep the exising block's open and close braces with associated trivia in our updated block.
            var updatedBlock = SyntaxFactory.Block(
                block.OpenBraceToken,
                SyntaxFactory.List(statementsBeforeAssertMultiple.Append(assertMultiple).Concat(statementsAfterAssertMultiple)),
                block.CloseBraceToken);

            SyntaxNode newRoot = root.ReplaceNode(block, updatedBlock);

            var codeAction = CodeAction.Create(
                WrapWithAssertMultiple,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                WrapWithAssertMultiple);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }
    }
}
