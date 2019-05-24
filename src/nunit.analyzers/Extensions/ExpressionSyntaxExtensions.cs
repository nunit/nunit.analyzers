using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnit.Analyzers.Extensions
{
    internal static class ExpressionSyntaxExtensions
    {
        public static List<ExpressionSyntax> SplitCallChain(this ExpressionSyntax expression)
        {
            // e.g. 'Is.EqualTo(str).IgnoreCase'
            // returns 'Is', 'Is.EqualTo(str)', 'Is.EqualTo(str).IgnoreCase'

            var parts = new List<ExpressionSyntax>();
            var currentNode = expression;

            while (currentNode != null)
            {
                if (currentNode is InvocationExpressionSyntax invocation)
                {
                    parts.Add(invocation);
                    currentNode = invocation.Expression;
                }
                else if (currentNode is MemberAccessExpressionSyntax memberAccess)
                {
                    currentNode = memberAccess.Expression;

                    // We don't need 'Is.EqualTo' and 'Is.EqualTo(str)' separately, 
                    // therefore add memberAccess only if parent is member access as well
                    if (memberAccess.Parent is MemberAccessExpressionSyntax)
                        parts.Add(memberAccess);
                }
                else
                {
                    currentNode = null;
                }
            }

            parts.Reverse();

            return parts;
        }

        /// <summary>
        /// Returns argument expression by parameter name.
        /// </summary>
        public static ExpressionSyntax GetArgumentExpression(this InvocationExpressionSyntax invocationSyntax,
            IMethodSymbol methodSymbol, string parameterName)
        {
            var arguments = invocationSyntax.ArgumentList.Arguments;

            // Try find named argument
            var argument = arguments.FirstOrDefault(a => a.NameColon?.Name.Identifier.Text == parameterName);

            if (argument == null)
            {
                var methodParameter = methodSymbol.Parameters.FirstOrDefault(p => p.Name == parameterName);

                if (methodParameter == null)
                    return null;

                var parameterIndex = methodSymbol.Parameters.IndexOf(methodParameter);

                argument = arguments.ElementAtOrDefault(parameterIndex);
            }

            return argument?.Expression;
        }
    }
}
