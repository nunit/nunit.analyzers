using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;
using NUnit.Analyzers.Operations;

namespace NUnit.Analyzers.SomeItemsIncompatibleTypes
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SomeItemsIncompatibleTypesAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.SomeItemsIncompatibleTypes,
            title: SomeItemsIncompatibleTypesConstants.Title,
            messageFormat: SomeItemsIncompatibleTypesConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Error,
            description: SomeItemsIncompatibleTypesConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(descriptor);

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            if (!AssertHelper.TryGetActualAndConstraintOperations(assertOperation,
                out var actualOperation, out var constraintExpression))
            {
                return;
            }

            foreach (var constraintPart in constraintExpression.ConstraintParts)
            {
                if ((!IsDoesContain(constraintPart) && !IsContainsItem(constraintPart))
                    || (constraintPart.Root?.Type?.GetFullMetadataName() != NUnitFrameworkConstants.FullNameOfSomeItemsConstraint
                        && constraintPart.Root?.Type?.GetFullMetadataName() != NUnitV4FrameworkConstants.FullNameOfSomeItemsConstraintGeneric))
                {
                    continue;
                }

                if (constraintPart.HasIncompatiblePrefixes())
                    return;

                var expectedType = constraintPart.GetExpectedArgument()?.Type;
                var actualType = AssertHelper.GetUnwrappedActualType(actualOperation);

                if (actualType is null || expectedType is null)
                    continue;

                if (actualType.IsIEnumerable(out var elementType))
                {
                    // Cannot determine element type for non-generic IEnumerable, therefore consider valid.
                    if (elementType is null)
                        continue;

                    IInvocationOperation? usingInvocation = constraintPart.GetSuffix(NUnitFrameworkConstants.NameOfEqualConstraintUsing) as IInvocationOperation;
                    if (usingInvocation is not null)
                    {
                        IMethodSymbol target = usingInvocation.TargetMethod;
                        ImmutableArray<ITypeSymbol> typeArguments = target.TypeArguments;
                        switch (typeArguments.Length)
                        {
                            case 1:
                                // IEqualityComparer<T> or IComparer<T> or Comparison<T>
                                // Type must match.
                                break;
                            case 2:
                                // Func<TCollection, TMember, bool>
                                // Allows type translation to TMember.
                                if (NUnitEqualityComparerHelper.CanBeEqual(typeArguments[0], elementType, context.Compilation))
                                {
                                    elementType = typeArguments[1];
                                }

                                break;
                            case 0:
                                // IEqualityComparer, IComparer
                                // Could potentially compare any type to any type
                            default:
                                // Unknown new method
                                continue;
                        }
                    }

                    // Valid, if collection element type matches expected type.
                    if (NUnitEqualityComparerHelper.CanBeEqual(elementType, expectedType, context.Compilation))
                        continue;
                }

                var actualTypeDisplay = actualType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                var expectedTypeDisplay = expectedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    constraintPart.GetLocation(),
                    ConstraintDiagnosticDescription(constraintPart),
                    actualTypeDisplay,
                    expectedTypeDisplay));
            }
        }

        private static bool IsDoesContain(ConstraintExpressionPart constraintPart)
        {
            return constraintPart.GetConstraintName() == NUnitFrameworkConstants.NameOfDoesContain;
        }

        private static bool IsContainsItem(ConstraintExpressionPart constraintPart)
        {
            return constraintPart.HelperClass?.Name == NUnitFrameworkConstants.NameOfContains
                && constraintPart.GetConstraintName() == NUnitFrameworkConstants.NameOfContainsItem;
        }

        private static string ConstraintDiagnosticDescription(ConstraintExpressionPart constraintPart)
        {
            return constraintPart.HelperClass?.Name is not null
                ? $"{constraintPart.HelperClass?.Name}.{constraintPart.GetConstraintName()}"
                : (constraintPart.GetConstraintName() ?? "");
        }
    }
}
