using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;
using NUnit.Analyzers.Operations;

namespace NUnit.Analyzers.ComparableTypes
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ComparableTypesAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly ImmutableHashSet<string> SupportedConstraints = ImmutableHashSet.Create(
            NUnitFrameworkConstants.NameOfIsLessThan,
            NUnitFrameworkConstants.NameOfIsLessThanOrEqualTo,
            NUnitFrameworkConstants.NameOfIsGreaterThan,
            NUnitFrameworkConstants.NameOfIsGreaterThanOrEqualTo);

        private static readonly DiagnosticDescriptor comparableTypesDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.ComparableTypes,
            title: ComparableTypesConstants.Title,
            messageFormat: ComparableTypesConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Error,
            description: ComparableTypesConstants.Description);

        private static readonly DiagnosticDescriptor comparableOnObjectDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.ComparableOnObject,
            title: ComparableOnObjectConstants.Title,
            messageFormat: ComparableOnObjectConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: ComparableOnObjectConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(comparableTypesDescriptor, comparableOnObjectDescriptor);

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            if (!AssertHelper.TryGetActualAndConstraintOperations(assertOperation,
                out var actualOperation, out var constraintExpression))
            {
                return;
            }

            if (actualOperation is null)
                return;

            var actualType = AssertHelper.GetUnwrappedActualType(actualOperation);

            if (actualType is null)
                return;

            foreach (var constraintPartExpression in constraintExpression.ConstraintParts)
            {
                if (constraintPartExpression.HasIncompatiblePrefixes()
                    || HasCustomComparer(constraintPartExpression)
                    || constraintPartExpression.HasUnknownExpressions())
                {
                    continue;
                }

                var constraintMethod = constraintPartExpression.GetConstraintMethod();
                if (constraintMethod is null)
                    continue;

                if (!SupportedConstraints.Contains(constraintMethod.Name))
                    continue;

                var expectedOperation = constraintPartExpression.GetExpectedArgument();
                if (expectedOperation is null)
                    continue;

                var expectedType = expectedOperation.Type;
                if (expectedType is null)
                    continue;

                if (actualType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
                    actualType = ((INamedTypeSymbol)actualType).TypeArguments[0];

                if (expectedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
                    expectedType = ((INamedTypeSymbol)expectedType).TypeArguments[0];

                if (actualType.SpecialType == SpecialType.System_Object ||
                    expectedType.SpecialType == SpecialType.System_Object)
                {
                    // An instance of object might not implement IComparable resulting in runtime errors.
                    context.ReportDiagnostic(Diagnostic.Create(
                        comparableOnObjectDescriptor,
                        expectedOperation.Syntax.GetLocation(),
                        constraintMethod.Name));
                }
                else if (!CanCompare(actualType, expectedType, context.Compilation))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        comparableTypesDescriptor,
                        expectedOperation.Syntax.GetLocation(),
                        constraintMethod.Name));
                }
            }
        }

        private static bool CanCompare(ITypeSymbol actualType, ITypeSymbol expectedType, Compilation compilation)
        {
            if (IsIComparable(actualType, expectedType) || IsIComparable(expectedType, actualType))
                return true;

            var conversion = compilation.ClassifyConversion(actualType, expectedType);
            if (conversion.IsNumeric)
            {
                // Shortcut numerics as per NUnitComparer
                return true;
            }

            // NUnit doesn't demand that IComparable is for the same type.
            // But MS does: https://docs.microsoft.com/en-us/dotnet/api/system.icomparable.compareto?view=netcore-3.1
            if (SymbolEqualityComparer.Default.Equals(actualType, expectedType) && IsIComparable(actualType))
                return true;

            return false;
        }

        private static bool IsIComparable(ITypeSymbol typeSymbol, ITypeSymbol comparableTypeArguments)
        {
            const string iComparable = "System.IComparable`1";

            if (typeSymbol is ITypeParameterSymbol typeParameterSymbol)
            {
                var constraints = typeParameterSymbol.ConstraintTypes;
                return constraints.Any(t => IsIComparable(t, comparableTypeArguments));
            }

            if (typeSymbol.TypeKind == TypeKind.Interface &&
                typeSymbol.GetFullMetadataName() == iComparable)
            {
                if (SymbolEqualityComparer.Default.Equals(((INamedTypeSymbol)typeSymbol).TypeArguments[0], comparableTypeArguments))
                {
                    return true;
                }
            }

            if (typeSymbol.AllInterfaces.Any(i => i.TypeArguments.Length == 1
                && SymbolEqualityComparer.Default.Equals(i.TypeArguments[0], comparableTypeArguments)
                && i.GetFullMetadataName() == iComparable))
            {
                return true;
            }

            // NUnit allows for an CompareTo method, even if not implementing IComparable.
            return typeSymbol.GetAllMembers().Any(x => x is IMethodSymbol methodSymbol && methodSymbol.Name == "CompareTo"
                && methodSymbol.Parameters.Length == 1 &&
                SymbolEqualityComparer.Default.Equals(methodSymbol.Parameters[0].Type, comparableTypeArguments));
        }

        private static bool IsIComparable(ITypeSymbol typeSymbol)
        {
            const string iComparable = "System.IComparable";

            if (typeSymbol is ITypeParameterSymbol typeParameterSymbol)
            {
                var constraints = typeParameterSymbol.ConstraintTypes;
                return constraints.Any(t => IsIComparable(t));
            }

            return (typeSymbol.TypeKind == TypeKind.Interface && typeSymbol.GetFullMetadataName() == iComparable) ||
                typeSymbol.AllInterfaces.Any(i => i.TypeArguments.Length == 0 && i.GetFullMetadataName() == iComparable);
        }

        private static bool HasCustomComparer(ConstraintExpressionPart constraintPartExpression)
        {
            return constraintPartExpression.GetSuffixesNames().Any(s => s == NUnitFrameworkConstants.NameOfUsing);
        }
    }
}
