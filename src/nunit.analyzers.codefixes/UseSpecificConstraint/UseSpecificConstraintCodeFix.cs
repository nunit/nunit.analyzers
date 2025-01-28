using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NUnit.Analyzers.UseSpecificConstraint
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    internal class UseSpecificConstraintCodeFix : CodeFixProvider
    {
        internal const string UseSpecificConstraint = "Use specific constraint";

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            AnalyzerIdentifiers.UseSpecificConstraint);

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

            if (node is ArgumentSyntax argument)
            {
                node = argument.Expression;
            }

            if (node is not InvocationExpressionSyntax originalExpression ||
                originalExpression.Expression is not MemberAccessExpressionSyntax memberAccessExpression)
            {
                return;
            }

            var diagnostic = context.Diagnostics.First();
            var constraint = diagnostic.Properties[AnalyzerPropertyKeys.SpecificConstraint]!;

            var newExpression = MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                memberAccessExpression.Expression,
                IdentifierName(constraint));

            SyntaxNode newRoot = root.ReplaceNode(originalExpression, newExpression);

            var codeAction = CodeAction.Create(
                UseSpecificConstraint,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                UseSpecificConstraint);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }
    }
}
