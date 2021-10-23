using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.UseCollectionConstraint
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public class UseCollectionConstraintCodeFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(AnalyzerIdentifiers.UsePropertyConstraint);

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

            if (!(node is ArgumentSyntax actualArgument))
                return;

            if (!(actualArgument.Expression is MemberAccessExpressionSyntax actualExpression))
                return;

            if (!(actualArgument.Parent is ArgumentListSyntax argumentList))
                return;

            if (!(argumentList.Arguments.Count > 1 && argumentList.Arguments[1] is ArgumentSyntax constraintArgument))
                return;

            var constraintMemberExpression = constraintArgument.Expression as MemberAccessExpressionSyntax;
            var constraintExpression = constraintArgument.Expression as InvocationExpressionSyntax;
            if (!(constraintExpression is null))
            {
                constraintMemberExpression = constraintExpression.Expression as MemberAccessExpressionSyntax;
            }

            if (constraintMemberExpression is null)
                return;

            // Find the left most term.
            ExpressionSyntax innerConstraintExpression = constraintMemberExpression.Expression;
            while (!(innerConstraintExpression is SimpleNameSyntax))
            {
                if (innerConstraintExpression is InvocationExpressionSyntax invocationExpression)
                {
                    innerConstraintExpression = invocationExpression.Expression;
                }
                else if (innerConstraintExpression is MemberAccessExpressionSyntax memberAccessExpression)
                {
                    innerConstraintExpression = memberAccessExpression.Expression;
                }
                else
                {
                    break;
                }
            }

            if (!(innerConstraintExpression is SimpleNameSyntax simple && simple.Identifier.ValueText == NunitFrameworkConstants.NameOfIs))
                return;

            var propertyName = actualExpression.Name;
            string description;

            var nodesToReplace = new Dictionary<SyntaxNode, SyntaxNode>()
            {
                // Replace <expression>.<property> with <expression>
                { actualArgument.Expression, actualExpression.Expression }
            };

            // Special case, check for '.EqualTo(0)'
            if (constraintMemberExpression.Name.Identifier.ValueText == NunitFrameworkConstants.NameOfIsZero ||
                IsInvocationTo(constraintExpression, NunitFrameworkConstants.NameOfIsEqualTo, "0") ||
                IsInvocationTo(constraintExpression, NunitFrameworkConstants.NameOfIsLessThan, "1"))
            {
                // Replace Is.EqualTo(0) with Is.Empty
                // Replace Is.Not.EqualTo(0) with Is.Not.Empty
                var isEmptyExpression = SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        constraintMemberExpression.Expression,
                        SyntaxFactory.IdentifierName(NunitFrameworkConstants.NameOfIsEmpty));

                nodesToReplace.Add(constraintArgument.Expression, isEmptyExpression);

                description = $"Use {isEmptyExpression}";
            }
            else if (constraintMemberExpression.Name.Identifier.ValueText == NunitFrameworkConstants.NameOfIsPositive ||
                IsInvocationTo(constraintExpression, NunitFrameworkConstants.NameOfIsGreaterThan, "0") ||
                IsInvocationTo(constraintExpression, NunitFrameworkConstants.NameOfIsGreaterThanOrEqualTo, "1"))
            {
                // Replace Is.GreatherThan(0)/Is.GreaterThanOrEqualTo(1)/Is.LessThan(1) with Is.Not.Empty
                var isNotEmptyExpression = SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(NunitFrameworkConstants.NameOfIs),
                            SyntaxFactory.IdentifierName(NunitFrameworkConstants.NameOfIsNot)),
                        SyntaxFactory.IdentifierName(NunitFrameworkConstants.NameOfIsEmpty));

                nodesToReplace.Add(constraintArgument.Expression, isNotEmptyExpression);

                description = $"Use {isNotEmptyExpression}";
            }
            else
            {
                // Replace Is. with Has.<property>.
                var hasPropertyExpression = SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(NunitFrameworkConstants.NameOfHas),
                        propertyName);

                nodesToReplace.Add(innerConstraintExpression, hasPropertyExpression);

                description = $"Use {NunitFrameworkConstants.NameOfHas}.{propertyName}";
            }

            var newRoot = root.ReplaceNodes(nodesToReplace.Keys, (node, _) => nodesToReplace[node]);

            var codeAction = CodeAction.Create(
                description,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                description);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        private static bool IsInvocationTo(InvocationExpressionSyntax? invocationExpression, string name, string value)
        {
            if (invocationExpression is null)
                return false;

            if (!(invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression))
                return false;

            var arguments = invocationExpression.ArgumentList.Arguments;

            return memberAccessExpression.Name.Identifier.ValueText == name &&
                   arguments.Count == 1 &&
                   arguments[0].Expression is LiteralExpressionSyntax literalExpression &&
                   literalExpression.Token.ValueText == value;
        }
    }
}
