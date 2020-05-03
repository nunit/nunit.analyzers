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
            var constraintExpression = GetConstraintExpression(suggestedConstraintString, expected);
            return (actual, constraintExpression);
        }

        private static (ExpressionSyntax? actual, ExpressionSyntax? expected) GetActualExpected(SyntaxNode conditionNode)
        {
            if (conditionNode is BinaryExpressionSyntax binaryExpression &&
                (binaryExpression.IsKind(SyntaxKind.EqualsExpression) || binaryExpression.IsKind(SyntaxKind.NotEqualsExpression)))
            {
                return (binaryExpression.Left, binaryExpression.Right);
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
                    else if (invocation.Expression.IsKind(SyntaxKind.IdentifierName)
                        && arguments.Count == 2)
                    {
                        return (arguments[0].Expression, arguments[1].Expression);
                    }
                }
            }

            return (null, null);
        }
    }
}
