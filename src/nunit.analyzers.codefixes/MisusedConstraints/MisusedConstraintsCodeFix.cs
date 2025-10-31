using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NUnit.Analyzers.MisusedConstraints
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public class MisusedConstraintsCodeFix : CodeFixProvider
    {
        internal const string FixMisusedConstraint = "Fix the constraint";

        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(AnalyzerIdentifiers.MisusedConstraints);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            if (root is null || semanticModel is null)
                return;

            context.CancellationToken.ThrowIfCancellationRequested();

            var argument = root.FindNode(context.Span) as ArgumentSyntax;
            if (argument is null)
                return;

            ExpressionSyntax expression = IdentifierName(NUnitFrameworkConstants.NameOfIs);

            expression = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                expression,
                IdentifierName(NUnitFrameworkConstants.NameOfIsNot));
            expression = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                expression,
                IdentifierName(NUnitFrameworkConstants.NameOfIsNull));
            expression = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                expression,
                IdentifierName(NUnitFrameworkConstants.NameOfConstraintExpressionAnd));
            expression = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                expression,
                IdentifierName(NUnitFrameworkConstants.NameOfIsNot));
            expression = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                expression,
                IdentifierName(NUnitFrameworkConstants.NameOfIsEmpty));

            var newRoot = root
                .ReplaceNode(argument.Expression, expression);

            var codeAction = CodeAction.Create(
                FixMisusedConstraint,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                FixMisusedConstraint);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }
    }
}
