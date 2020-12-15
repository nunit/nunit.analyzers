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
    public class ComparisonConstraintUsageCodeFix : BaseConditionConstraintCodeFix
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(AnalyzerIdentifiers.ComparisonConstraintUsage);

        protected override (ExpressionSyntax? actual, ExpressionSyntax? constraintExpression) GetActualAndConstraintExpression(ExpressionSyntax conditionNode, string suggestedConstraintString, bool swapOperands)
        {
            var (actual, expected) = GetActualExpected(conditionNode);
            var constraintExpression = GetConstraintExpression(suggestedConstraintString, swapOperands ? actual : expected);
            return (swapOperands ? expected : actual, constraintExpression);
        }

        private static (ExpressionSyntax? actual, ExpressionSyntax? expected) GetActualExpected(SyntaxNode conditionNode)
        {
            if (conditionNode is BinaryExpressionSyntax binaryExpression)
            {
                return (binaryExpression.Left, binaryExpression.Right);
            }

            return (null, null);
        }
    }
}
