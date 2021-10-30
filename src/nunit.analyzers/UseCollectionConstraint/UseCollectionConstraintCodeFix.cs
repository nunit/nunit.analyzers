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

            if (!(node is ArgumentSyntax actualArgument) ||
                !(actualArgument.Expression is MemberAccessExpressionSyntax actualExpression) ||
                !(actualArgument.Parent is ArgumentListSyntax argumentList) ||
                !(argumentList.Arguments.Count > 1 && argumentList.Arguments[1] is ArgumentSyntax constraintArgument))
            {
                return;
            }

            // We have either a MemberAccessExpression (Is.Zero) or an InvocationExpression (Is.EqualTo(0))
            var constraintMemberExpression = constraintArgument.Expression as MemberAccessExpressionSyntax;
            var constraintExpression = constraintArgument.Expression as InvocationExpressionSyntax;
            if (!(constraintExpression is null))
            {
                constraintMemberExpression = constraintExpression.Expression as MemberAccessExpressionSyntax;
            }

            if (constraintMemberExpression is null)
                return;

            ExpressionSyntax innerConstraintExpression = FindLeftMostTerm(constraintMemberExpression);

            if (!(innerConstraintExpression is SimpleNameSyntax simple && simple.Identifier.ValueText == NUnitFrameworkConstants.NameOfIs))
                return;

            var nodesToReplace = new Dictionary<SyntaxNode, SyntaxNode>()
            {
                // Replace <expression>.<property> with <expression> in first argument
                { actualArgument.Expression, actualExpression.Expression }
            };

            string description;

            MemberAccessExpressionSyntax? emptyOrNotEmptyExpression = MatchWithEmpty(constraintMemberExpression,
                constraintExpression, innerConstraintExpression);
            if (!(emptyOrNotEmptyExpression is null))
            {
                nodesToReplace.Add(constraintArgument.Expression, emptyOrNotEmptyExpression);

                description = $"Use {emptyOrNotEmptyExpression}";
            }
            else
            {
                // Replace Is. with Has.<property>.
                var propertyName = actualExpression.Name;
                var hasPropertyExpression = SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfHas),
                        propertyName);

                nodesToReplace.Add(innerConstraintExpression, hasPropertyExpression);

                description = $"Use {NUnitFrameworkConstants.NameOfHas}.{propertyName}";
            }

            var newRoot = root.ReplaceNodes(nodesToReplace.Keys, (node, _) => nodesToReplace[node]);

            var codeAction = CodeAction.Create(
                description,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                description);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        private static MemberAccessExpressionSyntax? MatchWithEmpty(
            MemberAccessExpressionSyntax constraintMemberExpression,
            InvocationExpressionSyntax? constraintExpression,
            ExpressionSyntax innerConstraintExpression)
        {
            MemberAccessExpressionSyntax? emptyOrNotEmptyExpression = null;

            if (constraintMemberExpression.Name.Identifier.ValueText == NUnitFrameworkConstants.NameOfIsZero ||
                IsInvocationTo(constraintExpression, NUnitFrameworkConstants.NameOfIsEqualTo, "0") ||
                IsInvocationTo(constraintExpression, NUnitFrameworkConstants.NameOfIsLessThan, "1"))
            {
                // Replace: '.Zero', '.EqualTo(0)' and '.LessThan(1)' with .Empty
                emptyOrNotEmptyExpression = SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        constraintMemberExpression.Expression,
                        SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIsEmpty));
            }
            else if (constraintMemberExpression.Name.Identifier.ValueText == NUnitFrameworkConstants.NameOfIsPositive ||
                IsInvocationTo(constraintExpression, NUnitFrameworkConstants.NameOfIsGreaterThan, "0") ||
                IsInvocationTo(constraintExpression, NUnitFrameworkConstants.NameOfIsGreaterThanOrEqualTo, "1"))
            {
                // Replace:'Positive', '.GreatherThan(0)', '.GreaterThanOrEqualTo(1)' with .Not.Empty
                // Take care of double negatives: '.Not.Positive' becomes 'Empty'.
                emptyOrNotEmptyExpression = SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IsNot(constraintMemberExpression) ?
                            innerConstraintExpression :
                            SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    constraintMemberExpression.Expression,
                                    SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIsNot)),
                        SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIsEmpty));
            }

            return emptyOrNotEmptyExpression;
        }

        private static bool IsNot(MemberAccessExpressionSyntax constraintMemberExpression)
        {
            // Detect 'Is.Not' and 'Is.Not.<something>'
            return IsNot(constraintMemberExpression.Name) ||
                (constraintMemberExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression &&
                    IsNot(memberAccessExpression.Name));

            static bool IsNot(SimpleNameSyntax simpleName) =>
                simpleName.Identifier.ValueText == NUnitFrameworkConstants.NameOfIsNot;
        }

        private static ExpressionSyntax FindLeftMostTerm(MemberAccessExpressionSyntax constraintMemberExpression)
        {
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

            return innerConstraintExpression;
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
