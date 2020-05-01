using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;
using NUnit.Analyzers.Syntax;

namespace NUnit.Analyzers.EqualToIncompatibleTypes
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EqualToIncompatibleTypesAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.EqualToIncompatibleTypes,
            title: EqualToIncompatibleTypesConstants.Title,
            messageFormat: EqualToIncompatibleTypesConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: EqualToIncompatibleTypesConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        protected override void AnalyzeAssertInvocation(SyntaxNodeAnalysisContext context,
            InvocationExpressionSyntax assertExpression, IMethodSymbol methodSymbol)
        {
            var cancellationToken = context.CancellationToken;
            var semanticModel = context.SemanticModel;

            if (!AssertHelper.TryGetActualAndConstraintExpressions(assertExpression, semanticModel,
                out var actualExpression, out var constraintExpression))
            {
                return;
            }

            foreach (var constraintPartExpression in constraintExpression.ConstraintParts)
            {
                if (HasIncompatiblePrefixes(constraintPartExpression)
                    || HasCustomEqualityComparer(constraintPartExpression)
                    || constraintPartExpression.HasUnknownExpressions())
                    continue;

                var constraintMethod = constraintPartExpression.GetConstraintMethod();

                if (constraintMethod?.Name != NunitFrameworkConstants.NameOfIsEqualTo
                    || constraintMethod.ReturnType?.GetFullMetadataName() != NunitFrameworkConstants.FullNameOfEqualToConstraint)
                {
                    continue;
                }

                var expectedArgumentExpression = constraintPartExpression.GetExpectedArgumentExpression();

                if (expectedArgumentExpression == null)
                    continue;

                var actualTypeInfo = semanticModel.GetTypeInfo(actualExpression, cancellationToken);
                var actualType = actualTypeInfo.Type ?? actualTypeInfo.ConvertedType;

                if (actualType == null)
                    continue;

                actualType = AssertHelper.UnwrapActualType(actualType);

                var expectedType = semanticModel.GetTypeInfo(expectedArgumentExpression, cancellationToken).Type;

                if (!CanBeAssertedForEquality(actualType, expectedType, semanticModel))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        descriptor,
                        expectedArgumentExpression.GetLocation()));
                }
            }
        }

        private static bool HasIncompatiblePrefixes(ConstraintPartExpression constraintPartExpression)
        {
            // Currently only 'Not' suffix supported, as all other suffixes change actual type for constraint
            // (e.g. All, Some, Property, Count, etc.)

            return constraintPartExpression.GetPrefixesNames().Any(s => s != NunitFrameworkConstants.NameOfIsNot);
        }

        private static bool HasCustomEqualityComparer(ConstraintPartExpression constraintPartExpression)
        {
            return constraintPartExpression.GetSuffixesNames().Any(s => s == NunitFrameworkConstants.NameOfUsing);
        }

        private static bool CanBeAssertedForEquality(
            ITypeSymbol? actualType,
            ITypeSymbol? expectedType,
            SemanticModel semanticModel,
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

            var conversion = semanticModel.Compilation.ClassifyConversion(actualType, expectedType);

            // Same Type possible
            if (conversion.IsIdentity || conversion.IsReference || conversion.IsBoxing || conversion.IsUnboxing)
            {
                return true;
            }

            // Numeric conversion
            if (conversion.IsNumeric)
                return true;

            // Protection against possible infinite recursion
            if (checkedTypes == default(ImmutableHashSet<(ITypeSymbol, ITypeSymbol)>))
                checkedTypes = ImmutableHashSet<(ITypeSymbol, ITypeSymbol)>.Empty;

            if (checkedTypes.Contains((actualType, expectedType)))
                return false;

            checkedTypes = checkedTypes.Add((actualType, expectedType));

            var actualFullName = actualType.GetFullMetadataName();
            var expectedFullName = expectedType.GetFullMetadataName();

            var namedActualType = actualType as INamedTypeSymbol;
            var namedExpectedType = expectedType as INamedTypeSymbol;

            if (namedActualType != null && namedExpectedType != null)
            {
                // Tuples
                if (IsTuple(actualFullName) && IsTuple(expectedFullName))
                {
                    return namedActualType.TypeArguments.Length == namedExpectedType.TypeArguments.Length
                        && Enumerable.Range(0, namedActualType.TypeArguments.Length).All(i =>
                            CanBeAssertedForEquality(namedActualType.TypeArguments[i], namedExpectedType.TypeArguments[i],
                                semanticModel, checkedTypes));
                }

                // ValueTuples
                if (namedActualType.IsTupleType && namedExpectedType.IsTupleType)
                {
                    return namedActualType.TupleElements.Length == namedActualType.TupleElements.Length
                        && Enumerable.Range(0, namedActualType.TupleElements.Length).All(i =>
                            CanBeAssertedForEquality(namedActualType.TupleElements[i].Type, namedExpectedType.TupleElements[i].Type,
                                semanticModel, checkedTypes));
                }

                // Dictionaries
                if (IsDictionary(namedActualType, actualFullName, out var actualKeyType, out var actualValueType)
                    && IsDictionary(namedExpectedType, expectedFullName, out var expectedKeyType, out var expectedValueType))
                {
                    // Unlike for KeyValuePairs, Dictionaries Keys should match exactly

                    var keysConversion = semanticModel.Compilation.ClassifyConversion(actualKeyType, expectedKeyType);
                    var keysMatching = keysConversion.IsIdentity || keysConversion.IsReference;

                    return keysMatching && CanBeAssertedForEquality(actualValueType, expectedValueType, semanticModel, checkedTypes);
                }

                // KeyValuePairs
                if (IsKeyValuePair(namedActualType, actualFullName, out actualKeyType, out actualValueType)
                    && IsKeyValuePair(namedExpectedType, expectedFullName, out expectedKeyType, out expectedValueType))
                {
                    return CanBeAssertedForEquality(actualKeyType, expectedKeyType, semanticModel, checkedTypes)
                        && CanBeAssertedForEquality(actualValueType, expectedValueType, semanticModel, checkedTypes);
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
                    return CanBeAssertedForEquality(actualElementType, expectedElementType, semanticModel, checkedTypes);
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
            return fullName.StartsWith("System.Tuple`");
        }

        private static bool IsIEquatable(ITypeSymbol typeSymbol, ITypeSymbol equatableTypeArguments)
        {
            return typeSymbol.AllInterfaces.Any(i => i.TypeArguments.Length == 1
                && i.TypeArguments[0].Equals(equatableTypeArguments)
                && i.GetFullMetadataName() == "System.IEquatable`1");
        }
    }
}
