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

            var assertExpression = invocationNode.Expression as MemberAccessExpressionSyntax;

            if (assertExpression == null)
                return;

            ExpressionSyntax? original;
            ExpressionSyntax? replacement;

            switch (assertExpression.Name.ToString())
            {
                case NunitFrameworkConstants.NameOfAssertAreSame:
                    original = assertExpression;
                    replacement = assertExpression.WithName(SyntaxFactory.IdentifierName(NunitFrameworkConstants.NameOfAssertAreEqual));
                    break;
                case NunitFrameworkConstants.NameOfAssertAreNotSame:
                    original = assertExpression;
                    replacement = assertExpression.WithName(SyntaxFactory.IdentifierName(NunitFrameworkConstants.NameOfAssertAreNotEqual));
                    break;
                case NunitFrameworkConstants.NameOfAssertThat:
                    // Find the 'SameAs' member access expression
                    var constraintArgument = invocationNode.ArgumentList.Arguments[1];
                    var isSameExpression = constraintArgument
                        .DescendantNodesAndSelf()
                        .Where(s => s.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                        .OfType<MemberAccessExpressionSyntax>()
                        .SingleOrDefault(m => m.Name.ToString() == NunitFrameworkConstants.NameOfIsSameAs);

                    if (isSameExpression == null)
                        return;

                    original = isSameExpression;
                    replacement = isSameExpression.WithName(SyntaxFactory.IdentifierName(NunitFrameworkConstants.NameOfIsEqualTo));
                    break;
                default:
                    return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            var newRoot = root.ReplaceNode(original, replacement);

            context.RegisterCodeFix(
                CodeAction.Create(
                    CodeFixConstants.UseIsEqualToDescription,
                    _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                    CodeFixConstants.UseIsEqualToDescription), diagnostic);
        }
    }
}
