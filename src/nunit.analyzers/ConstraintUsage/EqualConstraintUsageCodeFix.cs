using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.ConstraintUsage
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public class EqualConstraintUsageCodeFix : BaseConditionConstraintCodeFix
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(AnalyzerIdentifiers.EqualConstraintUsage);

        protected override (ExpressionSyntax? actual, ExpressionSyntax? constraintExpression) GetActualAndConstraintExpression(ExpressionSyntax conditionNode, string suggestedConstraintString)
        {
            var (actual, expected) = GetActualExpected(conditionNode);

            InvocationExpressionSyntax? constraintExpression;

            if (expected is ExpressionSyntax expression)
            {
                constraintExpression = GetConstraintExpression(suggestedConstraintString, expression);
            }
            else if (expected is PatternSyntax pattern)
            {
                constraintExpression = this.ConvertPattern(
                    SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIs),
                    pattern);
            }
            else
            {
                constraintExpression = null;
            }

            return (actual, constraintExpression);
        }

        private static (ExpressionSyntax? actual, ExpressionOrPatternSyntax? expected) GetActualExpected(SyntaxNode conditionNode)
        {
            if (conditionNode is BinaryExpressionSyntax binaryExpression &&
                (binaryExpression.IsKind(SyntaxKind.EqualsExpression) || binaryExpression.IsKind(SyntaxKind.NotEqualsExpression)))
            {
                return (binaryExpression.Left, binaryExpression.Right);
            }
            else if (conditionNode is IsPatternExpressionSyntax isPatternExpression)
            {
                return (isPatternExpression.Expression, isPatternExpression.Pattern);
            }
            else
            {
                if (conditionNode is PrefixUnaryExpressionSyntax prefixUnary
                    && prefixUnary.IsKind(SyntaxKind.LogicalNotExpression))
                {
                    conditionNode = prefixUnary.Operand;
                }

                if (conditionNode is InvocationExpressionSyntax invocation)
                {
                    var arguments = invocation.ArgumentList.Arguments;

                    // actual.Equals(expected)
                    if (invocation.Expression is MemberAccessExpressionSyntax memberAccess
                        && arguments.Count == 1)
                    {
                        return (memberAccess.Expression, arguments[0].Expression);
                    }

                    // Equals(actual, expected)
                    if (invocation.Expression.IsKind(SyntaxKind.IdentifierName)
                        && arguments.Count == 2)
                    {
                        return (arguments[0].Expression, arguments[1].Expression);
                    }
                }
            }

            return (null, null);
        }

        /// <summary>
        /// Converts an 'is' pattern to a corresponding nunit EqualTo invocation.
        /// </summary>
        /// <remarks>
        /// We support:
        ///   constant-pattern,
        ///   relational-pattern: &lt;, &lt;=, &gt;, &gt;=.
        ///   not supported-pattern,
        ///   supported-pattern or supported-pattern,
        ///   supported-pattern and supported-pattern.
        /// </remarks>
        private InvocationExpressionSyntax? ConvertPattern(ExpressionSyntax member, PatternSyntax pattern)
        {
            if (pattern is ConstantPatternSyntax constantPattern)
            {
                return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        member,
                        SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIsEqualTo)),
                    SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(constantPattern.Expression))));
            }
            else if (pattern is RelationalPatternSyntax relationalPattern)
            {
                string? identifier = relationalPattern.OperatorToken.Kind() switch
                {
                    SyntaxKind.LessThanToken => NUnitFrameworkConstants.NameOfIsLessThan,
                    SyntaxKind.LessThanEqualsToken => NUnitFrameworkConstants.NameOfIsLessThanOrEqualTo,
                    SyntaxKind.GreaterThanToken => NUnitFrameworkConstants.NameOfIsGreaterThan,
                    SyntaxKind.GreaterThanEqualsToken => NUnitFrameworkConstants.NameOfIsGreaterThanOrEqualTo,
                    _ => null,
                };

                if (identifier is not null)
                {
                    return SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            member,
                            SyntaxFactory.IdentifierName(identifier)),
                        SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(relationalPattern.Expression))));
                }
            }
            else if (pattern is UnaryPatternSyntax unaryPattern && unaryPattern.IsKind(SyntaxKind.NotPattern))
            {
                return this.ConvertPattern(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        member,
                        SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIsNot)),
                    unaryPattern.Pattern);
            }
            else if (pattern is BinaryPatternSyntax binaryPattern)
            {
                string? constraint = binaryPattern.Kind() switch
                {
                    SyntaxKind.OrPattern => NUnitFrameworkConstants.NameOfConstraintExpressionOr,
                    SyntaxKind.AndPattern => NUnitFrameworkConstants.NameOfConstraintExpressionAnd,
                    _ => null,
                };

                if (constraint is not null)
                {
                    InvocationExpressionSyntax? leftExpression = this.ConvertPattern(member, binaryPattern.Left);

                    if (leftExpression is not null)
                    {
                        return this.ConvertPattern(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                leftExpression,
                                SyntaxFactory.IdentifierName(constraint)),
                            binaryPattern.Right);
                    }
                }
            }

            return null;
        }
    }
}
