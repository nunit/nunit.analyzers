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
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.SameAsOnValueTypes
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public class SameAsOnValueTypesCodeFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(AnalyzerIdentifiers.SameAsOnValueTypes);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            context.CancellationToken.ThrowIfCancellationRequested();

            var diagnostic = context.Diagnostics.First();
            var invocationNode = root.FindNode(diagnostic.Location.SourceSpan) as InvocationExpressionSyntax;

            if (invocationNode == null)
                return;

            // Find the 'SameAs' member access expression
            var constraintArgument = invocationNode.ArgumentList.Arguments[1];
            var isSameExpression = constraintArgument
                .DescendantNodesAndSelf()
                .Where(s => s.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                .OfType<MemberAccessExpressionSyntax>()
                .SingleOrDefault(m => m.Name.ToString() == NunitFrameworkConstants.NameOfIsSameAs);

            if (isSameExpression == null)
                return;

            var isEqualToExpression = isSameExpression.WithName(SyntaxFactory.IdentifierName(NunitFrameworkConstants.NameOfIsEqualTo));

            context.CancellationToken.ThrowIfCancellationRequested();

            var newRoot = root.ReplaceNode(isSameExpression, isEqualToExpression);

            context.RegisterCodeFix(
                CodeAction.Create(
                    CodeFixConstants.UseIsEqualToDescription,
                    _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                    CodeFixConstants.UseIsEqualToDescription), diagnostic);
        }
    }
}
