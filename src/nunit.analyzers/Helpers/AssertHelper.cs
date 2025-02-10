using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Operations;

namespace NUnit.Analyzers.Helpers
{
    internal static class AssertHelper
    {
        /// <summary>
        /// Get provided 'actual' and 'expression' arguments to Assert.That method.
        /// </summary>
        /// <returns>
        /// True, if arguments found. Otherwise - false.
        /// </returns>
        public static bool TryGetActualAndConstraintOperations(
            IInvocationOperation assertOperation,
            [NotNullWhen(true)] out IOperation? actualOperation,
            [NotNullWhen(true)] out ConstraintExpression? constraintExpression)
        {
            if (assertOperation.TargetMethod.Name == NUnitFrameworkConstants.NameOfAssertThat
                && assertOperation.Arguments.Length >= 2)
            {
                actualOperation = assertOperation.Arguments[0].Value;
                constraintExpression = new ConstraintExpression(assertOperation.Arguments[1].Value);
                return true;
            }
            else
            {
                actualOperation = null;
                constraintExpression = null;

                return false;
            }
        }

        // Unwrap underlying type from delegate or awaitable.
        public static ITypeSymbol UnwrapActualType(ITypeSymbol actualType)
        {
            if (actualType is INamedTypeSymbol namedType)
            {
                var fullTypeName = namedType.GetFullMetadataName();
                if (fullTypeName == NUnitFrameworkConstants.FullNameOfActualValueDelegate ||
                    fullTypeName == NUnitFrameworkConstants.FullNameOfTestDelegate)
                {
                    ITypeSymbol returnType = namedType.DelegateInvokeMethod!.ReturnType;

                    if (returnType.IsAwaitable(out ITypeSymbol? awaitReturnType))
                        returnType = awaitReturnType;

                    return returnType;
                }
            }

            return actualType;
        }

        /// <summary>
        /// Get TypeSymbol from <paramref name="actualOperation"/>, and unwrap from delegate or awaitable.
        /// </summary>
        public static ITypeSymbol? GetUnwrappedActualType(IOperation actualOperation)
        {
            var actualType = actualOperation.Type;

            if (actualType is null || actualType.Kind == SymbolKind.ErrorType)
                return null;

            return UnwrapActualType(actualType);
        }

        /// <summary>
        /// Checks if the operation is an expression only using literal values.
        /// </summary>
        public static bool IsLiteralOperation(IOperation operation)
        {
            if (operation is ILiteralOperation)
                return true;

            if (operation is IUnaryOperation unary)
                return IsLiteralOperation(unary.Operand);

            if (operation is IBinaryOperation binary)
                return IsLiteralOperation(binary.LeftOperand) && IsLiteralOperation(binary.RightOperand);

            return false;
        }

        /// <summary>
        /// Detects if the current statement is an argument to Assert.Multiple.
        /// </summary>
        public static bool IsInsideAssertMultiple(SyntaxNode node)
        {
            // Look for Assert.Multiple(delegate) invocation.
            SyntaxNode currentNode = node;
            InvocationExpressionSyntax? possibleAssertMultipleInvocation;
            while ((possibleAssertMultipleInvocation = currentNode.Ancestors().OfType<InvocationExpressionSyntax>().FirstOrDefault()) is not null)
            {
                // Is the statement inside a Block which is part of an Assert.Multiple.
                if (IsAssert(possibleAssertMultipleInvocation, NUnitFrameworkConstants.NameOfMultiple, NUnitV4FrameworkConstants.NameOfMultipleAsync))
                {
                    return true;
                }

                // Keep looking at possible parent nested expression.
                currentNode = possibleAssertMultipleInvocation;
            }

            // Look for using (Assert.EnterMultipleScope()) invocation.
            currentNode = node;
            UsingStatementSyntax? usingStatement;
            while ((usingStatement = currentNode.Ancestors().OfType<UsingStatementSyntax>().FirstOrDefault()) is not null)
            {
                // Is the using expression an Assert.EnterMultipleScope.
                if (usingStatement.Expression is InvocationExpressionSyntax usingInvocation &&
                    IsAssert(usingInvocation, NUnitV4FrameworkConstants.NameOfEnterMultipleScope))
                {
                    return true;
                }

                // Keep looking at possible parent nested expression.
                currentNode = usingStatement;
            }

            return false;
        }

        /// <summary>
        /// Checks if the current statement is an Assert.<paramref name="requestedMembers"/> statement.
        /// </summary>
        public static bool IsAssert(ExpressionSyntax? expression, params string[] requestedMembers)
        {
            return IsAssert(expression, out string member, out _) && requestedMembers.Contains(member);
        }

        public static bool IsAssert(ExpressionSyntax? expression,
                                    out string member,
                                    [NotNullWhen(true)] out ArgumentListSyntax? argumentList)
        {
            return IsAssert(expression, x => x is NUnitFrameworkConstants.NameOfAssert, out member, out argumentList);
        }

        public static bool IsAssertClassicAssertOrAssume(ExpressionSyntax? expression,
                                           out string member,
                                           [NotNullWhen(true)] out ArgumentListSyntax? argumentList)
        {
            return IsAssert(expression,
                            x => x is NUnitFrameworkConstants.NameOfAssert
                                   or NUnitLegacyFrameworkConstants.NameOfClassicAssert
                                   or NUnitFrameworkConstants.NameOfAssume,
                            out member, out argumentList);
        }

        private static bool IsAssert(ExpressionSyntax? expression,
                                     Func<string, bool> isNameOfAssert,
                                     out string member,
                                     [NotNullWhen(true)] out ArgumentListSyntax? argumentList)
        {
            if (expression is InvocationExpressionSyntax invocationExpression &&
                invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression)
            {
                SimpleNameSyntax? className = GetClassName(memberAccessExpression);

                if (className is not null &&
                    isNameOfAssert(className.Identifier.Text))
                {
                    member = memberAccessExpression.Name.Identifier.Text;
                    argumentList = invocationExpression.ArgumentList;
                    return true;
                }
            }

            member = string.Empty;
            argumentList = null;
            return false;
        }

        private static SimpleNameSyntax? GetClassName(MemberAccessExpressionSyntax memberAccessExpression)
        {
            if (memberAccessExpression.Expression is MemberAccessExpressionSyntax nestedMemberAccessExpression)
                return nestedMemberAccessExpression.Name;

            if (memberAccessExpression.Expression is IdentifierNameSyntax identifierName)
                return identifierName;

            return null;
        }
    }
}
