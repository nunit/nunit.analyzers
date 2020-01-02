using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Syntax;

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
        public static bool TryGetActualAndConstraintExpressions(
            InvocationExpressionSyntax assertExpression,
            SemanticModel semanticModel,
            out ExpressionSyntax actualExpression,
            out ConstraintExpression constraintExpression)
        {
            if (assertExpression.Expression is MemberAccessExpressionSyntax memberAccessSyntax
                && memberAccessSyntax.Name.Identifier.Text == NunitFrameworkConstants.NameOfAssertThat
                && assertExpression.ArgumentList.Arguments.Count >= 2)
            {
                actualExpression = assertExpression.ArgumentList.Arguments[0].Expression;
                constraintExpression = new ConstraintExpression(assertExpression.ArgumentList.Arguments[1].Expression, semanticModel);

                return true;
            }
            else
            {
                actualExpression = null;
                constraintExpression = null;

                return false;
            }
        }
    }
}
