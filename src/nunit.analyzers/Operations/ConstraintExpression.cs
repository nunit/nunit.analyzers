using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.Operations
{
    /// <summary>
    /// Represents assert constraint expression, e.g. 'Is.EqualTo(expected)', 'Is.Not.Null & Is.Not.Empty'.
    /// </summary>
    internal class ConstraintExpression
    {
        private readonly IOperation expressionOperation;
        private ConstraintExpressionPart[]? constraintParts;

        public ConstraintExpression(IOperation expressionOperation)
        {
            this.expressionOperation = expressionOperation;
        }

        public ConstraintExpressionPart[] ConstraintParts
        {
            get
            {
                if (this.constraintParts == null)
                {
                    this.constraintParts = SplitConstraintByOperators(this.expressionOperation)
                        .Select(part => new ConstraintExpressionPart(part))
                        .ToArray();
                }

                return this.constraintParts;
            }
        }

        /// <summary>
        /// Split constraints into parts by binary ('&' or '|')  or constraint expression operators.
        /// Returns oparations split by call chains.
        /// </summary>
        private static IEnumerable<List<IOperation>> SplitConstraintByOperators(IOperation constraintExpression)
        {
            return SplitConstraintByBinaryOperators(constraintExpression)
                .SelectMany(e => SplitConstraintByConstraintExpressionOperators(e));
        }

        /// <summary>
        /// If provided constraint expression is combined using &, | operators - return multiple split expressions.
        /// Otherwise - returns single <paramref name="constraintExpression"/> value.
        /// </summary>
        private static IEnumerable<IOperation> SplitConstraintByBinaryOperators(IOperation constraintExpression)
        {
            if (constraintExpression is IConversionOperation conversion)
                constraintExpression = conversion.Operand;

            if (constraintExpression is IBinaryOperation binaryOperation)
            {
                foreach (var leftPart in SplitConstraintByBinaryOperators(binaryOperation.LeftOperand))
                    yield return leftPart;

                foreach (var rightPart in SplitConstraintByBinaryOperators(binaryOperation.RightOperand))
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
        /// For each part returns operations split by call chains.
        /// </summary>
        private static IEnumerable<List<IOperation>>
            SplitConstraintByConstraintExpressionOperators(IOperation constraintExpression)
        {
            // e.g. Does.Contain(a).IgnoreCase.And.Contain(b).And.Not.Null
            // -->
            // Does.Contain(a).IgnoreCase,
            // Does.Contain(a).IgnoreCase.And.Contain(b)
            // Does.Contain(a).IgnoreCase.And.Contain(b).And.Not.Null

            var callChainParts = constraintExpression.SplitCallChain();

            var currentPartOperations = new List<IOperation>();

            foreach (var callChainPart in callChainParts)
            {
                if (IsConstraintExpressionOperator(callChainPart))
                {
                    yield return currentPartOperations;
                    currentPartOperations = new List<IOperation>();
                }
                else
                {
                    currentPartOperations.Add(callChainPart);
                }
            }

            yield return currentPartOperations;
        }

        /// <summary>
        /// Returns true if current expression is And/Or/With constraint operator.
        /// </summary>
        private static bool IsConstraintExpressionOperator(IOperation operation)
        {
            if (operation is IPropertyReferenceOperation propertyReference)
            {
                var name = propertyReference.Property.Name;

                if (name == NunitFrameworkConstants.NameOfConstraintExpressionAnd
                    || name == NunitFrameworkConstants.NameOfConstraintExpressionOr)
                {
                    return true;
                }
                else if (name == NunitFrameworkConstants.NameOfConstraintExpressionWith)
                {
                    // 'With' is allowed only when defined on Constraint (in this case it is 'And' equivalent).
                    // When defined in ConstraintExpression - it's NOP, and is not expression operator.

                    if (propertyReference.Property.ContainingType.Name == NunitFrameworkConstants.NameOfConstraint)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
