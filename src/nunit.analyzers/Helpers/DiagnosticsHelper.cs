using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.Helpers
{
    internal static class DiagnosticsHelper
    {
        public static bool LastArgumentIsNonParamsArray(ImmutableArray<IArgumentOperation> arguments)
        {
            // Find out if the 'params' argument is an existing array and not one created from a params creation.
            return arguments[arguments.Length - 1].ArgumentKind != ArgumentKind.ParamArray;
        }

        public static ImmutableDictionary<string, string?> GetProperties(string methodName, ImmutableArray<IArgumentOperation> arguments)
        {
            bool argsIsArray = LastArgumentIsNonParamsArray(arguments);
            bool isEmptyCandidate = IsEmptyCandidate(arguments[0]);
            return new Dictionary<string, string?>
            {
                [AnalyzerPropertyKeys.ModelName] = methodName,
                [AnalyzerPropertyKeys.ArgsIsArray] = argsIsArray ? "ArgsIsArray" : string.Empty,
                [AnalyzerPropertyKeys.IsEmptyCandidate] = isEmptyCandidate ? "IsEmpty" : string.Empty
            }.ToImmutableDictionary();
        }

        private static bool IsEmptyCandidate(IArgumentOperation argumentOperation)
        {
            var operation = argumentOperation.Value;

            if (operation is IConversionOperation conversionOperation)
                operation = conversionOperation.Operand;

            if (operation is ILiteralOperation literalOperation &&
                literalOperation.ConstantValue.HasValue &&
                string.IsNullOrEmpty(literalOperation.ConstantValue.ToString()))
            {
                // Argument empty string literal.
                return true;
            }

            ISymbol? symbol;

            if (operation is IInvocationOperation invocationOperation &&
                invocationOperation.Instance is null &&
                invocationOperation.Arguments.Length == 0)
            {
                // Static method invocation with no arguments.
                // E.g.: Array.Empty<T>() or Enumerable.Empty<T>()
                symbol = invocationOperation.TargetMethod;
            }
            else if (operation is IMemberReferenceOperation memberReference &&
                memberReference.Instance is null)
            {
                // Static field or property.
                // E.g.: String.Empty, Guid.Empty or ImmutableArray<T>.Empty
                symbol = memberReference.Member;
            }
            else
            {
                symbol = null;
            }

            if (operation.Type is not null &&
                symbol is not null && symbol.Name == "Empty")
            {
                // We have to check the type of the member to be sure it's a supported type.
                // NUnit supports string, Guid, DirectoryInfo, ICollection, IEnumerable
                // and any type that has a Count property.
                var type = operation.Type;
                if (type is IArrayTypeSymbol ||
                    type.SpecialType is SpecialType.System_String
                                     or SpecialType.System_Collections_IEnumerable ||
                    type.OriginalDefinition.SpecialType is SpecialType.System_Collections_Generic_IEnumerable_T)
                {
                    return true;
                }

                string typeString = type.ToString()!;
                if (typeString is "System.Guid"
                               or "System.IO.DirectoryInfo")
                {
                    return true;
                }

                // Check if the type implements IEnumerable or IEnumerable<T>
                // Note that ICollection derives from IEnumerable, so no need to check that separately.
                if (type.AllInterfaces.Any(i => i.OriginalDefinition.SpecialType is SpecialType.System_Collections_Generic_IEnumerable_T ||
                                                i.SpecialType is SpecialType.System_Collections_IEnumerable))
                {
                    return true;
                }

                if (type is INamedTypeSymbol namedType && namedType.MemberNames.Contains("Count"))
                {
                    // Type has a Count property.
                    return true;
                }

                // A member called Empty which is not one of the supported types is not considered empty.
                return false;
            }

            return false;
        }
    }
}
