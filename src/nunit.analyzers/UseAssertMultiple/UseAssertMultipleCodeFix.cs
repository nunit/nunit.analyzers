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
                    }
                    else
                    {
                        when = 2;
                    }
                }

                statements[when].Add(statement);
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
                            SyntaxFactory.Argument(
                                SyntaxFactory.ParenthesizedLambdaExpression(
                                    SyntaxFactory.Block(statementsInsideAssertMultiple)))
                        })))).WithAdditionalAnnotations(Formatter.Annotation);

            var updatedBlock = SyntaxFactory.Block(
                statementsBeforeAssertMultiple.Append(assertMultiple).Concat(statementsAfterAssertMultiple));

            SyntaxNode newRoot = root.ReplaceNode(block, updatedBlock);

            var codeAction = CodeAction.Create(
                WrapWithAssertMultiple,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                WrapWithAssertMultiple);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }
    }
}
