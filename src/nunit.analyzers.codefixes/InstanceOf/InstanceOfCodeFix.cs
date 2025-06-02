using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.InstanceOf
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public class InstanceOfCodeFix : CodeFixProvider
    {
        internal const string UseInstanceOfConstraint = "Use Is.InstanceOf constraint";

        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(AnalyzerIdentifiers.InstanceOf);

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

            if (node.Parent is not ArgumentListSyntax argumentListSyntax
                || argumentListSyntax.Arguments.Count < 1
                || argumentListSyntax.Arguments[0].Expression is not BinaryExpressionSyntax binaryExpression
                || binaryExpression.Kind() != SyntaxKind.IsExpression
                || binaryExpression.Right is not TypeSyntax rightHandType)
            {
                return;
            }

            var properties = context.Diagnostics[0].Properties;
            bool isConstraintIsTrue = properties[InstanceOfAnalyzer.IsConstraintIsTrue] == true.ToString();
            ExpressionSyntax expression = isConstraintIsTrue
                ? SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIs)
                : SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIs),
                                SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIsNot));

            var newArguments = new[]
            {
                SyntaxFactory.Argument(binaryExpression.Left),
                SyntaxFactory.Argument(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            expression,
                            SyntaxFactory
                                .GenericName(NUnitFrameworkConstants.NameOfIsInstanceOf)
                                .WithTypeArgumentList(
                                    SyntaxFactory.TypeArgumentList(
                                        SyntaxFactory.SingletonSeparatedList(rightHandType))))))
            };

            var newArgumentListSyntax = argumentListSyntax.WithArguments(newArguments);

            var newRoot = root.ReplaceNode(argumentListSyntax, newArgumentListSyntax);

            var codeAction = CodeAction.Create(
               UseInstanceOfConstraint,
               _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
               UseInstanceOfConstraint);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }
    }
}
