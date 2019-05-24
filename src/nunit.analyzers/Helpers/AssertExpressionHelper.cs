using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.Helpers
{
    internal static class AssertExpressionHelper
    {
        /// <summary>
        /// Get provided 'actual' and 'expression' arguments to Assert.That method
        /// </summary>
        /// <returns>
        /// True, if arguments found. Otherwise - false.
        /// </returns>
        public static bool TryGetActualAndConstraintExpressions(InvocationExpressionSyntax assertExpression,
            out ExpressionSyntax actualExpression, out ExpressionSyntax constraintExpression)
        {
            if (assertExpression.Expression is MemberAccessExpressionSyntax memberAccessSyntax
                && memberAccessSyntax.Name.Identifier.Text == NunitFrameworkConstants.NameOfAssertThat
                && assertExpression.ArgumentList.Arguments.Count >= 2)
            {
                actualExpression = assertExpression.ArgumentList.Arguments[0].Expression;
                constraintExpression = assertExpression.ArgumentList.Arguments[1].Expression;

                return true;
            }
            else
            {
                actualExpression = null;
                constraintExpression = null;

                return false;
            }
        }

        /// <summary>
        /// Returns 'expected' assertion arguments expressions, along with corresponding constraint method symbols.
        /// Returns multiple pairs if multiple constraints are combined.
        /// </summary>
        public static List<(ExpressionSyntax expectedArgument, IMethodSymbol constraintMethod)> GetExpectedArguments(
            ExpressionSyntax constraintExpression, SemanticModel semanticModel)
        {
            var expectedArguments = new List<(ExpressionSyntax, IMethodSymbol)>();

            var constraintParts = SplitConstraintByOperators(constraintExpression);

            foreach (var constraintPart in constraintParts)
            {
                var invocations = constraintPart.SplitCallChain()
                    .OfType<InvocationExpressionSyntax>()
                    .Where(i => i.ArgumentList.Arguments.Count == 1);

                foreach (var invocation in invocations)
                {
                    var symbol = semanticModel.GetSymbolInfo(invocation).Symbol;

                    if (symbol is IMethodSymbol methodSymbol
                        && methodSymbol.Parameters.Length == 1
                        && methodSymbol.Parameters[0].Name == NunitFrameworkConstants.NameOfExpectedParameter)
                    {
                        var argument = invocation.ArgumentList.Arguments[0];
                        expectedArguments.Add((argument.Expression, methodSymbol));
                    }
                }
            }

            return expectedArguments;
        }

        /// <summary>
        /// If provided constraint expression is combined using &, | operators - return multiple split expressions.
        /// Otherwise - returns single <paramref name="constraintExpression"/> value
        /// </summary>
        public static IEnumerable<ExpressionSyntax> SplitConstraintByOperators(ExpressionSyntax constraintExpression)
        {
            if (constraintExpression is BinaryExpressionSyntax binaryExpression)
            {
                foreach (var leftPart in SplitConstraintByOperators(binaryExpression.Left))
                    yield return leftPart;

                foreach (var rightPart in SplitConstraintByOperators(binaryExpression.Right))
                    yield return rightPart;
            }
            else
            {
                yield return constraintExpression;
            }
        }
    }
}
