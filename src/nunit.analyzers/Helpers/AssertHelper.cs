using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Syntax;

namespace NUnit.Analyzers.Helpers
{
    internal static class AssertHelper
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
            [NotNullWhen(true)] out ExpressionSyntax? actualExpression,
            [NotNullWhen(true)] out ConstraintExpression? constraintExpression)
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

        // Unwrap underlying type from delegate or awaitable.
        public static ITypeSymbol UnwrapActualType(ITypeSymbol actualType)
        {
            if (actualType is INamedTypeSymbol namedType && namedType.DelegateInvokeMethod != null)
                actualType = namedType.DelegateInvokeMethod.ReturnType;

            if (actualType.IsAwaitable(out var awaitReturnType) && awaitReturnType.SpecialType != SpecialType.System_Void)
                actualType = awaitReturnType;

            return actualType;
        }

        /// <summary>
        /// Get TypeSymbol from <paramref name="expressionSyntax"/>, and unwrap from delegate or awaitable.
        /// </summary>
        public static ITypeSymbol? GetUnwrappedActualType(ExpressionSyntax actualExpression, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var actualTypeInfo = semanticModel.GetTypeInfo(actualExpression, cancellationToken);
            var actualType = actualTypeInfo.Type ?? actualTypeInfo.ConvertedType;

            if (actualType == null || actualType.Kind == SymbolKind.ErrorType)
                return null;

            return UnwrapActualType(actualType);
        }
    }
}
