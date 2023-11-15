using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnit.Analyzers.Extensions
{
    internal static class ExpressionSyntaxExtensions
    {
        /// <summary>
        /// Returns argument expression by parameter name.
        /// </summary>
        public static ExpressionSyntax? GetArgumentExpression(
            this InvocationExpressionSyntax invocationSyntax,
            IMethodSymbol methodSymbol,
            string parameterName)
        {
            var arguments = invocationSyntax.ArgumentList.Arguments;

            // Try find named argument
            var argument = arguments.FirstOrDefault(a => a.NameColon?.Name.Identifier.Text == parameterName);

            if (argument is null)
            {
                var methodParameter = methodSymbol.Parameters.FirstOrDefault(p => p.Name == parameterName);

                if (methodParameter is null)
                    return null;

                var parameterIndex = methodSymbol.Parameters.IndexOf(methodParameter);

                argument = arguments.ElementAtOrDefault(parameterIndex);
            }

            return argument?.Expression;
        }

        public static string? GetName(this ExpressionSyntax expression)
        {
            switch (expression)
            {
                case MemberAccessExpressionSyntax memberAccess:
                    return memberAccess.Name.Identifier.Text;
                case InvocationExpressionSyntax invocation:
                    return GetName(invocation.Expression);
                case IdentifierNameSyntax identifierName:
                    return identifierName.Identifier.Text;
                default:
                    return null;
            }
        }

        public static bool IsSuppressNullableWarning(this ExpressionSyntax expression)
        {
#if NETSTANDARD1_6
            return false;
#else
            return expression.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SuppressNullableWarningExpression);
#endif
        }
    }
}
