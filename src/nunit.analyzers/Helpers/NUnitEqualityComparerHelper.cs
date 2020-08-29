using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.Helpers
{
    internal static class NUnitEqualityComparerHelper
    {
        /// <summary>
        /// Returns true if it is possible that <see cref="NUnit.Framework.Constraints.NUnitEqualityComparer.AreEqual"/>
        /// returns true for given argument types.
        /// False otherwise.
        /// </summary>
        public static bool CanBeEqual(
            ITypeSymbol? actualType,
            ITypeSymbol? expectedType,
            Compilation compilation,
            ImmutableHashSet<(ITypeSymbol, ITypeSymbol)>? checkedTypes = null)
        {
            if (actualType == null
                || actualType.TypeKind == TypeKind.Error
                || expectedType == null
                || expectedType.TypeKind == TypeKind.Error)
            {
                // Can't tell anything specific if type is wrong.
                return true;
            }

            if (actualType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
                actualType = ((INamedTypeSymbol)actualType).TypeArguments[0];

            if (expectedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
                expectedType = ((INamedTypeSymbol)expectedType).TypeArguments[0];

            var conversion = compilation.ClassifyConversion(actualType, expectedType);

            // Same Type possible
            if (conversion.IsIdentity || conversion.IsReference || conversion.IsBoxing || conversion.IsUnboxing)
            {
                return true;
            }

            // Numeric conversion
            if (conversion.IsNumeric)
                return true;

            // Protection against possible infinite recursion
            if (checkedTypes == default)
                checkedTypes = ImmutableHashSet<(ITypeSymbol, ITypeSymbol)>.Empty;

            if (checkedTypes.Contains((actualType, expectedType)))
                return false;

            checkedTypes = checkedTypes.Add((actualType, expectedType));

            var actualFullName = actualType.GetFullMetadataName();
            var expectedFullName = expectedType.GetFullMetadataName();

            if (actualType is INamedTypeSymbol namedActualType
                && expectedType is INamedTypeSymbol namedExpectedType)
            {
                // Tuples
                if (IsTuple(actualFullName) && IsTuple(expectedFullName))
                {
                    return namedActualType.TypeArguments.Length == namedExpectedType.TypeArguments.Length
                        && Enumerable.Range(0, namedActualType.TypeArguments.Length).All(i =>
                            CanBeEqual(namedActualType.TypeArguments[i], namedExpectedType.TypeArguments[i],
                                compilation, checkedTypes));
                }

                // ValueTuples
                if (namedActualType.IsTupleType && namedExpectedType.IsTupleType)
                {
                    return namedActualType.TupleElements.Length == namedActualType.TupleElements.Length
                        && Enumerable.Range(0, namedActualType.TupleElements.Length).All(i =>
                            CanBeEqual(namedActualType.TupleElements[i].Type, namedExpectedType.TupleElements[i].Type,
                                compilation, checkedTypes));
                }

                // Dictionaries
                if (IsDictionary(namedActualType, actualFullName, out var actualKeyType, out var actualValueType)
                    && IsDictionary(namedExpectedType, expectedFullName, out var expectedKeyType, out var expectedValueType))
                {
                    // Unlike for KeyValuePairs, Dictionaries Keys should match exactly

                    var keysConversion = compilation.ClassifyConversion(actualKeyType, expectedKeyType);
                    var keysMatching = keysConversion.IsIdentity || keysConversion.IsReference;

                    return keysMatching && CanBeEqual(actualValueType, expectedValueType, compilation, checkedTypes);
                }

                // KeyValuePairs
                if (IsKeyValuePair(namedActualType, actualFullName, out actualKeyType, out actualValueType)
                    && IsKeyValuePair(namedExpectedType, expectedFullName, out expectedKeyType, out expectedValueType))
                {
                    return CanBeEqual(actualKeyType, expectedKeyType, compilation, checkedTypes)
                        && CanBeEqual(actualValueType, expectedValueType, compilation, checkedTypes);
                }
            }

            // IEnumerables
            if (actualType.IsIEnumerable(out var actualElementType) && expectedType.IsIEnumerable(out var expectedElementType))
            {
                if (actualElementType == null || expectedElementType == null)
                {
                    // If actual or expected values implement only non-generic IEnumerable, we cannot determine
                    // whether types are suitable
                    return true;
                }
                else
                {
                    return CanBeEqual(actualElementType, expectedElementType, compilation, checkedTypes);
                }
            }

            // Streams
            if (IsStream(actualType, actualFullName) && IsStream(expectedType, expectedFullName))
                return true;

            // IEquatables
            if (IsIEquatable(actualType, expectedType) || IsIEquatable(expectedType, actualType))
                return true;

            return false;
        }

        private static bool IsStream(ITypeSymbol typeSymbol, string fullName)
        {
            const string streamFullName = "System.IO.Stream";

            return fullName == streamFullName
                || typeSymbol.GetAllBaseTypes().Any(t => t.GetFullMetadataName() == streamFullName);
        }

        private static bool IsKeyValuePair(INamedTypeSymbol typeSymbol, string fullSymbolName,
            [NotNullWhen(true)] out ITypeSymbol? keyType, [NotNullWhen(true)] out ITypeSymbol? valueType)
        {
            const string keyValuePairFullName = "System.Collections.Generic.KeyValuePair`2";

            if (typeSymbol.TypeArguments.Length == 2
                && fullSymbolName == keyValuePairFullName)
            {
                keyType = typeSymbol.TypeArguments[0];
                valueType = typeSymbol.TypeArguments[1];
                return true;
            }
            else
            {
                keyType = null;
                valueType = null;
                return false;
            }
        }

        private static bool IsDictionary(INamedTypeSymbol typeSymbol, string fullSymbolName,
            [NotNullWhen(true)] out ITypeSymbol? keyType, [NotNullWhen(true)] out ITypeSymbol? valueType)
        {
            const string dictionaryFullName = "System.Collections.Generic.Dictionary`2";

            if (typeSymbol.TypeArguments.Length == 2
                && fullSymbolName == dictionaryFullName)
            {
                keyType = typeSymbol.TypeArguments[0];
                valueType = typeSymbol.TypeArguments[1];
                return true;
            }
            else
            {
                keyType = null;
                valueType = null;
                return false;
            }
        }

        private static bool IsTuple(string fullName)
        {
            return fullName.StartsWith("System.Tuple`", StringComparison.Ordinal);
        }

        private static bool IsIEquatable(ITypeSymbol typeSymbol, ITypeSymbol equatableTypeArguments)
        {
            return typeSymbol.AllInterfaces.Any(i => i.TypeArguments.Length == 1
                && i.TypeArguments[0].Equals(equatableTypeArguments)
                && i.GetFullMetadataName() == "System.IEquatable`1");
        }
    }
}
