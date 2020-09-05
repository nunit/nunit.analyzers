using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
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
            if (assertOperation.TargetMethod.Name == NunitFrameworkConstants.NameOfAssertThat
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
                if (fullTypeName == NunitFrameworkConstants.FullNameOfActualValueDelegate ||
                    fullTypeName == NunitFrameworkConstants.FullNameOfTestDelegate)
                {
                    ITypeSymbol returnType = namedType.DelegateInvokeMethod.ReturnType;

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

            if (actualType == null || actualType.Kind == SymbolKind.ErrorType)
                return null;

            return UnwrapActualType(actualType);
        }
    }
}
