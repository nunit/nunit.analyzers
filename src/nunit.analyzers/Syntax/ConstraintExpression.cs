using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.Syntax
{
    /// <summary>
    /// Represents assert constraint expression, e.g. 'Is.EqualTo(expected)', 'Is.Not.Null & Is.Not.Empty'
    /// </summary>
    internal class ConstraintExpression
    {
        private readonly ExpressionSyntax expression;
        private readonly SemanticModel semanticModel;
        private ConstraintPartExpression[] constraintParts;

        public ConstraintPartExpression[] ConstraintParts
        {
            get
            {
                if (this.constraintParts == null)
                {
                    this.constraintParts = SplitConstraintByOperators(this.expression, this.semanticModel)
                        .Select(part => new ConstraintPartExpression(part, this.semanticModel))
                        .ToArray();
                }

                return this.constraintParts;
            }
        }

        public ConstraintExpression(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            this.expression = expression;
            this.semanticModel = semanticModel;
        }

        /// <summary>
        /// Split constraints into parts by binary ('&' or '|')  or constraint expression operators
        /// </summary>
        private static IEnumerable<List<ExpressionSyntax>> SplitConstraintByOperators(ExpressionSyntax constraintExpression, SemanticModel semanticModel)
        {
            return SplitConstraintByBinaryOperators(constraintExpression)
                .SelectMany(e => SplitConstraintByConstraintExpressionOperators(e, semanticModel));
        }

        /// <summary>
        /// If provided constraint expression is combined using &, | operators - return multiple split expressions.
        /// Otherwise - returns single <paramref name="constraintExpression"/> value
        /// </summary>
        private static IEnumerable<ExpressionSyntax> SplitConstraintByBinaryOperators(ExpressionSyntax constraintExpression)
        {
            if (constraintExpression is BinaryExpressionSyntax binaryExpression)
            {
                foreach (var leftPart in SplitConstraintByBinaryOperators(binaryExpression.Left))
                    yield return leftPart;

                foreach (var rightPart in SplitConstraintByBinaryOperators(binaryExpression.Right))
                    yield return rightPart;
            }
            else
            {
                yield return constraintExpression;
            }
        }

        /// <summary>
        /// If constraint expression is combined using And, Or, With properties - 
        /// returns parts of expression split by those properties.
        /// </summary>
        private static IEnumerable<List<ExpressionSyntax>>
            SplitConstraintByConstraintExpressionOperators(ExpressionSyntax constraintExpression, SemanticModel semanticModel)
        {
            // e.g. Does.Contain(a).IgnoreCase.And.Contain(b).And.Not.Null
            // --> 
            // Does.Contain(a).IgnoreCase,
            // Does.Contain(a).IgnoreCase.And.Contain(b)
            // Does.Contain(a).IgnoreCase.And.Contain(b).And.Not.Null

            var callChainParts = constraintExpression.SplitCallChain();

            var currentPartExpressions = new List<ExpressionSyntax>();

            foreach (var callChainPart in callChainParts)
            {
                if (IsConstraintExpressionOperator(callChainPart, semanticModel))
                {
                    yield return currentPartExpressions;
                    currentPartExpressions = new List<ExpressionSyntax>();
                }
                else
                {
                    currentPartExpressions.Add(callChainPart);
                }
            }

            yield return currentPartExpressions;
        }

        /// <summary>
        /// Returns true if current expression is And/Or/With constraint operator
        /// </summary>
        private static bool IsConstraintExpressionOperator(ExpressionSyntax expressionSyntax, SemanticModel semanticModel)
        {
            if (expressionSyntax is MemberAccessExpressionSyntax memberAccessExpression)
            {
                var name = memberAccessExpression.Name.Identifier.Text;

                if (name == NunitFrameworkConstants.NameOfConstraintExpressionAnd
                    || name == NunitFrameworkConstants.NameOfConstraintExpressionOr)
                {
                    return true;
                }
                else if (name == NunitFrameworkConstants.NameOfConstraintExpressionWith)
                {
                    // 'With' is allowed only when defined on Constraint (in this case it is 'And' equivalent).
                    // When defined in ConstraintExpression - it's NOP, and is not expression operator.
                    var symbol = semanticModel.GetSymbolInfo(memberAccessExpression).Symbol;

                    if (symbol is IPropertySymbol propertySymbol
                        && propertySymbol.ContainingType.Name == NunitFrameworkConstants.NameOfConstraint)
                    {
                        return true;
                    }

                }
            }

            return false;
        }
    }
}
