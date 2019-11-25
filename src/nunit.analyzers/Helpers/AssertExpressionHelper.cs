using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
            ExpressionSyntax constraintExpression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            var expectedArguments = new List<(ExpressionSyntax, IMethodSymbol)>();

            var constraintParts = SplitConstraintByOperators(constraintExpression);

            foreach (var constraintPart in constraintParts)
            {
                var expected = GetExpectedArgumentFromConstraintPart(constraintPart, semanticModel, cancellationToken);

                if (expected != null)
                {
                    expectedArguments.Add(expected.Value);
                }
            }

            return expectedArguments;
        }

        /// <summary>
        /// Split constraints into parts by binary ('&' or '|')  or constraint expression operators
        /// </summary>
        public static IEnumerable<ExpressionSyntax> SplitConstraintByOperators(ExpressionSyntax constraintExpression)
        {
            return SplitConstraintByBinaryOperators(constraintExpression)
                .SelectMany(SplitConstraintByConstraintExpressionOperators);
        }

        public static IEnumerable<ExpressionSyntax> GetConstraintExpressionPrefixes(
            ExpressionSyntax constraintExpression, SemanticModel semanticModel)
        {
            // e.g. Has.Property("Prop").Not.EqualTo("1")
            // -->
            // Has.Property("Prop"),
            // Has.Property("Prop").Not

            // Take expressions until found expression returning constraint
            return GetExpressionsFromCurrentPart(constraintExpression)
                .Where(e => e is MemberAccessExpressionSyntax || e is InvocationExpressionSyntax)
                .TakeWhile(e => !ReturnsConstraint(e, semanticModel));
        }

        public static IEnumerable<ExpressionSyntax> GetConstraintExpressionSuffixes(
            ExpressionSyntax constraintExpression, SemanticModel semanticModel)
        {
            // e.g. Has.Property("Prop").Not.EqualTo("1").IgnoreCase
            // -->
            // Has.Property("Prop").Not.EqualTo("1").IgnoreCase

            // Skip all suffixes, and first expression returning constraint (e.g. 'EqualTo("1")')
            return GetExpressionsFromCurrentPart(constraintExpression)
                .SkipWhile(e => !ReturnsConstraint(e, semanticModel))
                .Skip(1);
        }

        /// <summary>
        /// If provided constraint expression is combined using &, | operators - return multiple split expressions.
        /// Otherwise - returns single <paramref name="constraintExpression"/> value
        /// </summary>
        private static IEnumerable<ExpressionSyntax> SplitConstraintByBinaryOperators(ExpressionSyntax constraintExpression)
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

        /// <summary>
        /// If constraint expression is combined using And, Or, With properties - 
        /// returns parts of expression split by those properties.
        /// /// </summary>
        private static IEnumerable<ExpressionSyntax> SplitConstraintByConstraintExpressionOperators(ExpressionSyntax constraintExpression)
        {
            // e.g. Does.Contain(a).IgnoreCase.And.Contain(b).And.Not.Null
            // --> 
            // Does.Contain(a).IgnoreCase,
            // Does.Contain(a).IgnoreCase.And.Contain(b)
            // Does.Contain(a).IgnoreCase.And.Contain(b).And.Not.Null

            // (You cannot separate only Not.Null part in any way)

            var callChainParts = constraintExpression.SplitCallChain();

            for (var i = 1; i < callChainParts.Count; i++)
            {
                if (IsConstraintExpressionOperator(callChainParts[i]))
                {
                    yield return callChainParts[i - 1];
                }
            }

            yield return constraintExpression;
        }

        private static (ExpressionSyntax expectedArgument, IMethodSymbol constraintMethod)? GetExpectedArgumentFromConstraintPart(
            ExpressionSyntax constraintPart,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            // Each constraint part might have only one method, accepting 'expected' argument.

            foreach (var expression in GetExpressionsFromCurrentPart(constraintPart).Reverse())
            {
                if (expression is InvocationExpressionSyntax invocation && invocation.ArgumentList.Arguments.Count == 1)
                {
                    var symbol = semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol;

                    if (symbol is IMethodSymbol methodSymbol
                        && methodSymbol.Parameters.Length == 1
                        && methodSymbol.Parameters[0].Name == NunitFrameworkConstants.NameOfExpectedParameter)
                    {
                        var argument = invocation.ArgumentList.Arguments[0];

                        return (argument.Expression, methodSymbol);
                    }
                }
            }

            return null;
        }

        private static IEnumerable<ExpressionSyntax> GetExpressionsFromCurrentPart(ExpressionSyntax constraintPart)
        {
            return constraintPart.SplitCallChain().AsEnumerable()
                .Reverse()
                .TakeWhile(e => !(IsConstraintExpressionOperator(e)))
                .Reverse();
        }

        /// <summary>
        /// Returns true if current expression is And/Or/With constraint operator
        /// </summary>
        private static bool IsConstraintExpressionOperator(ExpressionSyntax expressionSyntax)
        {
            if (expressionSyntax is MemberAccessExpressionSyntax memberAccessExpression)
            {
                var name = memberAccessExpression.Name.Identifier.Text;

                if (name == NunitFrameworkConstants.NameOfConstraintExpressionAnd
                    || name == NunitFrameworkConstants.NameOfConstraintExpressionOr
                    || name == NunitFrameworkConstants.NameOfConstraintExpressionWith)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ReturnsConstraint(ExpressionSyntax expressionSyntax, SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetSymbolInfo(expressionSyntax).Symbol;

            ITypeSymbol returnType = null;

            if (symbol is IMethodSymbol methodSymbol)
            {
                returnType = methodSymbol.ReturnType;
            }
            else if (symbol is IPropertySymbol propertySymbol)
            {
                returnType = propertySymbol.Type;
            }

            return returnType != null && returnType.IsConstraint();
        }
    }
}
